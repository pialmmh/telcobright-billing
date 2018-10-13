using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Accounting
{
    public class AccBalanceAdjRowDataCollector
    {
        public int RowId { get; set; }
        public int PartnerId { get; set; }
        public string PartnerName { get; set; }
        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public string Currency { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime StartDateTime { get; set; }
        public string ServiceAccountAlias { get; set; }
    }
}
