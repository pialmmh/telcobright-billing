using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LibraryExtensions;
using MediationModel;
using TelcobrightFileOperations;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Mediation.Cdr;

namespace TelcobrightMediation
{
    public class CdrReProcessingPreProcessor : AbstractCdrJobPreProcessor
    {
        public CdrReProcessingPreProcessor(CdrCollectorInputData cdrCollectorInputData, List<cdr> finalCdrs)
            : base(cdrCollectorInputData, finalCdrs.Count,new List<cdrinconsistent>())
        {
            finalCdrs.ForEach(c => base.NonPartialCdrs.Add(c));
        }

        public override void GetCollectionResults(out CdrCollectionResult newCollectionResult, out CdrCollectionResult oldCollectionResult)
        {
            List<CdrExt> newCdrExts = this.CreateNewCdrExts();
            if (newCdrExts.GroupBy(c => c.UniqueBillId).Any(g => g.Count() > 1))
                throw new Exception("Duplicate billId for CdrExts in CdrJob");
            List<CdrExt> oldCdrExts = this.CreateOldCdrExts();
            if (oldCdrExts.GroupBy(c => c.UniqueBillId).Any(g => g.Count() > 1))
                throw new Exception("Duplicate billId for Old CdrExts in CdrJob");
            base.PopulatePrevAccountingInfo(oldCdrExts);
            var emptyCdrInconsistents = new List<cdrinconsistent>();
            newCollectionResult = new CdrCollectionResult(base.CdrCollectorInputData.Ne,
                newCdrExts,emptyCdrInconsistents, base.RawCount);
            oldCollectionResult = new CdrCollectionResult(base.CdrCollectorInputData.Ne,
                oldCdrExts, emptyCdrInconsistents, base.RawCount);
        }

        protected override List<CdrExt> CreateNewCdrExts()
        {
            return this.NonPartialCdrs
                .Select(cdr => CdrExtFactory.CreateCdrExtWithNonPartialCdr(cdr, CdrNewOldType.NewCdr)).ToList();
        }

        protected override List<CdrExt> CreateOldCdrExts()
        {
            return this.NonPartialCdrs
                .Select(cdr => CdrExtFactory.CreateCdrExtWithNonPartialCdr(
                 new IcdrImplConverter<cdr>().Convert(CdrManipulatingUtil.Clone(cdr))
                 , CdrNewOldType.OldCdr)).ToList();
        }
    }
}
