using System.Collections.Generic;

namespace TelcobrightMediation
{
    public class ServiceGroupConfiguration
    {
        public List<int> PartnerRules { get; set; }
        public List<RatingRule> Ratingtrules { get; set; }
        public Dictionary<string, string> MediationChecklistForUnAnsweredCdrs { get; set; }
        public Dictionary<string, string> MediationChecklistForAnsweredCdrs { get; set; }
        public int IdServiceGroup { get; }
        public Dictionary<string, string> Params { get; set; }

        public ServiceGroupConfiguration(int idServiceGroup)
        {
            this.IdServiceGroup = idServiceGroup;
            this.MediationChecklistForAnsweredCdrs = new Dictionary<string, string>();
            this.MediationChecklistForUnAnsweredCdrs = new Dictionary<string, string>();
        }
    }
}