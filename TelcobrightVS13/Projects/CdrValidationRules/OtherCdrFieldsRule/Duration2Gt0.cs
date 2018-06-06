using System;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation;

namespace CdrValidationRules
{
    [Export(typeof(IValidationRule<>))]
    public class Duration2Gt0 : IValidationRule<cdr>
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string ValidationMessage => $@"duration2 must be > 0 when DurationSec >= {this.minDurationSec}";
        public object Data { get; set; }
        private decimal minDurationSec = 0;
        public bool IsPrepared { get; private set; }

        public void Prepare()
        {
            this.minDurationSec = Convert.ToDecimal(this.Data);
            this.IsPrepared = true;
        }

        public bool Validate(cdr obj)
        {
            if (this.IsPrepared == false)
                throw new Exception("Rule is not prepared, method Prepare must be called first.");
            return obj.DurationSec >= this.minDurationSec ? obj.Duration2 > 0 : obj.Duration2 == 0;
        }
    }
}
