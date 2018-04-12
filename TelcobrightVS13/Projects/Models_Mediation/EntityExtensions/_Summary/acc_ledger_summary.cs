using System;

namespace MediationModel
{
    public partial class acc_ledger_summary : ISummary<acc_ledger_summary, ValueTuple<long, DateTime>>
    {
        public ValueTuple<long, DateTime> GetTupleKey()
        {
            return new ValueTuple<long, DateTime>(this.idAccount, this.transactionDate);
        }
        public void Merge(acc_ledger_summary newSummary)
        {
            this.AMOUNT += newSummary.AMOUNT;
        }
        public void Multiply(int value)
        {
            this.AMOUNT = this.AMOUNT * value;
        }

        public acc_ledger_summary CloneWithFakeId()
        {
            return new acc_ledger_summary()
            {
                id=-1,//must set externally
                idAccount = this.idAccount,
                transactionDate = this.transactionDate,
                AMOUNT = this.AMOUNT
            };
        }
    }
}
