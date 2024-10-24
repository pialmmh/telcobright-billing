using System;
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
        public string HelpText => "Egress Partner Identification by Trunk Group";
        public int Id => 2;

        public int Execute(cdr thisCdr, MefPartnerRulesContainer data)
        {
            thisCdr.OutPartnerId = 0;
            string tbPartnerDb = data.MediationContext.Tbc.Telcobrightpartner.databasename;
            CdrSetting cdrSetting = data.MediationContext.CdrSetting;
            var key = new ValueTuple<int, string>(thisCdr.SwitchId, thisCdr.OutgoingRoute);
            route thisRoute = null;
            data.SwitchWiseRoutes.TryGetValue(key, out thisRoute);
            if (thisRoute != null)
            {
                thisCdr.OutPartnerId = thisRoute.idPartner;
                return thisRoute.idPartner;
            }
            if (cdrSetting.useCasStyleProcessing == true &&
                    tbPartnerDb == "mnh_cas" && thisCdr.OutPartnerId <= 0 &&
                    (thisCdr.OutgoingRoute == "1950" || thisCdr.OutgoingRoute == "1960")
               )
            {
                ANSOutByPrefix ansOutByPrefix = new ANSOutByPrefix();
                int idPartner = ansOutByPrefix.Execute(thisCdr, data);
                if (idPartner > 0)
                {
                    thisCdr.OutPartnerId = idPartner;
                    return idPartner;
                }
            }
            else if (cdrSetting.useCasStyleProcessing == true && thisCdr.OutPartnerId <= 0)
            {
                ANSOutByPrefix ansOutByPrefix = new ANSOutByPrefix();
                int idPartner = ansOutByPrefix.Execute(thisCdr, data);
                if (idPartner > 0)
                {
                    thisCdr.OutPartnerId = idPartner;
                    return idPartner;
                }
            }
            thisCdr.OutPartnerId = 0;
            return 0;
        }
    }
}
