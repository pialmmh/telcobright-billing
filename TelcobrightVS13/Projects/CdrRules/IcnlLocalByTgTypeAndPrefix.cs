using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation;
using TelcobrightMediation.Cdr;
using System.Linq;

namespace CdrRules
{

    [Export("CdrRule", typeof(ICdrRule))]
    public class IcnlLocalByTgTypeAndPrefix : ICdrRule
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "Local call by trunk group type and prefix";
        public int Id => 3;
        public object Data { get; set; }
        public bool IsPrepared { get; private set; }
        private Dictionary<ValueTuple<int, string>, route> switchWiseRoutes = 
            new Dictionary<ValueTuple<int, string>, route>();
        private Dictionary<string, partnerprefix> AnsPrefixes { get; set; } 
        public void Prepare(object input)
        {
            MediationContext mediationContext = (MediationContext) input;
            this.switchWiseRoutes = mediationContext.Routes;
            this.AnsPrefixes = mediationContext.DictAnsOrig;
            this.IsPrepared = true;
        }

        public bool CheckIfTrue(cdr thisCdr)
        {
            if(this.IsPrepared==false) throw new Exception("Rule is not prepared, method Prepare needs to be called first.");
            thisCdr.AnsIdOrig = null;
            AnsPrefixFinder.FindOriginatingAnsPrefix(thisCdr,this.AnsPrefixes,thisCdr.OriginatingCallingNumber);
            var dicRoutes = this.switchWiseRoutes;
            var key = new ValueTuple<int, string>(thisCdr.SwitchId, thisCdr.IncomingRoute);
            route incomingRoute = null;
            dicRoutes.TryGetValue(key, out incomingRoute);
            key = new ValueTuple<int, string>(thisCdr.SwitchId, thisCdr.OutgoingRoute);
            route outgoingRoute = null;
            dicRoutes.TryGetValue(key, out outgoingRoute);
            return incomingRoute?.NationalOrInternational == RouteLocalityType.National
                &&outgoingRoute?.NationalOrInternational==RouteLocalityType.National
                   && thisCdr.AnsIdOrig > 0;
        }
    }
}
