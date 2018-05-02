using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Cdr
{
    public class IncrementalTransactionCreator
    {
        private CdrProcessor CdrProcessor { get; }
        private CdrEraser CdrEraser { get; }
        private CdrJobContext CdrJobContext { get; }
        private List<CdrExt> NewSuccessfulCdrExts { get; }
        private ConcurrentDictionary<string, CdrExt> OldSuccessfulCdrExts { get; }
        private List<acc_transaction> IncrementalTransactions{ get; set; }

        public IncrementalTransactionCreator(CdrJob cdrJob)
        {
            this.CdrProcessor = cdrJob.CdrProcessor;
            this.CdrEraser = cdrJob.CdrEraser;
            this.CdrJobContext = cdrJob.CdrJobContext;
            this.NewSuccessfulCdrExts = this.CdrProcessor?.CollectionResult.ProcessedCdrExts
                                            .Where(c => c.Cdr.ChargingStatus == 1).ToList() ?? new List<CdrExt>();

            this.OldSuccessfulCdrExts = new ConcurrentDictionary<string, CdrExt>();
            this.CdrEraser?.CollectionResult.ConcurrentCdrExts.Values.Where(c => c.Cdr.ChargingStatus == 1).ToList()
                .ForEach(c =>
                {
                    if (this.OldSuccessfulCdrExts.TryAdd(c.UniqueBillId, c) == false)
                        throw new Exception("Could not add successful cdrext to concurrent dictionary in " +
                                            "incemental transaction creator, something went wrong.");
                });
        }

        public List<acc_transaction> CreateIncrementalTransactions()
        {
            this.IncrementalTransactions= new List<acc_transaction>();
            if (this.CdrProcessor != null) //new cdr exists
            {
                if (this.CdrEraser == null) //only new cdrs but no old cdrs
                {
                    this.NewSuccessfulCdrExts.Where(c=>c.Cdr.ChargingStatus==1).ToList().ForEach(newCdrExt =>
                    {
                        TransactionMetaDataUpdater transactionMetaDataUpdater = new TransactionMetaDataUpdater(newCdrExt.Cdr);
                        CreateIncTransForNewCdrExtWithNoOldInstance(newCdrExt,transactionMetaDataUpdater);
                    });
                }
                else if (this.CdrEraser != null) //new + old cdr co-existence
                {
                    CreateIncrementalTransActionsForNewAndOldCdrCase();
                }
            }
            else // no new cdr, cdrEraser case
            {
                CreateIncrementalTransactionsForCdrEraser();
            }
            return this.IncrementalTransactions;
        }

        private void CreateIncTransForNewCdrExtWithNoOldInstance(CdrExt newCdrExt, TransactionMetaDataUpdater transactionMetaDataUpdater)
        {
            var cdr = newCdrExt.Cdr;
            foreach (var transactionContainer in newCdrExt.AccWiseTransactionContainers.Values)
            {
                acc_transaction incrementalTransaction = transactionContainer.NewTransaction.Clone();
                transactionContainer.SetIncrementalTransaction(incrementalTransaction, transactionMetaDataUpdater);
                this.IncrementalTransactions.Add(incrementalTransaction);
            }
        }

        private void CreateIncrementalTransactionsForCdrEraser()
        {
            if (this.CdrEraser == null)
                throw new Exception("Both cdrProcessor & cdrEraser are found null while processing transactions.");
            foreach (var oldCdrExt in this.OldSuccessfulCdrExts.Values)
            {
                foreach (var kv in oldCdrExt.AccWiseTransactionContainers)
                {
                    var oldTransactionContainer = kv.Value;
                    var incrementalTransaction = oldTransactionContainer.OldTransactions.Last().Clone();
                    incrementalTransaction.id =
                        this.CdrJobContext.AutoIncrementManager.GetNewCounter("acc_transaction");
                    incrementalTransaction.amount = (-1) * oldTransactionContainer.OldTransactions.Sum(t => t.amount);
                    this.IncrementalTransactions.Add(incrementalTransaction);
                    //no need to update meta data in old cdr, it's going to be deleted anyway.
                }
            }
        }

        private void CreateIncrementalTransActionsForNewAndOldCdrCase()
        {
            foreach (var newCdrExt in this.NewSuccessfulCdrExts)
            {
                CdrExt oldCdrExt = null;
                TransactionMetaDataUpdater transactionMetaDataUpdater =new TransactionMetaDataUpdater(newCdrExt.Cdr);
                if (this.OldSuccessfulCdrExts.TryGetValue(newCdrExt.UniqueBillId, out oldCdrExt) == false) //no old cdr
                {
                    CreateIncTransForNewCdrExtWithNoOldInstance(newCdrExt, transactionMetaDataUpdater);
                }
                else //new+old cdr co-existence
                {
                    foreach (KeyValuePair<long, AccWiseTransactionContainer> kv in newCdrExt
                        .AccWiseTransactionContainers)
                    {
                        long accountId = kv.Key;
                        AccWiseTransactionContainer newTransactionContainer = kv.Value;
                        AccWiseTransactionContainer oldTransactionContainer = null;
                        if (oldCdrExt.AccWiseTransactionContainers.TryGetValue(accountId,
                                out oldTransactionContainer) == false)
                        {
                            throw new Exception("OldTransaction container not found in old CdrExt instance.");
                        }

                        var incTrans = newTransactionContainer.NewTransaction.Clone();
                        incTrans.amount =
                            newTransactionContainer.NewTransaction.amount -
                            oldTransactionContainer.OldTransactions.Sum(t => t.amount);
                        newTransactionContainer.SetIncrementalTransaction(incTrans,transactionMetaDataUpdater);
                    }
                }
            }
        }


        public void ValidateTransactions(List<acc_transaction> incrementalTransactions)
        {
            decimal fractionComparsionTollerance = this.CdrJobContext.MediationContext.CdrSetting
                .FractionalNumberComparisonTollerance;
            var sumOfNewTransactionAmounts = this.NewSuccessfulCdrExts
                .SelectMany(c => c.AccWiseTransactionContainers.Values).Sum(t => t.NewTransaction.amount);
            var sumOfOldTransactionAmounts = this.OldSuccessfulCdrExts.Values.SelectMany(c => c.AccWiseTransactionContainers
                .Values.SelectMany(t => t.OldTransactions)).Sum(t => t.amount);

            decimal sumOfOldTransactionsFromCdrMetaData =
                Convert.ToDecimal(this.OldSuccessfulCdrExts.Values.Sum(c => c.Cdr.TransactionMetaTotal));
            if (Math.Abs(sumOfOldTransactionsFromCdrMetaData - sumOfOldTransactionAmounts) >
                fractionComparsionTollerance)
            {
                throw new Exception("Transaction total of old cdrs does not match the total from old cdrs meta data.");
            }

            var sumOfIncrementalTransactionAmounts = this.IncrementalTransactions.Sum(t => t.amount);
            if (Math.Abs(sumOfNewTransactionAmounts-
                (sumOfOldTransactionAmounts + sumOfIncrementalTransactionAmounts))>fractionComparsionTollerance)
            {
                throw new Exception(
                    "Sum of new transctions amount does not match the sum of old & incremental amounts. ");
            }
        }
    }
}
