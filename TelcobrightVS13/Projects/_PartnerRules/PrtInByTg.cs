using System;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;

namespace PartnerRules
{

    [Export("Partner", typeof(IPartnerRule))]
    public class PrtInByTg : IPartnerRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public string HelpText => "Incoming Partner/Partner Identification by Trunk Group";
        public int Id => 1;
        public void Execute(cdr thisCdr, MefPartnerRulesContainer pData)
        {
            var key = new ValueTuple<int,string>(thisCdr.SwitchId, thisCdr.incomingroute);
            route thisRoute = null;
            pData.SwitchWiseRoutes.TryGetValue(key, out thisRoute);
            if (thisRoute != null)
            {
                thisCdr.inPartnerId = thisRoute.idPartner;
                //set post paid/pre-paid flag
                thisCdr.CustomerPrePaid =Convert.ToByte(thisRoute.partner.CustomerPrePaid);
            }
        }
    }
}
