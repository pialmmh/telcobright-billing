using System.Collections.Generic;
using MediationModel;

namespace TelcobrightMediation
{
    public class MefPartnerRulesContainer
    {
        public PartnerRuleComposer CmpPartner = new PartnerRuleComposer();
        //same dicroute object as in mediation data
        public Dictionary<string, route> DicRouteIncludePartner = new Dictionary<string, route>();//<switchid-route,route>
        public IDictionary<string, IPartnerRule> DicExtensions = new Dictionary<string, IPartnerRule>();
    }
}