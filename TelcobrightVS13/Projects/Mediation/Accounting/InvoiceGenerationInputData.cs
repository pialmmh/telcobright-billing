using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Accounting
{
    public class InvoiceGenerationInputData:ITelcobrightJobInput
    {
        public IServiceGroup ServiceGroup { get; }
        private Dictionary<string, IInvoiceGenerationRule> InvoiceGenerationRules { get; set; }
        public IInvoiceGenerationRule InvoiceGenerationRule { get; set; }
        public TelcobrightConfig Tbc { get; }
        public PartnerEntities Context { get; }
        public int BatchSizeForJobSegment { get; set; }
        public InvoiceDataCollector InvoiceDataCollector { get; set; }
        public AccountingContext AccountingContext { get; set; }
        public job TelcobrightJob { get;}
        public InvoiceGenerationInputData(TelcobrightConfig tbc,AccountingContext accountingContext, 
            PartnerEntities context, string invoiceGenerationRuleName,int batchSizeForJobSegment, 
            InvoiceDataCollector invoiceDataCollector,job telcobrightJob,
            IServiceGroup serviceGroup)
        {
            this.Tbc = tbc;
            this.AccountingContext = accountingContext;
            Context = context;
            BatchSizeForJobSegment = batchSizeForJobSegment;
            InvoiceDataCollector = invoiceDataCollector;
            this.TelcobrightJob = telcobrightJob;
            InvoiceGenerationRuleComposer invoiceGenerationRuleComposer=new InvoiceGenerationRuleComposer();
            invoiceGenerationRuleComposer.Compose();
            this.InvoiceGenerationRules = invoiceGenerationRuleComposer.InvoiceGenerationRules
                .ToDictionary(c => c.RuleName);
            this.InvoiceGenerationRule = this.InvoiceGenerationRules[invoiceGenerationRuleName];
            this.ServiceGroup = serviceGroup;
        }
    }
}
