using TelcobrightMediation;
using MySql.Data.MySqlClient;
using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using TelcobrightFileOperations;
using System.Reflection;
using System.Threading.Tasks;
using MediationModel;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using Quartz;
using LibraryExtensions;
using QuartzTelcobright;
using TelcobrightInfra;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;
using System.IO;
using System.Text;
using System.Threading;
using TelcobrightInfra.PerformanceAndOptimization;
using TelcobrightMediation.Mediation.Cdr;

namespace LogPreProcessor
{
    class ThreadSafePredecoder
    {
        public CdrJobInputData CdrJobInputData { get; set; }
        public PartnerEntities PartnerEntitiesNewInstance { get; set; }
        public ITelcobrightJob NewCdrFileJobNewInstance { get; set; }
        public string PredecodedFileName { get; set; }
        public string PredecodedDirName { get; set; }

        public ThreadSafePredecoder(CdrJobInputData cdrJobInputData, PartnerEntities partnerEntitiesNewInstance, ITelcobrightJob newCdrFileJobNewInstance, string predecodedFileName, string predecodedDirName)
        {
            CdrJobInputData = cdrJobInputData;
            PartnerEntitiesNewInstance = partnerEntitiesNewInstance;
            NewCdrFileJobNewInstance = newCdrFileJobNewInstance;
            PredecodedFileName = predecodedFileName;
            PredecodedDirName = predecodedDirName;
        }

        public PredecoderOutput preDecodeToFile()
        {
            Console.WriteLine("Predecoding CdrJob for Switch:" + this.CdrJobInputData.Ne.SwitchName + ", JobName:" +
                              this.CdrJobInputData.Job.JobName);
            PredecoderOutput output = new PredecoderOutput();
            try
            {
                var preProcessorInput = new Dictionary<string, object>
                {
                    {"cdrJobInputData", this.CdrJobInputData},
                    {"preDecodingStageOnly", true}
                };
                NewCdrPreProcessor newCdrPreProcessor =
                    (NewCdrPreProcessor)this.NewCdrFileJobNewInstance.PreprocessJob(preProcessorInput); //EXECUTE preDecoding
                long originalCdrFileSize = newCdrPreProcessor.OriginalCdrFileSize;
                List<string[]> txtRows = newCdrPreProcessor.TxtCdrRows;

                List<string[]> cdrInconsistents = newCdrPreProcessor.OriginalCdrinconsistents
                    .Select(c => CdrConversionUtil.ConvertCdrinconsistentToTxtRow(c, this.CdrJobInputData.MediationContext.TotalFieldTelcobright)).ToList();
                txtRows.AddRange(cdrInconsistents);
                List<string> rowsAsCsvLinesFieldsEnclosedWithBacktick =
                    txtRows.Select(row => string.Join(",",
                            row.Select(field => new StringBuilder("`").Append(field).Append("`").ToString()).ToArray()))
                        .ToList();
                File.WriteAllLines(this.PredecodedFileName, rowsAsCsvLinesFieldsEnclosedWithBacktick);
                FileInfo predecodedFileInfo = new FileInfo(this.PredecodedFileName);
                //if (originalCdrFileSize > 0)
                //{
                //    if (!cdrInconsistents.Any() && !txtRows.Any())
                //    {
                //        throw new Exception($"No decoded rows found for non empty cdr file {this.CdrJobInputData.Job.JobName}");
                //    }
                //    if (cdrInconsistents.Any() || txtRows.Any())
                //    {
                //        if (predecodedFileInfo.Length == 0)
                //            throw new Exception("Predecoded File size cannot be zero.");
                //    }
                //}
                
                output.SuccessfulJob = this.CdrJobInputData.Job;
                output.WrittenFileSize = predecodedFileInfo.Length;
                output.FailedJob = null;
                output.ExceptionMessage = "";
                this.PartnerEntitiesNewInstance.Database.Connection.Close();
                return output;
            }
            catch (Exception e)
            {
                this.PartnerEntitiesNewInstance.Database.Connection.Close();
                Console.WriteLine(e);
                output.FailedJob = this.CdrJobInputData.Job;
                output.ExceptionMessage = e.Message;
                output.SuccessfulJob = null;
                return output;
            }
        }
    }

    class PredecoderOutput
    {
        public job SuccessfulJob { get; set; }
        public long WrittenFileSize { get; set; }
        public job FailedJob { get; set; }
        public string ExceptionMessage { get; set; }
    }
    [Export("LogPreprocessor", typeof(EventPreprocessingRule))]
    public class CdrPredecoder : EventPreprocessingRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => this.GetType().ToString();
        public string HelpText => "Predecodes cdr rows as telcoright string[] in text file";
        public bool ProcessCollectionOnly { get; set; } = true;
        public bool IsPrepared { get; set; }
        public object RuleConfigData { get; set; }
        private int maxParallelFileSizeForDecode { get; set; }
        private readonly object locker = new object();
        public void PrepareRule()
        {
            Dictionary<string, object> dataAsDic = (Dictionary<string, object>) this.RuleConfigData;
            string val=(string) dataAsDic["maxParallelFileForPreDecode"];
            this.maxParallelFileSizeForDecode = Convert.ToInt32(val);
            this.IsPrepared = true;
        }

        public void Execute(Object input)
        {
            if (this.IsPrepared == false)
                throw new Exception(
                    $"Rule {this.RuleName} is not prepared, rule must have initial data in config file, then method prepare()" +
                    $" must be called during mediation, before executing rule.");
            Dictionary<string, object> dataAsDic = (Dictionary<string, object>)input;
            MediationContext mediationContext = (MediationContext)dataAsDic["mediationContext"];
            ne thisSwitch = (ne)dataAsDic["ne"];
            TelcobrightConfig tbc = mediationContext.Tbc;
            List<job> newCdrFileJobs = (List<job>)dataAsDic["newCdrFileJobs"];
            PartnerEntities context = (PartnerEntities)dataAsDic["partnerEntities"];
            //quartz probably was getting hanged when connections are running out due to parallel processing, close them frequently and re-open;
            context.Database.Connection.Close();
            context.Database.Connection.Open();
            NeAdditionalSetting neAdditionalSetting = (NeAdditionalSetting)dataAsDic["neAdditionalSetting"];
            if (neAdditionalSetting==null)
                throw new Exception("NeAdditionalSettings cannot be null while predecoding cdrs.");
            if (neAdditionalSetting.PreDecodeAsTextFile == false)
                return;
            validateIfCdrJob(newCdrFileJobs);
            cleanUpAlreadyFinishedButRemainingPredecodedFiles(thisSwitch, tbc, newCdrFileJobs, context);

            int maxParallelPreDecoding = neAdditionalSetting.MaxConcurrentFilesForParallelPreDecoding;
            int maxNumberOfFilesToPreDecode = neAdditionalSetting.MaxNumberOfFilesInPreDecodedDirectory;

            int noOfExistingPreDecodedfiles = getNoOfExistingFilesInPreDecodedDir(thisSwitch, tbc, newCdrFileJobs);
            if (noOfExistingPreDecodedfiles >= maxNumberOfFilesToPreDecode) return;

            CollectionSegmenter<job> segmentedJobs = new CollectionSegmenter<job>(newCdrFileJobs, 0);
            DbCommand cmd = context.Database.Connection.CreateCommand();
            ITelcobrightJob newCdrFileJob = null;
            mediationContext.MefJobContainer.DicExtensionsIdJobWise.TryGetValue(
                newCdrFileJobs.First().idjobdefinition.ToString(), out newCdrFileJob);
            if (newCdrFileJob == null)
                throw new Exception("JobRule not found in MEF collection.");

            segmentedJobs.ExecuteMethodInSegments(maxParallelPreDecoding, segment =>
            {
                var enumerable = segment as job[] ?? segment.ToArray();
                var successResult = new BlockingCollection<PredecoderOutput>();
                var failedResults = new BlockingCollection<PredecoderOutput>();
                List<ThreadSafePredecoder> threadSafePredecoders =
                    prepareThreadSafePreDecoders(mediationContext, thisSwitch, tbc, context, newCdrFileJob, enumerable);

                //no need for try catch, handled within preDecodeFile();

                //Parallel.ForEach(threadSafePredecoders, predecoder =>
                //{
                //    PredecoderOutput output = predecoder.preDecodeToFile();//predecodehere
                //    if (output.SuccessfulJob != null)
                //    {
                //        successResult.Add(output);
                //    }
                //    else
                //    {
                //        if (output.FailedJob == null)
                //        {
                //            var exception = new Exception(
                //                "Both successful and failed jobs cannot be null after predecoding of cdr files.");
                //            Console.WriteLine(exception);
                //            throw exception;
                //        }
                //        failedResults.Add(output);
                //    }
                //}); //parallel

                threadSafePredecoders.Take(1).ToList().ForEach(predecoder=> {
                    PredecoderOutput output = predecoder.preDecodeToFile();//predecodehere
                    if (output.SuccessfulJob != null)
                    {
                        successResult.Add(output);
                    }
                    else
                    {
                        if (output.FailedJob == null)
                        {
                            var exception = new Exception(
                                "Both successful and failed jobs cannot be null after predecoding of cdr files.");
                            Console.WriteLine(exception);
                            throw exception;
                        }
                        failedResults.Add(output);
                    }
                }); //parallel)
                    //Parallel.ForEach(threadSafePredecoders, predecoder =>



                if (successResult.Any()) updateSuccessfulJobs(thisSwitch, context, cmd, successResult);
                if(failedResults.Any()) updateFailedJobs(thisSwitch, context, cmd, failedResults, tbc);
            });
        }

        

        private void validateIfCdrJob(List<job> newCdrFileJobs)
        {
            foreach (var job in newCdrFileJobs)
            {
                if (job.idjobdefinition != 1)
                {
                    var exception = new Exception(
                        $"Job type must be 1= newCdrFileJob for cdrPredecoding. jobid:{job.id}, jobName:{job.JobName}");
                    Console.WriteLine(exception);
                    throw exception;
                }
            }
        }

        private void updateFailedJobs(ne thisSwitch, PartnerEntities context, DbCommand cmd, BlockingCollection<PredecoderOutput> failedResults,
            TelcobrightConfig tbc)
        {
            if (cmd.Connection.State != ConnectionState.Open)
                cmd.Connection.Open();
            cmd.CommandText = $" set autocommit=1;";
            cmd.ExecuteNonQuery();
            foreach (var result in failedResults)
            {
                try
                {
                    cmd.CommandText = $" update job set jobsummary='Possibly Corrupted', Error='{result.ExceptionMessage}' where id={result.FailedJob.id};";
                    cmd.ExecuteNonQuery();
                    string preDecodedDirName = "";
                    string preDecodedFileName = "";
                    getPathNamesForPreDecoding(result.FailedJob, thisSwitch, tbc, out preDecodedDirName,
                        out preDecodedFileName);
                    if (File.Exists(preDecodedFileName))
                    {
                        File.Delete(preDecodedFileName);
                    }
                }
                catch (Exception e)
                {
                    var exception = new Exception(
                        $@"Couldn't update status while predecoding {result.FailedJob.JobName} jobs for ne: {thisSwitch.SwitchName}
                               --{e.Message} ");
                    Console.WriteLine(exception);
                    ErrorWriter.WriteError(exception, "Cdr-Predecoder", null, "NE:" + thisSwitch.idSwitch, "", context);
                    continue;
                }
                
            }
            cmd.Connection.Close();
        }


        private void updateSuccessfulJobs(ne thisSwitch, PartnerEntities context, DbCommand cmd, BlockingCollection<PredecoderOutput> successfullPreDecodedJobs)
        {
            //can't write to db in parallel, reader busy error occurs
            //no worries about failed jobs, they will be retried again or decoded again in decoder if .predecoded file doesn't exist
            //no need for commit or rollback too.
            if (cmd.Connection.State != ConnectionState.Open)
                cmd.Connection.Open();
            cmd.CommandText = $" set autocommit=0;";
            cmd.ExecuteNonQuery();
            foreach (var result in successfullPreDecodedJobs)
            {
                try
                {
                    cmd.CommandText = $" update job set status=2, Error=null where id={result.SuccessfulJob.id}";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    //cmd.CommandText = $"rollback;";//don't rollback, if one is updated fine...
                    //cmd.ExecuteNonQuery();
                    var exception = new Exception(
                            $@"Couldn't update status while predecoding {result.SuccessfulJob.JobName} jobs for ne: {thisSwitch.SwitchName}
                               --{e.Message} ");
                    ErrorWriter.WriteError(exception, "Cdr-Predecoder", null, "NE:" + thisSwitch.idSwitch,"", context);
                    continue; //with next switch
                }
            }
            //for undetectable reason, job status was not being updated, verify status again and throw exception if required
            string sql = $@"select * from job where idjobdefinition=1 and idne={thisSwitch.idSwitch} and status=7
                                     and id in ({string.Join(",", successfullPreDecodedJobs.Select(j => j.SuccessfulJob.id))});";
            List<job> jobsWithStatusUpdateFailed = context.Database.SqlQuery<job>(sql).ToList();
            if (jobsWithStatusUpdateFailed.Any())
            {
                //cmd.CommandText = $"rollback;";
                //cmd.ExecuteNonQuery();
                context.Database.Connection.Close();
                var exception =
                    new Exception(
                        $"Couldn't update status while predecoding {jobsWithStatusUpdateFailed.Count} jobs for ne: {thisSwitch.SwitchName}");
                Console.WriteLine(exception);
                throw exception;
            }
            cmd.CommandText = $"commit;";
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
            if (context.Database.Connection.State != ConnectionState.Open)
                context.Database.Connection.Open();
        }

        private List<ThreadSafePredecoder> prepareThreadSafePreDecoders(MediationContext mediationContext, ne thisSwitch, TelcobrightConfig tbc, PartnerEntities context, ITelcobrightJob newCdrFileJob, job[] enumerable)
        {
            List<ThreadSafePredecoder> threadSafePredecoders = new List<ThreadSafePredecoder>();
            foreach (var job in enumerable)
            {
                ThreadSafePredecoder threadSafePredecoder = preparePreDecoderInput(mediationContext, thisSwitch,
                    tbc, context,
                    newCdrFileJob, job);
                if (!Directory.Exists(threadSafePredecoder.PredecodedDirName))
                    Directory.CreateDirectory(threadSafePredecoder.PredecodedDirName);
                threadSafePredecoders.Add(threadSafePredecoder);
            }

            return threadSafePredecoders;
        }

        private ThreadSafePredecoder preparePreDecoderInput(MediationContext mediationContext, ne thisSwitch, TelcobrightConfig tbc, 
            PartnerEntities context, ITelcobrightJob newCdrFileJob, job job)
        {
            var cdrJobInputData = new CdrJobInputData(mediationContext, context, thisSwitch, job);
            ITelcobrightJob newCdrFileJobNewInstance = newCdrFileJob.createNewNonSingletonInstance();
            PartnerEntities partnerEntitiesNewInstance =
                new PartnerEntities(DbUtil.GetEntityConnectionString(tbc.DatabaseSetting));
            partnerEntitiesNewInstance.Database.Connection.Open();

            string predecodedDirName, predecodedFileName;
            getPathNamesForPreDecoding(job, thisSwitch, tbc, out predecodedDirName,
                out predecodedFileName);
            ThreadSafePredecoder threadSafePredecoder = new ThreadSafePredecoder
            (
                cdrJobInputData,
                partnerEntitiesNewInstance,
                newCdrFileJobNewInstance,
                predecodedFileName,
                predecodedDirName
            );
            return threadSafePredecoder;
        }

        private static void cleanUpAlreadyFinishedButRemainingPredecodedFiles(ne thisSwitch, TelcobrightConfig tbc,
            List<job> newCdrFileJobs, PartnerEntities context)
        {
            //newcdr deletes predecoded files, but still predecoded files may exist due to shutdown of telcobright process
            //or, if there are more than 500 jobs in predecoded folder due to exception in past preProcessing, no new job files will be predecoded
            //so clean up erronous and existing predecoded files 
            string sql =
                $@"select * from job where idjobdefinition=1 and status in (1,7) and idne={thisSwitch.idSwitch} 
                            and jobname in ({string.Join(",", newCdrFileJobs.Select(j => $"'{j.JobName}'"))})";
            List<job> jobsToDeletePredecodeFiles = context.Database.SqlQuery<job>(sql).ToList();
            string preDecodedDirName = "";
            foreach (job alreadyFinishedJob in jobsToDeletePredecodeFiles)
            {
                string preDecodedFileName = "";
                getPathNamesForPreDecoding(alreadyFinishedJob, thisSwitch, tbc, out preDecodedDirName,
                    out preDecodedFileName);
                if (File.Exists(preDecodedFileName))
                {
                    File.Delete(preDecodedFileName);
                }
            }
            //now take orphan files in existing dir, if they don't have a corresponding job with status=2 (prepared) delete
            Dictionary<string, FileInfo> existingPredecodedfilesSet =  //key=full file name, value = only file name without .predecoded extension
                Directory.GetFiles(preDecodedDirName, "*.predecoded")
                .Select(f => new
                    {
                        Filename = Path.GetFileNameWithoutExtension(Path.GetFileName(f)),
                        FileInfo = new FileInfo(f)
                    }).ToDictionary(a => a.Filename, a => a.FileInfo);
            if (existingPredecodedfilesSet.Any()==false)
            {
                return;
            }
            ////jobstatus 2=prepared
            sql = $@"select * from job where idjobdefinition=1 and status =2 and idne={thisSwitch.idSwitch} 
                            and jobname in ({string.Join(",", existingPredecodedfilesSet.Keys.Select(fileNameWithoutExt => $"'{fileNameWithoutExt}'"))})";
            Dictionary<string, job> jobsInPreparedStatusSubset = context.Database.SqlQuery<job>(sql).ToList()
                .Select(j => new
                {
                    Filename = $"{j.JobName}",
                    Job = j
                }).ToDictionary(a => a.Filename, a => a.Job);


            List<FileInfo> finalOrphanFiles = new List<FileInfo>();
            if (jobsInPreparedStatusSubset.Any())
            {
                foreach (var kv in existingPredecodedfilesSet)
                {
                    string existingFileName = kv.Key;
                    FileInfo fInfo = kv.Value;
                    if (!jobsInPreparedStatusSubset.ContainsKey(existingFileName))
                    {
                        finalOrphanFiles.Add(fInfo);
                    }
                }
            }
            else
            {
                finalOrphanFiles = existingPredecodedfilesSet.Values.ToList();
            }
            foreach (FileInfo orphanFileInfo in finalOrphanFiles)
            {
                if (File.Exists(orphanFileInfo.FullName))
                {
                    File.Delete(orphanFileInfo.FullName);
                }
            }
        }


        private static int getNoOfExistingFilesInPreDecodedDir(ne thisSwitch, TelcobrightConfig tbc, List<job> newCdrFileJobs)
        {
            string preDecodedDirName = "";
            string preDecodedFileName = "";
            getPathNamesForPreDecoding(newCdrFileJobs.First(), thisSwitch, tbc, out preDecodedDirName, out preDecodedFileName);
            if (!Directory.Exists(preDecodedDirName)) return 0;
            int noOfExistingPreDecodedfiles = Directory.GetFiles(preDecodedDirName, "*.predecoded").Length;
            return noOfExistingPreDecodedfiles;
        }

        
        private static void getPathNamesForPreDecoding(job thisJob, ne thisSwitch, TelcobrightConfig tbc, out string predecodedDirName, out string predecodedFileName)
        {
            string fileLocationName = thisSwitch.SourceFileLocations;
            FileLocation fileLocation = tbc.DirectorySettings.FileLocations[fileLocationName];
            string newCdrFileName = fileLocation.GetOsNormalizedPath(fileLocation.StartingPath)
                                    + Path.DirectorySeparatorChar + thisJob.JobName;
            FileInfo newCdrFileInfo = new FileInfo(newCdrFileName);
            predecodedDirName = newCdrFileInfo.DirectoryName + Path.DirectorySeparatorChar +
                                       "predecoded";
            if(!Directory.Exists(predecodedDirName))
            {
                Directory.CreateDirectory(predecodedDirName);
            }
            predecodedFileName = predecodedDirName + Path.DirectorySeparatorChar + newCdrFileInfo.Name +
                                       ".predecoded";
        }
    }
}


