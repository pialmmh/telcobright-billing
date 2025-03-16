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
using TelcobrightInfra;
using TelcobrightInfra.PerformanceAndOptimization;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Cdr.Collection.PreProcessors;
using TelcobrightMediation.Config;
using MemCache;
using System.Security.Cryptography;
using System.Globalization;

namespace Jobs
{

    [Export("Job", typeof(ITelcobrightJob))]
    public class NewCdrFileJob : ITelcobrightJob
    {
        public string RuleName => "JobNewCdrFile";
        public virtual string HelpText => "New Cdr Job, processes a new CDR file";
        public override string ToString() => this.RuleName;
        public virtual int Id => 1;
        private List<FileInfo> FilesToDeleteAfterJobCompletion = new List<FileInfo>();
        private CdrJobOutput HandledJobsOutput = new CdrJobOutput();//required for deleting pre-decoded files in post processing, list when merged jobs.
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
            List<string[]> newSriRows = new List<string[]>();
            NewCdrPreProcessor preProcessor = null; //preprecessor.txtrows contains decoded raw cdrs in string[] format
            this.Input = (CdrJobInputData)jobInputData;
            CdrSetting cdrSetting = this.Input.CdrSetting;
            if (this.Input.IsBatchJob == false) //not batch job
            {
                if (this.HandledJobsOutput.Jobs == null)
                {
                    this.HandledJobsOutput.Jobs = new List<job>();
                }
                this.HandledJobsOutput.Jobs.Add(this.Input.Job);
                preProcessor = DecodeNewCdrFile(preDecodingStage: false);
                initAndFormatTxtRowsBeforeCdrConversion(preProcessor);
            }
            else //batch or merged job
            {
                Dictionary<long, NewCdrWrappedJobForMerge> mergedJobsDic = getMergedJobs();
                this.HandledJobsOutput.Jobs = new List<job>();
                this.HandledJobsOutput.Jobs.AddRange(
                    mergedJobsDic.Values.Select(wrappedJob => wrappedJob.Job));
                NewCdrWrappedJobForMerge head = mergedJobsDic.First().Value;
                List<NewCdrWrappedJobForMerge> tail = mergedJobsDic.Skip(1).Select(kv => kv.Value).ToList();

                validateMergedCount(head, tail);
                preProcessor = head.PreProcessor;
            } //end if batch job

            //at this moment preProcessor has either records from a single job or merged jobs
            //duplicate cdr filter part ****************
            var neAdditionalSetting = CollectorInput.CdrJobInputData.NeAdditionalSetting;

            List<NewAndOldEventsWrapper<string[]>> allPreAggWrappers = new List<NewAndOldEventsWrapper<string[]>>();
            List<EventAggregationResult> mtAggregationResults = new List<EventAggregationResult>();
            List<EventAggregationResult> sriAggregationResults = new List<EventAggregationResult>();
            if (neAdditionalSetting.PerformPreaggregation)
            {
                if (cdrSetting.DescendingOrderWhileListingFiles)
                {
                    throw new Exception("DescendingOrder not supported while perform predecoding");
                }
                Console.WriteLine("Performing InMemory Aggregation ....");

                foreach (var row in preProcessor.TxtCdrRows) //change needed (this section only for sri )
                {
                    if (!(row[Sn.SmsType] == "1" || row[Sn.SmsType] == "2")) continue;
                    string packetFrameTime = row[Sn.PacketFrameTime];
                    string redirectingNumber = row[Sn.Imsi];
                    string codec = row[Sn.Codec];
                    long newIdCall = GenerateIdCall(packetFrameTime, codec, redirectingNumber);
                    row[Sn.IdCall] = newIdCall.ToString();
                }
                SriHelper sriHelper = new SriHelper(this.Input.Tbc, preProcessor, preProcessor.TxtCdrRows);
                var oldSriRows = sriHelper.FetchRows();
                foreach (var row in oldSriRows)
                {
                    if (row[Sn.Codec].IsNullOrEmptyOrWhiteSpace())
                    {
                        ;
                    }
                }

                var oldSriRowsByIdCall = oldSriRows.ToDictionary(row => row[Sn.IdCall]);
                newSriRows = preProcessor.TxtCdrRows
                .Where(row => row[Sn.SmsType] == "1" || row[Sn.SmsType] == "2")
                .ToList();
                preProcessor.NewSriRows = newSriRows;
                preProcessor.TxtCdrRows.AddRange(oldSriRows);
                AbstractCdrDecoder decoder = preProcessor.Decoder;

                List<string[]> l1FailedRows;
                List<NewAndOldEventsWrapper<string[]>> l1AggResults =
                    decoder.PreAggregateL1(preProcessor, out l1FailedRows);

                var failedSriRows = l1FailedRows.Where(asrii => asrii[Sn.SmsType] == "1" || asrii[Sn.SmsType] == "2")
                                                .ToList();
                failedSriRows = consistantSriRows(failedSriRows);
                List<NewAndOldEventsWrapper<string[]>> sriAggCandidates = new List<NewAndOldEventsWrapper<string[]>>();
                List<string> sriAggById = new List<string>();
                l1AggResults.ForEach(nao =>
                {
                    if (nao.NewUnAggInstances.Any(s => s[Sn.SmsType] == "1" || s[Sn.SmsType] == "2"))
                    {
                        sriAggById.Add(nao.UniqueBillId);
                        sriAggCandidates.Add(new NewAndOldEventsWrapper<string[]>()
                        {
                            UniqueBillId = nao.UniqueBillId,
                            NewUnAggInstances = nao.NewUnAggInstances,
                            OldUnAggInstances = nao.OldUnAggInstances

                        });
                    }
                }
                );

                //l1AggResults.RemoveAll(r => sriAggById.Contains(r.UniqueBillId));
                //l1AggResults.RemoveAll(r => r.NewUnAggInstances.Any(s => s[Sn.SmsType] == "1" || s[Sn.SmsType] == "2"));
                newSriRows.ForEach(row =>
                {
                    if (row[Sn.SmsType] == "1" && row[Sn.TerminatingCalledNumber].IsNullOrEmptyOrWhiteSpace())
                    {
                        ;
                    }
                });
                sriAggCandidates.ForEach(sac => sriAggregationResults.Add(decoder.Aggregate(sac)));
                var successfulSriAggregationResults = sriAggregationResults.Where(ar => ar.AggregatedInstance != null)
                    .ToList();
                sriAggregationResults.ForEach(ar =>
                {
                    if(ar.AggregatedInstance[Sn.TerminatingCalledNumber].IsNullOrEmptyOrWhiteSpace())
                    {
                        ;
                    }
                });
                // need to be inserted to sri partial table
                var newFailedSriRows = failedSriRows
                .Where(failedRow => !oldSriRowsByIdCall.ContainsKey(failedRow[Sn.IdCall]))
                .ToList();


                foreach (string[] row in successfulSriAggregationResults
                    .SelectMany(ar => ar.NewInstancesToBeDiscardedAfterAggregation))
                {
                    //need to be deleted from sri partial table
                    if (oldSriRowsByIdCall.ContainsKey(row[Sn.IdCall]))
                        sriHelper.OldRowsToBeDiscardedAfterAggregation.Add(row); 
                }
                
                

                //sri should be cleared from mt after DB operations.
                preProcessor.TxtCdrRows.RemoveAll(row => row[Sn.SmsType] == "1" || row[Sn.SmsType] == "2");
                l1FailedRows.RemoveAll(row => row[Sn.SmsType] == "1" || row[Sn.SmsType] == "2");
                l1AggResults.RemoveAll(nao=>nao.NewUnAggInstances.Any(s=>s[Sn.SmsType] == "1" || s[Sn.SmsType] == "2"));

                Console.WriteLine("Performing L2 Aggregation ....");

                DayWiseEventCollector<string[]> dayWiseEventCollector = getEventCollector(decoder, l1FailedRows);
                SmsHubStyleAggregator<string[]> aggregator = new SmsHubStyleAggregator<string[]>(dayWiseEventCollector);
                List<NewAndOldEventsWrapper<string[]>> l2FailedWrappers;
                List<NewAndOldEventsWrapper<string[]>> l2AggResults = aggregator.aggregateCdrs(out l2FailedWrappers);
                //if (!l2AggResults.TrueForAll(wr =>
                // {
                //     var b = wr.NewUnAggInstances.Count == 1 && wr.OldUnAggInstances.Count == 1;
                //     if (b == false)
                //     {
                //         ;
                //     }
                //     return b;
                // }))
                //    throw new Exception("L2 agg result must have 1 new and 1 old instance");
                //if (!l2AggResults.TrueForAll(wr => 
                //        new[]{"1","3"}.Contains(wr.OldUnAggInstances.First()[Sn.SmsType])
                //        && new[] { "2", "4" }.Contains(wr.NewUnAggInstances.First()[Sn.SmsType])))
                //    throw new Exception("New instances must be ReturnResult and Old instances must be SRI/MT FWD");
                if (!l2FailedWrappers.TrueForAll(wr => wr.NewUnAggInstances.Count + wr.OldUnAggInstances.Count == 1))
                    throw new Exception("Failed aggregation wrappers must contain either 1 success or failed response.");
                allPreAggWrappers =
                    l1AggResults.Concat(l2AggResults).Concat(l2FailedWrappers).ToList();
                allPreAggWrappers.ForEach(paw => mtAggregationResults.Add(decoder.Aggregate(paw)));

                var successfulAggregationResults = mtAggregationResults.Where(ar => ar.AggregatedInstance != null)
                    .ToList();
                var failedAggregationResults = mtAggregationResults.Where(ar => ar.AggregatedInstance == null)
                    .ToList();
                preProcessor.FinalAggregatedInstances = successfulAggregationResults.Select(ar => ar.AggregatedInstance).ToList();

                // Redis
                //var aggins = successfulAggregationResults.Select(ar => ar.AggregatedInstance).ToList();
                //List<string[]> newAggSriToBeDiscarded;
                //var aggMt = aggregateMtWithSri(aggins, out newAggSriToBeDiscarded);
                //// Remove aggregated sri, not to be inserted in db
                //preProcessor.NewRowsToBeDiscardedAfterAggregation.AddRange(newAggSriToBeDiscarded);
                //// Final result of aggregated  mt
                //preProcessor.FinalAggregatedInstances = aggMt;
                //Redis

                //DB Memory Engine For Aggregated SRI
                var aggSri = successfulSriAggregationResults.Select(ar => ar.AggregatedInstance).ToList();
                sriHelper.InsertRow(newFailedSriRows);
                sriHelper.DeleteRow();
                insertAggregatedSri(aggSri);
                //DB Memory Engine For Aggregated SRI

                preProcessor.NewRowsCouldNotBeAggregated = failedAggregationResults
                    .SelectMany(ar => ar.NewInstancesCouldNotBeAggregated).ToList();
                preProcessor.OldRowsCouldNotBeAggregated = failedAggregationResults
                    .SelectMany(ar => ar.OldInstancesCouldNotBeAggregated).ToList();
                foreach (string[] row in successfulAggregationResults
                    .SelectMany(ar => ar.NewInstancesToBeDiscardedAfterAggregation))
                {
                    preProcessor.NewRowsToBeDiscardedAfterAggregation.Add(row);
                }
                foreach (string[] row in successfulAggregationResults
                    .SelectMany(ar => ar.OldInstancesToBeDiscardedAfterAggregation))
                {
                    preProcessor.OldRowsToBeDiscardedAfterAggregation.Add(row);
                }
                preProcessor.OldPartialInstancesFromDB = successfulAggregationResults
                    .SelectMany(ar => ar.OldPartialInstancesFromDB)
                    .Concat(failedAggregationResults.SelectMany(ar => ar.OldPartialInstancesFromDB)).ToList();

                var inputCount = dayWiseEventCollector.InputEvents.Count + l1AggResults.Count * 2;
                var existingRows = dayWiseEventCollector.ExistingEventsInDb;
                preProcessor.TxtCdrRows = preProcessor.FinalAggregatedInstances;

                if (inputCount + existingRows.Count != preProcessor.FinalAggregatedInstances.Count + preProcessor.NewRowsToBeDiscardedAfterAggregation.Count
                    + preProcessor.OldRowsToBeDiscardedAfterAggregation.Count +
                    +preProcessor.NewRowsCouldNotBeAggregated.Count + preProcessor.OldRowsCouldNotBeAggregated.Count)
                {
                    throw new Exception("Input and aggregated rows count did not match expected value");
                }
                //preProcessor.ValidateAggregation(this.Input.Job);

            }// preAgg

            if (preProcessor.TxtCdrRows.Count > 0)
            {
                if (CollectorInput.Ne.FilterDuplicateCdr == 1 && !neAdditionalSetting.PerformPreaggregation)
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
                    if (neAdditionalSetting != null && !neAdditionalSetting.AggregationStyle
                            .IsNullOrEmptyOrWhiteSpace()) //move it to a mef rule later
                    {
                        if (CollectorInput.Ne.FilterDuplicateCdr != 1)
                        {
                            throw new Exception("Duplicate Filtering must be on when cdr aggregation is enabled.");
                        }
                        if (neAdditionalSetting.AggregationStyle == "telcobridge" && !neAdditionalSetting.PerformPreaggregation)
                        {
                            Console.WriteLine("Aggregating ....");
                            foreach (var row in preProcessor.DecodedCdrRowsBeforeDuplicateFiltering)
                            {
                                if (preProcessor.FinalNonDuplicateEvents.ContainsKey(row[Fn.UniqueBillId]))
                                {
                                    preProcessor.RowsToConsiderForAggregation.Add(row);
                                }
                            }
                            preProcessor.FinalNonDuplicateEvents =//exclude partials, later add them when after agg
                               preProcessor.FinalNonDuplicateEvents.Where(kv => kv.Value[Fn.Partialflag] != "1")
                               .ToDictionary(kv => kv.Key, kv => kv.Value);

                            preProcessor.NewDuplicateEvents =
                                preProcessor.NewDuplicateEvents.Where(r =>
                                preProcessor.ExistingUniqueEventInstancesFromDB.Contains(r[Fn.UniqueBillId])).ToList();

                            preProcessor = aggregateCdrs(preProcessor);
                            preProcessor.TxtCdrRows = preProcessor.FinalAggregatedInstances;

                            //append the aggregated instances back in the final non dup events
                            foreach (var r in preProcessor.FinalAggregatedInstances)
                            {
                                preProcessor.FinalNonDuplicateEvents.Add(r[Fn.UniqueBillId], r);
                            }
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
            newCollectionResult.NewSriRows = newSriRows;
            //dup cdr related
            newCollectionResult.FinalNonDuplicateEvents = preProcessor.FinalNonDuplicateEvents;
            newCollectionResult.NewDuplicateEvents = preProcessor.NewDuplicateEvents;
            //aggregation related
            newCollectionResult.NewRowsCouldNotBeAggreagated = preProcessor.NewRowsCouldNotBeAggregated;
            newCollectionResult.OldRowsCouldNotBeAggreagated = preProcessor.OldRowsCouldNotBeAggregated;
            newCollectionResult.NewRowsToBeDiscardedAfterAggregation =
                                    preProcessor.NewRowsToBeDiscardedAfterAggregation;//partial new unagg instances
            newCollectionResult.OldRowsToBeDiscardedAfterAggregation =
                preProcessor.OldRowsToBeDiscardedAfterAggregation;//partial old unagg instances
            newCollectionResult.DebugCdrsForDump = preProcessor.DebugCdrsForDump;
            newCollectionResult.OldPartialInstancesFromDB = preProcessor.OldPartialInstancesFromDB;
            preProcessor.NewDuplicateEvents = newCollectionResult.NewDuplicateEvents;

            PartialCdrTesterData partialCdrTesterData =
                OrganizeTestDataForPartialCdrs(preProcessor, newCollectionResult);
            CdrJob cdrJob = (new CdrJobFactory(this.Input, this.RawCount)).CreateCdrJob(preProcessor,
                newCollectionResult, oldCollectionResult, partialCdrTesterData);

            if (cdrJob.CdrProcessor.CollectionResult.ConcurrentCdrExts.Count > 0 ||
                cdrJob.CdrProcessor.CollectionResult.NewRowsCouldNotBeAggreagated.Count > 0) //job not empty, or has records
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
            this.HandledJobsOutput.FilesToCleanUp = this.FilesToDeleteAfterJobCompletion;
            return this.HandledJobsOutput;
        }

        private List<string[]> consistantSriRows(List<string[]> sriRows)
        {
            foreach (var row in sriRows)
            {
                string startTime = row[Sn.StartTime];

                // Convert the StartTime to MySQL format using the ConvertToMySqlFormat function
                try
                {
                    row[Sn.StartTime] = ConvertToMySqlFormat(startTime);
                    row[Sn.ChargingStatus] = (row[Sn.ChargingStatus] == "False" || row[Sn.ChargingStatus] == "0") ? "0" : "1";
                    row[Sn.Partialflag] = (row[Sn.ChargingStatus] == "False" || row[Sn.ChargingStatus] == "0") ? "0" : "1";
                }
                catch (FormatException ex)
                {
                    // Handle the case where the date format is invalid (optional)
                    Console.WriteLine($"Error parsing StartTime: {ex.Message}");
                }
            }
            return sriRows;
        }
        private void insertAggregatedSri(List<string[]> aggSri)
        {
            aggSri = consistantSriRows(aggSri);
            ImsiHelper imsiHelper = new ImsiHelper(this.Input.Tbc);
            imsiHelper.InsertRow(aggSri);
        }
        private long GenerateIdCall(string packetFrameTime, string codec, string redirectingNumber)
        {
            // Combine the fields into a single string
            string combinedString = packetFrameTime + codec + redirectingNumber;

            // Compute SHA256 hash from the combined string
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedString));

                // Convert the first 8 bytes of the hash to a long value
                long uniqueId = BitConverter.ToInt64(hashBytes, 0);

                // Ensure the ID is positive
                return Math.Abs(uniqueId);
            }
        }
        private string ConvertToMySqlFormat(string dateStr)
        {
            // Define the input date formats explicitly.
            var formats = new[] { "M/d/yyyy h:mm:ss tt", "yyyy-MM-dd HH:mm:ss" };  // Using "h" for single-digit hours in 12-hour format

            DateTime parsedDate;
            foreach (var format in formats)
            {
                // Try to parse the input date string with the defined formats and invariant culture
                if (DateTime.TryParseExact(dateStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                {
                    // Return the date in MySQL compatible format (yyyy-MM-dd HH:mm:ss)
                    return parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            // If the date could not be parsed, throw an exception
            throw new FormatException($"Invalid date format: {dateStr}");
        }
        private DayWiseEventCollector<string[]> getEventCollector(AbstractCdrDecoder decoder, List<string[]> failedPreAggCandidates)
        {
            List<string[]> rowsToConsiderForAggregation = failedPreAggCandidates;
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
            return dayWiseEventCollector;
        }


        private NewCdrPreProcessor filterDuplicates(NewCdrPreProcessor preProcessor, CdrSetting cdrSetting, Dictionary<long, CdrMergedJobError> jobsWithDupCdrsDuringMergeProcessing)
        {
            Console.WriteLine("CdrJobProcessor: Filtering duplicates...");
            var ne = preProcessor.CdrCollectorInputData.Ne;
            var neWiseAdditionalSettings = preProcessor.CdrSetting.NeWiseAdditionalSettings;
            NeAdditionalSetting neAdditionalSettings = null;
            neWiseAdditionalSettings.TryGetValue(ne.idSwitch, out neAdditionalSettings);
            if (neAdditionalSettings != null && neAdditionalSettings.PerformPreaggregation)
            {
                preProcessor = this.filterDuplicateCdrsDummy(preProcessor);
                return preProcessor;
            }
            else
            {
                preProcessor = this.filterDuplicateCdrs(preProcessor);
            }

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
                                Job = this.HandledJobsOutput.Jobs.First(j => j.JobName == r[Fn.Filename]),
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
                if (this.Input.MediationContext.Tbc.CdrSetting.MoveCdrToDriveAfterProcessing.Trim() != "")
                {
                    MoveCdrAfterProcessing(this.Input.Context, this.Input.MediationContext.Tbc,
                        cdrJob.CdrProcessor.CdrJobContext.TelcobrightJob);
                }
            }
        }

        private void FinalizeNonMergedJob(CdrJob cdrJob)
        {
            var collectionResult = cdrJob.CdrProcessor.CollectionResult;
            if (collectionResult.OriginalRowsBeforeMerge.Count > 0) //job not empty, or has records
            {
                decimal totalCdrDuration = cdrJob.CdrProcessor.CollectionResult
                        .OriginalRowsBeforeMerge.Where(r => r[Fn.Partialflag] != "1").Sum(r => r[Fn.DurationSec].IsNullOrEmptyOrWhiteSpace()
                            ? 0
                            : Convert.ToDecimal(r[Fn.DurationSec]));
                decimal totalActualDurationInconsistent = cdrJob.CdrProcessor.CollectionResult
                        .CdrInconsistents.Where(c => c.PartialFlag != "1").Sum(r => r.DurationSec.IsNullOrEmptyOrWhiteSpace()
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
                if (this.Input.MediationContext.Tbc.CdrSetting.MoveCdrToDriveAfterProcessing.Trim() != "")
                {
                    MoveCdrAfterProcessing(this.Input.Context, this.Input.MediationContext.Tbc,
                        cdrJob.CdrProcessor.CdrJobContext.TelcobrightJob);
                }
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
            decimal totalCdrDuration = mergedJob.OriginalRows
                .Where(r => r[Fn.Partialflag] != "1").Sum(r => r[Fn.DurationSec].IsNullOrEmptyOrWhiteSpace()
                            ? 0
                            : Convert.ToDecimal(r[Fn.DurationSec]));
            decimal totalActualDurationInconsistent = mergedJob.OriginalCdrinconsistents
                .Where(c => c.PartialFlag != "1").Sum(r => r.DurationSec.IsNullOrEmptyOrWhiteSpace()
                        ? 0
                        : Convert.ToDecimal(r.DurationSec));
            decimal totalActualDuration = totalCdrDuration + totalActualDurationInconsistent;
            WriteJobCompletionIfCollectionNotEmpty(preProcessor.OriginalRowsBeforeMerge.Count, telcobrightJob, context, totalActualDuration);
            if (this.Input.CdrSetting.DisableCdrPostProcessingJobCreationForAutomation == false)
            {
                CreateNewCdrPostProcessingJobs(this.Input.Context, this.Input.MediationContext.Tbc, telcobrightJob);
            }
            DeletePreDecodedFile(this.Input.Context, this.Input.MediationContext.Tbc,
                telcobrightJob);
            if (this.Input.MediationContext.Tbc.CdrSetting.MoveCdrToDriveAfterProcessing.Trim() != "")
            {
                MoveCdrAfterProcessing(this.Input.Context, this.Input.MediationContext.Tbc,
                    telcobrightJob);
            }
        }

        public object PostprocessJobBeforeCommit(object data)
        {
            List<job> handledJobs = ((CdrJobOutput)data).Jobs;
            foreach (var job in handledJobs)
            {
                DeletePreDecodedFile(this.Input.Context, this.Input.MediationContext.Tbc, job);
                if (this.Input.MediationContext.Tbc.CdrSetting.MoveCdrToDriveAfterProcessing.Trim() != "")
                {
                    MoveCdrAfterProcessing(this.Input.Context, this.Input.MediationContext.Tbc, job);
                }
            }
            return handledJobs;
        }

        public object PostprocessJobAfterCommit(object data)
        {
            var retVal = (CdrJobOutput)data; //EXECUTE

            foreach (var fileToDelete in retVal.FilesToCleanUp)
            {
                if (File.Exists(fileToDelete.FullName))
                {
                    File.Delete(fileToDelete.FullName);
                }
            }
            return retVal;
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
                List<string[]> decodedCdrRows = new List<string[]>();
                var casStyleProcessing = this.CollectorInput.CdrSetting.useCasStyleProcessing;
                try
                {
                    decodedCdrRows = decoder.DecodeFile(this.CollectorInput, out cdrinconsistents);
                    if (casStyleProcessing)
                    {
                        decodedCdrRows = decodedCdrRows
                            .Where(r => (!r[Fn.ConnectTime].IsNullOrEmptyOrWhiteSpace() && r[Fn.ConnectTime].ConvertToDateTimeFromMySqlFormat() >= this.CollectorInput.CdrSetting.ExcludeBefore)
                                        || (!r[Fn.StartTime].IsNullOrEmptyOrWhiteSpace() && r[Fn.StartTime].ConvertToDateTimeFromMySqlFormat() >= this.CollectorInput.CdrSetting.ExcludeBefore))
                            .ToList();
                    }

                }
                catch (Exception e)
                {
                    if (e.Message.Contains("OutOfMemoryException"))
                    {
                        Console.WriteLine("WARNING!!!!!!!! MANUAL GARBAGE COLLECTION AND COMPACTION OF LOH.");
                        GarbageCollectionHelper.CompactGCNowForOnce();
                        decodedCdrRows = decoder.DecodeFile(this.CollectorInput, out cdrinconsistents);
                        if (casStyleProcessing)
                        {
                            decodedCdrRows = decodedCdrRows
                                .Where(r => (!r[Fn.ConnectTime].IsNullOrEmptyOrWhiteSpace() &&
                                             r[Fn.ConnectTime].ConvertToDateTimeFromMySqlFormat() >=
                                             this.CollectorInput.CdrSetting.ExcludeBefore)
                                            || (!r[Fn.StartTime].IsNullOrEmptyOrWhiteSpace() &&
                                                r[Fn.StartTime].ConvertToDateTimeFromMySqlFormat() >=
                                                this.CollectorInput.CdrSetting.ExcludeBefore))
                                .ToList();
                        }
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
            if (preProcessor.TxtCdrRows.Any() && this.Input.NeAdditionalSetting.DumpAllInstancesToDebugCdrTable)
            {
                foreach (var row in preProcessor.TxtCdrRows)
                {
                    preProcessor.DebugCdrsForDump.Add(row);
                }
                CreateTableForDebugCdr();
            }
            preProcessor.OriginalCdrFileSize = fileInfo.Length;
            return preProcessor;
        }

        private void CreateTableForDebugCdr()
        {
            DebugCdrHelper.showWarning();
            var constr = DbUtil.getDbConStrWithDatabase(this.Input.MediationContext.Tbc.DatabaseSetting);
            using (MySqlConnection con = new MySqlConnection(constr)
            ) //use separate connection as ddl may commit unwanted changes
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("", con))
                {
                    var tableName = "cdrdebug";
                    cmd.CommandText = DebugCdrHelper.getCreateTableSqlIfNotExists(tableName);
                    cmd.ExecuteNonQuery();
                }
                con.Close();
                DebugCdrHelper.showWarning();
            }
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
                    if (this.Input.NeAdditionalSetting != null &&
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
                this.FilesToDeleteAfterJobCompletion.Add(new FileInfo(preDecodedFileName));
        }

        protected void MoveCdrAfterProcessing(PartnerEntities context, TelcobrightConfig tbc, job cdrJob)
        {
            string fileLocationName = this.CollectorInput.Ne.SourceFileLocations;
            FileLocation fileLocation = this.CollectorInput.Tbc.DirectorySettings.FileLocations[fileLocationName];
            string newCdrFileName = fileLocation.GetOsNormalizedPath(fileLocation.StartingPath)
                                    + Path.DirectorySeparatorChar + cdrJob.JobName;
            FileInfo newCdrFileInfo = new FileInfo(newCdrFileName);
            string driveToMove = tbc.CdrSetting.MoveCdrToDriveAfterProcessing;
            if (Directory.Exists(driveToMove) == false)
                Directory.CreateDirectory(driveToMove);
            //string preDecodedFileName = predecodedDirName + Path.DirectorySeparatorChar + newCdrFileInfo.Name + ".predecoded";
            string targetFileName = driveToMove + newCdrFileInfo.FullName.Split(':')[1];
            string targetDirPath = driveToMove + newCdrFileInfo.DirectoryName.Split(':')[1];
            if (Directory.Exists(targetDirPath) == false)
            {
                Directory.CreateDirectory(targetDirPath);
            }
            if (File.Exists(targetFileName))
            {
                File.Delete(targetFileName);
            }
            File.Copy(newCdrFileInfo.FullName, targetFileName);
            FileInfo copiedFileInfo = new FileInfo(targetFileName);
            if (newCdrFileInfo.Length == copiedFileInfo.Length)
            {
                this.FilesToDeleteAfterJobCompletion.Add(newCdrFileInfo);
            }
            else
            {
                throw new Exception("Couldn't move cdr file after processing, file size didn't match after move.");
            }
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
            decodedCdrRows.ForEach(r => decodedRowsBeforeDuplicateFiltering.Add(r));
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
            HashSet<string> existingUniqueEventInstancesFromDB = null;
            Dictionary<string, string[]> finalNonDuplicateEvents =
                duplicaterEventFilter.filterDuplicateCdrs(out excludedDuplicateCdrs, out existingUniqueEventInstancesFromDB);

            preProcessorWithCollectedRows.FinalNonDuplicateEvents = finalNonDuplicateEvents;

            var textCdrCollectionPreProcessor = new NewCdrPreProcessor(
                txtCdrRows: finalNonDuplicateEvents.Values.ToList(),
                inconsistentCdrs: cdrinconsistents,
                cdrCollectorInputData: this.CollectorInput)
            {
                DecodedCdrRowsBeforeDuplicateFiltering = decodedRowsBeforeDuplicateFiltering,
                FinalNonDuplicateEvents = finalNonDuplicateEvents,
                NewDuplicateEvents = excludedDuplicateCdrs,
                DebugCdrsForDump = preProcessorWithCollectedRows.DebugCdrsForDump,
                Decoder = decoder,
            };
            foreach (string tuple in existingUniqueEventInstancesFromDB)
            {
                textCdrCollectionPreProcessor.ExistingUniqueEventInstancesFromDB.Add(tuple);
            }
            //adjust raw count due to filtering
            int newRawCount = textCdrCollectionPreProcessor.TxtCdrRows.Count +
                              textCdrCollectionPreProcessor.InconsistentCdrs.Count +
                              textCdrCollectionPreProcessor.NewDuplicateEvents.Count;
            if (newRawCount != textCdrCollectionPreProcessor.DecodedCdrRowsBeforeDuplicateFiltering.Count
                + textCdrCollectionPreProcessor.InconsistentCdrs.Count)
            {
                throw new Exception("Cdr count mismatch after duplicate filtering!");
            }
            textCdrCollectionPreProcessor.RawCount = newRawCount;
            return textCdrCollectionPreProcessor;
        }

        public NewCdrPreProcessor filterDuplicateCdrsDummy(NewCdrPreProcessor preProcessorWithCollectedRows)
        {
            AbstractCdrDecoder decoder = preProcessorWithCollectedRows.Decoder;
            List<string[]> decodedCdrRows = preProcessorWithCollectedRows.TxtCdrRows;
            List<string[]> decodedRowsBeforeDuplicateFiltering = new List<string[]>();
            decodedCdrRows.ForEach(r => decodedRowsBeforeDuplicateFiltering.Add(r));
            List<cdrinconsistent> cdrinconsistents = preProcessorWithCollectedRows.InconsistentCdrs.ToList();
            //Dictionary<string, string[]> finalNonDuplicateEvents = decodedCdrRows.ToDictionary(r=>r[Fn.UniqueBillId]);

            var textCdrCollectionPreProcessor = new NewCdrPreProcessor(
                txtCdrRows: preProcessorWithCollectedRows.TxtCdrRows,
                inconsistentCdrs: cdrinconsistents,
                cdrCollectorInputData: this.CollectorInput)
            {
                DecodedCdrRowsBeforeDuplicateFiltering = decodedRowsBeforeDuplicateFiltering,
                FinalNonDuplicateEvents = new Dictionary<string, string[]>(),
                NewDuplicateEvents = new List<string[]>(),
                DebugCdrsForDump = preProcessorWithCollectedRows.DebugCdrsForDump,
                Decoder = decoder,
                RowsToConsiderForAggregation = decodedCdrRows
            };

            //adjust raw count due to filtering
            int newRawCount = textCdrCollectionPreProcessor.TxtCdrRows.Count +
                              textCdrCollectionPreProcessor.InconsistentCdrs.Count +
                              textCdrCollectionPreProcessor.NewDuplicateEvents.Count;
            if (newRawCount != textCdrCollectionPreProcessor.DecodedCdrRowsBeforeDuplicateFiltering.Count
                + textCdrCollectionPreProcessor.InconsistentCdrs.Count)
            {
                throw new Exception("Cdr count mismatch after duplicate filtering!");
            }
            textCdrCollectionPreProcessor.RawCount = newRawCount;
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

            Dictionary<string, EventAggregationResult> aggregationResults = aggregator.aggregateCdrs();
            var successfulAggregationResults = aggregationResults.Values.Where(ar => ar.AggregatedInstance != null)
                .ToList();
            var failedAggregationResults = aggregationResults.Values.Where(ar => ar.AggregatedInstance == null)
                .ToList();
            preprocessor.FinalAggregatedInstances = successfulAggregationResults.Select(ar => ar.AggregatedInstance).ToList();
            preprocessor.NewRowsCouldNotBeAggregated = failedAggregationResults
                .SelectMany(ar => ar.NewInstancesCouldNotBeAggregated).ToList();
            preprocessor.OldRowsCouldNotBeAggregated = failedAggregationResults
                .SelectMany(ar => ar.OldInstancesCouldNotBeAggregated).ToList();
            foreach (string[] row in successfulAggregationResults
                .SelectMany(ar => ar.NewInstancesToBeDiscardedAfterAggregation))
            {
                preprocessor.NewRowsToBeDiscardedAfterAggregation.Add(row);
            }
            foreach (string[] row in successfulAggregationResults
                .SelectMany(ar => ar.OldInstancesToBeDiscardedAfterAggregation))
            {
                preprocessor.OldRowsToBeDiscardedAfterAggregation.Add(row);
            }
            preprocessor.OldPartialInstancesFromDB = successfulAggregationResults
                .SelectMany(ar => ar.OldPartialInstancesFromDB)
                .Concat(failedAggregationResults.SelectMany(ar => ar.OldPartialInstancesFromDB)).ToList();

            var inputRows = dayWiseEventCollector.InputEvents;
            var existingRows = dayWiseEventCollector.ExistingEventsInDb;
            if (inputRows.Count + existingRows.Count != preprocessor.FinalAggregatedInstances.Count + preprocessor.NewRowsToBeDiscardedAfterAggregation.Count
                + preprocessor.OldRowsToBeDiscardedAfterAggregation.Count +
                +preprocessor.NewRowsCouldNotBeAggregated.Count + preprocessor.OldRowsCouldNotBeAggregated.Count)
            {
                throw new Exception("Input and aggregated rows count did not match expected value");
            }
            return preprocessor;
        }
        public List<string[]> aggregateMtWithSri(List<string[]> aggins, out List<string[]> newAggSriToBeDiscarded)
        {
            var redisConnectionString = "localhost:6379";
            // Filter collections based on SmsType values
            var aggSri = aggins.Where(asrii => asrii[Sn.SmsType] == "1").ToList();
            var aggMt = aggins.Where(amti => amti[Sn.SmsType] == "3").ToList();

            List<DateTime> dates = aggMt.AsParallel()
                                       .Select(r => r[Sn.StartTime].ConvertToDateTimeFromMySqlFormat())
                                       .ToList();
            DateTime minDateTime = dates.Min().AddMinutes(-1);
            DateTime maxDateTime = dates.Max().AddMinutes(1);

            var imsiCache = new RedCache(redisConnectionString);
            var aggSriFromCache = imsiCache.GetRecordsInTimeRange(minDateTime, maxDateTime);
            var newAggSri = new List<string[]>(aggSri); // Copy original Sri list
            aggSri.AddRange(aggSriFromCache);

            // Create a dictionary for fast lookup of aggSri by Imsi
            var sriDictionary = aggSri
                .GroupBy(sri => sri[Sn.Imsi])
                .ToDictionary(g => g.Key, g => g.ToList());

            // List to track items to remove from aggins
            var itemsToRemove = new HashSet<string>();

            // Process each element in aggMt
            foreach (var mt in aggMt)
            {
                var sriMatches = new List<string[]>();
                if (sriDictionary.TryGetValue(mt[Sn.Imsi], out sriMatches))
                {
                    foreach (var sri in sriMatches)
                    {
                        DateTime mtTime = Convert.ToDateTime(mt[Sn.StartTime]);
                        DateTime sriTime = Convert.ToDateTime(sri[Sn.StartTime]);

                        if ((mtTime - sriTime).TotalSeconds >= 60)
                        {
                            mt[Sn.TerminatingCalledNumber] = sri[Sn.TerminatingCalledNumber];
                            sri[Sn.Imsi] = "done";
                            itemsToRemove.Add(sri[Sn.UniqueBillId]);
                            break;
                        }
                    }
                }
            }

            newAggSriToBeDiscarded = new List<string[]>(newAggSri); // Copy newAggSri before filtering

            // Select the discarded items (those whose UniqueBillId was in itemsToRemove)
            var discardedItems = aggSriFromCache.Where(ai => itemsToRemove.Contains(ai[Sn.UniqueBillId])).ToList();
            newAggSri = newAggSri.Where(ai => !itemsToRemove.Contains(ai[Sn.UniqueBillId])).ToList();

            // Remove matched records from cache
            foreach (var discardedItem in discardedItems)
            {
                imsiCache.Remove(discardedItem[Sn.Imsi], DateTime.Parse(discardedItem[Sn.StartTime]));
            }

            // Store unmatched Sri records back in cache
            foreach (var sri in newAggSri)
            {
                imsiCache.Add(sri[Sn.Imsi], DateTime.Parse(sri[Sn.StartTime]), sri[Sn.TerminatingCalledNumber]);
            }

            imsiCache.RemoveRecordsBeforeTime(minDateTime);

            return aggMt;
        }

    }
}
