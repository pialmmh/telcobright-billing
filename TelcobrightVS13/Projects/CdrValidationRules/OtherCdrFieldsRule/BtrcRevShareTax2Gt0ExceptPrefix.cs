using System;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation;
using System.Collections.Generic;
using System.Linq;

namespace CdrValidationRules
{
    [Export(typeof(IValidationRule<>))]
    public class BtrcRevShareTax2Gt0ExceptPrefix : IValidationRule<cdr>
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        private decimal minDurationSec = 0;
        private List<string> prefixesToExclude { get; set; }
        public object Data { get; set; }
        public string ValidationMessage =>
            $@"BTRC RevShare (Tax2) must be > 0 when DurationSec >= {this.minDurationSec}";
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
            return obj.DurationSec >= this.minDurationSec ? obj.Tax2 != 0 : obj.Tax2 == 0M;
        }
    }
}
