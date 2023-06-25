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
            var processes = System.Diagnostics.Process.GetProcesses().ToList();
            foreach (var process in processes.Where(p => p.ProcessName.ToLower().Contains("werfault")))
            {
                process.Kill();
            }
            TelcobrightHeartbeat heartbeat1 = new TelcobrightHeartbeat("FileLister",1,3600, "Listing files from remote server.",3600);//todo: change to 3600
            TelcobrightHeartbeat heartbeat2 = new TelcobrightHeartbeat("FileLister",2,3600, "Writing jobs to db.",3600);
            //return;//todo
            JobDataMap jobDataMap = schedulerContext.JobDetail.JobDataMap;
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                schedulerContext, operatorName);
            string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName,tbc);
            try
            {
                SyncPair syncPair = tbc.DirectorySettings.SyncPairs[jobDataMap.GetString("syncPair")];
                if (syncPair.SkipSourceFileListing == true) return;
                SyncLocation srcLocation = syncPair.SrcSyncLocation;
                SyncLocation dstLocation = syncPair.DstSyncLocation;
                heartbeat1.start();//heatrbit1 start
                List<string> newFileNames = srcLocation.GetFileNamesFiltered(syncPair.SrcSettings, tbc);
                if (tbc.CdrSetting.DescendingOrderWhileListingFiles == true)
                    newFileNames = newFileNames.OrderByDescending(c => c).ToList();
                if(newFileNames.Any()) heartbeat1.end(); //heartbit1 successful, expect at least one good heartbeat in an hour
                using(PartnerEntities context=new PartnerEntities(entityConStr))
                {
                    var connection = context.Database.Connection;
                    connection.Open();
                    Dictionary<string, string> newJobNameVsFileName = newFileNames.Select(f => // kv<jobName,fileName>
                        new
                        {
                            jobName = FileUtil.prepareJobNamesToCheckIfExists(tbc, syncPair.Name, f, context),
                            fileName = f
                        }).ToDictionary(a => a.jobName, a => a.fileName);
                    Dictionary<string, string> existingJobNames = getExistingJobNames(context, newJobNameVsFileName);
                    newFileNames = new List<string>();
                    foreach (KeyValuePair<string, string> kv in newJobNameVsFileName)
                    {
                        bool jobExists = existingJobNames.ContainsKey(kv.Key);
                        if (jobExists == false)
                        {
                            newFileNames.Add(kv.Value);
                        }
                    }

                    List<job> jobs = new List<job>();
                    foreach (string fileName in newFileNames)
                    {
                        try
                        {
                            var fileCopyJob = FileUtil.CreateFileCopyJob(tbc, syncPair.Name, fileName, context);
                            if (fileCopyJob != null) jobs.Add(fileCopyJob);
                        }
                        catch (Exception e1)
                        {
                            Console.WriteLine(e1);
                            ErrorWriter wr = new ErrorWriter(e1, "FileLister/Filename:" + fileName, null, "", operatorName);
                            continue; //with next file
                        }
                    }
                    if (jobs.Any())
                    {
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

        private static Dictionary<string, string> getExistingJobNames(PartnerEntities context, Dictionary<string, string> newJobNameVsFileName)
        {
            return context.Database
                                    .SqlQuery<string>($@"select jobname from job 
                                             where idjobdefinition=6 
                                             jobname in ({string.Join(",", newJobNameVsFileName.Keys.Select(f => $"'{f}'"))})")
                                                         .ToDictionary(f => f.ToString());
        }
    }
}

