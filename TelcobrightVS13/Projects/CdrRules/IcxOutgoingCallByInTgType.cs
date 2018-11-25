using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation;

namespace CdrRules
{

    [Export("CdrRule", typeof(ICdrRule))]
    public class IcxOutgoingCallByInTgType : ICdrRule
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "Outgoing call identifier by national/international trunk group of ANS";
        public int Id => 1;
        public object Data { get; set; }
        public bool IsPrepared { get; private set; }
        public Dictionary<ValueTuple<int, string>, route> SwitchWiseRoutes;

        public void Prepare(object input)
        {
            MediationContext mediationContext = (MediationContext)input;
            this.Data = mediationContext.Routes;
            this.SwitchWiseRoutes = (Dictionary<ValueTuple<int, string>, route>) this.Data;
            this.IsPrepared = true;
        }

        public bool CheckIfTrue(cdr thisCdr)
        {
            if(this.IsPrepared==false) throw new Exception("Rule is not prepared, method Prepare needs to be called first.");
            var dicRoutes = this.SwitchWiseRoutes;
            var key = new ValueTuple<int, string>(thisCdr.SwitchId, thisCdr.IncomingRoute);
            route thisRoute = null;
            dicRoutes.TryGetValue(key, out thisRoute);
            return thisRoute?.partner.PartnerType == IcxPartnerType.ANS
                   && thisRoute.NationalOrInternational == RouteLocalityType.International;
        }
    }
}
