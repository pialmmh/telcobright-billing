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
            var key = new ValueTuple<int,string>(thisCdr.SwitchId,thisCdr.OutgoingRoute);
            route thisRoute = null;
            data.SwitchWiseRoutes.TryGetValue(key, out thisRoute);
            if (thisRoute != null)
            {
                thisCdr.OutPartnerId = thisRoute.idPartner;
                return thisRoute.idPartner;
            }
            if (thisCdr.OutPartnerId <= 0)
            {
                CdrSetting cdrSettOutg = data.MediationContext.CdrSetting;
                if (cdrSettOutg.useCasStyleProcessing == true)
                {
                    ANSOutByPrefix ansOutByPrefix = new ANSOutByPrefix();
                    int idPartner = ansOutByPrefix.Execute(thisCdr, data);
                    if (idPartner > 0)
                    {
                        thisCdr.OutPartnerId = idPartner;
                        return idPartner;
                    }
                }
            }
            thisCdr.OutPartnerId = 0;
            return 0;
        }
    }
}
