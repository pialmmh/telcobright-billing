using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Cdr
{
    public class TransactionContainerForSingleAccount
    {
        public List<acc_transaction> OldTransactions { get; set; }=new List<acc_transaction>();
        public acc_transaction  NewTransaction { get; set; }
        public acc_transaction IncrementalTransaction { get; set; }
    }
}
