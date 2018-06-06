using System;
using System.ComponentModel.Composition;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation;

namespace CdrValidationRules
{
    [Export(typeof(IValidationRule<>))]
    public class StrDurationSecGtEq0 : IValidationRule<string[]>
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string ValidationMessage => "DurationSec must be Greater than or Equal to 0";
        public object Data { get; set; }
        public bool IsPrepared { get; private set; }

        public void Prepare()
        {
            this.IsPrepared = true;
        }

        public bool Validate(string[] obj)
        {
            if (this.IsPrepared == false)
                throw new Exception("Rule is not prepared, method Prepare must be called first.");
            double durationSec = 0;
            if (double.TryParse(obj[14], out durationSec)==true)
            {
                return durationSec >= 0;
            }
            return false;
        }
    }
}
