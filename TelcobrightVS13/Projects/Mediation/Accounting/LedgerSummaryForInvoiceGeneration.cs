using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        public DateTime EndDateWithTime
        {
            get { return EndDate.Date.AddDays(1).AddSeconds(-1); }
        }

        public String ServiceAccount { get; set; }
        public int TimeZone { get; set; }
    }
}
