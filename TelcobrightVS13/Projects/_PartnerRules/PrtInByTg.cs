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
        public string HelpText => "Ingress Partner Identification by Trunk Group";
        public int Id => 1;
        public int Execute(cdr thisCdr, MefPartnerRulesContainer data)
        {
            thisCdr.InPartnerId = 0;
            string tbPartnerDb = data.MediationContext.Tbc.Telcobrightpartner.databasename;
            CdrSetting cdrSetting = data.MediationContext.CdrSetting;
            var key = new ValueTuple<int, string>(thisCdr.SwitchId, thisCdr.IncomingRoute);
            route thisRoute = null;
            data.SwitchWiseRoutes.TryGetValue(key, out thisRoute);
            if (thisRoute != null)
            {
                thisCdr.InPartnerId = thisRoute.idPartner;
                return thisRoute.idPartner;
            }

            if (thisCdr.InPartnerId <= 0)
            {
                if (cdrSetting.useCasStyleProcessing == true &&
                    tbPartnerDb == "mnh_cas" && thisCdr.InPartnerId <= 0
                    && thisCdr.IncomingRoute == "1974")
                {
                    ANSInByPrefix ansInByPrefix = new ANSInByPrefix();
                    int idPartner = ansInByPrefix.Execute(thisCdr, data);
                    if (idPartner > 0)
                    {
                        thisCdr.InPartnerId = idPartner;
                        return idPartner;
                    }
                }
            }

            thisCdr.InPartnerId = 0;
            return 0;
        }
    }
}
