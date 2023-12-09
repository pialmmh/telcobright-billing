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
using TelcobrightInfra.PerformanceAndOptimization;
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
        private static readonly Random rndSuffixForDupCorrection = new Random();
        protected Func<NewCdrPreProcessor, List<string[]>, List<CdrAndInconsistentWrapper>> parallelConvertToCdr = (preProcessor, txtRows) =>
        {
            ParallelIterator<string[], CdrAndInconsistentWrapper> parallelConverter =
                new ParallelIterator<string[], CdrAndInconsistentWrapper>(txtRows);
            List<CdrAndInconsistentWrapper> cdrAndInconsistents =
                parallelConverter.getOutput(r => preProcessor.ConvertToCdr(r));
            return cdrAndInconsistents;
        };

        public object PreprocessJob(object data)
        {
            Dictionary<string, object> dataAsDic = (Dictionary<string, object>)data;
            this.Input = (CdrJobInputData)dataAsDic["cdrJobInputData"];
            this.PreDecodingStageOnly = checkIfPreDecodingStage(dataAsDic);
            NewCdrPreProcessor preProcessor = null;
            if (PreDecodingStageOnly)
            {
                preProcessor = DecodeNewCdrFile(preDecodingStage: true);
                initAndFormatTxtRowsBeforeCdrConversion(preProcessor);
                return preProcessor; 
            }
            preProcessor = DecodeNewCdrFile(preDecodingStage: false);
            initAndFormatTxtRowsBeforeCdrConversion(preProcessor);
            return preProcessor;
        }


        public virtual Object Execute(ITelcobrightJobInput jobInputData)
        {
            NewCdrPreProcessor preProcessor = null; //preprecessor.txtrows contains decoded raw cdrs in string[] format
            this.Input = (CdrJobInputData) jobInputData;
            CdrSetting cdrSetting = this.Input.CdrSetting;
            if (this.Input.IsBatchJob == false) //not batch job
            {
                this.HandledJobs = new List<job> {this.Input.Job};
                preProcessor = DecodeNewCdrFile(preDecodingStage: false);
                initAndFormatTxtRowsBeforeCdrConversion(preProcessor);
            }
            else //batch or merged job
            {
                Dictionary<long, NewCdrWrappedJobForMerge> mergedJobsDic = getMergedJobs();
                this.HandledJobs = new List<job>();
                this.HandledJobs.AddRange(
                    mergedJobsDic.Values.Select(wrappedJob => wrappedJob.Job));
                NewCdrWrappedJobForMerge head = mergedJobsDic.First().Value;
                List<NewCdrWrappedJobForMerge> tail = mergedJobsDic.Skip(1).Select(kv => kv.Value).ToList();

                validateMergedCount(head, tail);
                preProcessor = head.PreProcessor;
            } //end if batch job

            //at this moment preProcessor has either records from a single job or merged jobs
            //duplicate cdr filter part ****************

            if (preProcessor.TxtCdrRows.Count > 0)
            {
                if (CollectorInput.Ne.FilterDuplicateCdr == 1)
                {
                    Dictionary<long, CdrMergedJobError> jobsWithDupCdrsDuringMergeProcessing =
                        new Dictionary<long, CdrMergedJobError>(); //key= jobId
                    preProcessor = filterDuplicates(preProcessor, cdrSetting, jobsWithDupCdrsDuringMergeProcessing);
                    if (jobsWithDupCdrsDuringMergeProcessing.Any())
                    {
                        var exception =
                            new Exception($"Duplicate billids found after filtering duplicates.");
                        foreach (var mergedJobError in jobsWithDupCdrsDuringMergeProcessing.Values)
                        {
                            if (exception.Data.Contains(mergedJobError.Job.id.ToString()) == false)
                                exception.Data.Add(mergedJobError.Job.id.ToString(), mergedJobError);
                        }
                        Console.WriteLine(exception);
                        throw exception;
                    }
                }
                //aggregate cdr part
                if (preProcessor.TxtCdrRows.Count > 0)
                {
                    var neAdditionalSetting = CollectorInput.CdrJobInputData.NeAdditionalSetting;
                    if (neAdditionalSetting != null && !neAdditionalSetting.AggregationStyle
                            .IsNullOrEmptyOrWhiteSpace()) //move it to a mef rule later
                    {
                        if (CollectorInput.Ne.FilterDuplicateCdr != 1)
                        {
                            throw new Exception("Duplicate Filtering must be on when cdr aggregation is enabled.");
                        }
                        if (neAdditionalSetting.AggregationStyle == "telcobridge")
                        {
                            Dictionary<string, string[]> dupFilteredBillIdsForThisJob =
                                preProcessor.FinalNonDuplicateEvents;
                            Dictionary<string, object> tupleGenInput = new Dictionary<string, object>()
                            {
                                {"collectorInput", this.CollectorInput},
                                {"row", null}
                            };
                            preProcessor.RowsToConsiderForAggregation = preProcessor
                                .DecodedCdrRowsBeforeDuplicateFiltering
                                .Where(r =>
                                {
                                    tupleGenInput["row"] = r;
                                    return dupFilteredBillIdsForThisJob.ContainsKey(
                                        preProcessor.Decoder.getTupleExpression(tupleGenInput));
                                }).ToList();
                            preProcessor = aggregateCdrs(preProcessor);
                            preProcessor.TxtCdrRows = preProcessor.FinalAggregatedInstances;
                            preProcessor.FinalNonDuplicateEvents =
                                preProcessor.FinalAggregatedInstances.ToDictionary(r => r[Fn.UniqueBillId]);
                            preProcessor.ValidateAggregation(this.Input.Job);
                        }
                    }
                }
            }

            openDbConAndStartTransaction(); //open new connection and start transaction
            List<CdrAndInconsistentWrapper> cdrAndInconsistents =
                parallelConvertToCdr(preProcessor, preProcessor.TxtCdrRows);
            cdrAndInconsistents.ForEach(c => preProcessor
                .AddToBaseCollection(c)); //add convertedCdrs to base collection

            CdrCollectionResult newCollectionResult = null, oldCollectionResult = null;
            preProcessor.GetCollectionResults(out newCollectionResult, out oldCollectionResult);
            //dup cdr related
            newCollectionResult.FinalNonDuplicateEvents = preProcessor.FinalNonDuplicateEvents;
            newCollectionResult.DuplicateEvents = preProcessor.DuplicateEvents;
            //aggregation related
            newCollectionResult.RowsCouldNotBeAggreagated = preProcessor.RowsCouldNotBeAggregated;
            newCollectionResult.RowsToBeDiscardedAfterAggregation = preProcessor.RowsToBeDiscardedAfterAggregation;
            foreach (string[] row in preProcessor.DuplicateEvents)
            {
                row[Fn.Switchid] = this.Input.Ne.idSwitch.ToString();
                row[Fn.Filename] = this.CollectorInput.TelcobrightJob.JobName;
                newCollectionResult.DuplicateEvents.Add(row);
            }
            PartialCdrTesterData partialCdrTesterData =
                OrganizeTestDataForPartialCdrs(preProcessor, newCollectionResult);
            CdrJob cdrJob = (new CdrJobFactory(this.Input, this.RawCount)).CreateCdrJob(preProcessor,
                newCollectionResult, oldCollectionResult, partialCdrTesterData);

            if (cdrJob.CdrProcessor.CollectionResult.ConcurrentCdrExts.Count > 0 ||
                cdrJob.CdrProcessor.CollectionResult.RowsCouldNotBeAggreagated.Count>0) //job not empty, or has records
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

        private NewCdrPreProcessor filterDuplicates(NewCdrPreProcessor preProcessor, CdrSetting cdrSetting, Dictionary<long, CdrMergedJobError> jobsWithDupCdrsDuringMergeProcessing)
        {
            Console.WriteLine("CdrJobProcessor: Filtering duplicates...");
            preProcessor = this.filterDuplicateCdrs(preProcessor);

            Dictionary<string, List<string[]>> billIdWiseDuplicateRows =
                preProcessor.TxtCdrRows.GroupBy(r => r[Fn.UniqueBillId])
                    .Select(g => new
                    {
                        UniqueBillId = g.Key,
                        Rows = g.ToList()
                    }).Where(a => a.Rows.Count > 1)
                    .ToDictionary(a => a.UniqueBillId, a => a.Rows);

            if (billIdWiseDuplicateRows.Any())
            {
                if (cdrSetting.AutoCorrectDuplicateBillId)
                {
                    foreach (string[] row in billIdWiseDuplicateRows.Values.SelectMany(r => r))
                    {
                        row[Fn.UniqueBillId] = "d_" + row[Fn.UniqueBillId] + "_" +
                                               rndSuffixForDupCorrection.Next(); //auto correct erronous duplicate billid from switch e.g. dialogic
                    }
                }
                else
                {
                    foreach (var kv in billIdWiseDuplicateRows)
                    {
                        string uniqueBillId = kv.Key;
                        List<string[]> rows = kv.Value;
                        foreach (var r in rows)
                        {
                            var mergedJobError = new CdrMergedJobError
                            {
                                Filename = r[Fn.Filename],
                                Job = this.HandledJobs.First(j => j.JobName == r[Fn.Filename]),
                                UniqueBillid = uniqueBillId,
                                Starttime = r[Fn.StartTime],
                                Answertime = r[Fn.AnswerTime],
                                CalledNumber = r[Fn.OriginatingCalledNumber],
                                CallingNumber = r[Fn.OriginatingCallingNumber],
                                Duration = r[Fn.DurationSec]
                            };
                            if (jobsWithDupCdrsDuringMergeProcessing.ContainsKey(mergedJobError.Job.id) == false)
                            {
                                jobsWithDupCdrsDuringMergeProcessing.Add(mergedJobError.Job.id, mergedJobError);
                            }
                        }
                    }
                }

            }

            return preProcessor;
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
                    $"Head cdr count must match sum of tail jobs for merge processing. Job id:{head.Job.id}, job name:{head.Job.JobName}");
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
                decimal totalCdrDuration = cdrJob.CdrProcessor.CollectionResult
                        .OriginalRowsBeforeMerge.Sum(r => r[Fn.DurationSec].IsNullOrEmptyOrWhiteSpace() 
                        ? 0 
                        : Convert.ToDecimal(r[Fn.DurationSec]));
                decimal totalActualDurationInconsistent = cdrJob.CdrProcessor.CollectionResult
                        .CdrInconsistents.Sum(r => r.DurationSec.IsNullOrEmptyOrWhiteSpace()
                        ? 0
                        : Convert.ToDecimal(r.DurationSec));
                decimal totalActualDuration = totalCdrDuration + totalActualDurationInconsistent;
                WriteJobCompletionIfCollectionNotEmpty(cdrJob.CdrProcessor.CollectionResult.RawCount,
                    this.Input.Job, cdrJob.CdrProcessor.CdrJobContext.Context, totalActualDuration);
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
            job telcobrightJob = mergedJob.Job;
            var preProcessor = mergedJob.PreProcessor;
            if (preProcessor.TxtCdrRows.Any() == false)
            {
                WriteJobCompletionIfCollectionIsEmpty(0, telcobrightJob, context);
                //throw new Exception($"Instance in a merged new cdr job cannot contain 0 record. Job id:{telcobrightJob.id}, Jobname:{telcobrightJob.JobName}");
            }
            decimal totalCdrDuration = mergedJob.OriginalRows.Sum(r => r[Fn.DurationSec].IsNullOrEmptyOrWhiteSpace()
                        ? 0
                        : Convert.ToDecimal(r[Fn.DurationSec]));
            decimal totalActualDurationInconsistent = mergedJob.OriginalCdrinconsistents.Sum(r => r.DurationSec.IsNullOrEmptyOrWhiteSpace()
                    ? 0
                    : Convert.ToDecimal(r.DurationSec));
            decimal totalActualDuration = totalCdrDuration + totalActualDurationInconsistent;
            WriteJobCompletionIfCollectionNotEmpty(preProcessor.OriginalRowsBeforeMerge.Count, telcobrightJob, context,totalActualDuration);
            if (this.Input.CdrSetting.DisableCdrPostProcessingJobCreationForAutomation == false)
            {
                CreateNewCdrPostProcessingJobs(this.Input.Context, this.Input.MediationContext.Tbc, telcobrightJob);
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
                decoder = (AbstractCdrDecoder)decoder.createNewNonSingletonInstance();//singleton was causing io problem during predecoding file I/O
            }
            FileInfo fileInfo = new FileInfo(fileName);
            List<cdrinconsistent> cdrinconsistents = new List<cdrinconsistent>();
            if (this.PreDecodingStageOnly)//PREDECODING
            {
                FileInfo cdrFileInfo = new FileInfo(fileName);
                FileAndPathHelperMutable pathHelper = new FileAndPathHelperMutable();
                if (pathHelper.IsFileLockedOrBeingWritten(cdrFileInfo) == true)
                {
                    throw new Exception("Could not get exclusive lock on file before decoding, file transfer may be not finished yet through the network or FTP.");
                }
                List<string[]> decodedCdrRows= new List<string[]>();
                try
                {
                    decodedCdrRows = decoder.DecodeFile(this.CollectorInput, out cdrinconsistents);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("OutOfMemoryException"))
                    {
                        Console.WriteLine("WARNING!!!!!!!! MANUAL GARBAGE COLLECTION AND COMPACTION OF LOH.");
                        GarbageCollectionHelper.CompactGCNowForOnce();
                        decodedCdrRows = decoder.DecodeFile(this.CollectorInput, out cdrinconsistents);
                    }
                    else
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
                NewCdrPreProcessor newCdrPreProcessor =
                    new NewCdrPreProcessor(decodedCdrRows, cdrinconsistents, this.CollectorInput);
                newCdrPreProcessor.Decoder = decoder;
                newCdrPreProcessor.OriginalCdrFileSize = fileInfo.Length;
                return newCdrPreProcessor;
            }
            NewCdrPreProcessor preProcessor = (NewCdrPreProcessor)cdrCollector.Collect();
            preProcessor.OriginalCdrFileSize = fileInfo.Length;
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
                if (this.CollectorInput.Ne.UseIdCallAsBillId == 1)
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
                preProcessor.SetFileNameWithJobName(collectorinput.TelcobrightJob.JobName, txtRow);
                preProcessor
                    .AdjustStartTimeBasedOnCdrSettingsForSummaryTimeField(
                        collectorinput.Tbc.CdrSetting.SummaryTimeField, txtRow);
                if (cdrSetting.AutoCorrectDuplicateBillId == true)
                {
                    if (this.Input.NeAdditionalSetting!=null &&
                     !this.Input.NeAdditionalSetting.AggregationStyle.IsNullOrEmptyOrWhiteSpace())
                    {
                        throw new Exception("Autocorrect Duplicate BillId not supported when cdr aggregation is enabled.");
                    }
                }
            });
            MefValidator<string[]> inconistentValidator =
                NewCdrPreProcessor.CreateValidatorForInconsistencyCheck(collectorinput);
            if (cdrSetting.PartialCdrEnabledNeIds
                .Contains(collectorinput.Ne.idSwitch))
            {
                if (cdrSetting.AutoCorrectDuplicateBillId == true)
                {
                    preProcessor.TxtCdrRows =
                        AbstractCdrJobPreProcessor.ChangeDuplicateBillIdsForPartialCdrs(preProcessor.TxtCdrRows);
                }
            }
            else
            {
                if (this.Input.NeAdditionalSetting == null ||
                    this.Input.NeAdditionalSetting.AggregationStyle.IsNullOrEmptyOrWhiteSpace())
                {
                    preProcessor.TxtCdrRows.AsParallel().ForAll(row => row[Fn.Partialflag] = "0");
                }
            }

            if (cdrSetting.AutoCorrectBillIdsWithPrevChargeableIssue == true)
            {
                //preProcessor.TxtCdrRows = CdrJob.ChangeBillIdsWithPrevChargeableIssue(preProcessor.TxtCdrRows);
            }
            ParallelIterator<string[], cdrinconsistent> parallelIterator =
                new ParallelIterator<string[], cdrinconsistent>(preProcessor.TxtCdrRows);
            CdrInconsistentValidator validator = new CdrInconsistentValidator(collectorinput.CdrJobInputData,
                inconistentValidator);
            List<cdrinconsistent> inconsistentCdrs = parallelIterator.getOutput(validator.CheckAndConvertIfInconsistent);
            inconsistentCdrs = inconsistentCdrs.Where(c => c != null).ToList();
            foreach (var inconsistentCdr in inconsistentCdrs)
            {
                preProcessor.InconsistentCdrs.Add(inconsistentCdr);
            }
            //Parallel.ForEach(preProcessor.TxtCdrRows, txtRow =>
            //{
            //    preProcessor.CheckAndConvertIfInconsistent(collectorinput.CdrJobInputData,
            //        inconistentValidator, txtRow);
            //});
            if (preProcessor.InconsistentCdrs.Any())
            {
                List<long> inconsistentIdCalls = preProcessor.InconsistentCdrs.Select(c => Convert.ToInt64(c.IdCall)).ToList();
                preProcessor.TxtCdrRows = preProcessor.TxtCdrRows
                    .Where(c => !inconsistentIdCalls.Contains(Convert.ToInt64(c[Fn.IdCall])))
                    .ToList();
            }

        }
        private static void SetIdCallsInSameOrderAsCollected(NewCdrPreProcessor preProcessor, CdrCollectorInputData collectorinput)
        {
            //keep the cdrs in the same order as received, don't use parallel
            preProcessor.TxtCdrRows.ForEach(txtRow => preProcessor.SetIdCall(collectorinput.AutoIncrementManager, txtRow));
        }

        protected void WriteJobCompletionIfCollectionNotEmpty(int rawCount, job telcobrightJob, PartnerEntities context,
            decimal totalActualDuration)
        {
            
            using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(context))
            {
                string sql =
                    $" update job set CompletionTime={DateTime.Now.ToMySqlField()}, " +
                    $" status=1, " +
                    $"NoOfSteps={rawCount}," +
                    $"progress={rawCount},jobsummary={totalActualDuration}," +
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
                    $"progress={rawCount}, jobsummary=0," +
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
                createJobsForSplitCase(context, tbc, cdrJob, unsplitFileName);
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
            List<long> dependentJobIdsBeforeDelete = new List<long>() { cdrJob.id };
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
            job newDelJob = FileUtil.CreateFileDeleteJob(cdrJob.JobName, fileLocation, context,
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
        public NewCdrPreProcessor filterDuplicateCdrs(NewCdrPreProcessor preProcessorWithCollectedRows)
        {
            AbstractCdrDecoder decoder = preProcessorWithCollectedRows.Decoder;
            List<string[]> decodedCdrRows = preProcessorWithCollectedRows.TxtCdrRows;
            List<string[]> decodedRowsBeforeDuplicateFiltering = new List<string[]>();
            decodedCdrRows.ForEach(r=>decodedRowsBeforeDuplicateFiltering.Add(r));
            List<cdrinconsistent> cdrinconsistents = preProcessorWithCollectedRows.InconsistentCdrs.ToList();
            DbCommand cmd = this.CollectorInput.CdrJobInputData.Context.Database.Connection.CreateCommand();
            DayWiseEventCollector<string[]> dayWiseEventCollector = new DayWiseEventCollector<string[]>
            (uniqueEventsOnly: true,
                collectorInput: this.CollectorInput,
                dbCmd: cmd, decoder: decoder,
                inputEvents: decodedCdrRows,//decoded rows
                sourceTablePrefix: decoder.UniqueEventTablePrefix);
            dayWiseEventCollector.createNonExistingTables();
            dayWiseEventCollector.collectTupleWiseExistingEvents(decoder);
            DuplicaterEventFilter<string[]> duplicaterEventFilter = new DuplicaterEventFilter<string[]>(dayWiseEventCollector);
            List<string[]> excludedDuplicateCdrs = null;
            Dictionary<string, string[]> finalNonDuplicateEvents = 
                duplicaterEventFilter.filterDuplicateCdrs(out excludedDuplicateCdrs);

            preProcessorWithCollectedRows.FinalNonDuplicateEvents = finalNonDuplicateEvents;

            var textCdrCollectionPreProcessor = new NewCdrPreProcessor(txtCdrRows: finalNonDuplicateEvents.Values.ToList(), 
                inconsistentCdrs: cdrinconsistents,
                cdrCollectorInputData: this.CollectorInput)
            {
                DecodedCdrRowsBeforeDuplicateFiltering = decodedRowsBeforeDuplicateFiltering,
                FinalNonDuplicateEvents = finalNonDuplicateEvents,
                DuplicateEvents = excludedDuplicateCdrs,
                Decoder = decoder
            };
            return textCdrCollectionPreProcessor;
        }

        public NewCdrPreProcessor aggregateCdrs(NewCdrPreProcessor preprocessor)
        {
            AbstractCdrDecoder decoder = preprocessor.Decoder;
            List<string[]> rowsToConsiderForAggregation = preprocessor.RowsToConsiderForAggregation;
            //List<cdrinconsistent> cdrinconsistents = preprocessor.InconsistentCdrs.ToList();
            DbCommand cmd = this.CollectorInput.CdrJobInputData.Context.Database.Connection.CreateCommand();
            DayWiseEventCollector<string[]> dayWiseEventCollector = new DayWiseEventCollector<string[]>
            (uniqueEventsOnly: false,
                collectorInput: this.CollectorInput,
                dbCmd: cmd, decoder: decoder,
                inputEvents: rowsToConsiderForAggregation,//decoded rows
                sourceTablePrefix: decoder.PartialTablePrefix);
            dayWiseEventCollector.createNonExistingTables();
            dayWiseEventCollector.collectTupleWiseExistingEvents(decoder);
            TelcobridgeStyleAggregator<string[]> aggregator = new TelcobridgeStyleAggregator<string[]>(dayWiseEventCollector);
            
            Dictionary<string,EventAggregationResult> aggregationResults = aggregator.aggregateCdrs();
            var successfulAggregationResults = aggregationResults.Values.Where(ar => ar.AggregatedInstance != null)
                .ToList();
            var failedAggregationResults = aggregationResults.Values.Where(ar => ar.AggregatedInstance == null)
                .ToList();
            preprocessor.FinalAggregatedInstances = successfulAggregationResults.Select(ar=>ar.AggregatedInstance).ToList();
            preprocessor.RowsCouldNotBeAggregated = failedAggregationResults
                .SelectMany(ar => ar.InstancesCouldNotBeAggregated).ToList();
            preprocessor.RowsToBeDiscardedAfterAggregation = successfulAggregationResults
                .SelectMany(ar => ar.InstancesToBeDiscardedAfterAggregation).ToList();

            var inputRows = dayWiseEventCollector.InputEvents;
            if (inputRows.Count!=preprocessor.FinalAggregatedInstances.Count+preprocessor.RowsToBeDiscardedAfterAggregation.Count
                +preprocessor.RowsCouldNotBeAggregated.Count)
            {
                throw new Exception("Input and aggregated rows count did not match expected value");    
            }
            return preprocessor;
        }
    }
}
