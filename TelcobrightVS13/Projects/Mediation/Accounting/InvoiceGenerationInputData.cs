using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Accounting
{
    public class InvoiceGenerationInputData
    {
        public TelcobrightConfig Tbc { get; }
        public PartnerEntities Context { get; }
        public int BatchSizeForJobSegment { get; set; }
        public InvoiceDataCollector InvoiceDataCollector { get; set; }
        public AccountingContext AccountingContext { get; set; }
        public job TelcobrightJob { get;}
        public InvoiceGenerationInputData(TelcobrightConfig tbc,AccountingContext accountingContext, PartnerEntities context, int batchSizeForJobSegment, 
            InvoiceDataCollector invoiceDataCollector,job telcobrightJob)
        {
            this.Tbc = tbc;
            this.AccountingContext = accountingContext;
            Context = context;
            BatchSizeForJobSegment = batchSizeForJobSegment;
            InvoiceDataCollector = invoiceDataCollector;
            this.TelcobrightJob = telcobrightJob;
        }
    }
}
