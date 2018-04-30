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
        bool PartialCollectionEnabled => this.CdrSetting.PartialCdrEnabledNeIds.Contains(this.CdrJobContext.Ne.idSwitch);
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

            this.CdrProcessor?.Process();

            IncrementalTransactionCreator transactionProcessor = new IncrementalTransactionCreator(this);
            List<acc_transaction> incrementalTransactions= transactionProcessor.CreateIncrementalTransactions();
            transactionProcessor.ValidateTransactions(incrementalTransactions);
            this.CdrJobContext.AccountingContext.ExecuteTransactions(incrementalTransactions);

            ValidateWithMediationTester(this.CdrJobContext.CdrjobInputData);
            CdrWritingResult cdrWritingResult = this.CdrProcessor?.WriteCdrs();
            if (this.PartialCollectionEnabled)
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
            Assert.IsTrue(mediationTester.DurationSumInCdrAndTableWiseSummariesAreTollerablyEqual(this.CdrProcessor));
            Assert.IsTrue(mediationTester.DurationSumInCdrAndSummaryAreTollerablyEqual(this.CdrProcessor));
            Assert.IsTrue(mediationTester.SummaryCountTwiceAsCdrCount(this.CdrProcessor));
            Assert.IsTrue(mediationTester
                .SumOfPrevDayWiseDurationsAndNewSummaryInstancesIsEqualToSameInMergedSummaryCache(
                    this.CdrProcessor));
        }
    }
}
