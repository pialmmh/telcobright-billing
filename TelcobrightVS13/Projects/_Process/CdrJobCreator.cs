using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TelcobrightFileOperations;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using MySql.Data.MySqlClient;
using Quartz;
using QuartzTelcobright;
using TelcobrightMediation.Config;

namespace Process
{

    [Export("TelcobrightProcess", typeof(ITelcobrightProcess))]
    public class CdrJobCreator : ITelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => this.GetType().ToString();
        public string HelpText => "Method to Create Cdr Job";
        public int ProcessId => 101;

        public void Execute(IJobExecutionContext schedulerContext)
        {
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            try
            {
                TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                    schedulerContext, operatorName);
                string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName);
                using (PartnerEntities context = new PartnerEntities(entityConStr))
                {
                    int idOprator = context.telcobrightpartners
                        .Where(c => c.databasename == tbc.DatabaseSetting.GetOperatorName).Select(c => c.idCustomer)
                        .First();
                    foreach (ne thisSwitch in context.nes.Where(c => c.idCustomer == idOprator).ToList())
                    {
                        try
                        {
                            List<job> newJobCacheForWritingAtOnceToDB = new List<job>();
                            if (thisSwitch.SkipCdrListed == 1) continue;
                            Console.WriteLine($"Checking new cdr files for Switch {thisSwitch.SwitchName} in vault...");
                            string vaultName = thisSwitch.SourceFileLocations;
                            Vault vault = tbc.DirectorySettings.Vaults.First(c => c.Name == vaultName);
                            var fileNames = vault.GetFileListLocal()
                                .Where(fInfo => fInfo.Extension == thisSwitch.FileExtension
                                    && !fInfo.Name.EndsWith(".tmp") && !fInfo.Name.Contains(".filepart")).ToList();
                            Console.WriteLine($"Found {fileNames.Count} files, checking split history...");
                            if (tbc.CdrSetting.DescendingOrderWhileListingFiles == true)
                                fileNames = fileNames.OrderByDescending(c => c.Name).ToList();
                            foreach (FileInfo fileInfo in fileNames)
                            {
                                FileSplitSetting fileSplitSetting = tbc.CdrSetting.FileSplitSetting;
                                if (fileSplitSetting == null ||
                                    fileInfo.Length <= fileSplitSetting.SplitFileIfSizeBiggerThanMbyte)
                                {
                                    job newJob = CreateSingleCdrJob(context, thisSwitch, fileInfo, fileInfo.Name);
                                    newJobCacheForWritingAtOnceToDB.Add(newJob);
                                    //Console.WriteLine("Found cdr file for switch " + thisSwitch.SwitchName + ": " + newJob.JobName);
                                    if (tbc.CdrSetting.BatchSizeForCdrJobCreationCheckingExistence ==
                                        newJobCacheForWritingAtOnceToDB.Count)
                                    {
                                        writeJobs(context, thisSwitch, newJobCacheForWritingAtOnceToDB);
                                    }
                                }
                                else if (fileInfo.Length > fileSplitSetting.SplitFileIfSizeBiggerThanMbyte)
                                {
                                    SplitFileByByteLen(context, thisSwitch, fileInfo, fileSplitSetting);
                                }
                            }
                            writeJobs(context, thisSwitch, newJobCacheForWritingAtOnceToDB);
                        } //try
                        catch (Exception e1)
                        {
                            Console.WriteLine(e1);
                            ErrorWriter wr =
                                new ErrorWriter(e1, "CdrJobCreator/SwitchId:" + thisSwitch.idSwitch, null, "",
                                    operatorName);
                        } //catch
                    } //for each customerswitchinfo
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter wr = new ErrorWriter(e1, "CdrJobCreator", null, "", operatorName);
            }
        }

        private static void writeJobs(PartnerEntities context, ne thisSwitch, List<job> newJobCacheForWritingAtOnceToDB)
        {
            if (!newJobCacheForWritingAtOnceToDB.Any())
            {
                Console.WriteLine(" No new cdr file found, exiting...");
            }
            Console.WriteLine($"Found {newJobCacheForWritingAtOnceToDB.Count} cdr files in vault, excluding duplicates...");
            List<string> newJobNames = newJobCacheForWritingAtOnceToDB.Select(j => j.JobName).ToList();
            var sql = $@"select * from ({string.Join(" union all ", newJobNames.Select(name => "select '" + name + "' as jobname "))})
                                            as newJob 
                                            where jobname not in 
                                            (select jobname from job where idne={thisSwitch.idSwitch.ToString()} 
                                            and idjobdefinition=1)";
            List<string> nonExistingJobNames = context.Database.SqlQuery<string>(sql).ToList();
            List<job> jobsToWrite = newJobCacheForWritingAtOnceToDB
                .Where(j => nonExistingJobNames.Contains(j.JobName)).ToList();

            if (jobsToWrite.Any())
            {
                Console.WriteLine("Writing " + jobsToWrite.Count + " cdr jobs to db...");
                context.jobs.AddRange(jobsToWrite);
                context.SaveChanges();
            }
            Console.WriteLine(jobsToWrite.Count + " cdrjobs created successfully for switch: " + thisSwitch.SwitchName);
            newJobCacheForWritingAtOnceToDB = new List<job>();
        }

        private static void SplitFileByByteLen(PartnerEntities context, ne thisSwitch, FileInfo fileInfo, FileSplitSetting fileSplitSetting)
        {
            if (fileSplitSetting.FileSplitType == "byte")
            {
                if (fileInfo.Length % fileSplitSetting.BytesPerRecord > 0)
                    throw new Exception($@"Filesize ({fileInfo.Length}) is not a multiple of 
                                                                number of bytes per cdr ({fileSplitSetting.BytesPerRecord})");

                //save original file in unsplit directory before spliting
                DirectoryInfo currentDir = new DirectoryInfo(fileInfo.DirectoryName);
                var unsplitPath = currentDir.FullName + Path.DirectorySeparatorChar + "unsplit";
                bool unsplitExists = Directory.Exists(unsplitPath);
                if (unsplitExists == false)
                {
                    Directory.CreateDirectory(unsplitPath);
                }
                string unsplitBackupFileName = unsplitPath + Path.DirectorySeparatorChar + fileInfo.Name;
                if (File.Exists(unsplitBackupFileName))
                {
                    File.Delete(unsplitBackupFileName);
                }
                File.Copy(fileInfo.FullName, unsplitBackupFileName);

                List<string> splitedFileNames = FileSplitter.SplitFile(fileInfo,//split the files
                    fileSplitSetting.BytesPerRecord * fileSplitSetting.MaxRecordsInSingleFile);
                File.Delete(fileInfo.FullName);

                //create a database file to log the association of the split files to the original unsplit
                //this is required to backup original file instead of the split versions after cdr job processing
                string historyFileName = unsplitPath + Path.DirectorySeparatorChar +
                                         fileInfo.Name + ".history";
                if (File.Exists(historyFileName))
                {
                    File.Delete(historyFileName);
                }
                string splitHistory = string.Join(",", splitedFileNames.Select(f => Path.GetFileName(f)));
                File.AppendAllText(historyFileName, splitHistory + Environment.NewLine);

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
