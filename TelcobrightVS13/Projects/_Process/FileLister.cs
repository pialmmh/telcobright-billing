using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
    [Export("TelcobrightProcess", typeof(ITelcobrightProcess))]

    public class ProcessFileLister : ITelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => "ProcessFileLister";
        public string HelpText => "Sync one source local or remote directory to multiple local or remote directories";
        public int ProcessId => 106;

        public class FileSyncPair
        {
            public FileLocation SrcLocation { get; set; }
            public FileLocation DstLocation { get; set; }
        }

        public void Execute(IJobExecutionContext schedulerContext)
        {
            JobDataMap jobDataMap = schedulerContext.JobDetail.JobDataMap;
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName);
            try
            {
                TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                    schedulerContext, operatorName);
                SyncPair syncPair = tbc.DirectorySettings.SyncPairs[jobDataMap.GetString("SyncPair")];
                if (syncPair.SkipSourceFileListing == true) return;
                SyncLocation srcLocation = syncPair.SrcSyncLocation;
                SyncLocation dstLocation = syncPair.DstSyncLocation;
                List<string> fileNames = srcLocation.GetFileNamesFiltered(syncPair.SrcSettings, tbc);
                if (tbc.CdrSetting.DescendingOrderWhileListingFiles == true)
                    fileNames = fileNames.OrderByDescending(c => c).ToList();

                using (PartnerEntities context=new PartnerEntities(entityConStr))
                {
                    foreach (string fileName in fileNames)
                    {
                        try
                        {
                            FileUtil.CreateFileCopyJob(tbc, syncPair.Name, fileName, context);
                        }
                        catch (Exception e1)
                        {
                            Console.WriteLine(e1);
                            ErrorWriter wr = new ErrorWriter(e1, "FileLister/Filename:" + fileName, null, "", operatorName);
                            continue; //with next file
                        }
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
