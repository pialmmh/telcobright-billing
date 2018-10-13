using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Accounting
{
    public static class TempTransactionHelper
    {
        public static void CreateTempTransaction(long accountId, decimal amount, DateTime payDate, DbCommand cmd,
            account account)
        {
            acc_temp_transaction transaction = new acc_temp_transaction();
            transaction.transactionTime = payDate;
            transaction.amount = amount;
            transaction.glAccountId = accountId;
            transaction.debitOrCredit = "d";
            transaction.idEvent = -1;
            transaction.uomId = account.uom;
            transaction.BalanceBefore = 0;
            transaction.BalanceAfter = 0;
            transaction.jsonDetail = string.Empty;
            cmd.CommandText = string.Concat(
                StaticExtInsertColumnHeaders.acc_temp_transaction.Replace("(id,", "("),
                transaction.GetExtInsertValues().Replace("(0,", "("));
            cmd.ExecuteNonQuery();
        }

        public static acc_transaction ConvertTempTransactionToTransaction(acc_temp_transaction tempTrans)
        {
            return new acc_transaction
            {
                id = tempTrans.id,
                transactionTime = tempTrans.transactionTime,
                seqId = tempTrans.seqId,
                debitOrCredit = tempTrans.debitOrCredit,
                idEvent = tempTrans.idEvent,
                uniqueBillId = tempTrans.uniqueBillId,
                description = tempTrans.description,
                glAccountId = tempTrans.glAccountId,
                uomId = tempTrans.uomId,
                amount = tempTrans.amount,
                BalanceBefore = tempTrans.BalanceBefore,
                BalanceAfter = tempTrans.BalanceAfter,
                isBillable = tempTrans.isBillable,
                isPrepaid = tempTrans.isPrepaid,
                isBilled = tempTrans.isBilled,
                cancelled = tempTrans.cancelled,
                createdByJob = tempTrans.createdByJob,
                changedByJob = tempTrans.changedByJob,
                jsonDetail = tempTrans.jsonDetail
            };
        }
    }
}
