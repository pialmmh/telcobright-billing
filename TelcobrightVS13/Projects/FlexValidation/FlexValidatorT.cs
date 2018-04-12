using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Spring.Core.TypeResolution;

namespace FlexValidation
{
    public class FlexValidator<T>
    {
        public bool ContinueOnError { get; set; }
        public bool ThrowExceptionOnFirstError { get; set; }
        private readonly List<ValidationRule> _flexRules = new List<ValidationRule>();
        private readonly bool _isDoneParsingRules = false;
        public FlexValidator() { }//Empty constructor for json deserialization
        public Dictionary<string, Func<string, DateTime>> DateParsers { get; set; }=new Dictionary<string, Func<string, DateTime>>();

        public Dictionary<string, Func<string, int>> IntParsers { get; set; } =
            new Dictionary<string, Func<string, int>>();

        public Dictionary<string, Func<string, long>> LongParsers { get; set; } =
            new Dictionary<string, Func<string, long>>();

        public Dictionary<string, Func<string, double>> DoubleParsers { get; set; } =
            new Dictionary<string, Func<string, double>>();

        public Dictionary<string, Func<string, bool>> BooleanParsers { get; set; } =
            new Dictionary<string, Func<string, bool>>();
        [JsonConstructor]
        public FlexValidator(bool continueOnError,bool throwExceptionOnFirstError,
            Dictionary<string, string> validationExpressionsWithErrorMessage)//constructor
        {
            TypeRegistry.RegisterType("Convert", typeof(System.Convert));
            TypeRegistry.RegisterType("Parsers", typeof(FlexValidation.Parsers));
            this.ContinueOnError = continueOnError;
            this.ThrowExceptionOnFirstError = throwExceptionOnFirstError;
            if (this._isDoneParsingRules == false)
            {
                foreach (KeyValuePair<string, string> kv in validationExpressionsWithErrorMessage)
                {
                    ValidationRule flexRule = new ValidationRule(kv);
                    this._flexRules.Add(flexRule);
                }
                this._isDoneParsingRules = true;
            }
        }

        public virtual ValidationResult Validate(T instance)
        {
            if (this._isDoneParsingRules==false)
            {
                throw new Exception("Flexvalidator's rules are not parsed.");
            }
            var validationResult = new ValidationResult();
            foreach (ValidationRule flexRule in this._flexRules)
            {
                ValidatableObject<T> validatableObject=new ValidatableObject<T>(instance,this);
                bool isValid = (bool)flexRule.GetParsedExpression().GetValue(validatableObject, null);
                if (isValid == false)
                {
                    validationResult.AppendValidationClause(new ValidationClause(false, flexRule.ValidationMessage));
                    if (this.ThrowExceptionOnFirstError == true)
                    {
                        throw new Exception(flexRule.ValidationMessage);
                    }
                    if (this.ContinueOnError == false) break;
                }
                else
                {
                    validationResult.AppendValidationClause(new ValidationClause(true, string.Empty));
                }
            }
            return validationResult;
        }
        
        public virtual ValidationResult ValidateWithoutBoxing(T instance)
        {
            var validationResult = new ValidationResult();
            foreach (ValidationRule flexRule in this._flexRules)
            {
                bool isValid = (bool)flexRule.GetParsedExpression().GetValue(instance, null);
                if (isValid == false)
                {
                    validationResult.AppendValidationClause(new ValidationClause(false, flexRule.ValidationMessage));
                    if (this.ThrowExceptionOnFirstError == true)
                    {
                        throw new Exception(flexRule.ValidationMessage);
                    }
                    if (this.ContinueOnError == false) break;
                }
                else
                {
                    validationResult.AppendValidationClause(new ValidationClause(true, string.Empty));
                }
            }
            return validationResult;
        }
        public List<ValidationRule> GetFlexRules()
        {
            return this._flexRules;
        }
    }
}