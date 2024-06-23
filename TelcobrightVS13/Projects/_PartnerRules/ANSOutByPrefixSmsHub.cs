using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using System.Linq;
using MediationModel;

namespace PartnerRules
{

    [Export("Partner", typeof(IPartnerRule))]
    public class ANSOutByPrefixSmsHub : IPartnerRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public string HelpText => "Egress Partner Identification by outgoing prefix, ANSOutByPrefixSmsHub";
        public int Id => 11;
        public int Execute(cdr thisCdr, MefPartnerRulesContainer data)
        {
            Dictionary<string, partnerprefix> ansPrefixes880 = data.MediationContext.AnsPrefixes880;
            thisCdr.OutPartnerId = 0;
            string originatingCalledNumber = thisCdr.OriginatingCalledNumber;

            foreach (KeyValuePair<string, partnerprefix> kv in ansPrefixes880)
            {
                string prefix = kv.Key;
                partnerprefix ansPrefix = kv.Value;

                if (originatingCalledNumber.StartsWith(prefix))
                {
                    thisCdr.OutPartnerId = ansPrefix.idPartner;
                    return ansPrefix.idPartner;
                }
            }
            return 0;
        }
    }
}
