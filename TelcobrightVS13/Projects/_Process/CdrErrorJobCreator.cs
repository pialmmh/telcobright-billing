using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TelcobrightFileOperations;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Quartz;
using QuartzTelcobright;
using TelcobrightInfra;
using TelcobrightMediation.Config;

namespace Process
{

    [Export("TelcobrightProcess", typeof(AbstractTelcobrightProcess))]
    public class CdrErrorJobCreator : AbstractTelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public override string RuleName => this.GetType().ToString();
        public override string HelpText => "Auto create Cdr Error Processing Job";
        public override int ProcessId => 110;

    
        public override void Execute(IJobExecutionContext schedulerContext)
        {
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            //this.TbConsole.WriteLine($"CdrErrorJobCreater {operatorName}");
            //return;
            try
            {
                TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                    schedulerContext, operatorName);
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
                            if (thisSwitch.SkipCdrDecoded == 1) continue;
                            bool errorCdrExists = context.Database.SqlQuery<long>(
                                    $@"select idcall from cdrerror where switchid={thisSwitch.idSwitch} limit 0,1")
                                .ToList()
                                .Any();
                            if (errorCdrExists == false) continue;
                            bool errorJobExists = context.Database.SqlQuery<string>(
                                    $@"select jobname from job where idne={thisSwitch.idSwitch}
                                       and jobname like 'autoError_%' and status!=1 limit 0,1").ToList().Any();
                            if (errorJobExists == true) continue;
                            List<SqlSingleWhereClauseBuilder> whereParamsSingle =
                                new List<SqlSingleWhereClauseBuilder>()
                                {
                                    new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And)
                                    {
                                        Expression = "starttime>=",
                                        ParamType = SqlWhereParamType.Datetime,
                                        AndOrType = 0,
                                        ParamValue = "2023-01-01 00:00:00",
                                        PrependWith = null,
                                        ApendWith = null
                                    },
                                    new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And)
                                    {
                                        Expression = "starttime<=",
                                        ParamType = SqlWhereParamType.Datetime,
                                        ParamValue = "2044-01-01 23:59:59",
                                        PrependWith = null,
                                        ApendWith = null
                                    },
                                    new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And)
                                    {
                                        Expression = "switchid=",
                                        ParamType = SqlWhereParamType.Numeric,
                                        ParamValue = thisSwitch.idSwitch.ToString(),
                                        PrependWith = null,
                                        ApendWith = null
                                    },
                                };

                            BatchSqlJobParamJson thisJobParam = new BatchSqlJobParamJson(
                                tableName: "cdrerror",
                                batchSize: 10000,
                                lstWhereParamsSingle: whereParamsSingle,
                                lstWhereParamsMulti: new List<SqlMultiWhereClauseBuilder>(),
                                columnExpressions: new List<string>() {"IdCall as RowId", "starttime as RowDateTime"},
                                startPartitionDate: new DateTime(2022, 01, 01),
                                endPartitionDate: new DateTime(2044, 01, 01),
                                partitionColName: "starttime",
                                rowIdColName: "idCall"
                            );

                            job newjob = new job();
                            newjob.Progress = 0;
                            newjob.idjobdefinition = 2;
                            newjob.Status = 6; //created
                            newjob.JobName = "autoError_" + DateTime.Now.ToMySqlFormatWithMsWithoutQuote() + "_" +
                                             thisSwitch.SwitchName;
                            newjob.CreationTime = DateTime.Now;
                            newjob.idNE = thisSwitch.idSwitch;
                            newjob.JobParameter = JsonConvert.SerializeObject(thisJobParam);
                            newjob.priority = 5;
                            context.jobs.Add(newjob);
                            context.SaveChanges();
                        } //try
                        catch (Exception e1)
                        {
                            this.TbConsole.WriteLine(e1.ToString());;
                            ErrorWriter wr =
                                new ErrorWriter(e1, "CdrErrorJobCreator/SwitchId:" + thisSwitch.idSwitch, null, "",
                                    operatorName);
                        } //catch
                    } //for each customerswitchinfo
                }
            }
            catch (Exception e1)
            {
                this.TbConsole.WriteLine(e1.ToString());;
                
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
