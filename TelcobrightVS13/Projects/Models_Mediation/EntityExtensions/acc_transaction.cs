using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediationModel
{
    public partial class acc_transaction
    {
        public acc_transaction Clone()
        {
            var newTrans = new acc_transaction();
            newTrans.id = this.id;
            newTrans.transactionTime = this.transactionTime;
            newTrans.seqId = this.seqId;
            newTrans.debitOrCredit = this.debitOrCredit;
            newTrans.idEvent = this.idEvent;
            newTrans.uniqueBillId = this.uniqueBillId;
            newTrans.description = this.description;
            newTrans.glAccountId = this.glAccountId;
            newTrans.uomId = this.uomId;
            newTrans.amount = this.amount;
            newTrans.BalanceBefore = this.BalanceBefore;
            newTrans.BalanceAfter = this.BalanceAfter;
            newTrans.isBillable = this.isBillable;
            newTrans.isPrepaid = this.isPrepaid;
            newTrans.isBilled = this.isBilled;
            newTrans.cancelled = this.cancelled;
            newTrans.createdByJob = this.createdByJob;
            newTrans.changedByJob = this.changedByJob;
            newTrans.jsonDetail = this.jsonDetail;

            return newTrans;
        }
    }
}
