using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Accounting
{
    public class AccountActionRule
    {
        public bool IsFixedAmount { get; set; }
        public bool IsPercent { get; set; }
        public bool IsFormulaBased { get; set; }
        public decimal Amount { get; set; }
        public decimal ACR { get; set; }
        public decimal ACD { get; set; }
    }
}
