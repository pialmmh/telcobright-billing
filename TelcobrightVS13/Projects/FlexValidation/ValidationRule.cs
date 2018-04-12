using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Spring.Expressions;

namespace FlexValidation
{
    public class ValidationRule
    {
        protected IExpression ParsedExpression { get; set; }
        public string ValidationMessage { get; set; }
        protected bool IsParsed { get; set; }
        public ValidationRule(KeyValuePair<string, string> validationExpressionsWithErrorMessage)
        {
            this.ParsedExpression = Expression.Parse(validationExpressionsWithErrorMessage.Key);
            this.ValidationMessage = validationExpressionsWithErrorMessage.Value;
            this.IsParsed = true;
        }
        public IExpression GetParsedExpression()
        {
            if (this.IsParsed == false) throw new Exception("Experssions must be parsed before use for faster performance.");
            return this.ParsedExpression;
        }
    }
}
