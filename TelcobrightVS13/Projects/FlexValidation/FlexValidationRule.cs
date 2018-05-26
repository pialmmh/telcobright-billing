using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Spring.Expressions;

namespace FlexValidation
{
    public class FlexValidationRule:IValidationRule
    {

        protected IExpression ParsedExpression { get; set; }
        public string ValidationMessage { get; set; }
        protected bool IsParsed { get; set; }
        public FlexValidationRule(KeyValuePair<string, string> validationExpressionsWithErrorMessage)
        {
            this.ParsedExpression = Expression.Parse(validationExpressionsWithErrorMessage.Key);
            this.ValidationMessage = validationExpressionsWithErrorMessage.Value;
            this.IsParsed = true;
        }
        public bool Validate(object validatableObject)
        {
            if (this.IsParsed == false) throw new Exception("Expressions must be parsed before use for faster performance.");
            bool isValid = (bool)this.ParsedExpression.GetValue(validatableObject, null);
            return isValid;
        }
    }
}
