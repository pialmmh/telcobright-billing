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
    public class PrtOutByIpOrPointCode : IPartnerRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public string HelpText => "Egress Partner Identification by IpAddress or PointCode";
        public int Id => 6;
        public int Execute(cdr thisCdr, MefPartnerRulesContainer data)
        {
            Dictionary<string,partner> partners= data.MediationContext.Partners.Values.ToDictionary(p=>p.idPartner.ToString());
            Dictionary<string, ipaddressorpointcode> ipOrPcs = data.MediationContext.IpAddressorPointCodes;

            int? pc = thisCdr.DPC;
            partner partner = null;
            if (pc!=null && pc > 0) //pointcode
            {
                partner = partners[pc.ToString()];
            }
            if (partner != null)
            {
                string ipAddr = thisCdr.TerminatingIP;
                if (!ipAddr.IsNullOrEmptyOrWhiteSpace() && ipAddr.Contains("."))
                {
                    partner = partners[ipAddr.ToString()];
                }
            }
            
            if (partner!= null)
            {
                thisCdr.OutPartnerId= partner.idPartner;
                return partner.idPartner;
            }
            thisCdr.OutPartnerId = 0;
            return 0;
        }
    }
}
