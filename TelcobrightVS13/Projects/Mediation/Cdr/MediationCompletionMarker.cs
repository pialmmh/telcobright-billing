using System;
using System.Globalization;
using System.Linq;
using System.Text;
using LibraryExtensions;
using MediationModel;
using FlexValidation;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.EntityHelpers;

namespace TelcobrightMediation
{
    public static class MediationErrorChecker
    {
        public static ValidationResult ExecuteValidationRules(CdrExt cdrExt, CdrJobContext cdrJobContext)
        {
            FlexValidator<cdr> validator =
                cdrJobContext.MediationContext.CommonMediationCheckListValidator; //common checklist
            ValidationResult validationResult = null;
            validationResult = validator.Validate(cdrExt.Cdr);
            if (validationResult.IsValid == false) return validationResult;

            var sgData = cdrJobContext.MediationContext.MefServiceGroupContainer;
            int idServiceGroup = sgData.IdServiceGroupWiseServiceGroups[cdrExt.Cdr.ServiceGroup].Id;
            if (cdrExt.MediationResult.ChargingStatus == 0) //if still true, check against answered checklist
            {
                validator = sgData.MediationChecklistValidatorForUnAnsweredCdrs[idServiceGroup];
                if (validator.GetFlexRules().Any()) validationResult = validator.Validate(cdrExt.Cdr);
                if (validationResult.IsValid == false) return validationResult;
            }

            if (cdrExt.MediationResult.ChargingStatus == 1) //if still true, check against answered checklist
            {
                validator = sgData.MediationChecklistValidatorForAnsweredCdrs[idServiceGroup];
                if (validator.GetFlexRules().Any()) validationResult = validator.Validate(cdrExt.Cdr);
                else throw new Exception("No validation rules are specified for successful calls.");
                if (validationResult.IsValid == false) return validationResult;
            }
            return validationResult;
        }
    }
}
