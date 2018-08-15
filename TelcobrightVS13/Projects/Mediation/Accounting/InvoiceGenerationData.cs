using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Accounting
{
    public class InvoiceGenerationData
    {
        public PartnerEntities Context { get; }
        public int BatchSizeForJobSegment { get; set; }
        public InvoiceDataCollector InvoiceDataCollector { get; set; }

        public InvoiceGenerationData(PartnerEntities context, int batchSizeForJobSegment, InvoiceDataCollector invoiceDataCollector)
        {
            Context = context;
            BatchSizeForJobSegment = batchSizeForJobSegment;
            InvoiceDataCollector = invoiceDataCollector;
        }
    }
}
