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

namespace FlexValidation
{
    public class MefValidator<T>
    {
        [ImportMany(typeof(IValidationRule<>))]
        public IEnumerable<IValidationRule<T>> ValidationRules { get; set; }

        public bool ContinueOnError { get; set; }
        public bool ThrowExceptionOnFirstError { get; set; }
        private readonly List<IValidationRule<T>> rules = new List<IValidationRule<T>>();

        public MefValidator()
        {
        } //Empty constructor for json deserialization

        [JsonConstructor]
        public MefValidator(bool continueOnError, bool throwExceptionOnFirstError,
            string pathToMefRulesDir) //constructor
        {
            this.ContinueOnError = continueOnError;
            this.ThrowExceptionOnFirstError = throwExceptionOnFirstError;
            this.ComposeFromPath(pathToMefRulesDir);
            foreach (var validationRule in this.ValidationRules)
            {
                this.rules.Add(validationRule);
            }
        }

        private void ComposeFromPath(string path)
        {
            var catalog = new DirectoryCatalog(path);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

        public virtual ValidationResult Validate(T instance)
        {
            if (this.rules.Any() == false)
            {
                throw new Exception("No rules found.");
            }
            var validationResult = new ValidationResult();
            foreach (IValidationRule<T> flexRule in this.rules)
            {
                bool isValid = (bool) flexRule.Validate(instance);
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
    }
}