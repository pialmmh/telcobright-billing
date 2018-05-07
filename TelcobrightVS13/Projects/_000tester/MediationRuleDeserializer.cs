using System.Collections.Generic;
using Newtonsoft.Json;

namespace Utils
{
    public class MediationRulesByServiceGroup
    {
        public int IdServiceGroup { get; set; }
        public string ServiceGroupname { get; set; }//for extension look up
        public List<int> PartnerRules { get; set; }
        public List<RatingRule> Ratingtrules { get; set; }
    }

    public class RatingRule
    {
    }

    public class MediationRuleDeserializer
    {
        public void ReadAndDeserialize()
        {
            List<MediationRulesByServiceGroup> medrules = new List<MediationRulesByServiceGroup>();
            List<string> jsonRules = new List<string>();
            using (jslEntities context = new jslEntities())
            {
                //jsonRules = context.mediationrules.Select(c => c.medrulesjson).ToList();
            }
            foreach (string jsonRule in jsonRules)
            {
                MediationRulesByServiceGroup medrule=
                    JsonConvert.DeserializeObject<MediationRulesByServiceGroup>(jsonRule);
                //File.WriteAllText();
            }
            
        }
    }
}
