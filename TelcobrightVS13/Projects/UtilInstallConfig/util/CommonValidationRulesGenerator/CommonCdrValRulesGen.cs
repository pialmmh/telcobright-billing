using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CdrValidationRules;
using CdrValidationRules.CdrValidationRules.CommonCdrValidationRules;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation;

namespace InstallConfig._CommonValidation
{
    public class CommonCdrValRulesGen
    {
        private DateTime NotAllowedCallDateTimeBefore { get; }

        public CommonCdrValRulesGen(DateTime notAllowedCallDateTimeBefore)
        {
            this.NotAllowedCallDateTimeBefore = notAllowedCallDateTimeBefore;
        }

        public virtual List<IValidationRule<cdr>> GetRules()
        {
            return new List<IValidationRule<cdr>>()
            {
                new UniqueBillIdNonEmpty(),
                new SeqNumGreaterThanZero(),
                new PartialFlagGtEq0(),
                new IncomingRouteNotEmpty(),
                new OriginatingCalledNumberNotEmpty(),
                new DurationSecGtEq0(),
                new StartTimeIsValidAndWithinLimit()
                {
                    Data = this.NotAllowedCallDateTimeBefore
                },
                new EndTimeIsGtEqStartTime(),
                new ValidFlagGt0(),
                new SwitchIdGt0(),
                new IdCallGt0(),
                new FileNameNotEmpty(),
                new InPartnerIdGt0(),
                //new FinalRecordMustBe1(),
                new ChargingStatus1WhenDurationGt0(),
             };
        }
    }
}
