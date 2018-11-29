using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Cdr
{
    
    public static class AnsPrefixFinder
    {
        public static void FindOriginatingAnsPrefix(cdr thisCdr, Dictionary<string, partnerprefix> ansPrefixes, string originatingCallingNumber)
        {
            string ansPrefixOrig = "";
            int? ansIdOrig = null;
            for (int iteration = 0; iteration < originatingCallingNumber.Length; iteration++)
            {
                partnerprefix thisPrefix = null;
                string matchStr = originatingCallingNumber.Substring(0, iteration + 1);
                ansPrefixes.TryGetValue(matchStr, out thisPrefix);
                if (thisPrefix != null)
                {
                    ansPrefixOrig = thisPrefix.Prefix;
                    ansIdOrig = thisPrefix.idPartner;
                }
            }
            thisCdr.AnsPrefixOrig = ansPrefixOrig;
            thisCdr.AnsIdOrig = ansIdOrig;
        }
    }
}
