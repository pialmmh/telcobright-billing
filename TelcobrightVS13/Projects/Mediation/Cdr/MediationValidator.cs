using System;
using System.Globalization;
using System.Linq;
using System.Text;
using LibraryExtensions;
using MediationModel;
using FlexValidation;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    public static class MediationValidator
    {
        public static string ExecuteRules(CdrExt cdrExt, CdrJobContext cdrJobContext)
        {
            var validator = cdrJobContext.MediationContext.CommonMediationCheckListValidator; //common checklist
            foreach (IValidationRule<cdr> rule in validator.Rules)
            {
                if (rule.Validate(cdrExt.Cdr) == false) return rule.ValidationMessage;
            }
            var sgData = cdrJobContext.MediationContext.MefServiceGroupContainer;
            int idServiceGroup = cdrExt.Cdr.ServiceGroup;
            if (idServiceGroup <= 1) return "ServiceGroup must be > 0";
            if (cdrExt.MediationResult.ChargingStatus == 0) //if still true, check against answered checklist
            {
                sgData.MediationChecklistValidatorForUnAnsweredCdrs.TryGetValue(idServiceGroup, out validator);
                foreach (IValidationRule<cdr> rule in validator.Rules)
                {
                    if (rule.Validate(cdrExt.Cdr) == false) return rule.ValidationMessage;
                }
            }

            if (cdrExt.MediationResult.ChargingStatus == 1) //if still true, check against answered checklist
            {
                sgData.MediationChecklistValidatorForAnsweredCdrs.TryGetValue(idServiceGroup, out validator);
                foreach (IValidationRule<cdr> rule in validator.Rules)
                {
                    if (rule.Validate(cdrExt.Cdr) == false) return rule.ValidationMessage;
                }
            }
            return string.Empty;
        }
    }
}
