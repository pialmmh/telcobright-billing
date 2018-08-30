using System.Collections.Generic;
using System.Linq;
using MediationModel;

namespace TelcobrightMediation.Accounting
{
    public class InvoicePostProcessingData
    {
        public InvoiceGenerationInputData InvoiceGenerationInputData { get; set; }
        public invoice Invoice { get; set; }
        public invoice_item InvoiceItem { get; private set; }
        public Dictionary<string,string> OtherDataAsMap { get; set; }
        public InvoicePostProcessingData(InvoiceGenerationInputData invoiceGenerationInputData, invoice invoiceWithItem, 
            Dictionary<string, string> otherDataAsMap)
        {
            this.InvoiceGenerationInputData = invoiceGenerationInputData;
            this.Invoice = invoiceWithItem;
            this.OtherDataAsMap = otherDataAsMap;
            this.InvoiceItem = invoiceWithItem.invoice_item.Single();
        }
    }
}