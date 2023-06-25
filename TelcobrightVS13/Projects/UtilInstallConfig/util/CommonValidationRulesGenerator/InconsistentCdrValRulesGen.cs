using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CdrValidationRules;
using LibraryExtensions;
using TelcobrightMediation;

namespace InstallConfig._CommonValidation
{
    public class InconsistentCdrValRulesGen
    {
        private DateTime NotAllowedCallDateTimeBefore { get; }

        public InconsistentCdrValRulesGen(DateTime notAllowedCallDateTimeBefore)
        {
            this.NotAllowedCallDateTimeBefore = notAllowedCallDateTimeBefore;
        }

        public virtual List<IValidationRule<string[]>> GetRules()
        {
            return new List<IValidationRule<string[]>>()
            {
                new StrUniqueBillidNotEmpty(),
                new StrSeqNumGreaterThanZero(),
                new StrIncomingRouteNotEmpty(),
                
                new StrOriginatingCalledNumberNotEmpty(),
                new StrDurationSecGtEq0(),
                new StrStartTimeIsValidAndWithinLimit()
                {
                    Data = this.NotAllowedCallDateTimeBefore
                },
                new StrEndTimeIsGtEqStartTime(),
                new StrValidFlagMustBe1()
            };
        }
    }
}
