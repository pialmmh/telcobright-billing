using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using TelcobrightFileOperations;
using LibraryExtensions;
using MediationModel;
using Quartz;
using QuartzTelcobright;
using TelcobrightMediation.Config;

namespace Process
{

    [Export("TelcobrightProcess", typeof(AbstractTelcobrightProcess))]
    public class CdrJobCreator : AbstractTelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public override string RuleName => this.GetType().ToString();
        public override string HelpText => "Method to Create Cdr Job";
        public override int ProcessId => 101;

        public override void Execute(IJobExecutionContext schedulerContext)
        {
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");

            this.TbConsole.WriteLine($"CdrJobCreater {operatorName}");
            try
            {
                TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                    schedulerContext, operatorName);
                CdrSetting cdrSetting = tbc.CdrSetting;
                string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName, tbc);
                using (PartnerEntities context = new PartnerEntities(entityConStr))
                {
                    int idOprator = context.telcobrightpartners
                        .Where(c => c.databasename == tbc.Telcobrightpartner.databasename).Select(c => c.idCustomer)
                        .First();
                    foreach (ne thisSwitch in context.nes.Where(c => c.idCustomer == idOprator).ToList())
                    {
                        try
                        {
                            List<job> newJobCacheForWritingAtOnceToDB = new List<job>();
                            if (thisSwitch.SkipCdrListed == 1) continue;
                            Console.WriteLine($"Checking new cdr files for Switch {thisSwitch.SwitchName} in vault...");
                            this.TbConsole.WriteLine($"Checking new cdr files for Switch {thisSwitch.SwitchName} in vault...");
                            string vaultName = thisSwitch.SourceFileLocations;
                            //Vault vault = tbc.DirectorySettings.Vaults.First(c => c.Name == vaultName);
                            FileLocation fileLocation = tbc.DirectorySettings.FileLocations[vaultName];
                            string vaultPath = fileLocation.StartingPath;
                            DirectoryLister dirlister = new DirectoryLister();
                            if (cdrSetting.UnzipCompressedFiles==true)
                            {
                                List<FileInfo> zipFiles = dirlister.ListLocalDirectoryZipfileNonRecursive(vaultPath);
                                if (zipFiles.Count > 0)
                                {
                                    Console.WriteLine($"Found {zipFiles.Count} zip files. Unzipping ...");
                                    this.TbConsole.WriteLine($"Found {zipFiles.Count} zip files. Unzipping ...");
                                    foreach (FileInfo compressedFile in zipFiles)
                                    {
                                        CompressedFileHelperForVault compressedFileHelper= 
                                            new CompressedFileHelperForVault(new List<string> { thisSwitch.FileExtension},vaultPath);
                                        compressedFileHelper.ExtractWithSafeCopy(compressedFile.FullName);
                                        compressedFile.Delete();//code reaching here means extraction successful, exceptions will not hit this and file will be kept
                                        UnZipper unzipper = new UnZipper(compressedFile.FullName);
                                        unzipper.UnZipAll();
                                    }
                                }
                            }
                            
                            //var fileNames = vault.GetFileListLocal()
                            //var fileNames = Directory.GetFiles()
                            //Dictionary<string, FileInfo> fileNames = new Dictionary<string, FileInfo>();

                            //List<FileInfo> localFiles = this.LocalLocation.GetLocalFilesNonRecursive();
                            List<string> validFilePrefixes =
                                thisSwitch.CDRPrefix.Split(',').Select(s => s.Trim()).ToList();

                            List<FileInfo> fileInfos = dirlister.ListLocalDirectoryNonRecursive(vaultPath)
                                .Where(fInfo =>
                                {
                                    if (thisSwitch.FileExtension == null)
                                    {
                                        return validFilePrefixes.Any(p => fInfo.Name.StartsWith(p)) && !fInfo.Name.EndsWith(".tmp") && !fInfo.Name.Contains(".filepart");
                                    }
                                    return validFilePrefixes.Any(p => fInfo.Name.StartsWith(p)) &&
                                                                        fInfo.Extension == thisSwitch.FileExtension
                                                                        && !fInfo.Name.EndsWith(".tmp") && !fInfo.Name.Contains(".filepart");
                                }).ToList();
                            int minDurationToSkip = fileLocation.DurationSecToSkipVeryNewPossiblyIncompleteFiles;

                            fileInfos = fileInfos.Where(f =>
                              {
                                  DateTime currentTime = DateTime.Now;
                                  return (currentTime - f.CreationTime).TotalSeconds > minDurationToSkip;
                              }).ToList();

                            FileSplitSetting fileSplitSetting = tbc.CdrSetting.FileSplitSetting;
                            if (fileSplitSetting != null)
                            {
                                foreach (FileInfo file in fileInfos)
                                {
                                    if (file.Length > fileSplitSetting.SplitFileIfSizeBiggerThanMbyte)
                                    {
                                        SplitFileByByteLen(file, fileSplitSetting);
                                    }
                                }
                            }

                           
                            //most of the files should be finished written by now, still...
                            //double check if file is still being written, by trying exclusive f open
                            var templist = fileInfos;
                            fileInfos = new List<FileInfo>();
                            foreach (var fileInfo in templist)
                            {
                                if (FileAndPathHelper.IsFileLockedOrBeingWritten(fileInfo) == false)
                                {
                                    fileInfos.Add(fileInfo);
                                }
                            }
                            //**************
                            Dictionary<string, FileInfo> newJobNameVsFileInfos = fileInfos.Select(f => // kv<jobName,fileName>
                               new
                               {
                                   jobName = f.Name,
                                   fileName = f
                               }).ToDictionary(a => a.jobName, a => a.fileName);
                            Dictionary<string, string> existingJobNames
                                = getExistingJobNames(context, newJobNameVsFileInfos, thisSwitch.idSwitch);

                            fileInfos = new List<FileInfo>();
                            foreach (KeyValuePair<string, FileInfo> kv in newJobNameVsFileInfos)
                            {
                                bool jobExists = existingJobNames.ContainsKey(kv.Key);
                                if (jobExists == false)
                                {
                                    fileInfos.Add(kv.Value);
                                }
                            }
                            //*************
                            Console.WriteLine($"Found {fileInfos.Count} new files with no prev job, checking split history...");
                            this.TbConsole.WriteLine($"Found {fileInfos.Count} new files with no prev job, checking split history...");
                            if (tbc.CdrSetting.DescendingOrderWhileListingFiles == true)
                                fileInfos = fileInfos.OrderByDescending(c => c.Name).ToList();
                            foreach (FileInfo fileInfo in fileInfos)
                            {
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
                                        newJobCacheForWritingAtOnceToDB = new List<job>();
                                    }
                                }

                                //else if (fileInfo.Length > fileSplitSetting.SplitFileIfSizeBiggerThanMbyte)
                                //{
                                //    SplitFileByByteLen(context, thisSwitch, fileInfo, fileSplitSetting);
                                //}
                            }
                            if (newJobCacheForWritingAtOnceToDB.Count > 0)
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
        private static void writeJobs(PartnerEntities context, ne thisSwitch, List<job> newJobCacheForWritingAtOnceToDB)
        {
            if (!newJobCacheForWritingAtOnceToDB.Any())
            {
                Console.WriteLine(" No new cdr file found, exiting...");
            }
            Console.WriteLine(
                $"Found {newJobCacheForWritingAtOnceToDB.Count} cdr files in vault, excluding duplicates...");
            Console.WriteLine("Writing " + newJobCacheForWritingAtOnceToDB.Count + " cdr jobs to db...");
            context.jobs.AddRange(newJobCacheForWritingAtOnceToDB);
            context.SaveChanges();
            Console.WriteLine("switch: " + thisSwitch.SwitchName + ", new cdr job created:"
                                + newJobCacheForWritingAtOnceToDB.Count + ", already existing:" +
                                newJobCacheForWritingAtOnceToDB.Count);
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
