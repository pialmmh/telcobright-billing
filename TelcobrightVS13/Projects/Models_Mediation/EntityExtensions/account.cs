using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediationModel
{
    public partial class account
    {
        public void ExecuteTransaction(acc_transaction transaction)
        {
            transaction.BalanceBefore = this.balanceAfter;
            transaction.BalanceAfter = this.balanceAfter + transaction.amount;
            if (this.superviseNegativeBalance == 1 && transaction.BalanceAfter < this.negativeBalanceLimit)
            {
                throw new NotSupportedException("NegativeBalanceSupervision must be off to allow negative balance. " +
                                                "Balance after this transaction will exceed negative balance limit=" +
                                                this.negativeBalanceLimit +
                                                " for account name: " + this.accountName);
            }
            this.balanceBefore = this.balanceAfter;
            this.lastAmount = transaction.amount;
            this.balanceAfter = transaction.BalanceAfter;
            this.lastUpdated = DateTime.Now;
        }
    }
}
