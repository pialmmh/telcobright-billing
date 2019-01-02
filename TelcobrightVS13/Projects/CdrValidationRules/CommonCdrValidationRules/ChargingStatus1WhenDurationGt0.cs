using System;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation;

namespace CdrValidationRules
{
    [Export(typeof(IValidationRule<>))]
    public class ChargingStatus1WhenDurationGt0 : IValidationRule<cdr>
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string ValidationMessage => "ChargingStatus must be 1 when DurationSec Greater than zero";
        public object Data { get; set; }
        public bool IsPrepared { get; private set; }

        public void Prepare()
        {
            this.IsPrepared = true;
        }

        public bool Validate(cdr obj)
        {
            if (this.IsPrepared == false)
                throw new Exception("Rule is not prepared, method Prepare must be called first.");
            return obj.DurationSec > 0 ? obj.ChargingStatus == 1 
                : (obj.ChargingStatus == 0 || obj.ChargingStatus == 1);
        }
    }
}
