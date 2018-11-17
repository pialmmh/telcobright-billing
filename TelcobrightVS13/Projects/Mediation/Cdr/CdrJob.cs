using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TelcobrightMediation.Cdr
{
    public class CdrJob : ISegmentedJob
    {
        public CdrProcessor CdrProcessor { get; }
        public CdrEraser CdrEraser { get; }
        public int ActualStepsCount { get; }
        public CdrJobContext CdrJobContext { get; }
        private MediationContext MediationContext => this.CdrJobContext.CdrjobInputData.MediationContext;
        private CdrSetting CdrSetting => this.MediationContext.Tbc.CdrSetting;
        private PartialCdrTesterData PartialCdrTesterData { get; }
        private DbCommand DbCmd => this.CdrEraser.CdrJobContext.DbCmd;
        public CdrJob(CdrProcessor cdrProcessor, CdrEraser cdrEraser, int actualStepsCount,
            PartialCdrTesterData partialCdrTesterData)
        {
            this.CdrJobContext = cdrProcessor?.CdrJobContext ?? cdrEraser.CdrJobContext;
            this.CdrProcessor = cdrProcessor;
            this.CdrEraser = cdrEraser;
            this.ActualStepsCount = actualStepsCount;
            this.PartialCdrTesterData = partialCdrTesterData;
            //this.CdrMetaBeforeCdrjob = ReadCdrMetaFromDb();
        }

        public void Execute()
        {
            if(this.CdrSetting.DisableParallelMediation==true)
                Console.WriteLine("WARNING!!! PARALLEL MEDIATION IS DISABLED. THIS WILL AFFECT PERFORMANCE.");
            try
            {
                this.CdrEraser?.RegenerateOldSummaries();
                this.CdrEraser?.ValidateSummaryReGeneration();
                if (this.CdrEraser != null)
                {
                    ValidateCdrEraserWithMediationTester(this.CdrJobContext.CdrjobInputData,
                        this.CdrEraser.CollectionResult.ConcurrentCdrExts.Values.AsParallel());
                }
                this.CdrEraser?.UndoOldSummaries();
                this.CdrEraser?.UndoOldChargeables();
                this.CdrEraser?.DeleteOldCdrs();
                var prevLatencyMode = GCSettings.LatencyMode;
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
                //GC.TryStartNoGCRegion(1000*256);
                this.CdrProcessor?.Mediate();
                GCSettings.LatencyMode = prevLatencyMode;
                ParallelQuery<CdrExt> parallelCdrExts = this.CdrProcessor?.GenerateSummaries();
                this.CdrProcessor?.MergeNewSummariesIntoCache(parallelCdrExts);
                this.CdrProcessor?.ProcessChargeables(parallelCdrExts);

                IncrementalTransactionCreator transactionProcessor = new IncrementalTransactionCreator(this);
                List<acc_transaction> incrementalTransactions = transactionProcessor.CreateIncrementalTransactions();
                transactionProcessor.ValidateTransactions();
                this.CdrJobContext.AccountingContext.ExecuteTransactions(incrementalTransactions);

                if (this.CdrProcessor != null)
                {
//applicable for only new cdrs 
                    ValidateCdrProcessorWithMediationTester(this.CdrJobContext.CdrjobInputData,
                        parallelCdrExts);
                }
                //write summaries, so that durationTotal in summary can be validated after writing cdrs
                foreach (var summaryCache in this.CdrJobContext.CdrSummaryContext.TableWiseSummaryCache.Values)
                {
                    summaryCache.WriteAllChanges(this.CdrJobContext.DbCmd,
                        this.CdrJobContext.SegmentSizeForDbWrite);
                }
                var cdrWritingResult = this.CdrProcessor?.WriteCdrs(parallelCdrExts);

                if (this.CdrProcessor != null && this.CdrProcessor.PartialProcessingEnabled
                    && this.CdrProcessor.CdrJobContext.TelcobrightJob.idjobdefinition == 1) //1=new cdr only
                {
                    PartialCdrTester partialCdrTester =
                        new PartialCdrTester(this, cdrWritingResult, this.PartialCdrTesterData);
                    partialCdrTester.ValidatePartialCdrMediation();
                }
                this.CdrJobContext.AccountingContext.WriteAllChanges();
                this.CdrJobContext.AutoIncrementManager.WriteAllChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                var mediationContext = this.CdrProcessor?.CdrJobContext.MediationContext;
                bool cacheLimitExceeded = false;
                cacheLimitExceeded = RateCacheCleaner.CheckAndClearRateCache(mediationContext, e);
                if (cacheLimitExceeded == false) throw;
                cacheLimitExceeded = RateCacheCleaner.ClearTempRateTable(e, this.CdrJobContext.DbCmd);
                if (cacheLimitExceeded == false) throw;
            }
        }
        protected void ValidateCdrProcessorWithMediationTester(CdrJobInputData input, ParallelQuery<CdrExt> processedCdrExts)
        {
            MediationTester mediationTester =
                new MediationTester(input.TelcobrightJob);
            if (!mediationTester.DurationSumInCdrAndSummaryAreEqual(processedCdrExts))
                throw new Exception("Duration sum in cdr and summary are not tollerably equal");
            if (!mediationTester.SummaryCountTwiceAsCdrCount(processedCdrExts))
                throw new Exception("Summary count is not twice as cdr count");
            //if (!mediationTester.CdrDurationMatchesSumOfInsertedAndUpdatedSummaryDurationInCache(this.CdrProcessor)) ;
            //throw new Exception("Cdr duration does not match inserted & updated summary instances duration in cache. ");
            //if(!mediationTester
            //    .SumOfPrevDayWiseDurationsAndNewSummaryInstancesIsEqualToSameInMergedSummaryCache(
            //        this.CdrProcessor))
            //    throw new Exception("Sum of prev day wise durations and new summary instances is not equal to same " +
            //                        "in merged summary cache");
            if (!mediationTester.DurationSumOfNonPartialRawPartialsAndRawDurationAreEqual(this.CdrProcessor))
                throw new Exception("Duration sum Of non partial and raw Partial cdrs " +
                                    "are not equal to duration in raw instances.");
        }
        protected void ValidateCdrEraserWithMediationTester(CdrJobInputData input, ParallelQuery<CdrExt> processedCdrExts)
        {
            MediationTester mediationTester =
                new MediationTester(input.TelcobrightJob);
            if (!mediationTester.DurationSumInCdrAndSummaryAreEqual(processedCdrExts))
                throw new Exception("Duration sum in cdr and summary are not tollerably equal");
            if (!mediationTester.SummaryCountTwiceAsCdrCount(processedCdrExts))
                throw new Exception("Summary count is not twice as cdr count");
            //if(!mediationTester
            //    .SumOfPrevDayWiseDurationsAndNewSummaryInstancesIsEqualToSameInMergedSummaryCache(
            //        this.CdrProcessor))
            //    throw new Exception("Sum of prev day wise durations and new summary instances is not equal to same " +
            //                        "in merged summary cache");
            //if (!mediationTester.DurationSumOfNonPartialRawPartialsAndRawDurationAreTollerablyEqual(this.CdrProcessor))
            //    throw new Exception("Duration sum Of non partial and raw Partial cdrs " +
            //                        "are not equal to duration in raw instances.");
        }
        protected void AssertWithMediationTester(CdrJobInputData input, ParallelQuery<CdrExt> processedCdrExts)
        {
            MediationTester mediationTester =
                new MediationTester(input.TelcobrightJob);
            Assert.IsTrue(mediationTester.DurationSumInCdrAndSummaryAreEqual(processedCdrExts));
            Assert.IsTrue(mediationTester.SummaryCountTwiceAsCdrCount(processedCdrExts));
            //todo: do something about this test which makes database trip & need to modify this to work with both eraser & processor
            //Assert.IsTrue(mediationTester
            //  .SumOfPrevDayWiseDurationsAndNewSummaryInstancesIsEqualToSameInMergedSummaryCache(
            //        this.CdrProcessor));
            Assert.IsTrue(
                mediationTester.DurationSumOfNonPartialRawPartialsAndRawDurationAreEqual(this.CdrProcessor));
        }
        
    }
}
