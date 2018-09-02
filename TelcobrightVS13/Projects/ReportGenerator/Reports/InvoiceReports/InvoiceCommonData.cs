using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcobrightMediation;


namespace ReportGenerator.Reports.InvoiceReports
{
    public class InvoiceCommonData
    {
        public String Type { get; set; }
        public DateTime BillingFrom { get; set; }
        public DateTime BillingTo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public String InvoiceNo { get; set; }
        public String Currency { get; set; }
        public String TimeZone { get; set; }

        public decimal ConversionRate { get; set; }
        public DateTime ConversionRateDate { get; set; }

        public PartnerEx Partner { get; set; }
        public List<InvoiceDataBasic> InvoiceItems { get; set; }

        public InvoiceCommonData()
        {
            InvoiceItems = new List<InvoiceDataBasic>();
        }
    }
}
