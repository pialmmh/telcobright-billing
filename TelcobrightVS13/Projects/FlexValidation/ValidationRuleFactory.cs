using System.Collections.Generic;
using FlexValidation;

namespace TelcobrightMediation.Config
{
    public class ValidationRuleFactory : IValidationRuleFactory
    {
        public ValidationRule CreateValidationRule(KeyValuePair<string, string> validationExpressionsWithMessages)
        {
            return new ValidationRule(validationExpressionsWithMessages);
        }
    }
}