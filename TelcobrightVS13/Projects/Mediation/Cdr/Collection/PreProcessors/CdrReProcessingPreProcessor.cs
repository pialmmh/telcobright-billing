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
    public class CdrReProcessingPreProcessor : AbstractCdrReProcessingPreProcessor
    {
        public CdrReProcessingPreProcessor(CdrCollectorInputData cdrCollectorInputData, List<cdr> finalCdrs)
            : base(cdrCollectorInputData, finalCdrs){}
        public override void GetCollectionResults(out CdrCollectionResult newCollectionResult,
            out CdrCollectionResult oldCollectionResult)
        {
            newCollectionResult = CreateNewCollectionResult();
            oldCollectionResult = CreateOldCollectionResult();
        }
        protected override CdrCollectionResult CreateOldCollectionResult()
        {
            List<CdrExt> oldCdrExts = this.CreateOldCdrExts();
            if (oldCdrExts.GroupBy(c => c.UniqueBillId).Any(g => g.Count() > 1))
                throw new Exception("Duplicate billId for Old CdrExts in CdrJob");
            var allIdCalls = oldCdrExts.Select(c => c.Cdr.IdCall).ToList();
            if (allIdCalls.GroupBy(i => i).Any(g => g.Count() > 1))
            {
                throw new Exception("Duplicate idcalls for CdrExts in CdrJob");
            }
            List<CdrExt> successfulOldCdrExts = oldCdrExts.Where(c => c.Cdr.ChargingStatus == 1).ToList();
            if (successfulOldCdrExts.Any())
            {
                base.LoadPrevAccountingInfoForSuccessfulCdrExts(successfulOldCdrExts);
                base.VerifyPrevAccountingInfoCollection(successfulOldCdrExts,
                    base.CdrSetting.FractionalNumberComparisonTollerance);
            }
            var emptyCdrInconsistents = new List<cdrinconsistent>();
            var oldCollectionResult = new CdrCollectionResult(base.CdrCollectorInputData.Ne,
                oldCdrExts, emptyCdrInconsistents, base.RawCount, new List<string[]>());
            return oldCollectionResult;
        }
        protected override List<CdrExt> CreateOldCdrExts()
        {
            return this.NonPartialCdrs
                .Select(cdr => 
                        CdrExtFactory.CreateCdrExtWithNonPartialOrFinalInstance(
                        cdr: new IcdrImplConverter<cdr>().Convert(CdrConversionUtil.Clone(cdr))
                        , treatCdrAsNewOldType: CdrNewOldType.OldCdr)
                ).ToList();
        }
    }
}
