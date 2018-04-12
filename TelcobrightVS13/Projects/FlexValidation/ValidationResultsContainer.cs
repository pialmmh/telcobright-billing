using System;
using System.Collections.Generic;
using System.Linq;

namespace FlexValidation
{
    public class ValidationResult
    {
        public override string ToString() => this._validationStatus.ToString();
        private readonly List<ValidationClause> _validationClauses= new List<ValidationClause>();
        private ValidationStatusEnum _validationStatus=ValidationStatusEnum.NotSet;
        public string FirstValidationFailureMessage { get; private set; } = string.Empty;
        public string LastValidationFailureMessage { get; private set; } = string.Empty;
        public List<string> AllValidationFailureMessages()
        {
            return this._validationClauses.Select(c => c.ValidationMessage).ToList();
        }
        public string GetConcatedFailureMessages()
        {
            return string.Join("," + Environment.NewLine, this._validationClauses.Select(c => c.ValidationMessage));
        }
        public bool IsValid
        {
            get
            {
                if (this._validationStatus == ValidationStatusEnum.NotSet)
                {
                    throw new Exception("Validation status is not set.");
                }
                return this._validationStatus == ValidationStatusEnum.Valid ? true : false;
            }
        }
        public void AppendValidationClause(ValidationClause validationClause)
        {
            if (validationClause.IsValid == false) 
            {
                if (this.FirstValidationFailureMessage == string.Empty)
                {
                    this.FirstValidationFailureMessage = validationClause.ValidationMessage;
                }
                this.LastValidationFailureMessage = validationClause.ValidationMessage;
                this._validationStatus = ValidationStatusEnum.Invalid;
            }
            else if (validationClause.IsValid == true && this._validationStatus == ValidationStatusEnum.NotSet)
            {
                this._validationStatus=ValidationStatusEnum.Valid;
            }
            this._validationClauses.Add(validationClause);
        }

    }
}