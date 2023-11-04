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

namespace LogPreProcessor
{
    class PredecoderInput
    {
        public CdrJobInputData CdrJobInputData { get; set; }
        public PartnerEntities PartnerEntitiesNewInstance { get; set; }
        public ITelcobrightJob NewCdrFileJobNewInstance { get; set; }
        public string PredecodedFileName { get; set; }
        public string PredecodedDirName { get; set; }
    }

    class PredecoderOutput
    {
        public job SuccessfulJob { get; set; }
        public job FailedJob { get; set; }
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
            NeAdditionalSetting neAdditionalSetting = (NeAdditionalSetting)dataAsDic["neAdditionalSetting"];
            if (neAdditionalSetting?.PreDecodeAsTextFile == false)
                return;
            foreach (var job in newCdrFileJobs)
            {
                if (job.idjobdefinition != 1)
                    throw new Exception(
                        $"Job type must be 1= newCdrFileJob for cdrPredecoding. jobid:{job.id}, jobName:{job.JobName}");
            }
            
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
                BlockingCollection<job> successfullPreDecodedJobs = new BlockingCollection<job>();
                BlockingCollection<job> failedPreDecodedJobs = new BlockingCollection<job>();
                
                List<Task> decodingTasks = new List<Task>();
                foreach (var job in enumerable)
                {
                    PredecoderInput predecoderInput = preparePreDecoderInput(mediationContext, thisSwitch, tbc, context, 
                        newCdrFileJob, job, successfullPreDecodedJobs,failedPreDecodedJobs);
                    if (!Directory.Exists(predecoderInput.PredecodedDirName))
                        Directory.CreateDirectory(predecoderInput.PredecodedDirName);

                    Task task = new Task(() =>
                    {//no need for try catch, handled within preDecodeFile();
                        Console.WriteLine("Predecoding CdrJob for Switch:" + thisSwitch.SwitchName + ", JobName:" +
                                          job.JobName);
                        PredecoderOutput output = preDecodeFile(predecoderInput);
                        if (output.SuccessfulJob != null)
                        {
                            successfullPreDecodedJobs.Add(output.SuccessfulJob);
                        }
                        else
                        {
                            if (output.FailedJob == null)
                                throw new Exception("Failed job cannot be null when successful job is null too.");
                            failedPreDecodedJobs.Add(output.FailedJob);
                        }
                    });
                    decodingTasks.Add(task);
                }
                bool disableParallelMediationForDebug =
                    Convert.ToBoolean(ConfigurationManager.AppSettings["disableParallelMediationForDebug"]);
                if (disableParallelMediationForDebug)
                {
                    decodingTasks.ForEach(decodingTask => decodingTask.Start()); //single threaded
                }
                else
                {
                    Parallel.ForEach(decodingTasks, decodingTask => decodingTask.Start()); //parallel
                }
                //can't write to db in parallel, reader busy error occurs
                //no worries about failed jobs, they will be retried again or decoded again in decoder if .predecoded file doesn't exist
                //no need for commit or rollback too.
                foreach (var job in successfullPreDecodedJobs)
                {
                    try
                    {
                        cmd.CommandText = $" update job set status=2, Error=null where id={job.id}";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                foreach (job failedJob in failedPreDecodedJobs)
                {
                    string preDecodedDirName = "";
                    string preDecodedFileName = "";
                    getPathNamesForPreDecoding(failedJob, thisSwitch, tbc, out preDecodedDirName,
                        out preDecodedFileName);
                    if (File.Exists(preDecodedFileName))
                    {
                        File.Delete(preDecodedFileName);
                    }
                }
            });
        }

        private static PredecoderInput preparePreDecoderInput(MediationContext mediationContext, ne thisSwitch, TelcobrightConfig tbc, 
            PartnerEntities context, ITelcobrightJob newCdrFileJob, job job,
            BlockingCollection<job> successfullPreDecodedJobs, BlockingCollection<job> failedPreDecodedJobs)
        {
            var cdrJobInputData = new CdrJobInputData(mediationContext, context, thisSwitch, job);
            ITelcobrightJob newCdrFileJobNewInstance = newCdrFileJob.createNewNonSingletonInstance();
            PartnerEntities partnerEntitiesNewInstance =
                new PartnerEntities(DbUtil.GetEntityConnectionString(tbc.DatabaseSetting));
            partnerEntitiesNewInstance.Database.Connection.Open();

            string predecodedDirName, predecodedFileName;
            getPathNamesForPreDecoding(job, thisSwitch, tbc, out predecodedDirName,
                out predecodedFileName);
            PredecoderInput predecoderInput = new PredecoderInput
            {
                CdrJobInputData = cdrJobInputData,
                PartnerEntitiesNewInstance = partnerEntitiesNewInstance,
                NewCdrFileJobNewInstance = newCdrFileJobNewInstance,
                PredecodedFileName = predecodedFileName,
                PredecodedDirName = predecodedDirName, 
            };
            return predecoderInput;
        }

        private static void cleanUpAlreadyFinishedButRemainingPredecodedFiles(ne thisSwitch, TelcobrightConfig tbc, List<job> newCdrFileJobs, PartnerEntities context)
        {
            //newcdr deletes predecoded files, but still predecoded files may exist due to shutdown of telcobright process
            //or, if there are more than 500 jobs in predecoded folder due to exception in past preProcessing, no new job files will be predecoded
            //so clean up erronous and existing predecoded files 
            string sql = $@"select * from job where idjobdefinition=1 and status in (1,7) and idne={thisSwitch.idSwitch} 
                            and jobname in ({string.Join(",", newCdrFileJobs.Select(j => $"'{j.JobName}'"))})";
            List<job> jobsToDeletePredecodeFiles = context.Database.SqlQuery<job>(sql).ToList();
            foreach (job alreadyFinishedJob in jobsToDeletePredecodeFiles)
            {
                string preDecodedDirName = "";
                string preDecodedFileName = "";
                getPathNamesForPreDecoding(alreadyFinishedJob, thisSwitch, tbc, out preDecodedDirName, out preDecodedFileName);
                if (File.Exists(preDecodedFileName))
                {
                    File.Delete(preDecodedFileName);
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

        private static PredecoderOutput preDecodeFile(PredecoderInput input)
        {
            PredecoderOutput output= new PredecoderOutput();
            try
            {
                CdrJobInputData cdrJobInputData = input.CdrJobInputData;
                ITelcobrightJob newCdrFileJob = input.NewCdrFileJobNewInstance;
                string predecodedFileName = input.PredecodedFileName;
                var preProcessorInput = new Dictionary<string, object>
                {
                    {"cdrJobInputData", cdrJobInputData},
                    {"preDecodingStageOnly", true}
                };
                NewCdrPreProcessor newCdrPreProcessor =
                    (NewCdrPreProcessor) newCdrFileJob.PreprocessJob(preProcessorInput); //EXECUTE preDecoding
                List<string[]> txtRows = newCdrPreProcessor.TxtCdrRows;
                List<string> rowsAsCsvLinesFieldsEnclosedWithBacktick =
                    txtRows.Select(row => string.Join(",",
                            row.Select(field => new StringBuilder("`").Append(field).Append("`").ToString()).ToArray()))
                        .ToList();
                File.WriteAllLines(predecodedFileName, rowsAsCsvLinesFieldsEnclosedWithBacktick);
                output.SuccessfulJob = cdrJobInputData.Job;
                output.FailedJob = null;
                return output;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                output.FailedJob = input.CdrJobInputData.Job;
                output.SuccessfulJob = null;
                return output;
            }
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
            predecodedFileName = predecodedDirName + Path.DirectorySeparatorChar + newCdrFileInfo.Name +
                                       ".predecoded";
        }
    }
}


