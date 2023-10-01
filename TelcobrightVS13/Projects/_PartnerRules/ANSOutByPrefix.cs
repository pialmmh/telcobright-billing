using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using System.Linq;
using MediationModel;

namespace PartnerRules
{

    [Export("Partner", typeof(IPartnerRule))]
    public class ANSOutByPrefix : IPartnerRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public string HelpText => "Egress Partner Identification by outgoing prefix";
        public int Id => 4;
        public int Execute(cdr thisCdr, MefPartnerRulesContainer data)
        {

            List<partnerprefix> ansPrefixes0880 = data.MediationContext.AnsPrefixes0880.Values.ToList();
            List<partnerprefix> ansPrefixes880 = data.MediationContext.AnsPrefixes880.Values.ToList();
            List<partnerprefix> ansPrefixes0 = data.MediationContext.AnsPrefixes0.Values.ToList();
            List<partnerprefix> ansPrefixes = data.MediationContext.AnsPrefixes.Values.ToList();

            thisCdr.OutPartnerId = 0;
            string terminatingCalledNumber = thisCdr.TerminatingCalledNumber;

            foreach (partnerprefix ansPrefix in ansPrefixes0880)
            {
                string prefix = ansPrefix.Prefix;
                if (terminatingCalledNumber.StartsWith(prefix))
                {
                    thisCdr.OutPartnerId = ansPrefix.idPartner;
                    return ansPrefix.idPartner;
                }
            }
            if (thisCdr.OutPartnerId == 0)
            {
                foreach (partnerprefix ansPrefix in ansPrefixes880)
                {
                    string prefix = ansPrefix.Prefix;
                    if (terminatingCalledNumber.StartsWith(prefix))
                    {
                        thisCdr.OutPartnerId = ansPrefix.idPartner;
                        return ansPrefix.idPartner;
                    }
                }
            }
            if (thisCdr.OutPartnerId == 0)
            {
                foreach (partnerprefix ansPrefix in ansPrefixes0)
                {
                    string prefix = ansPrefix.Prefix;
                    if (terminatingCalledNumber.StartsWith(prefix))
                    {
                        thisCdr.OutPartnerId = ansPrefix.idPartner;
                        return ansPrefix.idPartner;
                    }
                }
            }
            if (thisCdr.OutPartnerId == 0)
            {
                foreach (partnerprefix ansPrefix in ansPrefixes)
                {
                    string prefix = ansPrefix.Prefix;
                    if (terminatingCalledNumber.StartsWith(prefix))
                    {
                        thisCdr.OutPartnerId = ansPrefix.idPartner;
                        return ansPrefix.idPartner;
                    }
                }
            }
            return 0;
        }
    }
}
