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
        protected BlockingCollection<cdr> NonPartialCdrs { get; } = new BlockingCollection<cdr>();
        protected BlockingCollection<cdrpartialrawinstance> RawPartialCdrInstances { get; } =
            new BlockingCollection<cdrpartialrawinstance>();
        protected BlockingCollection<PartialCdrContainer> PartialCdrContainers { get; set; } =
            new BlockingCollection<PartialCdrContainer>();
        public BlockingCollection<cdrinconsistent> InconsistentCdrs { get; }
            = new BlockingCollection<cdrinconsistent>();
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

        protected virtual void PopulatePrevAccountingInfo(List<CdrExt> oldCdrExts)
        {
            if (!oldCdrExts.Any()) return;
            PrevAccountingInfoPopulator prevAccountingInfoPopulator =
                new PrevAccountingInfoPopulator(this.Context, oldCdrExts);
            prevAccountingInfoPopulator.PopulatePreviousChargeables();
            prevAccountingInfoPopulator.PopulatePreviousTransactions();
        }
    }
}
