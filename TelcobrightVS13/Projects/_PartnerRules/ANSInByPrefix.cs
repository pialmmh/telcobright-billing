using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using System.Linq;
using MediationModel;

namespace PartnerRules
{

    [Export("Partner", typeof(IPartnerRule))]
    public class ANSInByPrefix : IPartnerRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public string HelpText => "Ingress Partner Identification by incoming partner prefix";
        public int Id => 3;
        public int Execute(cdr thisCdr, MefPartnerRulesContainer data)
        {
            List<partnerprefix> ansPrefixes0880 = data.MediationContext.AnsPrefixes0880.Values.ToList();
            List<partnerprefix> ansPrefixes880 = data.MediationContext.AnsPrefixes880.Values.ToList();
            List<partnerprefix> ansPrefixes0 = data.MediationContext.AnsPrefixes0.Values.ToList();
            List<partnerprefix> ansPrefixes = data.MediationContext.AnsPrefixes.Values.ToList();

            thisCdr.InPartnerId = 0;
            string originatingCallingNumber = thisCdr.OriginatingCallingNumber;
            
            foreach (partnerprefix ansPrefix in ansPrefixes0880)
            {
                string prefix = ansPrefix.Prefix;
                if (originatingCallingNumber.StartsWith(prefix))
                {
                    thisCdr.InPartnerId = ansPrefix.idPartner;
                    return ansPrefix.idPartner;
                }
            }
            if (thisCdr.InPartnerId == 0)
            {
                foreach (partnerprefix ansPrefix in ansPrefixes880)
                {
                    string prefix = ansPrefix.Prefix;
                    if (originatingCallingNumber.StartsWith(prefix))
                    {
                        thisCdr.InPartnerId = ansPrefix.idPartner;
                        return ansPrefix.idPartner;
                    }
                }
            }
            if (thisCdr.InPartnerId == 0)
            {
                foreach (partnerprefix ansPrefix in ansPrefixes0)
                {
                    string prefix = ansPrefix.Prefix;
                    if (originatingCallingNumber.StartsWith(prefix))
                    {
                        thisCdr.InPartnerId = ansPrefix.idPartner;
                        return ansPrefix.idPartner;
                    }
                }
            }
            //if (thisCdr.InPartnerId == 0)
            //{
            //    foreach (partnerprefix ansPrefix in ansPrefixes)
            //    {
            //        string prefix = ansPrefix.Prefix;
            //        if (originatingCallingNumber.StartsWith(prefix))
            //        {
            //            thisCdr.InPartnerId = ansPrefix.idPartner;
            //            return ansPrefix.idPartner;
            //        }
            //    }
            //}
            return 0;
        }
    }
}
