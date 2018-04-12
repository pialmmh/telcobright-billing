using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Accounting
{
    public class AccChargeableExt
    {
        public acc_chargeable AccChargeable { get; }
        public Rateext RateExt { get; set; }
        public account Account { get; set; }
        public AccChargeableExt(acc_chargeable accChargeable)
        {
            this.AccChargeable = accChargeable;
        }
    }
}
