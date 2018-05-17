using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;

namespace PartnerRules
{

    [Export("CdrRule", typeof(ICdrRule))]
    public class IcxOutgoingCallByInOutTg : ICdrRule
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "Outgoing call identifier by ANS & IOS TG";
        public int Id => 2;
        public object Data { get; set; }
        public bool IsPrepared { get; private set; }
        public void Prepare()
        {
            this.SwitchWiseRoutes = this.Data as Dictionary<ValueTuple<int, string>, route>;
            this.IsPrepared = true;
        }
        public Dictionary<ValueTuple<int, string>, route> SwitchWiseRoutes { get; set; }
        public bool CheckIfTrue(cdr thisCdr)
        {
            if (this.IsPrepared == false)
                throw new Exception("Rule is not prepared, method Prepare needs to be called first.");
            ValueTuple<int, string> key = new ValueTuple<int,string>(thisCdr.SwitchId, thisCdr.IncomingRoute);
            route inRoute = null;
            this.SwitchWiseRoutes.TryGetValue(key, out inRoute);
            if (inRoute?.partner.PartnerType == IcxPartnerType.ANS)
            {
                key = new ValueTuple<int, string>(thisCdr.SwitchId, thisCdr.OutgoingRoute);
                route outRoute = null;
                this.SwitchWiseRoutes.TryGetValue(key, out outRoute);
                return outRoute?.partner.PartnerType == IcxPartnerType.IOS;
            }
            return false;
        }
    }
}
