using System.Collections.Generic;
using FlexValidation;
using Spring.Expressions;

namespace TelcobrightMediation.Config
{
    public class ValidationRuleBasedOnTxtRowParsing : ValidationRule
    {
        public ValidationRuleBasedOnTxtRowParsing(KeyValuePair<string, string> validationExpressionsWithErrorMessage, string[] row)
            :base(validationExpressionsWithErrorMessage)
        {
            base.ParsedExpression = Expression.Parse(validationExpressionsWithErrorMessage.Key);
            base.ValidationMessage = validationExpressionsWithErrorMessage.Value;
            base.IsParsed = true;
        }
    }
}