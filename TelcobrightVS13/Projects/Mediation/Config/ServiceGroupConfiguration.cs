using System.Collections.Generic;
using MediationModel;
using TelcobrightMediation.Accounting;
namespace TelcobrightMediation
{
    public class ServiceGroupConfiguration
    {
        public List<int> PartnerRules { get; set; }
        public List<RatingRule> Ratingtrules { get; set; }
        public List<IValidationRule<cdr>> MediationChecklistForUnAnsweredCdrs { get; set; }
        public List<IValidationRule<cdr>> MediationChecklistForAnsweredCdrs { get; set; }
        public int IdServiceGroup { get; }
        public Dictionary<string, string> Params { get; set; }=new Dictionary<string, string>();
        public InvoiceGenerationConfig InvoiceGenerationConfig { get; set; }
        public List<IAutomationAction> AccountActions { get; set; }
        public bool Disabled { get; set; }
        public ServiceGroupConfiguration(int idServiceGroup)
        {
            this.IdServiceGroup = idServiceGroup;
            this.MediationChecklistForAnsweredCdrs = new List<IValidationRule<cdr>>();
            this.MediationChecklistForUnAnsweredCdrs = new List<IValidationRule<cdr>>();
        }
    }
}