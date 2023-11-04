using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.Common;
using System.IO;
using TelcobrightFileOperations;
using System.Linq;
using System.Text;
using MediationModel;
using System.Threading.Tasks;
using FlexValidation;
using LibraryExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Cdr.Collection.PreProcessors;
using TelcobrightMediation.Config;

namespace Jobs
{
    
    [Export("Job", typeof(ITelcobrightJob))]
    public class NewCdrFileJob : ITelcobrightJob
    {
        public string RuleName => "JobNewCdrFile";
        public virtual string HelpText => "New Cdr Job, processes a new CDR file";
        public override string ToString() => this.RuleName;
        public virtual int Id => 1;
        public List<job> HandledJobs;//required for deleting pre-decoded files in post processing, list when merged jobs.
        public bool PreDecodingStageOnly { get; private set; }
        protected int RawCount, NonPartialCount, UniquePartialCount, RawPartialCount, DistinctPartialCount = 0;
        protected decimal RawDurationTotalOfConsistentCdrs = 0;
        protected CdrJobInputData Input { get; set; }
        protected CdrCollectorInputData CollectorInput { get; set; }
        protected bool PartialCollectionEnabled => this.Input.MediationContext.Tbc.CdrSetting
            .PartialCdrEnabledNeIds.Contains(this.Input.Ne.idSwitch);
        protected Func<NewCdrPreProcessor, List<string[]>, List<CdrAndInconsistentWrapper>> parallelConvertToCdr = (preProcessor, txtRows) =>
        {
            //cdrinconsistent cdrInconsistent = null;
            //CdrAndInconsistentWrapper cdrAndInconsistentWrapper = preProcessor.ConvertToCdr(txtRow, out cdrInconsistent);

            ParallelIterator<string[], CdrAndInconsistentWrapper> parallelConverter=  
                                                new ParallelIterator<string[], CdrAndInconsistentWrapper>(txtRows);
            List<CdrAndInconsistentWrapper> cdrAndInconsistents =
                parallelConverter.getOutput(r => preProcessor.ConvertToCdr(r));
            return cdrAndInconsistents;
        };
        
        public object PreprocessJob(object data)
        {
            Dictionary<string, object> dataAsDic = (Dictionary<string, object>)data;
            this.Input = (CdrJobInputData)dataAsDic["cdrJobInputData"];
            this.PreDecodingStageOnly= checkIfPreDecodingStage(dataAsDic);
            NewCdrPreProcessor preProcessor = null;
            if (PreDecodingStageOnly)
            {
                preProcessor = DecodeNewCdrFile(preDecodingStage: true);
                return preProcessor;
            }
            preProcessor = DecodeNewCdrFile(preDecodingStage: false);
            return preProcessor;
        }

       
        public virtual Object Execute(ITelcobrightJobInput jobInputData)
        {
            NewCdrPreProcessor preProcessor = null;//preprecessor.txtrows contains decoded raw cdrs in string[] format
            this.Input = (CdrJobInputData)jobInputData;

            if (this.Input.IsBatchJob == false) //not batch job
            {
                this.HandledJobs = new List<job> { this.Input.Job };
                preProcessor = DecodeNewCdrFile(preDecodingStage: false);
            }
            else //batch or merged job
            {
                Dictionary<long, NewCdrWrappedJobForMerge> mergedJobsDic = getMergedJobs();
                this.HandledJobs = new List<job>();
                this.HandledJobs.AddRange(
                    mergedJobsDic.Values.Select(wrappedJob => wrappedJob.TelcobrightJob));
                NewCdrWrappedJobForMerge head = mergedJobsDic.First().Value;
                List<NewCdrWrappedJobForMerge> tail = mergedJobsDic.Skip(1).Select(kv => kv.Value).ToList();
                validateMergedCount(head, tail);
                preProcessor = head.PreProcessor;
            } //end if batch job

            //at this moment preProcessor has either records from a single job or merged jobs
            //duplicate cdr filter part ****************
            initAndFormatTxtRowsBeforeCdrConversion(preProcessor);
            if (CollectorInput.Ne.FilterDuplicateCdr == 1 && preProcessor.TxtCdrRows.Count > 0) //this.part is done using separate connection
            {//performs ddl statement through new table creation and may autocommit
                preProcessor = this.filterDuplicates(preProcessor);

                Dictionary<string, List<string[]>> billIdVsCount =
                    preProcessor.TxtCdrRows.GroupBy(r => r[Fn.UniqueBillId])
                        .Select(g => new
                        {
                            UniqueBillId = g.Key,
                            Rows = g.ToList()
                        }).ToDictionary(a => a.UniqueBillId, a=>a.Rows);

                foreach (var kv in billIdVsCount)
                {
                    string uniqueBillId = kv.Key;
                    List<string[]> rows = kv.Value;

                    List<CdrMergedJobError> mergedJobErrors = rows.Select(r => new CdrMergedJobError
                    {
                        Filename = r[Fn.Filename],
                        Job = this.HandledJobs.First(j => j.JobName == r[Fn.Filename]),
                        UniqueBillid = r[Fn.UniqueBillId],
                        Starttime = r[Fn.StartTime],
                        Answertime = r[Fn.AnswerTime],
                        CalledNumber = r[Fn.OriginatingCalledNumber],
                        CallingNumber = r[Fn.OriginatingCallingNumber],
                        Duration = r[Fn.DurationSec]
                    }).ToList();
                    //List<string> x = rows.Select(r =>
                    //{
                    //    var y= new
                    //    {
                    //        Filename = r[Fn.Filename],
                    //        UniqueBillid = r[Fn.UniqueBillId],
                    //        Starttime = r[Fn.StartTime],
                    //        Answertime = r[Fn.AnswerTime],
                    //        CalledNumber= r[Fn.OriginatingCalledNumber],
                    //        CallingNumber= r[Fn.OriginatingCallingNumber],
                    //        Duration= r[Fn.DurationSec]
                    //    };
                    //    return $"Filename, sessionid, starttime, answertime, callednumber,callingnumber,actualDuration\r\n" +
                    //           $"{y.Filename},{y.UniqueBillid},{y.Starttime},{y.Answertime},{y.CalledNumber},{y.CallingNumber},{y.Duration}\r\n";

                    //}).ToList();
                    if (rows.Count>1)
                    {
                        var exception = new Exception($"Duplicate billid: ({uniqueBillId}) found after filtering duplicates.");
                        foreach (var mergedJobError in mergedJobErrors)
                        {
                            if(exception.Data.Contains(mergedJobError.Job.id.ToString())==false)
                                exception.Data.Add(mergedJobError.Job.id.ToString(), mergedJobError);
                        }
                        throw exception;
                    }
                }
            }
            openDbConAndStartTransaction();//open new connection and start transaction

            List<CdrAndInconsistentWrapper> cdrAndInconsistents =
                parallelConvertToCdr(preProcessor, preProcessor.TxtCdrRows);
            cdrAndInconsistents.ForEach(c => preProcessor.AddToBaseCollection(c));//add convertedCdrs to base collection

            CdrCollectionResult newCollectionResult, oldCollectionResult = null;
            preProcessor.GetCollectionResults(out newCollectionResult, out oldCollectionResult);
            newCollectionResult.FinalNonDuplicateEvents = preProcessor.FinalNonDuplicateEvents;
            //newCollectionResult.DuplicateEvents = preProcessor.DuplicateEvents;
            foreach (string[] row in preProcessor.DuplicateEvents)
            {
                row[Fn.Switchid] = this.Input.Ne.idSwitch.ToString();
                row[Fn.Filename] = this.CollectorInput.TelcobrightJob.JobName;
                newCollectionResult.DuplicateEvents.Add(row);
            }

            PartialCdrTesterData partialCdrTesterData = OrganizeTestDataForPartialCdrs(preProcessor, newCollectionResult);
            CdrJob cdrJob = (new CdrJobFactory(this.Input, this.RawCount)).
                CreateCdrJob(preProcessor, newCollectionResult, oldCollectionResult, partialCdrTesterData);
            if (cdrJob.CdrProcessor.CollectionResult.ConcurrentCdrExts.Count > 0) //job not empty, or has records
            {
                cdrJob.Execute(); //MAIN EXECUTION/MEDIATION METHOD
            }
            else
            {
                handleAndFinalizeEmptyJob(cdrJob);
            }
            if (this.Input.IsBatchJob == false) //single job, not merged
            {
                FinalizeNonMergedJob(cdrJob);
            }
            else //
            {
                FinalizeMergedJobs(cdrJob);
            }
            return this.HandledJobs;
        }

        private void openDbConAndStartTransaction()
        {
            DbCommand cmd = this.CollectorInput.Context.Database.Connection.CreateCommand();
            if (cmd.Connection.State == ConnectionState.Open)
            {
                throw new Exception("Connection should only be open after preprocessing new cdr job.");
            }
            cmd.Connection.Open();
            this.Input.MediationContext.CreateTemporaryTables();
            cmd.ExecuteCommandText("set autocommit=0;");
        }

        private Dictionary<long, NewCdrWrappedJobForMerge> getMergedJobs()
        {
            Dictionary<long, NewCdrWrappedJobForMerge> mergedJobsDic = this.Input.MergedJobsDic;
            if (mergedJobsDic.Any() == false) //merged info must be present in cdr job input data
            {
                throw new Exception(
                    "New cdr vs raw collection cannot be empty for merged cdr job. There must be at least one job.");
            }
            return mergedJobsDic;
        }

        private static void validateMergedCount(NewCdrWrappedJobForMerge head, List<NewCdrWrappedJobForMerge> tail)
        {
            int mergedCount = head.PreProcessor.TxtCdrRows.Count + head.PreProcessor.InconsistentCdrs.Count;
            int headTailOriginalCount = head.OriginalRows.Count + head.OriginalCdrinconsistents.Count +
                                        tail.Sum(t => t.OriginalRows.Count + t.OriginalCdrinconsistents.Count);

            int headTailOriginalPreprocessorCount =
                head.PreProcessor.OriginalRowsBeforeMerge.Count + head.PreProcessor.OriginalCdrinconsistents.Count +
                tail.Sum(t => t.PreProcessor.OriginalRowsBeforeMerge.Count +
                              t.PreProcessor.OriginalCdrinconsistents.Count);

            if (mergedCount != headTailOriginalCount || mergedCount != headTailOriginalPreprocessorCount)
            {
                throw new Exception(
                    $"Head cdr count must match sum of tail jobs for merge processing. Job id:{head.TelcobrightJob.id}, job name:{head.TelcobrightJob.JobName}");
            }
        }

        private bool checkIfPreDecodingStage(Dictionary<string, object> dataAsDic)
        {
            object preDecodingStageOnly = null;
            if (dataAsDic.TryGetValue("preDecodingStageOnly", out preDecodingStageOnly) == false)
            {
                return false;
            }
            return (bool)preDecodingStageOnly;
        }


        private void handleAndFinalizeEmptyJob(CdrJob cdrJob)
        {
            if (cdrJob.CdrProcessor.CollectionResult.CdrInconsistents.Count > 0)
            {
                if (this.Input.Job.idjobdefinition == 1 &&
                    cdrJob.CdrProcessor.CollectionResult.CdrInconsistents.Count > 0)//newcdr
                {
                    cdrJob.CdrProcessor.WriteCdrInconsistent();
                }
            }
            else
            {
                if (!cdrJob.CdrProcessor.CdrJobContext.MediationContext.Tbc.CdrSetting.EmptyFileAllowed)
                {
                    throw new Exception("Empty new cdr files are not considered valid as per cdr setting.");
                }
            }
            WriteJobCompletionIfCollectionIsEmpty(0, this.Input.Job, cdrJob.CdrProcessor.CdrJobContext.Context);
            if (this.Input.CdrSetting.DisableCdrPostProcessingJobCreationForAutomation == false)
            {
                CreateNewCdrPostProcessingJobs(this.Input.Context, this.Input.MediationContext.Tbc, cdrJob.CdrProcessor.CdrJobContext.TelcobrightJob);
                DeletePreDecodedFile(this.Input.Context, this.Input.MediationContext.Tbc,
                    cdrJob.CdrProcessor.CdrJobContext.TelcobrightJob);
            }
        }

        private void FinalizeNonMergedJob(CdrJob cdrJob)
        {
            var collectionResult = cdrJob.CdrProcessor.CollectionResult;
            if (collectionResult.OriginalRowsBeforeMerge.Count > 0) //job not empty, or has records
            {
                //this.CollectorInput.CdrJobInputData.MergedJobsDic
                WriteJobCompletionIfCollectionNotEmpty(cdrJob.CdrProcessor.CollectionResult.RawCount,
                    this.Input.Job, cdrJob.CdrProcessor.CdrJobContext.Context);
            }
            else
            {
                if (cdrJob.CdrProcessor.CollectionResult.CdrInconsistents.Count <= 0)
                {
                    if (!cdrJob.CdrProcessor.CdrJobContext.MediationContext.Tbc.CdrSetting.EmptyFileAllowed)
                    {
                        throw new Exception("Empty new cdr files are not considered valid as per cdr setting.");
                    }
                }
                WriteJobCompletionIfCollectionIsEmpty(
                    cdrJob.CdrProcessor.CollectionResult.OriginalRowsBeforeMerge.Count,
                    this.Input.Job, cdrJob.CdrProcessor.CdrJobContext.Context);
            }
            if (this.Input.CdrSetting.DisableCdrPostProcessingJobCreationForAutomation == false)
            {
                CreateNewCdrPostProcessingJobs(this.Input.Context, this.Input.MediationContext.Tbc,
                    cdrJob.CdrProcessor.CdrJobContext.TelcobrightJob);
                DeletePreDecodedFile(this.Input.Context, this.Input.MediationContext.Tbc,
                    cdrJob.CdrProcessor.CdrJobContext.TelcobrightJob);

            }
        }

        private void FinalizeMergedJobs(CdrJob cdrJob)
        {
            Dictionary<long, NewCdrWrappedJobForMerge> mergedJobsDic =
                cdrJob.CdrProcessor.CdrJobContext.CdrjobInputData.MergedJobsDic;
            PartnerEntities context = cdrJob.CdrJobContext.Context;
            foreach (var newCdrWrappedJobForMerge in mergedJobsDic.Values)
            {
                FinalizeSingleMergedJob(newCdrWrappedJobForMerge, context);
            }
        }

        private void FinalizeSingleMergedJob(NewCdrWrappedJobForMerge mergedJob, PartnerEntities context)
        {
            job telcobrightJob = mergedJob.TelcobrightJob;
            var preProcessor = mergedJob.PreProcessor;
            if (preProcessor.TxtCdrRows.Any() == false)
            {
                WriteJobCompletionIfCollectionIsEmpty(0, telcobrightJob, context);
                //throw new Exception($"Instance in a merged new cdr job cannot contain 0 record. Job id:{telcobrightJob.id}, Jobname:{telcobrightJob.JobName}");
            }
            WriteJobCompletionIfCollectionNotEmpty(preProcessor.OriginalRowsBeforeMerge.Count, telcobrightJob, context);
            if (this.Input.CdrSetting.DisableCdrPostProcessingJobCreationForAutomation == false)
            {
                CreateNewCdrPostProcessingJobs(this.Input.Context, this.Input.MediationContext.Tbc,telcobrightJob);
            }
        }

        public object PostprocessJob(object data)
        {
            List<job> handledJobs = (List<job>)data;
            foreach (var job in handledJobs)
            {
                DeletePreDecodedFile(this.Input.Context, this.Input.MediationContext.Tbc, job);
            }
            return handledJobs;
        }

        public ITelcobrightJob createNewNonSingletonInstance()
        {
            Type t = this.GetType();
            return (ITelcobrightJob)Activator.CreateInstance(t);
        }

        private NewCdrPreProcessor DecodeNewCdrFile(bool preDecodingStage)
        {
            string fileName = getFullPathOfCdrFile();
            

            this.CollectorInput = new CdrCollectorInputData(this.Input, fileName);
            var cdrCollector = new FileBasedTextCdrCollector(this.CollectorInput);
            AbstractCdrDecoder decoder = cdrCollector.getDecoder();
            if (preDecodingStage)
            {
                decoder = (AbstractCdrDecoder) decoder.createNewNonSingletonInstance();//singleton was causing io problem during predecoding file I/O
            }
            List<cdrinconsistent> cdrinconsistents = new List<cdrinconsistent>();
            if (this.PreDecodingStageOnly)//PREDECODING
            {
                FileInfo cdrFileInfo = new FileInfo(fileName);
                FileAndPathHelperMutable pathHelper = new FileAndPathHelperMutable();
                if (pathHelper.IsFileLockedOrBeingWritten(cdrFileInfo) == true)
                {
                    throw new Exception("Could not get exclusive lock on file before decoding, file transfer may be not finished yet through the network or FTP.");
                }
                var decodedCdrRows = decoder.DecodeFile(this.CollectorInput, out cdrinconsistents);
                NewCdrPreProcessor newCdrPreProcessor =
                    new NewCdrPreProcessor(decodedCdrRows, cdrinconsistents, this.CollectorInput);
                newCdrPreProcessor.Decoder = decoder;
                return newCdrPreProcessor;
            }
            NewCdrPreProcessor preProcessor = (NewCdrPreProcessor)cdrCollector.Collect();
            return preProcessor;
        }

        private string getFullPathOfCdrFile()
        {
            string fileLocationName = this.Input.Ne.SourceFileLocations;
            FileLocation fileLocation =
                this.Input.MediationContext.Tbc.DirectorySettings.FileLocations[fileLocationName];
            string fileName = fileLocation.GetOsNormalizedPath(fileLocation.StartingPath)
                              + Path.DirectorySeparatorChar + this.Input.Job.JobName;
            return fileName;
        }

        
        
        public PartialCdrTesterData OrganizeTestDataForPartialCdrs(NewCdrPreProcessor preProcessor,
            CdrCollectionResult newCollectionResult)
        {
            this.RawCount = preProcessor.RawCount;
            newCollectionResult.RawDurationTotalOfConsistentCdrs =
                preProcessor.NonPartialCdrs.Sum(c => c.DurationSec) + preProcessor.PartialCdrContainers
                    .SelectMany(pc => pc.NewRawInstances).Sum(r => r.DurationSec);
            this.RawDurationTotalOfConsistentCdrs = newCollectionResult.RawDurationTotalOfConsistentCdrs;
            PartialCdrTesterData partialCdrTesterData = null;
            if (this.PartialCollectionEnabled)
            {
                this.NonPartialCount = preProcessor.TxtCdrRows.Count(r => r[Fn.Partialflag] == "0");
                List<string[]> partialRows = preProcessor.TxtCdrRows.Where(r =>
                    this.Input.CdrSetting.PartialCdrFlagIndicators.Contains(r[Fn.Partialflag])).ToList();
                this.RawPartialCount = partialRows.Count;
                if (preProcessor.TxtCdrRows.Count != this.NonPartialCount + this.RawPartialCount)
                    throw new Exception("TxtCdr rows with partial & non-partial flag do not match total decoded text rows");
                this.DistinctPartialCount = partialRows.GroupBy(r => r[Fn.UniqueBillId]).Count();
                partialCdrTesterData = new PartialCdrTesterData(this.NonPartialCount, this.RawCount,
                    newCollectionResult.RawDurationTotalOfConsistentCdrs, this.RawPartialCount);
            }
            return partialCdrTesterData;
        }
        

        protected void initAndFormatTxtRowsBeforeCdrConversion(NewCdrPreProcessor preProcessor)
        {
            var collectorinput = this.CollectorInput;
            var cdrSetting = collectorinput.CdrJobInputData.MediationContext.Tbc.CdrSetting;
            SetIdCallsInSameOrderAsCollected(preProcessor, collectorinput);
            
            Parallel.ForEach(preProcessor.TxtCdrRows, txtRow =>
            {
                if (this.CollectorInput.Ne.UseIdCallAsBillId==1)
                {
                    txtRow[Fn.UniqueBillId] = txtRow[Fn.IdCall];
                }
                if (cdrSetting.SummaryTimeField == SummaryTimeFieldEnum.AnswerTime)
                {
                    txtRow[Fn.SignalingStartTime] = txtRow[Fn.StartTime];
                    txtRow[Fn.StartTime] = txtRow[Fn.AnswerTime];
                }
                preProcessor.SetAllBlankFieldsToZerolengthString(txtRow);
                preProcessor.RemoveIllegalCharacters(collectorinput.Tbc.CdrSetting
                    .IllegalStrToRemoveFromFields, txtRow);
                preProcessor.SetSwitchid(txtRow);
                preProcessor.SetJobNameWithFileName(collectorinput.TelcobrightJob.JobName, txtRow);
                preProcessor
                    .AdjustStartTimeBasedOnCdrSettingsForSummaryTimeField(
                        collectorinput.Tbc.CdrSetting.SummaryTimeField, txtRow);

            });
            MefValidator<string[]> inconistentValidator =
                NewCdrPreProcessor.CreateValidatorForInconsistencyCheck(collectorinput);
            if (cdrSetting.PartialCdrEnabledNeIds
                .Contains(collectorinput.Ne.idSwitch))
            {
                if (cdrSetting.AutoCorrectDuplicateBillId == true)
                {
                    preProcessor.TxtCdrRows =
                        AbstractCdrJobPreProcessor.ChangeDuplicateBillIds(preProcessor.TxtCdrRows);
                }
            }
            else
            {
                //preProcessor.TxtCdrRows =
                  //  preProcessor.FilterCdrsWithDuplicateBillIdsAsInconsistent(preProcessor.TxtCdrRows);
                preProcessor.TxtCdrRows.AsParallel().ForAll(row=>row[Fn.Partialflag]="0");
            }

            if (cdrSetting.AutoCorrectBillIdsWithPrevChargeableIssue == true)
            {
                preProcessor.TxtCdrRows = CdrJob.ChangeBillIdsWithPrevChargeableIssue(preProcessor.TxtCdrRows);
            }
            Parallel.ForEach(preProcessor.TxtCdrRows, txtRow =>
            {
                preProcessor.CheckAndConvertIfInconsistent(collectorinput.CdrJobInputData,
                inconistentValidator, txtRow);
            });
            if (preProcessor.InconsistentCdrs.Any())
            {
                List<long> inconsistentIdCalls = preProcessor.InconsistentCdrs.Select(c => Convert.ToInt64(c.IdCall)).ToList();
                preProcessor.TxtCdrRows = preProcessor.TxtCdrRows
                    .Where(c => !inconsistentIdCalls.Contains(Convert.ToInt64(c[Fn.IdCall])))
                    .ToList();
            }

        }

        protected void PreformatRawCdrsForExceptionalCircumstances(NewCdrPreProcessor preProcessor)
        {
            Parallel.ForEach(preProcessor.TxtCdrRows, txtRow =>
            {
                preProcessor.SetAllBlankFieldsToZerolengthString(txtRow);
                preProcessor.RemoveIllegalCharacters(this.CollectorInput.Tbc.CdrSetting
                    .IllegalStrToRemoveFromFields, txtRow);
                preProcessor.SetSwitchid(txtRow);
                preProcessor.SetJobNameWithFileName(this.CollectorInput.TelcobrightJob.JobName, txtRow);
                preProcessor
                    .AdjustStartTimeBasedOnCdrSettingsForSummaryTimeField(
                        this.CollectorInput.Tbc.CdrSetting.SummaryTimeField, txtRow);
            });
        }


        private static void SetIdCallsInSameOrderAsCollected(NewCdrPreProcessor preProcessor, CdrCollectorInputData collectorinput)
        {
            //keep the cdrs in the same order as received, don't use parallel
            preProcessor.TxtCdrRows.ForEach(txtRow => preProcessor.SetIdCall(collectorinput.AutoIncrementManager, txtRow));
        }

        protected void WriteJobCompletionIfCollectionNotEmpty(int rawCount, job telcobrightJob, PartnerEntities context)
        {
            using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(context))
            {
                string sql =
                    $" update job set CompletionTime={DateTime.Now.ToMySqlField()}, " +
                    $" status=1, " +
                    $"NoOfSteps={rawCount}," +
                    $"progress={rawCount}," +
                    $"Error=null where id={telcobrightJob.id}";
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        protected void WriteJobCompletionIfCollectionIsEmpty(int rawCount, job telcobrightJob, PartnerEntities context)
        {
            using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(context))
            {
                string sql =
                    $" update job set CompletionTime={DateTime.Now.ToMySqlField()}, " +
                    $" status=1, " +
                    $"NoOfSteps={rawCount}," +//could be non zero if inconsistents exist
                    $"progress={rawCount}," +
                    $"Error=null where id={telcobrightJob.id}";
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        protected void CreateNewCdrPostProcessingJobs(PartnerEntities context, TelcobrightConfig tbc, job cdrJob)
        {
            string jobParam = cdrJob.JobParameter;
            string unsplitFileName = jobParam.Split('=')[1];
            if (unsplitFileName.IsNullOrEmptyOrWhiteSpace())
            {
                createJobsForUnsplitCase(context, tbc, cdrJob);
            }
            else
            {
                createJobsForSplitCase(context, tbc, cdrJob,unsplitFileName);
            }
        }
        protected void DeletePreDecodedFile(PartnerEntities context, TelcobrightConfig tbc, job cdrJob)
        {
            string fileLocationName = this.CollectorInput.Ne.SourceFileLocations;
            FileLocation fileLocation = this.CollectorInput.Tbc.DirectorySettings.FileLocations[fileLocationName];
            string newCdrFileName = fileLocation.GetOsNormalizedPath(fileLocation.StartingPath)
                                    + Path.DirectorySeparatorChar + cdrJob.JobName;
            FileInfo newCdrFileInfo = new FileInfo(newCdrFileName);
            string predecodedDirName = newCdrFileInfo.DirectoryName + Path.DirectorySeparatorChar + "predecoded";
            string preDecodedFileName = predecodedDirName + Path.DirectorySeparatorChar + newCdrFileInfo.Name + ".predecoded";
            if (File.Exists(preDecodedFileName))
                File.Delete(preDecodedFileName);
        }

        private static void createJobsForSplitCase(PartnerEntities context, TelcobrightConfig tbc, job cdrJob,
            string unsplitFileName)
        {
            List<long> dependentJobIdsBeforeDelete = new List<long>();
            if (tbc.CdrSetting.BackupSyncPairNames != null)
            {
                foreach (string syncPairname in tbc.CdrSetting.BackupSyncPairNames)
                {
                    job fileCopyJob = FileUtil.CreateFileCopyJob(tbc, syncPairname, unsplitFileName, context);
                    string doubleSlashNormalizedFileName = fileCopyJob.JobName.Replace(@"\\", @"\");
                    bool fileCopyJobExists =
                        context.jobs.Any(j => j.JobName == doubleSlashNormalizedFileName && j.idjobdefinition == 6);
                    if (fileCopyJobExists == false)
                    {
                        long fileCopyJobsId = FileUtil.WriteFileCopyJobSingle(fileCopyJob, context.Database.Connection);
                        dependentJobIdsBeforeDelete.Add(fileCopyJobsId);
                        //create delete job for unsplitFile
                        string vaultName = tbc.DirectorySettings.SyncPairs[cdrJob.ne.SourceFileLocations].Name;
                        FileLocation srcFileLocation = tbc.DirectorySettings.SyncPairs[vaultName]
                            .DstSyncLocation.FileLocation;
                        job newDelJob = FileUtil.CreateFileDeleteJob(unsplitFileName,
                            //tbc.DirectorySettings.FileLocations[vaultName],
                            srcFileLocation,
                            context,
                            new JobPreRequisite()
                            {
                                ExecuteAfterJobs = dependentJobIdsBeforeDelete,
                            }
                        );
                        newDelJob.JobName = newDelJob.JobName.Replace(@"\\", @"\");
                        InsertOrUpdateDeleteJob(context, fileCopyJobsId, newDelJob);

                    }
                }
            }
        }

        private static void createJobsForUnsplitCase(PartnerEntities context, TelcobrightConfig tbc, job cdrJob)
        {
            string fileToCopy = cdrJob.JobName;
            ne thisNe = cdrJob.ne;
            List<string> cdrBackupSyncPairNames = new List<string>();
            IEnumerable<string> syncPairnames = thisNe.BackupFileLocations?.Split(',').Select(s => s.Trim());
            if (syncPairnames != null)
                foreach (string syncPairname in syncPairnames)
                {
                    cdrBackupSyncPairNames.Add(syncPairname);
                }
            List<long> dependentJobIdsBeforeDelete = new List<long>() {cdrJob.id};
            if (tbc.CdrSetting.BackupSyncPairNames != null)
            {
                foreach (string syncPairname in tbc.CdrSetting.BackupSyncPairNames)
                {
                    if (cdrBackupSyncPairNames.Contains(syncPairname) == false) continue;
                    job fileCopyJob = FileUtil.CreateFileCopyJob(tbc, syncPairname, fileToCopy, context);
                    if (fileCopyJob != null)
                    {
                        bool jobExists =
                            context.jobs.Any(j => j.JobName == fileCopyJob.JobName && j.idjobdefinition == 6);
                        if (jobExists == false)
                        {
                            long insertedJobsId = FileUtil.WriteFileCopyJobSingle(fileCopyJob, context.Database.Connection);
                            dependentJobIdsBeforeDelete.Add(insertedJobsId);
                        }
                    }
                    //else job exists already, no need to create again.
                }
            }
            //create delete job
            string vaultName = cdrJob.ne.SourceFileLocations;
            FileLocation fileLocation = tbc.DirectorySettings.FileLocations[vaultName];
            job newDelJob= FileUtil.CreateFileDeleteJob(cdrJob.JobName, fileLocation, context,
                new JobPreRequisite()
                {
                    ExecuteAfterJobs = dependentJobIdsBeforeDelete,
                }
            );
            newDelJob.JobName = newDelJob.JobName.Replace(@"\\", @"\");
            InsertOrUpdateDeleteJob(context, -1, newDelJob);
        }
        private static void InsertOrUpdateDeleteJob(PartnerEntities context, long fileCopyJobsId, job newDelJob)
        {
            job existingDelJob =
                context.jobs.FirstOrDefault(c => c.idjobdefinition == newDelJob.idjobdefinition &&
                                                 c.JobName == newDelJob.JobName);
            if (existingDelJob != null)
            {
                //exists, update job prerequisite
                existingDelJob.JobParameter = existingDelJob.JobParameter.Replace(@"\", @"`");
                JobParamFileDelete existingDeljobParam = JsonConvert.DeserializeObject<JobParamFileDelete>(existingDelJob.JobParameter);
                existingDeljobParam.FileName = existingDeljobParam.FileName.Replace("`", @"\");
                existingDeljobParam.FileLocation.PathSeparator =
                    existingDeljobParam.FileLocation.PathSeparator.Replace("`", @"\");
                existingDeljobParam.JobPrerequisite.ExecuteAfterJobs.Add(fileCopyJobsId);
                existingDelJob.JobParameter = JsonConvert.SerializeObject(existingDeljobParam);
                DbCommand cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = $@"update job set JobParameter='{existingDelJob.JobParameter}'
                                                 where id={existingDelJob.id};";
                cmd.ExecuteNonQuery();
            }
            else
            {
                DbCommand cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = newDelJob.GetExtInsertCustom(
                    j =>
                    {
                        string jobName = newDelJob.JobName.Replace(@"\", @"\\");
                        return new StringBuilder(
                                $@"insert into job(idjobdefinition,priority,idne,jobname,status,jobparameter,creationtime) 
                                         values ({newDelJob.idjobdefinition},{newDelJob.priority},{0},'{jobName}',
                                         {newDelJob.Status},'{newDelJob.JobParameter}',{
                                        newDelJob.CreationTime.ToMySqlField()
                                    });")
                            .ToString();
                    }).ToString();
                cmd.ExecuteNonQuery();
            }
        }
        public NewCdrPreProcessor filterDuplicates(NewCdrPreProcessor preProcessorWithCollectedRows)
        {
            AbstractCdrDecoder decoder = preProcessorWithCollectedRows.Decoder;
            List<string[]> decodedCdrRows = preProcessorWithCollectedRows.TxtCdrRows;
            List<cdrinconsistent> cdrinconsistents = preProcessorWithCollectedRows.InconsistentCdrs.ToList();
            DbCommand cmd = this.CollectorInput.CdrJobInputData.Context.Database.Connection.CreateCommand();
            DayWiseEventCollector<string[]> dayWiseEventCollector = new DayWiseEventCollector<string[]>
                                                                        (uniqueEventsOnly: true,
                                                                            collectorInput: this.CollectorInput,
                                                                            dbCmd: cmd, decoder: decoder,
                                                                            decodedEvents: decodedCdrRows,//decoded rows
                                                                            sourceTablePrefix: decoder.PartialTablePrefix);
            dayWiseEventCollector.createNonExistingTables();
            dayWiseEventCollector.collectTupleWiseExistingEvents(decoder);
            DuplicaterEventFilter<string[]> duplicaterEventFilter = new DuplicaterEventFilter<string[]>(dayWiseEventCollector);
            List<string[]> excludedDuplicateCdrs = null;
            Dictionary<string, string[]> finalNonDuplicateEvents = duplicaterEventFilter.filterDuplicateCdrs(out excludedDuplicateCdrs);

            preProcessorWithCollectedRows.FinalNonDuplicateEvents = finalNonDuplicateEvents;

            var textCdrCollectionPreProcessor = new NewCdrPreProcessor(finalNonDuplicateEvents.Values.ToList(), cdrinconsistents,
                this.CollectorInput)
            {
                FinalNonDuplicateEvents = finalNonDuplicateEvents,
                DuplicateEvents = excludedDuplicateCdrs,
                Decoder = decoder
            };
            return textCdrCollectionPreProcessor;
        }

    }
}
