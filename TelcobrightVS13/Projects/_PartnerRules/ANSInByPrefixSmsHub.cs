using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using System.Linq;
using MediationModel;

namespace PartnerRules
{

    [Export("Partner", typeof(IPartnerRule))]
    public class ANSInByPrefixSmsHub : IPartnerRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public string HelpText => "Ingress Partner Identification by incoming partner prefix ANSInByPrefixSmsHub";
        public int Id => 10;
        public int Execute(cdr thisCdr, MefPartnerRulesContainer data)
        {
            Dictionary<string, partnerprefix> ansPrefixes880 = data.MediationContext.AnsPrefixes880;
            thisCdr.InPartnerId = 0;
            string originatingCallingNumber = thisCdr.OriginatingCallingNumber;

            foreach (KeyValuePair<string, partnerprefix> kv in ansPrefixes880)
            {
                string prefix = kv.Key;
                partnerprefix ansPrefix = kv.Value;

                if (originatingCallingNumber.StartsWith(prefix))
                {
                    thisCdr.InPartnerId = ansPrefix.idPartner;
                    return ansPrefix.idPartner;
                }
            }
            return 0;
        }
    }
}
