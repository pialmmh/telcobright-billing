using System.Collections.Generic;
using MediationModel;
namespace TelcobrightMediation
{
    public class LedgerSummaryFactory<TSource> : AbstractSummaryFactory<TSource> where TSource : acc_transaction
    {
        public override ISummary CreateNewInstance(TSource summarySourceObject)
        {
            return new acc_ledger_summary()
            {
                idAccount = summarySourceObject.glAccountId,
                transactionDate = summarySourceObject.transactionTime.Date
            };
        }
    }
}
