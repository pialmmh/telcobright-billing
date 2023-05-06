using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using MediationModel;
using Quartz;
using QuartzTelcobright;
using TelcobrightFileOperations;
using TelcobrightMediation;
using TelcobrightMediation.Config;

//not system namespace, telcobright's namespace

namespace Process
{
    [Export("TelcobrightProcess", typeof(AbstractTelcobrightProcess))]
    public class ProcessFileLister : AbstractTelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public override string RuleName => "ProcessFileLister";
        public override string HelpText => "Sync one source local or remote directory to multiple local or remote directories";
        public override int ProcessId => 106;

        public class FileSyncPair
        {
            public FileLocation SrcLocation { get; set; }
            public FileLocation DstLocation { get; set; }
        }
        public override void Execute(IJobExecutionContext schedulerContext)
        {
            foreach (var process in System.Diagnostics.Process.GetProcesses().Where(p => p.ProcessName.Contains("werfault")))
            {
                process.Kill();
            }
            TelcobrightHeartbeat heartbeat1 = new TelcobrightHeartbeat("",1, "Listing files from remote server.");
            TelcobrightHeartbeat heartbeat2 = new TelcobrightHeartbeat("",2, "Writing jobs to db.");
            //return;//todo
            JobDataMap jobDataMap = schedulerContext.JobDetail.JobDataMap;
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName);
            try
            {
                TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                    schedulerContext, operatorName);
                SyncPair syncPair = tbc.DirectorySettings.SyncPairs[jobDataMap.GetString("syncPair")];
                if (syncPair.SkipSourceFileListing == true) return;
                SyncLocation srcLocation = syncPair.SrcSyncLocation;
                SyncLocation dstLocation = syncPair.DstSyncLocation;
                heartbeat1.start();//heatrbit1 start
                List<string> fileNames = srcLocation.GetFileNamesFiltered(syncPair.SrcSettings, tbc);
                if (tbc.CdrSetting.DescendingOrderWhileListingFiles == true)
                    fileNames = fileNames.OrderByDescending(c => c).ToList();
                if (fileNames.Any()) heartbeat1.end(); //heartbit1 successful
                using (PartnerEntities context=new PartnerEntities(entityConStr))
                {
                    var connection = context.Database.Connection;
                    connection.Open();
                    List<job> jobs=new List<job>();
                    foreach (string fileName in fileNames)
                    {
                        try
                        {
                            var fileCopyJob = FileUtil.CreateFileCopyJob(tbc, syncPair.Name, fileName, context);
                            if(fileCopyJob!=null) jobs.Add(fileCopyJob);
                        }
                        catch (Exception e1)
                        {
                            Console.WriteLine(e1);
                            ErrorWriter wr = new ErrorWriter(e1, "FileLister/Filename:" + fileName, null, "", operatorName);
                            continue; //with next file
                        }
                    }
                    if (jobs.Any()) {
                        heartbeat2.start();// heartbit2 start, probe for successful db write.
                        FileUtil.WriteFileCopyJobMultiple(jobs, context);
                        heartbeat2.end();
                    }
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter wr = new ErrorWriter(e1, "FileLister", null, "",operatorName);
            }
        }

    }
}
