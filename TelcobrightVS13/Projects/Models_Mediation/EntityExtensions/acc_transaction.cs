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
            return new acc_transaction()
            {
                id = this.id,
                transactionTime = this.transactionTime,
                seqId = this.seqId,
                debitOrCredit = this.debitOrCredit,
                idEvent = this.idEvent,
                uniqueBillId = this.uniqueBillId,
                description = this.description,
                glAccountId = this.glAccountId,
                uomId = this.uomId,
                amount = this.amount,
                BalanceBefore = this.BalanceBefore,
                BalanceAfter = this.BalanceAfter,
                isBillable = this.isBillable,
                isPrepaid = this.isPrepaid,
                isBilled = this.isBilled,
                cancelled = this.cancelled,
                createdByJob = this.createdByJob,
                changedByJob = this.changedByJob,
                jsonDetail = this.jsonDetail

            };
        }
    }
}
