using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;

namespace PartnerRules
{

    [Export("Partner", typeof(IPartnerRule))]
    public class PrtInByBridgeTg : IPartnerRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public string HelpText => "Ingress Partner Identification by Bridge Trunk Group";
        public int Id => 3;
        public int Execute(cdr thisCdr, MefPartnerRulesContainer pData)
        {
            Dictionary<ValueTuple<int, string>, bridgedroute> bridgedRoutes = pData.MediationContext.BridgedRoutes;
            bridgedroute brRoute = null;
            var key = new ValueTuple<int, string>(thisCdr.SwitchId, thisCdr.IncomingRoute);
            bridgedRoutes.TryGetValue(key, out brRoute);
            if (brRoute != null)
            {
                int idPartner = brRoute.inPartner;
                thisCdr.InPartnerId = idPartner;
                return idPartner;
            }
            thisCdr.InPartnerId = 0;
            return 0;
        }
    }
}
