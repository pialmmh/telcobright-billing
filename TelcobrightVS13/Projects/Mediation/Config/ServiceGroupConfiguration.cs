using System.Collections.Generic;

namespace TelcobrightMediation
{
    public class ServiceGroupConfiguration
    {
        public bool Enabled { get; set; }
        public List<string> PartnerRules { get; set; }
        public List<RatingRule> Ratingtrules { get; set; }
        public Dictionary<string,string> MediationChecklistForUnAnsweredCdrs { get; set; }
        public Dictionary<string, string> MediationChecklistForAnsweredCdrs { get; set; }
        public ServiceGroupConfiguration(bool enabled)
        {
            this.Enabled = enabled;
            this.MediationChecklistForAnsweredCdrs = new Dictionary<string, string>();
            this.MediationChecklistForUnAnsweredCdrs = new Dictionary<string, string>();
        }
    }
}