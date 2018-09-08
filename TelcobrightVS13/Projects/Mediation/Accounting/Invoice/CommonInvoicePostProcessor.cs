using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using Newtonsoft.Json;

namespace TelcobrightMediation.Accounting.Invoice
{
    public class CommonInvoicePostProcessor
    {
        public InvoicePostProcessingData InvoicePostProcessingData { get;}
        public string CdrOrSummaryTableName { get;}
        public Dictionary<string,string> JsonDetail { get; }
        public CommonInvoicePostProcessor(InvoicePostProcessingData invoicePostProcessingData,
            string cdrOrSummaryTableName,Dictionary<string,string> jsonDetail)
        {
            InvoicePostProcessingData = invoicePostProcessingData;
            this.CdrOrSummaryTableName = cdrOrSummaryTableName;
            this.JsonDetail = jsonDetail;
        }
        public InvoicePostProcessingData Process()
        {
            invoice invoiceWithItem = this.InvoicePostProcessingData.Invoice;
            invoice_item invoiceItem=invoiceWithItem.invoice_item.Single();
            InvoiceSectionDataPopulator sectionDataPopulator =
                new InvoiceSectionDataPopulator(this.InvoicePostProcessingData, this.CdrOrSummaryTableName);
            List<InvoiceSection> invoiceSections = sectionDataPopulator.Populate().ToList();
            Dictionary<string, string> jsonDetailWithSection =
                sectionDataPopulator.AppendSectionToJsonDetail(this.JsonDetail, invoiceSections);
            invoiceItem.JSON_DETAIL = JsonConvert.SerializeObject(jsonDetailWithSection);
            return this.InvoicePostProcessingData;
        }
    }
}
