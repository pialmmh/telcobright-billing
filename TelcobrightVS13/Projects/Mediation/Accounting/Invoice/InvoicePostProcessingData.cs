using System;
using System.Collections.Generic;
using System.Linq;
using MediationModel;
using org.springframework.beans.propertyeditors;

namespace TelcobrightMediation.Accounting
{
    public class InvoicePostProcessingData
    {
        public InvoiceGenerationInputData InvoiceGenerationInputData { get; set; }
        public invoice Invoice { get; set; }
        public invoice_item InvoiceItem { get; private set; }
        public acc_temp_transaction TempTransaction { get; set; }
        public long ServiceAccountId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency => this.InvoiceItem.UOM_ID;
        public InvoicePostProcessingData(InvoiceGenerationInputData invoiceGenerationInputData, invoice invoiceWithItem,
            long serviceAccountId, DateTime startDate, DateTime endDate)
        {
            this.InvoiceGenerationInputData = invoiceGenerationInputData;
            this.Invoice = invoiceWithItem;
            this.InvoiceItem = invoiceWithItem.invoice_item.Single();
            this.ServiceAccountId = serviceAccountId;
            this.StartDate = startDate;
            this.EndDate = endDate;
        }
    }
}