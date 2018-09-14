using System;
using System.Collections.Generic;
using System.Linq;
using MediationModel;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Mediation.Cdr;

namespace TelcobrightMediation
{
    public abstract class AbstractCdrReProcessingPreProcessor : AbstractCdrJobPreProcessor
    {
        protected AbstractCdrReProcessingPreProcessor(CdrCollectorInputData cdrCollectorInputData, List<cdr> finalCdrs)
            : base(cdrCollectorInputData, finalCdrs.Count, new List<cdrinconsistent>())
        {
            finalCdrs.ForEach(c => base.NonPartialCdrs.Add(c));
        }
        protected override CdrCollectionResult CreateNewCollectionResult()
        {
            List<CdrExt> newCdrExts = this.CreateNewCdrExts();
            if (newCdrExts.GroupBy(c => c.UniqueBillId).Any(g => g.Count() > 1))
                throw new Exception("Duplicate billId for CdrExts in CdrJob");
            var allIdCalls = newCdrExts.Select(c => c.Cdr.IdCall).ToList();
            if (allIdCalls.GroupBy(i => i).Any(g => g.Count() > 1))
            {
                throw new Exception("Duplicate idcalls for CdrExts in CdrJob");
            }
            var emptyCdrInconsistents = new List<cdrinconsistent>();
            var newCollectionResult = new CdrCollectionResult(base.CdrCollectorInputData.Ne,
                newCdrExts, emptyCdrInconsistents, base.RawCount);
            return newCollectionResult;
        }
        protected override List<CdrExt> CreateNewCdrExts()
        {
            List<CdrExt> newCdrExts = new List<CdrExt>();
            Func<cdr, cdr> metaDataRemoverInCaseOfReProcessing = oldCdr =>
            {
                oldCdr.TransactionMetaTotal = 0;
                oldCdr.ChargeableMetaTotal = 0;
                oldCdr.SummaryMetaTotal = 0;
                return oldCdr;
            };
            base.NonPartialCdrs.ToList().ForEach(nonPartialCdr =>
            {
                var clonedCdr = new IcdrImplConverter<cdr>().Convert(CdrConversionUtil.Clone(nonPartialCdr));
                clonedCdr = metaDataRemoverInCaseOfReProcessing.Invoke(clonedCdr);
                newCdrExts.Add(CdrExtFactory.CreateCdrExtWithNonPartialOrFinalInstance(cdr: clonedCdr,
                    treatCdrAsNewOldType: CdrNewOldType.NewCdr));
            });
            return newCdrExts;
        }
    }
}