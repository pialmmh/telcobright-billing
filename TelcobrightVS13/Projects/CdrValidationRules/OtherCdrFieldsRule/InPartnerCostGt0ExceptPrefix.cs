using System;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation;
using System.Linq;
using System.Collections.Generic;

namespace CdrValidationRules
{
    [Export(typeof(IValidationRule<>))]
    public class InPartnerCostGt0ExceptPrefix : IValidationRule<cdr>
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string ValidationMessage => 
            $@"InPartnerCost must be > 0 when DurationSec >= {this.minDurationSec}";
        public object Data { get; set; }
        private decimal minDurationSec = 0;
        private List<string> prefixesToExclude { get; set; }
        public bool IsPrepared { get; private set; }
        public void Prepare()
        {
            
            Dictionary<string, object> data = (Dictionary<string, object>)this.Data;
            this.minDurationSec = Convert.ToDecimal(data["minDurationSec"]);
            this.prefixesToExclude = (List<string>)data["prefixesToExclude"];
            this.IsPrepared = true;
        }
        public bool Validate(cdr obj)
        {
            if (this.IsPrepared == false)
                throw new Exception("Rule is not prepared, method Prepare must be called first.");
            if (prefixesToExclude.Any(prefix => obj.OriginatingCalledNumber.StartsWith(prefix)))
            {
                return true;
            }
            return obj.DurationSec >= this.minDurationSec ? obj.InPartnerCost > 0 : obj.InPartnerCost == 0;
        }
    }
}
