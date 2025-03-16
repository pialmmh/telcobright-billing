using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using LibraryExtensions;
using MediationModel;
using Quartz;
using QuartzTelcobright;
using TelcobrightFileOperations;
using TelcobrightMediation;
using TelcobrightMediation.Config;
using System.IO;

//not system namespace, telcobright's namespace

namespace Process
{
    [DisallowConcurrentExecution]
    [Export("TelcobrightProcess", typeof(AbstractTelcobrightProcess))]
    public class ProcessFileLister : AbstractTelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }

        public override string RuleName => "ProcessFileLister";

        public override string HelpText =>
            "Sync one source local or remote directory to multiple local or remote directories";

        public override int ProcessId => 106;

        public class FileSyncPair
        {
            public FileLocation SrcLocation { get; set; }
            public FileLocation DstLocation { get; set; }
        }

        public override void Execute(IJobExecutionContext schedulerContext)
        {
            //string operatorName1 = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            //TelcobrightConfig tbc1 = ConfigFactory.GetConfigFromSchedulerExecutionContext(
            //    schedulerContext, operatorName1);
            //JobDataMap jobDataMap1 = schedulerContext.JobDetail.JobDataMap;
            //string syncPairName1 = jobDataMap1.GetString("syncPair");
            //SyncPair syncPair1 = tbc1.DirectorySettings.SyncPairs[syncPairName1];
            //if (syncPair1.SkipSourceFileListing == true) return;
            //SyncLocation srcLocation1 = syncPair1.SrcSyncLocation;
            //var binPath1 = System.IO.Path.GetDirectoryName(
            //System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            //binPath1 = binPath1.Substring(6);
            //var LogFileName1 = binPath1 + Path.DirectorySeparatorChar + "telcobright1.log";
            //StreamWriter writer = new StreamWriter(LogFileName1, true); // 'true' appends to the file
            //writer.WriteLine("FileLister start on - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " from " + srcLocation1);
            
            var processes = System.Diagnostics.Process.GetProcesses().ToList();
            foreach (var process in processes.Where(p => p.ProcessName.ToLower().Contains("werfault")))
            {
                process.Kill();
            }
            TelcobrightHeartbeat heartbeat1 =
                new TelcobrightHeartbeat("FileLister", 1, 3600, "Listing files from remote server.",
                    3600); //todo: change to 3600
            TelcobrightHeartbeat heartbeat2 =
                new TelcobrightHeartbeat("FileLister", 2, 3600, "Writing jobs to db.", 3600);
            //return;//todo
            JobDataMap jobDataMap = schedulerContext.JobDetail.JobDataMap;
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                schedulerContext, operatorName);
            string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName, tbc);
            PartnerEntities context = new PartnerEntities(entityConStr);

            try
            {
                string syncPairName = jobDataMap.GetString("syncPair");
                SyncPair syncPair = tbc.DirectorySettings.SyncPairs[syncPairName];
                //File.AppendAllText("debug.log", $"Started In : {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}, {this.GetType().ToString()} : {syncPair.Name.ToString()} " + Environment.NewLine);
                if (syncPair.SkipSourceFileListing == true) return;
                SyncLocation srcLocation = syncPair.SrcSyncLocation;
                SyncLocation dstLocation = syncPair.DstSyncLocation;
                heartbeat1.start(); //heartbit1 start
                List<string> newFileNames = srcLocation.GetFileNamesFiltered(syncPair.SrcSettings, tbc);
                if (tbc.CdrSetting.DescendingOrderWhileListingFiles == true)
                    newFileNames = newFileNames.OrderByDescending(c => c).ToList();

                int fileNameLengthFromRightWhileSorting = tbc.CdrSetting.FileNameLengthFromRightWhileSorting;
                if (fileNameLengthFromRightWhileSorting > 0)//true for sms hub
                {
                    if (tbc.CdrSetting.DescendingOrderWhileListingFilesByFileNameOnly == true)
                    {
                        newFileNames = newFileNames.OrderByDescending(name => name.Right(fileNameLengthFromRightWhileSorting)).ToList();
                    }
                    else if (tbc.CdrSetting.DescendingOrderWhileListingFilesByFileNameOnly == false)
                    {
                        newFileNames = newFileNames.OrderBy(name => name.Right(fileNameLengthFromRightWhileSorting)).ToList();
                    }
                }
              
                if (newFileNames.Any())
                    heartbeat1.end(); //heartbit1 successful, expect at least one good heartbeat in an hour
                {
                    var connection = context.Database.Connection;
                    connection.Open();
                    Dictionary<string, string> newJobNameVsFileName = newFileNames.Select(f => // kv<jobName,fileName>
                        new
                        {
                            jobName = FileUtil.prepareJobNamesToCheckIfExists(tbc, syncPair.Name, f),
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
                            ErrorWriter.WriteError(e1, "FileLister/Filename:" + fileName, null, "", operatorName, context);
                            continue; //with next file
                        }
                    }
                    if (jobs.Any())
                    {
                        heartbeat2.start(); // heartbit2 start, probe for successful db write.
                        FileUtil.WriteFileCopyJobMultiple(jobs, context);
                        heartbeat2.end();
                    }
                }
                //writer.WriteLine("FileLister end on - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " from " + srcLocation1);
                //File.AppendAllText("debug.log", $"Ended In : {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}, {this.GetType().ToString()} : {syncPair.Name.ToString()} " + Environment.NewLine);
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter.WriteError(e1, "FileLister", null, "", operatorName, context);
            }
        }

        private static Dictionary<string, string> getExistingJobNames(PartnerEntities context,
            Dictionary<string, string> newJobNameVsFileName)
        {

            List<string> newJobNames = newJobNameVsFileName.Keys.ToList();
            CollectionSegmenter<string> segmenter = new CollectionSegmenter<string>(newJobNames, 0);

            Func<IEnumerable<string>, List<string>> getExistingJobNamesInSegment = jobNames =>
                context.Database.SqlQuery<string>(
                    $@"select jobname from job 
                    where idjobdefinition=6 
                    and jobname in ({string.Join(",", jobNames.Select(f => "'" + f + "'"))})").ToList();

            List<string> existingJobNames = segmenter
                .ExecuteMethodInSegmentsWithRetval(200000, getExistingJobNamesInSegment)
                .ToList();
            return existingJobNames.ToDictionary(n => n);
        }
    }
}

