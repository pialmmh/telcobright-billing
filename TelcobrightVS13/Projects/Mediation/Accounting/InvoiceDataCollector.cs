using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jdk.nashorn.@internal.ir;

namespace TelcobrightMediation.Accounting
{
    public class InvoiceDataCollector
    {
        public int PartnerId { get; set; }
        public string PartnerName { get; set; }
        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        public String ServiceAccount { get; set; }
        public int TimeZone { get; set; }
        public int GmtOffset { get; set; }


        private const int DefaultGmtOffset = 21600;
        public DateTime StartDateTimeLocal
        {
            get { return this.StartDateTime.AddSeconds(DefaultGmtOffset - this.GmtOffset); }
        }

        public DateTime EndDateTimeLocal
        {
            get { return this.EndDateTime.AddSeconds(DefaultGmtOffset - this.GmtOffset); }
        }

        public List<DateTime> InvoiceDates { get; set; }

        public InvoiceDataCollector()
        {
            InvoiceDates = new List<DateTime>();
        }
    }
}
