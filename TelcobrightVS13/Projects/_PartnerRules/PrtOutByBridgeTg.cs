using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;

namespace PartnerRules
{

    [Export("Partner", typeof(IPartnerRule))]
    public class PrtOutByBridgeTg : IPartnerRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public string HelpText => "Egress Partner Identification by Bridge Trunk Group";
        public int Id => 4;
        public int Execute(cdr thisCdr, MefPartnerRulesContainer pData)
        {
            Dictionary<ValueTuple<int, string>, bridgedroute> bridgedRoutes = pData.MediationContext.BridgedRoutes;
            bridgedroute brRoute = null;
            var key = new ValueTuple<int, string>(thisCdr.SwitchId, thisCdr.OutgoingRoute);
            bridgedRoutes.TryGetValue(key, out brRoute);
            if (brRoute != null)
            {
                int idPartner = brRoute.outPartner;
                thisCdr.InPartnerId = idPartner;
                return idPartner;
            }
            thisCdr.OutPartnerId = 0;
            return 0;
        }
    }
}
