using System.Collections.Generic;
using MediationModel;

namespace TelcobrightMediation
{
    public class TransactionJobInputData:ITelcobrightJobInput
    {
        public TelcobrightConfig Tbc { get; }
        public job TelcobrightJob { get; } = null;
        public PartnerEntities Context { get; }
        public List<acc_temp_transaction> TempTransactions { get; set; }

        public TransactionJobInputData(TelcobrightConfig tbc, PartnerEntities context, 
            List<acc_temp_transaction> tempTransactions)
        {
            this.Tbc = tbc;
            this.Context = context;
            this.TempTransactions = tempTransactions;
        }
    }
}