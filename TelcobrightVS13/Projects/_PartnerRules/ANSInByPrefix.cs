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
            Dictionary<string, partnerprefix> ansPrefixes0880 = data.MediationContext.AnsPrefixes00880;
            Dictionary<string, partnerprefix> ansPrefixes880 = data.MediationContext.AnsPrefixes880;
            Dictionary<string, partnerprefix> ansPrefixes0 = data.MediationContext.AnsPrefixes0;
            Dictionary<string, partnerprefix> ansPrefixes = data.MediationContext.AnsPrefixes;

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
            //if (thisCdr.InPartnerId == 0)
            //{
            //    foreach (KeyValuePair<string, partnerprefix> kv in ansPrefixes0880)
            //    {
            //        string prefix = kv.Key;
            //        partnerprefix ansPrefix = kv.Value;

            //        if (originatingCallingNumber.StartsWith(prefix))
            //        {
            //            thisCdr.InPartnerId = ansPrefix.idPartner;
            //            return ansPrefix.idPartner;
            //        }
            //    }
            //}
            if (thisCdr.InPartnerId == 0)
            {
                foreach (KeyValuePair<string, partnerprefix> kv in ansPrefixes0)
                {
                    string prefix = kv.Key;
                    partnerprefix ansPrefix = kv.Value;

                    if (originatingCallingNumber.StartsWith(prefix))
                    {
                        thisCdr.InPartnerId = ansPrefix.idPartner;
                        return ansPrefix.idPartner;
                    }
                }
            }

            if (thisCdr.InPartnerId == 0)
            {
                foreach (KeyValuePair<string, partnerprefix> kv in ansPrefixes)
                {
                    string prefix = kv.Key;
                    partnerprefix ansPrefix = kv.Value;

                    if (originatingCallingNumber.StartsWith(prefix))
                    {
                        thisCdr.InPartnerId = ansPrefix.idPartner;
                        return ansPrefix.idPartner;
                    }
                }
            }
            return 0;
        }
    }
}
