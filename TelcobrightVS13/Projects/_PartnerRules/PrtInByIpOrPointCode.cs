using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using System.Linq;
using MediationModel;
using LibraryExtensions;
namespace PartnerRules
{

    [Export("Partner", typeof(IPartnerRule))]
    public class PrtInByIpOrPointCode : IPartnerRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public string HelpText => "Ingress Partner Identification by IpAddress or PointCode";
        public int Id => 5;
        public int Execute(cdr thisCdr, MefPartnerRulesContainer data)
        {
            Dictionary<string,partner> partners= data.MediationContext.Partners.Values.ToDictionary(p=>p.idPartner.ToString());
            Dictionary<string, ipaddressorpointcode> ipOrPcs = data.MediationContext.IpAddressorPointCodes;
            //var key = new ValueTuple<int,string>(thisCdr.SwitchId, thisCdr.IncomingRoute);

            int? pc = thisCdr.OPC;
            partner partner = null;
            if (pc!=null && pc > 0) //pointcode
            {
                partner = partners[pc.ToString()];
            }
            if (partner != null)
            {
                string ipAddr = thisCdr.OriginatingIP;
                if (!ipAddr.IsNullOrEmptyOrWhiteSpace() && ipAddr.Contains("."))
                {
                    partner = partners[ipAddr.ToString()];
                }
            }
            
            if (partner!= null)
            {
                thisCdr.InPartnerId = partner.idPartner;
                return partner.idPartner;
            }
            thisCdr.InPartnerId = 0;
            return 0;
        }
    }
}
