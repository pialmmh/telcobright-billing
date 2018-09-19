using System;
using System.Collections.Generic;
using System.Linq;
using LibraryExtensions;
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
        public Dictionary<DateTime,acc_ledger_summary> DayWiseLedgerSummaries { get; }
        public InvoicePostProcessingData(InvoiceGenerationInputData invoiceGenerationInputData, invoice invoiceWithItem,
            long serviceAccountId, DateTime startDate, DateTime endDate,
            Dictionary<DateTime, acc_ledger_summary> dayWiseledgerSummaries)
        {
            this.InvoiceGenerationInputData = invoiceGenerationInputData;
            this.Invoice = invoiceWithItem;
            this.InvoiceItem = invoiceWithItem.invoice_item.Single();
            this.ServiceAccountId = serviceAccountId;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.DayWiseLedgerSummaries = dayWiseledgerSummaries;
        }
        public string GetWhereClauseForDateRange()
        {
            var startDate = this.StartDate;
            var endDate = this.EndDate;
            return $@"tup_starttime>= {startDate.ToMySqlFormatWithQuote()}
                    and tup_starttime <= {endDate.ToMySqlFormatWithQuote()}";
        }
    }
}