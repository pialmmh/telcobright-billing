using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportGenerator.reports.invoice
{
    public class Invoice
    {
        public String Type { get; set; }
        public DateTime BillingFrom { get; set; }
        public DateTime BillingTo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public String InvoiceNo { get; set; }
        public String Currency { get; set; }
        public String TimeZone { get; set; }
        public Partner Partner { get; set; }
        public List<InvoiceItem> InvoiceItems { get; set; }

        public Invoice()
        {
            InvoiceItems = new List<InvoiceItem>();
        }
    }
}
