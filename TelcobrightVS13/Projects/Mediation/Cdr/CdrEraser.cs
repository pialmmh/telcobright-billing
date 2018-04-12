using LibraryExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TelcobrightMediation.Accounting;
using MediationModel;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.ApplicationServices;
using FlexValidation;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;
using TelcobrightMediation.EntityHelpers;
using TelcobrightMediation.Mediation.Cdr;
using TransactionTuple = System.ValueTuple<int, int, long, int>;
using CdrSummaryTuple = System.ValueTuple<int, int, int, string, string, decimal, decimal, System.ValueTuple<string, string, string, string, string, string, string, System.ValueTuple<string, string, string, string, string, string>>>;
namespace TelcobrightMediation
{
    public class CdrEraser
    {
        public CdrJobContext CdrJobContext { get; }
        private CdrCollectionResult CollectionResult { get; }
        private MediationContext MediationContext => this.CdrJobContext.MediationContext;
        public CdrEraser(CdrJobContext cdrJobContext,CdrCollectionResult newCollectionResult)
        {
            this.CdrJobContext = cdrJobContext;
            this.CollectionResult = newCollectionResult;
        }

        public void Process()
        {
            if (!this.CollectionResult.ConcurrentCdrExts.Any()) return;

            var oldCdrExts = this.CollectionResult.ConcurrentCdrExts.Values.ToList();
            this.CdrJobContext.AccountingContext.TransactionCache.PopulateCache(
                () => oldCdrExts.SelectMany(c => c.Transactions).ToDictionary(trans => trans.id.ToString()));
            //oldCdrExts.ForEach(oldCdrExt=>//create old summaries
            Parallel.ForEach(oldCdrExts,oldCdrExt=>
            {
                if (oldCdrExt.CdrNewOldType != CdrNewOldType.OldCdr)
                    throw new Exception("OldCdrs must have CdrNewOldtype status set to old.");
                List<string> summaryTargetTables = this.MediationContext.MefServiceGroupContainer
                    .IdServiceGroupWiseServiceGroups[Convert.ToInt32(oldCdrExt.Cdr.CallDirection)].GetSummaryTargetTables().Keys.ToList();
                summaryTargetTables.ForEach(targetTableName =>
                {
                    AbstractCdrSummary regeneratedSummary = (AbstractCdrSummary)this.CdrJobContext.CdrSummaryContext
                                        .TargetTableWiseSummaryFactory[targetTableName].CreateNewInstance(oldCdrExt);
                    //oldCdrExt.TableWiseSummaries.Add(targetTableName, recreatedSummary);
                    this.CdrJobContext.CdrSummaryContext.MergeSubstractSummary(targetTableName, regeneratedSummary);
                });
                foreach (var transaction in oldCdrExt.Transactions)
                {
                    acc_transaction cancelledTransaction = this.MarkPrevTransactionAsCancelled(transaction);
                    this.CdrJobContext.AccountingContext.TransactionCache
                        .AddExternallyUpdatedEntityToUpdatedItems(cancelledTransaction);
                    acc_transaction reversedTransaction = this.CreateReversedTransactions(
                        transaction, this.CdrJobContext.TelcobrightJob.id);
                    this.CdrJobContext.AccountingContext.ExecuteTransaction(reversedTransaction);
                }
            });
            var oldChargeables = oldCdrExts.SelectMany(c => c.Chargeables.Values).ToList();
            this.CdrJobContext.AccountingContext.ChargeableCache
                .PopulateCache(() => oldChargeables.ToDictionary(chargeable => chargeable.id.ToString()));
            this.CdrJobContext.AccountingContext.ChargeableCache.DeleteAll();
        }

        public void WriteChangesExceptContext()
        {
            int delCount=OldCdrDeleter.DeleteOldCdrs("cdr", this.CollectionResult.ConcurrentCdrExts.Values
                .Select(c => new KeyValuePair<long, DateTime>(c.Cdr.idcall, c.StartTime)).ToList(),
                this.CdrJobContext.SegmentSizeForDbWrite,this.CdrJobContext.DbCmd);
            if(delCount!=this.CollectionResult.RawCount)
                throw new Exception("Deleted number of cdrs do not match raw count in collection result.");
        }

        acc_transaction MarkPrevTransactionAsCancelled(acc_transaction oldTransaction)
        {
            if (oldTransaction.cancelled == 1)
                throw new Exception("Prev transaction cannot have cancelled status=1, could be erroneous coding.");
            oldTransaction.cancelled = 1;
            oldTransaction.isBillable = null;
            oldTransaction.isBilled = null;
            return oldTransaction;
        }

        acc_transaction CreateReversedTransactions(acc_transaction oldTrans,long idTelcobrightJob)
        {
            {
                if (oldTrans.cancelled != 1)
                    throw new Exception(
                        "Reversed transactions can only be created for transaction marked as 'cancelled'.");
                var newTrans = new acc_transaction();
                newTrans.id =
                    this.CdrJobContext.AccountingContext.AutoIncrementManager.GetNewCounter("acc_transaction");
                newTrans.transactionTime = oldTrans.transactionTime;
                if (oldTrans.debitOrCredit == "c")
                {
                    newTrans.debitOrCredit = "d";
                }
                else if (oldTrans.debitOrCredit == "d")
                {
                    newTrans.debitOrCredit = "c";
                }
                else throw new ArgumentOutOfRangeException();
                newTrans.idEvent = oldTrans.idEvent;
                newTrans.uniqueBillId = oldTrans.uniqueBillId;
                newTrans.description = "reversed";
                newTrans.glAccountId = oldTrans.glAccountId;
                newTrans.amount = (-1) * oldTrans.amount;
                newTrans.uomId = oldTrans.uomId;
                newTrans.isBillable = null;
                newTrans.isPrepaid = oldTrans.isPrepaid;
                newTrans.isBilled = null;
                newTrans.cancelled = 1;
                newTrans.createdByJob = idTelcobrightJob;
                newTrans.changedByJob = idTelcobrightJob;
                newTrans.jsonDetail = "";
                return newTrans;
            }
        }
    }

}
