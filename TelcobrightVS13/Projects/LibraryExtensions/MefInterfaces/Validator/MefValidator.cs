using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using MediationModel;
using Newtonsoft.Json;
using Spring.Context.Attributes;
using Spring.Core.TypeResolution;
using TelcobrightMediation;

namespace LibraryExtensions
{
    public class MefValidator<T>
    {
        [ImportMany(typeof(IValidationRule<>))]
        public List<IValidationRule<T>> Rules { get;}
        public bool ContinueOnError { get; set; }
        public bool ThrowExceptionOnFirstError { get; set; }
        public MefValidator()
        {
        } //Empty constructor for json deserialization

        [JsonConstructor]
        public MefValidator(bool continueOnError, bool throwExceptionOnFirstError,
            List<IValidationRule<T>> rules) //constructor
        {
            this.ContinueOnError = continueOnError;
            this.ThrowExceptionOnFirstError = throwExceptionOnFirstError;
            this.Rules = rules;
            this.Rules.ForEach(r=>r.Prepare());
        }

        public virtual ValidationResult Validate(T objToValidate)
        {
            if (this.Rules.Any() == false)
            {
                throw new Exception("No rules found.");
            }
            var validationResult = new ValidationResult();
            foreach (IValidationRule<T> rule in this.Rules)
            {
                bool isValid = rule.Validate(objToValidate);
                if (isValid == false)
                {
                    validationResult.AppendValidationClause(new ValidationClause(false, rule.ValidationMessage));
                    if (this.ThrowExceptionOnFirstError == true)
                    {
                        throw new Exception(rule.ValidationMessage);
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
    }
}