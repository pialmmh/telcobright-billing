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
        private Dictionary<ValueTuple<int, string>, route> routes = 
            new Dictionary<ValueTuple<int, string>, route>();
        private Dictionary<ValueTuple<int, string>, bridgedroute> bridgedRoutes =
            new Dictionary<ValueTuple<int, string>, bridgedroute>();
        private Dictionary<string, partnerprefix> AnsPrefixes { get; set; } 
        public void Prepare(object input)
        {
            MediationContext mediationContext = (MediationContext) input;
            this.routes = mediationContext.Routes;
            this.bridgedRoutes = mediationContext.BridgedRoutes;
            this.AnsPrefixes = mediationContext.DictAnsOrig;
            this.IsPrepared = true;
        }

        public bool CheckIfTrue(cdr thisCdr)
        {
            if(this.IsPrepared==false) throw new Exception("Rule is not prepared, method Prepare needs to be called first.");
            thisCdr.AnsIdOrig = null;
            route incomingRoute = null;
            route outgoingRoute = null;
            bridgedroute incomingBridgeRoute = null;
            bridgedroute outgoingBridgeRoute = null;

            AnsPrefixFinder.FindOriginatingAnsPrefix(thisCdr,this.AnsPrefixes,thisCdr.OriginatingCallingNumber);

            var key = new ValueTuple<int, string>(thisCdr.SwitchId, thisCdr.IncomingRoute);
            this.routes.TryGetValue(key, out incomingRoute);
            if (incomingRoute == null)
            {
                key = new ValueTuple<int, string>(thisCdr.SwitchId, thisCdr.IncomingRoute);
                this.bridgedRoutes.TryGetValue(key, out incomingBridgeRoute);
            }
            
            key = new ValueTuple<int, string>(thisCdr.SwitchId, thisCdr.OutgoingRoute);
            this.routes.TryGetValue(key, out outgoingRoute);
            if (outgoingRoute == null)
            {
                key = new ValueTuple<int, string>(thisCdr.SwitchId, thisCdr.OutgoingRoute);
                this.bridgedRoutes.TryGetValue(key, out outgoingBridgeRoute);
            }

            int inRouteLocalityType = Convert.ToInt32(incomingRoute?.NationalOrInternational ??
                                                      incomingBridgeRoute?.nationalOrInternational);

            int outRouteLocalityType = Convert.ToInt32(outgoingRoute?.NationalOrInternational ??
                                                       outgoingBridgeRoute?.nationalOrInternational);

            return inRouteLocalityType == RouteLocalityType.National
                && outRouteLocalityType==RouteLocalityType.National && thisCdr.AnsIdOrig > 0;
        }
    }
}
