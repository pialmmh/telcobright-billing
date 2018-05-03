using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        public CdrJob(CdrProcessor cdrProcessor, CdrEraser cdrEraser, int actualStepsCount,
            PartialCdrTesterData partialCdrTesterData)
        {
            this.CdrJobContext = cdrProcessor?.CdrJobContext ?? cdrEraser.CdrJobContext;
            this.CdrProcessor = cdrProcessor;
            this.CdrEraser = cdrEraser;
            this.ActualStepsCount = actualStepsCount;
            this.PartialCdrTesterData = partialCdrTesterData;
        }

        public void Execute()
        {
            this.CdrEraser?.RegenerateOldSummaries();
            this.CdrEraser?.ValidateSummaryReGeneration();
            if (this.CdrEraser != null)
            {
                ValidateCdrEraserWithMediationTester(this.CdrJobContext.CdrjobInputData,
                    this.CdrEraser.CollectionResult);
            }
            this.CdrEraser?.UndoOldSummaries();
            this.CdrEraser?.UndoOldChargeables();
            this.CdrEraser?.DeleteOldCdrs();

            this.CdrProcessor?.Mediate();
            this.CdrProcessor?.GenerateSummaries();
            this.CdrProcessor?.MergeNewSummariesIntoCache();
            this.CdrProcessor?.ProcessChargeables();

            IncrementalTransactionCreator transactionProcessor = new IncrementalTransactionCreator(this);
            List<acc_transaction> incrementalTransactions= transactionProcessor.CreateIncrementalTransactions();
            transactionProcessor.ValidateTransactions(incrementalTransactions);
            this.CdrJobContext.AccountingContext.ExecuteTransactions(incrementalTransactions);

            if (this.CdrProcessor!=null)
            {
                ValidateCdrProcessorWithMediationTester(this.CdrJobContext.CdrjobInputData,
                    this.CdrProcessor.CollectionResult);
            }
            CdrWritingResult cdrWritingResult = this.CdrProcessor?.WriteCdrs();
            if (this.CdrProcessor != null && this.CdrProcessor.PartialProcessingEnabled)
            {
                PartialCdrTester partialCdrTester =
                    new PartialCdrTester(this, cdrWritingResult, this.PartialCdrTesterData);
                partialCdrTester.ValidatePartialCdrMediation();
            }

            foreach (var summaryCache in this.CdrJobContext.CdrSummaryContext.TableWiseSummaryCache.Values)
            {
                summaryCache.WriteAllChanges(this.CdrJobContext.DbCmd,
                    this.CdrJobContext.SegmentSizeForDbWrite);
            }
            this.CdrJobContext.AccountingContext.WriteAllChanges();
            this.CdrJobContext.AutoIncrementManager.WriteAllChanges(this.CdrJobContext.DbCmd,
                this.CdrSetting.SegmentSizeForDbWrite);
        }
        protected void ValidateCdrProcessorWithMediationTester(CdrJobInputData input,CdrCollectionResult collectionResult)
        {
            MediationTester mediationTester =
                new MediationTester(input.Tbc.CdrSetting.FractionalNumberComparisonTollerance);
            if(!mediationTester.DurationSumInCdrAndSummaryAreTollerablyEqual(collectionResult))
                throw new Exception("Duration sum in cdr and summary are not tollerably equal");
            if(!mediationTester.SummaryCountTwiceAsCdrCount(collectionResult))
                throw new Exception("Summary count is not twice as cdr count");
            //if(!mediationTester
            //    .SumOfPrevDayWiseDurationsAndNewSummaryInstancesIsEqualToSameInMergedSummaryCache(
            //        this.CdrProcessor))
            //    throw new Exception("Sum of prev day wise durations and new summary instances is not equal to same " +
            //                        "in merged summary cache");
            if(!mediationTester.DurationSumOfNonPartialRawPartialsAndRawDurationAreTollerablyEqual(this.CdrProcessor))
                throw new Exception("Duration sum Of non partial and raw Partial cdrs " +
                                    "are not equal to duration in raw instances.");
        }
        protected void ValidateCdrEraserWithMediationTester(CdrJobInputData input, CdrCollectionResult collectionResult)
        {
            MediationTester mediationTester =
                new MediationTester(input.Tbc.CdrSetting.FractionalNumberComparisonTollerance);
            if (!mediationTester.DurationSumInCdrAndSummaryAreTollerablyEqual(collectionResult))
                throw new Exception("Duration sum in cdr and summary are not tollerably equal");
            if (!mediationTester.SummaryCountTwiceAsCdrCount(collectionResult))
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
        protected void AssertWithMediationTester(CdrJobInputData input, CdrCollectionResult collectionResult)
        {
            MediationTester mediationTester =
                new MediationTester(input.Tbc.CdrSetting.FractionalNumberComparisonTollerance);
            Assert.IsTrue(mediationTester.DurationSumInCdrAndSummaryAreTollerablyEqual(collectionResult));
            Assert.IsTrue(mediationTester.SummaryCountTwiceAsCdrCount(collectionResult));
            //todo: do something about this test which makes database trip & need to modify this to work with both eraser & processor
            //Assert.IsTrue(mediationTester
              //  .SumOfPrevDayWiseDurationsAndNewSummaryInstancesIsEqualToSameInMergedSummaryCache(
            //        this.CdrProcessor));
            Assert.IsTrue(
                mediationTester.DurationSumOfNonPartialRawPartialsAndRawDurationAreTollerablyEqual(this.CdrProcessor));
        }
    }
}
