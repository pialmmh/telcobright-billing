using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation.Cdr.CollectionRelated.Collector;
using TelcobrightMediation.Mediation.Cdr;
// ReSharper disable ConvertClosureToMethodGroup

namespace TelcobrightMediation.Cdr
{
    public abstract class AbstractCdrJobPreProcessor
    {
        public int RawCount { get; }
        protected CdrCollectorInputData CdrCollectorInputData { get; }
        protected PartnerEntities Context => this.CdrCollectorInputData.CdrJobInputData.Context;
        public BlockingCollection<cdr> NonPartialCdrs { get; } = new BlockingCollection<cdr>();

        protected BlockingCollection<cdrpartialrawinstance> RawPartialCdrInstances { get; } =
            new BlockingCollection<cdrpartialrawinstance>();

        public BlockingCollection<PartialCdrContainer> PartialCdrContainers { get; set; } =
            new BlockingCollection<PartialCdrContainer>();

        public BlockingCollection<cdrinconsistent> InconsistentCdrs { get; }
            = new BlockingCollection<cdrinconsistent>();

        public CdrSetting CdrSetting => this.CdrCollectorInputData.CdrSetting;
        public bool IsEmpty => (this.InconsistentCdrs.Any() == false && this.NonPartialCdrs.Any() == false
                                && this.RawPartialCdrInstances.Any() == false);

        public abstract void GetCollectionResults(out CdrCollectionResult newCollectionResult,
            out CdrCollectionResult oldCollectionResult);

        protected abstract List<CdrExt> CreateNewCdrExts();
        protected abstract List<CdrExt> CreateOldCdrExts();

        protected AbstractCdrJobPreProcessor(CdrCollectorInputData cdrCollectorInputData, int rawCount,
            List<cdrinconsistent> inconsistentCdrs)
        {
            this.RawCount = rawCount;
            this.CdrCollectorInputData = cdrCollectorInputData;
            inconsistentCdrs.ForEach(inconsistentCdr => this.InconsistentCdrs.Add(inconsistentCdr));
        }

        protected void LoadPrevAccountingInfoForSuccessfulCdrExts(List<CdrExt> oldSuccessfulCdrExts)
        {
            if (!oldSuccessfulCdrExts.Any()) return;
            PrevAccountingInfoPopulator prevAccountingInfoPopulator =
                new PrevAccountingInfoPopulator(this.Context, oldSuccessfulCdrExts);
            prevAccountingInfoPopulator.PopulatePreviousChargeables();
            prevAccountingInfoPopulator.PopulatePreviousTransactions();
        }
        protected void VerifyPrevAccountingInfoCollection(List<CdrExt> oldCdrExts, decimal fractionComparisonTollerence)
        {
            if (!oldCdrExts.Any()) return;
            VerifyPrevTransactionsCollectionStatus(oldCdrExts,fractionComparisonTollerence);
            VerifyPrevChargeablesCollectionStatus(oldCdrExts, fractionComparisonTollerence);
        }
        private void VerifyPrevTransactionsCollectionStatus(List<CdrExt> oldCdrExts,decimal fractionComparisonTollerence)
        {
            oldCdrExts.ForEach(c =>
            {
                if(Math.Abs(Convert.ToDecimal(c.Cdr.TransactionMetaTotal)-
                c.AccWiseTransactionContainers.Values.SelectMany(transContainer=>transContainer.OldTransactions)
                    .Sum(t=>t.amount))
                    >fractionComparisonTollerence)
                    throw new Exception("Sum of old transactions does not match meta data from cdr.");
            });
        }
        private void VerifyPrevChargeablesCollectionStatus(List<CdrExt> oldCdrExts, decimal fractionComparisonTollerence)
        {
            oldCdrExts.ForEach(c =>
            {
                if (Math.Abs(Convert.ToDecimal(c.Cdr.ChargeableMetaTotal) -
                             c.Chargeables.Values.Sum(chargeable => chargeable.BilledAmount))
                    > fractionComparisonTollerence)
                    throw new Exception("Sum of old chargeables does not match meta data from cdr.");
            });
        }
    }
}
