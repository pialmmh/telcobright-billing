using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Accounting
{
    public class InvoiceGenerationInputData : ITelcobrightJobInput
    {
        public Dictionary<string, IInvoiceGenerationRule> InvoiceGenerationRules { get;}
        public Dictionary<int,IServiceGroup> ServiceGroups { get; }
        public TelcobrightConfig Tbc { get; }
        public PartnerEntities Context { get; }
        public int BatchSizeForJobSegment { get; set; }
        public job TelcobrightJob { get; }
        public Dictionary<string,string> InvoiceJsonDetail { get; set; }
        public InvoiceGenerationInputData(TelcobrightConfig tbc,
            PartnerEntities context, job telcobrightJob)
        {
            this.Tbc = tbc;
            Context = context;
            BatchSizeForJobSegment = this.Tbc.CdrSetting.SegmentSizeForDbWrite;
            this.TelcobrightJob = telcobrightJob;
            InvoiceGenerationRuleComposer invoiceGenerationRuleComposer = new InvoiceGenerationRuleComposer();
            invoiceGenerationRuleComposer.Compose();
            this.InvoiceGenerationRules = invoiceGenerationRuleComposer.InvoiceGenerationRules
                .ToDictionary(c => c.RuleName);
            ServiceGroupComposer serviceGroupComposer = new ServiceGroupComposer();
            serviceGroupComposer.Compose();
            this.ServiceGroups = serviceGroupComposer.ServiceGroups.ToDictionary(c => c.Id);
        }
    }
}
