using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Linq;
using TelcobrightFileOperations;
using LibraryExtensions;
using LibraryExtensions.LibraryExtensions;
using MediationModel;
using Quartz;
using QuartzTelcobright;
using TelcobrightMediation.Config;
using Process.helper;

namespace Process
{

    [DisallowConcurrentExecution]
    [Export("TelcobrightProcess", typeof(AbstractTelcobrightProcess))]
    public class RamDiskCreator : AbstractTelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public override string RuleName => this.GetType().ToString();
        public override string HelpText => "Method to Create Cdr Job";
        public override int ProcessId => 112;

        public override void Execute(IJobExecutionContext schedulerContext)
        {
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                schedulerContext, operatorName);
            CdrSetting cdrSetting = tbc.CdrSetting;
            string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName, tbc);
            PartnerEntities context = new PartnerEntities(entityConStr);
            Console.WriteLine($"CdrJobCreater {operatorName}");
            try
            {
                int idOprator = context.telcobrightpartners
                   .Where(c => c.databasename == tbc.Telcobrightpartner.databasename).Select(c => c.idCustomer)
                   .First();
                foreach (ne thisSwitch in context.nes.Where(c => c.idCustomer == idOprator).ToList())
                {
                    string vaultName = thisSwitch.SourceFileLocations;
                    //Vault vault = tbc.DirectorySettings.Vaults.First(c => c.Name == vaultName);
                    FileLocation fileLocation = tbc.DirectorySettings.FileLocations[vaultName];
                    string vaultPath = fileLocation.StartingPath;
                    if (!Directory.Exists(vaultPath))
                    {
                        char[] mountingPoint = vaultPath.Split(':')[0].ToCharArray();
                        RamDiskHelper ramdisk = new RamDiskHelper(24576, mountingPoint[0]);
                        ramdisk.Create();
                    };
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter.WriteError(e1, "CdrJobCreator", null, "", operatorName, context);
            }
        }

        private bool CheckIfRamDiskExists(char mountingPoint)
        {
            return System.IO.Directory.Exists($"{mountingPoint}\\");
        }

        private static Dictionary<string, string> getExistingJobNames(PartnerEntities context,
            Dictionary<string, FileInfo> newJobNameVsFileName, int idSwitch)
        {

            List<string> newJobNames = newJobNameVsFileName.Keys.ToList();
            CollectionSegmenter<string> segmenter = new CollectionSegmenter<string>(newJobNames, 0);

            Func<IEnumerable<string>, List<string>> getExistingJobNamesInSegment = jobNames =>
                context.Database.SqlQuery<string>(
                    $@"select jobname from job 
                    where idjobdefinition=1 and idNe={idSwitch}
                    and jobname in ({string.Join(",", jobNames.Select(f => "'" + f + "'"))})").ToList();

            List<string> existingJobNames = segmenter
                .ExecuteMethodInSegmentsWithRetval(200000, getExistingJobNamesInSegment)
                .ToList();

            return existingJobNames.ToDictionary(n => n);
        }

        private static List<FileInfo> getCompletedJobNames(PartnerEntities context,
            Dictionary<string, FileInfo> existingJobs, int idSwitch)
        {
            CollectionSegmenter<string> segmenter = new CollectionSegmenter<string>(existingJobs.Keys, 0);
            Func<IEnumerable<string>, List<string>> getExistingJobNamesInSegment = jobNames =>
                context.Database.SqlQuery<string>(
                    $@"select jobname from job 
                    where status=1 and idjobdefinition=1 and idNe={idSwitch}
                    and jobname in ({string.Join(",", jobNames.Select(f => "'" + f + "'"))})").ToList();

            HashSet<string> completedJobNames = new HashSet<string>(segmenter
                .ExecuteMethodInSegmentsWithRetval(200000, getExistingJobNamesInSegment));

            return existingJobs.Where(kv => completedJobNames.Contains(kv.Key)).Select(kv => kv.Value).ToList();
        }

        private static void writeJobs(PartnerEntities context, ne thisSwitch, List<job> newJobCacheForWritingAtOnceToDB,
            int existingCount)
        {
            if (!newJobCacheForWritingAtOnceToDB.Any())
            {
                Console.WriteLine(" No new cdr file found, exiting...");
            }
            Console.WriteLine("Writing " + newJobCacheForWritingAtOnceToDB.Count + " cdr jobs to db...");
            context.jobs.AddRange(newJobCacheForWritingAtOnceToDB);
            context.SaveChanges();
            Console.WriteLine("switch: " + thisSwitch.SwitchName + ", new cdr job created:"
                                + newJobCacheForWritingAtOnceToDB.Count + ", already existing:" + existingCount);
            newJobCacheForWritingAtOnceToDB = new List<job>();
        }

        private static void SplitFileByByteLen(FileInfo fileInfo, FileSplitSetting fileSplitSetting)
        {
            if (fileSplitSetting.FileSplitType == "byte")
            {
                if (fileInfo.Length % fileSplitSetting.BytesPerRecord > 0)
                    throw new Exception($@"Filesize ({fileInfo.Length}) is not a multiple of 
                                                                number of bytes per cdr ({fileSplitSetting.BytesPerRecord})");

                //save original file in unsplit directory before spliting
                //DirectoryInfo currentDir = new DirectoryInfo(fileInfo.DirectoryName);
                //var unsplitPath = currentDir.FullName + Path.DirectorySeparatorChar + "unsplit";
                //bool unsplitExists = Directory.Exists(unsplitPath);
                //if (unsplitExists == false)
                //{
                //    Directory.CreateDirectory(unsplitPath);
                //}
                //string unsplitBackupFileName = unsplitPath + Path.DirectorySeparatorChar + fileInfo.Name;
                //if (File.Exists(unsplitBackupFileName))
                //{
                //    File.Delete(unsplitBackupFileName);
                //}
                //File.Copy(fileInfo.FullName, unsplitBackupFileName);

                FileSplitter.SplitFile(fileInfo,//split the files
                    fileSplitSetting.BytesPerRecord * fileSplitSetting.MaxRecordsInSingleFile);
                File.Delete(fileInfo.FullName);

                //create a database file to log the association of the split files to the original unsplit
                //this is required to backup original file instead of the split versions after cdr job processing

                //string historyFileName = unsplitPath + Path.DirectorySeparatorChar +
                //                         fileInfo.Name + ".history";
                //if (File.Exists(historyFileName))
                //{
                //    File.Delete(historyFileName);
                //}
                //string splitHistory = string.Join(",", splitedFileNames.Select(f => Path.GetFileName(f)));
                //File.AppendAllText(historyFileName, splitHistory + Environment.NewLine);

                var dirInfo = new DirectoryInfo(fileInfo.DirectoryName);
                string searchPattern = $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}*" +
                                    $"{fileInfo.Extension}";
                var splitFileInfos = dirInfo.GetFiles(searchPattern).ToList();
                long chunkSize = fileSplitSetting.BytesPerRecord * fileSplitSetting.MaxRecordsInSingleFile;
                long expectedSplitCount = fileInfo.Length / chunkSize;
                if (fileInfo.Length % chunkSize > 0) expectedSplitCount++;
                if (splitFileInfos.Any() == false || splitFileInfos.Count != expectedSplitCount)
                {
                    splitFileInfos.ForEach(f => f.MoveTo(f.FullName + ".tmp"));
                    throw new Exception($"File count after split({splitFileInfos?.Count}) " +
                                        $" does not match expected count({expectedSplitCount}).");
                }
                long splitFilesSize = splitFileInfos.Sum(f => f.Length);
                if (splitFilesSize != fileInfo.Length)
                {
                    splitFileInfos.ForEach(f => f.MoveTo(f.FullName + ".tmp"));
                    throw new Exception($"Total size of splitted files ({splitFilesSize}) " +
                                        $"is not equal to original file size({fileInfo.Length})");
                }

            }
            else
            {
                throw new NotImplementedException("No or unknown file split " +
                                                  "type specified. Value must be either 'byte' or 'text'. ");
            }
        }

        private static job CreateSingleCdrJob(PartnerEntities context, ne thisSwitch, FileInfo fileInfo,
            string jobName)
        {
            //confirm to check if there is an original, unsplit version of this file
            DirectoryInfo currentDir = new DirectoryInfo(fileInfo.DirectoryName);
            var unsplitPath = currentDir.FullName + Path.DirectorySeparatorChar + "unsplit";
            bool unsplitExists = Directory.Exists(unsplitPath);
            if (unsplitExists == false)
            {
                Directory.CreateDirectory(unsplitPath);
            }
            var extension = Path.GetExtension(fileInfo.Name);
            var fileNameAsArr = fileInfo.Name.Split('_');
            string historyFileNameOnly = String.Join("_", fileNameAsArr.Take(fileNameAsArr.Length - 1))
                 + extension + ".history";
            string historyFileName = unsplitPath + Path.DirectorySeparatorChar + historyFileNameOnly;
            string unsplitFileName = "";
            if (File.Exists(historyFileName) == true)
            {
                List<string> splitHistory = File.ReadAllText(historyFileName).Split(',')
                    .Select(name => name.Trim()).ToList();
                if (splitHistory.Contains(fileInfo.Name))
                {
                    unsplitFileName = "unsplit" + Path.DirectorySeparatorChar +
                        Path.GetFileName(historyFileName.Replace(".history", ""));
                }
            }

            //check if that filename already exists
            job newCdr = new job();
            newCdr.JobName = jobName;

            //if (exists == false) //File Name Does not exist
            //{
            int priority = context.enumjobdefinitions.First(c => c.id == 1).Priority;
            newCdr.idNE = thisSwitch.idSwitch;
            newCdr.CreationTime = DateTime.Now;
            newCdr.Status = 7; //local, so downloaded in local switch directory
            newCdr.priority = priority;
            newCdr.idjobdefinition = 1; //new cdr
            newCdr.JobParameter = "unsplitFileName=" + unsplitFileName;
            return newCdr;
            //context.jobs.Add(newCdr);
            //context.SaveChanges();
            //}
        }
    }
}
