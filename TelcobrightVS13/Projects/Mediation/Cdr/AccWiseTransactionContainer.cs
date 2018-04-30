using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging.Configuration;
using MediationModel;

namespace TelcobrightMediation.Cdr
{
    public class AccWiseTransactionContainer
    {
        public List<acc_transaction> OldTransactions { get; set; }=new List<acc_transaction>();
        public acc_transaction  NewTransaction { get; set; }
        private acc_transaction _incrementalTransaction;
        public acc_transaction IncrementalTransaction => this._incrementalTransaction;
        public void SetIncrementalTransaction(acc_transaction incrementalTransaction, 
            TransactionMetaDataUpdater transactionMetaDataUpdater)
        {
            transactionMetaDataUpdater.Update(incrementalTransaction);
            this._incrementalTransaction = incrementalTransaction;
        }
    }
}
