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

            ValidateWithMediationTester(this.CdrJobContext.CdrjobInputData);
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
            this.CdrJobContext.AutoIncrementManager.WriteState();
        }
        protected void ValidateWithMediationTester(CdrJobInputData input)
        {
            MediationTester mediationTester =
                new MediationTester(input.Tbc.CdrSetting.FractionalNumberComparisonTollerance);
            if(!mediationTester.DurationSumInCdrAndTableWiseSummariesAreTollerablyEqual(this.CdrProcessor.CollectionResult))
                throw new Exception("Duration sum in cdr and tableWiseSummaries are not tollerably equal");
            if(!mediationTester.DurationSumInCdrAndSummaryAreTollerablyEqual(this.CdrProcessor))
                throw new Exception("Duration sum in cdr and summary are not tollerably equal");
            if(!mediationTester.SummaryCountTwiceAsCdrCount(this.CdrProcessor))
                throw new Exception("Summary count is not twice as cdr count");
            if(!mediationTester
                .SumOfPrevDayWiseDurationsAndNewSummaryInstancesIsEqualToSameInMergedSummaryCache(
                    this.CdrProcessor))
                throw new Exception("Sum of prev day wise durations and new summary instances is not equal to same " +
                                    "in merged summary cache");
            if(!mediationTester.DurationSumOfNonPartialRawPartialsAndRawDurationAreTollerablyEqual(this.CdrProcessor))
                throw new Exception("Duration sum Of non partial and raw Partial cdrs " +
                                    "are not equal to duration in raw instances.");
        }
        protected void AssertWithMediationTester(CdrJobInputData input)
        {
            MediationTester mediationTester =
                new MediationTester(input.Tbc.CdrSetting.FractionalNumberComparisonTollerance);
            Assert.IsTrue(mediationTester.DurationSumInCdrAndTableWiseSummariesAreTollerablyEqual(this.CdrProcessor.CollectionResult));
            Assert.IsTrue(mediationTester.DurationSumInCdrAndSummaryAreTollerablyEqual(this.CdrProcessor));
            Assert.IsTrue(mediationTester.SummaryCountTwiceAsCdrCount(this.CdrProcessor));
            Assert.IsTrue(mediationTester
                .SumOfPrevDayWiseDurationsAndNewSummaryInstancesIsEqualToSameInMergedSummaryCache(
                    this.CdrProcessor));
            Assert.IsTrue(
                mediationTester.DurationSumOfNonPartialRawPartialsAndRawDurationAreTollerablyEqual(this.CdrProcessor));
        }
    }
}
