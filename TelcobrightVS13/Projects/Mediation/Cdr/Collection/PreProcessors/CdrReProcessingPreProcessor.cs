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

        public override void GetCollectionResults(out CdrCollectionResult newCollectionResult,
            out CdrCollectionResult oldCollectionResult)
        {
            newCollectionResult = CreateNewCollectionResult();
            oldCollectionResult = CreateOldCollectionResult();
        }

        protected override CdrCollectionResult CreateNewCollectionResult()
        {
            List<CdrExt> newCdrExts = this.CreateNewCdrExts();
            if (newCdrExts.GroupBy(c => c.UniqueBillId).Any(g => g.Count() > 1))
                throw new Exception("Duplicate billId for CdrExts in CdrJob");
            var emptyCdrInconsistents = new List<cdrinconsistent>();
            var newCollectionResult = new CdrCollectionResult(base.CdrCollectorInputData.Ne,
                newCdrExts, emptyCdrInconsistents, base.RawCount);
            return newCollectionResult;
        }

        protected override CdrCollectionResult CreateOldCollectionResult()
        {
            List<CdrExt> oldCdrExts = this.CreateOldCdrExts();
            if (oldCdrExts.GroupBy(c => c.UniqueBillId).Any(g => g.Count() > 1))
                throw new Exception("Duplicate billId for Old CdrExts in CdrJob");

            List<CdrExt> successfulOldCdrExts = oldCdrExts.Where(c => c.Cdr.ChargingStatus == 1).ToList();
            if (successfulOldCdrExts.Any())
            {
                base.LoadPrevAccountingInfoForSuccessfulCdrExts(successfulOldCdrExts);
                base.VerifyPrevAccountingInfoCollection(successfulOldCdrExts,
                    base.CdrSetting.FractionalNumberComparisonTollerance);
            }
            var emptyCdrInconsistents = new List<cdrinconsistent>();
            var oldCollectionResult = new CdrCollectionResult(base.CdrCollectorInputData.Ne,
                oldCdrExts, emptyCdrInconsistents, base.RawCount);
            return oldCollectionResult;
        }

        protected override List<CdrExt> CreateNewCdrExts()
        {
            List<CdrExt> newCdrExts = new List<CdrExt>();
            Func<cdr,cdr> metaDataRemoverInCaseOfReProcessing = oldCdr =>
            {
                oldCdr.TransactionMetaTotal = 0;
                oldCdr.ChargeableMetaTotal = 0;
                oldCdr.SummaryMetaTotal = 0;
                return oldCdr;
            };
            base.NonPartialCdrs.ToList().ForEach(nonPartialCdr =>
            {
                var clonedCdr = new IcdrImplConverter<cdr>().Convert(CdrManipulatingUtil.Clone(nonPartialCdr));
                clonedCdr = metaDataRemoverInCaseOfReProcessing.Invoke(clonedCdr);
                newCdrExts.Add(CdrExtFactory.CreateCdrExtWithNonPartialCdr(cdr: clonedCdr,
                    treatCdrAsNewOldType: CdrNewOldType.NewCdr));
            });
            return newCdrExts;
        }

        protected override List<CdrExt> CreateOldCdrExts()
        {
            return this.NonPartialCdrs
                .Select(cdr => 
                        CdrExtFactory.CreateCdrExtWithNonPartialCdr(
                        cdr: new IcdrImplConverter<cdr>().Convert(CdrManipulatingUtil.Clone(cdr))
                        , treatCdrAsNewOldType: CdrNewOldType.OldCdr)
                ).ToList();
        }
    }
}
