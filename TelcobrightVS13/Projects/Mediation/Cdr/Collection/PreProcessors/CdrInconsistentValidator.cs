using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexValidation;
using MediationModel;
using TelcobrightMediation.Mediation.Cdr;

namespace TelcobrightMediation
{
    public class CdrInconsistentValidator
    {
        public CdrJobInputData Input { get; }
        MefValidator<string[]> MefValidator { get; }

        public CdrInconsistentValidator(CdrJobInputData input, MefValidator<string[]> mefValidator)
        {
            Input = input;
            MefValidator = mefValidator;
        }
        public cdrinconsistent CheckAndConvertIfInconsistent(string[] txtRow)
        {
            ValidationResult validationResult = this.MefValidator.Validate(txtRow);
            foreach (IValidationRule<string[]> rule in this.MefValidator.Rules)
            {
                if (rule.Validate(txtRow) == false)
                {
                    txtRow[Fn.ErrorCode] = rule.ValidationMessage;
                    //base.InconsistentCdrs.Add(CdrConversionUtil.ConvertTxtRowToCdrinconsistent(txtRow));
                    var inconsistentCdr = CdrConversionUtil.ConvertTxtRowToCdrinconsistent(txtRow);
                    return inconsistentCdr;
                }
            }
            return null;
        }
    }
}
