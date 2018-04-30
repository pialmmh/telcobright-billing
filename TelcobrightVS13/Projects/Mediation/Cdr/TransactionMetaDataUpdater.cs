using MediationModel;

namespace TelcobrightMediation.Cdr
{
    public class TransactionMetaDataUpdater
    {
        private cdr Cdr { get; }
        public TransactionMetaDataUpdater(cdr cdr)
        {
            cdr.TransactionMetaTotal = cdr.TransactionMetaTotal ?? 0;
            this.Cdr = cdr;
        }
        public void Update(acc_transaction incrementalTransaction)
        {
            this.Cdr.TransactionMetaTotal += incrementalTransaction.amount;
        }
    }
}