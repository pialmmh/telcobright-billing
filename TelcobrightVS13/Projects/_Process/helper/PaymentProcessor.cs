using System.Collections.Generic;
using System.Linq;
using MediationModel;

namespace Process
{
    public class PaymentProcessor
    {
        //List<acc_tmp_credit> _lstPendingPayments = new List<acc_tmp_credit>();
        public void ProcessPendingPayments(PartnerEntities context)
        {
            //_lstPendingPayments = context.acc_tmp_credit.Where(c => c.processed != 1).ToList();
            //CachedAccountsByName accCache = new CachedAccountsByName("account");
            //accCache.PopulateCache(() => context.accounts.ToDictionary(c => c.accountName));
            //AccountingContext accContext = new AccountingContext(new CachedAccountsByName("account", "id"));

            //lstPendingPayments.ForEach(pendingPayment =>
            //{
            //    var jsonDetail = JsonConvert.DeserializeObject<Dictionary<string, string>>(pendingPayment.jsonDetail);
            //    List<DoubleEntryTransaction> transactions = new List<DoubleEntryTransaction>()
            //    {
            //        {
            //            new DoubleEntryTransaction(accContext,jsonDetail["debitAccount"],jsonDetail["creditAccount"],
            //            pendingPayment.quantity)
            //        },
            //    };
            //    TransactionHandler transactionHandler = new TransactionHandler(accContext, transactions);
            //}
            //);

        }
    }
}
