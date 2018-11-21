using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation;

namespace CdrRules
{

    [Export("CdrRule", typeof(ICdrRule))]
    public class IcnlIntlIncomingByTgType : ICdrRule
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "International Incoming call identifier for ICNL";
        public int Id => 5;
        public object Data { get; set; }
        public bool IsPrepared { get; private set; }
        public void Prepare(object input)
        {
            MediationContext mediationContext = (MediationContext) input;
            this.Data = mediationContext.Routes;
            this.SwitchWiseRoutes = (Dictionary<ValueTuple<int, string>, route>) this.Data;
            this.IsPrepared = true;
        }
        public Dictionary<ValueTuple<int, string>, route> SwitchWiseRoutes { get; set; }
        public bool CheckIfTrue(cdr thisCdr)
        {
            if (this.IsPrepared == false)
                throw new Exception("Rule is not prepared, method Prepare needs to be called first.");
            ValueTuple<int, string> key = new ValueTuple<int, string>(thisCdr.SwitchId, thisCdr.IncomingRoute);
            route inRoute = null;
            this.SwitchWiseRoutes.TryGetValue(key, out inRoute);
            key = new ValueTuple<int, string>(thisCdr.SwitchId, thisCdr.IncomingRoute);
            route outRoute = null;
            this.SwitchWiseRoutes.TryGetValue(key, out outRoute);
            if (outRoute != null)
            {
                return outRoute.NationalOrInternational == RouteLocalityType.International;
            }
            return false;
        }
    }
}
