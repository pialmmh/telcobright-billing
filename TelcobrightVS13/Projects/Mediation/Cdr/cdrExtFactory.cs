using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using TelcobrightMediation.Mediation.Cdr;

namespace TelcobrightMediation.Cdr
{
    public static class CdrExtFactory
    {
        public static CdrExt CreateCdrExtWithNonPartialCdr(cdr cdr, CdrNewOldType treatCdrAsNewOldType)
        {
            if (cdr.PartialFlag != 0) throw new Exception("Non partial cdr must have partial flag set to 0.");
            if (cdr.FinalRecord != 1)
                throw new Exception("Partial cdr equivalent instance must have final record set to 1.");
            return new CdrExt(cdr, treatCdrAsNewOldType)
            {
                PartialCdrContainer = null,
                MediationResult = new CdrMediationResult(cdr)
            };
        }

        public static CdrExt CreateCdrExtWithPartialCdrContainer(PartialCdrContainer partialCdrContainer,
            CdrNewOldType cdrNewOldType)
        {
            if (partialCdrContainer == null
                || partialCdrContainer.NewCdrEquivalent == null)
            {
                throw new Exception(
                    "Either partialCdrContainer or NewMediatableInstance is null, both should be set for partial cdrs.");
            }
            switch (cdrNewOldType)
            {
                case CdrNewOldType.NewCdr:
                    if (partialCdrContainer.NewCdrEquivalent.FinalRecord != 1)
                        throw new Exception("Partial cdr equivalent instance must have final record set to 1.");
                    if (partialCdrContainer.NewCdrEquivalent.PartialFlag <=0)
                        throw new Exception("Partial cdr equivalent instance must have partial flag >= 1.");
                    return new CdrExt(partialCdrContainer.NewCdrEquivalent, CdrNewOldType.NewCdr)
                    {
                        PartialCdrContainer = partialCdrContainer,
                        MediationResult = new CdrMediationResult(partialCdrContainer.NewCdrEquivalent)
                    };
                case CdrNewOldType.OldCdr:
                    if (partialCdrContainer.PrevProcessedCdrInstance == null)
                    {
                        throw new Exception(
                            "PrevProcessedInstance of partial cdr is null, old partial ext instance cannot be created.");
                    }
                    if (partialCdrContainer.PrevProcessedCdrInstance.FinalRecord != 1)
                        throw new Exception(
                            "Prev processed instance of partial cdr must have final record set to 1.");
                    if (partialCdrContainer.PrevProcessedCdrInstance.PartialFlag < 1)
                        throw new Exception(
                            "Prev processed instance for partial cdr must have partial flag >= 1.");
                    cdr clonedCdr =new IcdrImplConverter<cdr>().Convert(
                            CdrManipulatingUtil.Clone(partialCdrContainer.PrevProcessedCdrInstance));
                    return new CdrExt(clonedCdr, CdrNewOldType.OldCdr)
                    {
                        MediationResult = new CdrMediationResult(partialCdrContainer.PrevProcessedCdrInstance)
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(cdrNewOldType), cdrNewOldType, null);
            }
        }
    }
}
