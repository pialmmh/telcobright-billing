using FluentValidation;
using MediationModel;

namespace Utils
{
    public class CdrFieldListValidatorRuleSetBased : AbstractValidator<cdrfieldlist>
    {
        public CdrFieldListValidatorRuleSetBased()
        {
            RuleSet("rsFieldName", () =>
            {
                RuleFor(c => c.FieldName).NotEmpty();
            });
            RuleSet("rsIsNumeric", () =>
            {
                RuleFor(c => c.IsNumeric).GreaterThan(0);
            });
            RuleSet("rsFieldNumber", () =>
            {
                When(c => c.fieldnumber > 0, () =>
                {
                    RuleFor(c => c.IsDateTime).GreaterThanOrEqualTo(2);
                });
            });

        }
    }
}