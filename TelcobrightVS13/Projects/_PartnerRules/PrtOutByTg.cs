using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;

namespace PartnerRules
{

    [Export("Partner", typeof(IPartnerRule))]
    public class PrtOutByTg : IPartnerRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public string HelpText => "Outgoing Partner/Partner Identification by Trunk Group";
        public int Id => 2;

        public void Execute(cdr thisCdr, MefPartnerRulesContainer pData)
        {
            string key = thisCdr.SwitchId + "-" + thisCdr.outgoingroute;
            route thisRoute = null;
            pData.DicRouteIncludePartner.TryGetValue(key, out thisRoute);
            if (thisRoute != null)
            {
                thisCdr.outPartnerId = thisRoute.idPartner;
            }
        }
    }
}
