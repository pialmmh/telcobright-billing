using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using Newtonsoft.Json;
using org.springframework.beans.factory;

namespace TelcobrightMediation.Accounting
{
    public class BalanceAdjustmentHelper
    {
        private InvoiceGenerationInputData InvoiceGenerationInputData { get; }

        public BalanceAdjustmentHelper(InvoiceGenerationInputData invoiceGenerationInputData)
        {
            this.InvoiceGenerationInputData = invoiceGenerationInputData;
        }

        public BalanceAdjustmentPostProcessingData Process()
        {
            BalanceAdjustmentPostProcessingData adjustmentPostProcessingData = new BalanceAdjustmentPostProcessingData(this.InvoiceGenerationInputData);
            job telcobrightJob = InvoiceGenerationInputData.TelcobrightJob;
            Dictionary<string, string> jobParamsMap =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(telcobrightJob.JobParameter);
            var invoiceJsonDetail = jobParamsMap;//carry on jobs param along with invoice detail
            this.InvoiceGenerationInputData.JsonDetail = invoiceJsonDetail;
            var context = this.InvoiceGenerationInputData.Context;
            long serviceAccountId = Convert.ToInt64(invoiceJsonDetail["serviceAccountId"]);
            DateTime startDate = Convert.ToDateTime(invoiceJsonDetail["startDate"]);
            decimal amount = Convert.ToDecimal(invoiceJsonDetail["amount"]);

            adjustmentPostProcessingData.AccountId = serviceAccountId;
            adjustmentPostProcessingData.BalanceBefore = context.accounts.First(x => x.id == serviceAccountId).balanceAfter;
            adjustmentPostProcessingData.BalanceAfter =
                amount + context.acc_transaction
                    .Where(x => x.glAccountId == serviceAccountId && x.transactionTime >= startDate).Select(x => x.amount)
                    .DefaultIfEmpty(0)
                    .Sum();
            return adjustmentPostProcessingData;
        }
    }
}
