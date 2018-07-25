using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Accounting
{
    public class TransactionPostingJobInputData : ITelcobrightJobInput
    {
        public AccountingContext AccountingContext { get; }
        public List<acc_transaction> TransactionsTobePosted { get; }
        public job TelcobrightJob { get; }

        public TransactionPostingJobInputData(AccountingContext accountingContext,
            List<acc_transaction> transactionsTobePosted, job telcobrightJob)
        {
            AccountingContext = accountingContext;
            TransactionsTobePosted = transactionsTobePosted;
            this.TelcobrightJob = telcobrightJob;
        }
    }
}
