using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Accounting
{
    public class BalanceAdjustmentPostProcessingData
    {
        public InvoiceGenerationInputData InvoiceGenerationInputData { get; set; }
        public long AccountId { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }

        public BalanceAdjustmentPostProcessingData(InvoiceGenerationInputData invoiceGenerationInputData)
        {
            this.InvoiceGenerationInputData = invoiceGenerationInputData;
        }
    }
}
