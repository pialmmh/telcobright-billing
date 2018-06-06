using System;
using System.ComponentModel.Composition;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation;

namespace CdrValidationRules
{
    [Export(typeof(IValidationRule<>))]
    public class StartTimeIsValidAndWithinLimit : IValidationRule<cdr>
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string ValidationMessage => "StartTime must be > " +
                                           this.MinAllowedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        public object Data { get; set; }
        private DateTime MinAllowedDateTime { get; set; }
        public bool IsPrepared { get; private set; }

        public void Prepare()
        {
            this.MinAllowedDateTime = (DateTime)this.Data;
            this.IsPrepared = true;
        }

        public bool Validate(cdr obj)
        {
            if (this.IsPrepared == false)
                throw new Exception("Rule is not prepared, method Prepare must be called first.");
            return obj.StartTime > this.MinAllowedDateTime;
        }
    }
}

