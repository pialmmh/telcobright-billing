using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jdk.nashorn.@internal.ir;

namespace TelcobrightMediation.Accounting
{
    public class LedgerSummaryForInvoiceGeneration
    {
        public int PartnerId { get; set; }
        public string PartnerName { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        private DateTime StartDate { get; set; }
        private DateTime EndDate { get; set; }
        public decimal Amount { get; set; }

        public DateTime StartDateWithTime
        {
            get { return StartDate.Date; }
            set { StartDate = value.Date; }
        }

        public DateTime EndDateWithTime
        {
            get { return EndDate.Date.AddDays(1).AddSeconds(-1); }
            set { EndDate = value.Date; }
        }

        public String ServiceAccount { get; set; }
        public int TimeZone { get; set; }
        public int GmtOffset { get; set; }


        private const int DefaultGmtOffset = 21600;
        public DateTime StartDateWithTimeLocal
        {
            get { return StartDateWithTime.AddSeconds(DefaultGmtOffset - this.GmtOffset); }
        }

        public DateTime EndDateWithTimeLocal
        {
            get { return EndDateWithTime.AddSeconds(DefaultGmtOffset - this.GmtOffset); }
        }
    }
}
