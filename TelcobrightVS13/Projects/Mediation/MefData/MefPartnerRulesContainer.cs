using System;
using System.Collections.Generic;
using MediationModel;

namespace TelcobrightMediation
{
    public class MefPartnerRulesContainer
    {
        public PartnerRuleComposer CmpPartner = new PartnerRuleComposer();
        //same dicroute object as in mediation data
        public Dictionary<ValueTuple<int,string>, route> SwitchWiseRoutes = new Dictionary<ValueTuple<int,string>, route>();//<switchid-route,route>
        public IDictionary<int, IPartnerRule> DicExtensions = new Dictionary<int, IPartnerRule>();
        public MediationContext MediationContext { get; set; }
    }
}