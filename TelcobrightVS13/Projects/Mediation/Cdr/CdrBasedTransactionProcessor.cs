using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Cdr
{
    public class CdrBasedTransactionProcessor
    {
        private CdrProcessor CdrProcessor { get; }
        private CdrEraser CdrEraser { get; }
        private CdrJobContext CdrJobContext { get; }
        private int _matchedOldCdrs=0, _matchedOldTransactions=0;
        private List<CdrExt> NewProcessedCdrExts { get; }
        private ConcurrentDictionary<string, CdrExt> OldCdrExts { get; }
        public CdrBasedTransactionProcessor(CdrProcessor cdrProcessor, CdrEraser cdrEraser, CdrJobContext cdrJobContext)
        {
            this.CdrProcessor = cdrProcessor;
            this.CdrEraser = cdrEraser;
            this.CdrJobContext = cdrJobContext;
            this.NewProcessedCdrExts = this.CdrProcessor.CollectionResult.ProcessedCdrExts.ToList();
            this.OldCdrExts = this.CdrEraser?.CollectionResult.ConcurrentCdrExts ??
                                                              new ConcurrentDictionary<string, CdrExt>();
        }

        public List<acc_transaction> ProcessTransactionsIncrementally()
        {
            List<acc_transaction> incrementalTransactions = new List<acc_transaction>();
            if (this.CdrProcessor != null) //new cdr exists
            {
                if (this.CdrEraser == null) //no old cdrs
                {
                    this.NewProcessedCdrExts.SelectMany(c => c.AccWiseTransactionContainers.Values).ToList()
                        .ForEach(transactionContainer =>
                        {
                            transactionContainer.IncrementalTransaction = transactionContainer.NewTransaction.Clone();
                            incrementalTransactions.Add(transactionContainer.IncrementalTransaction);
                        });
                }
                else if (this.CdrEraser != null) //corresponding oldcdrs may exist for a newCdrExt
                {
                    foreach (var newCdrExt in this.NewProcessedCdrExts)
                    {
                        CdrExt oldCdrExt = null;
                        if (this.OldCdrExts.TryGetValue(newCdrExt.UniqueBillId, out oldCdrExt) == false) //no old cdr
                        {
                            foreach (var newTransactionContainer in newCdrExt.AccWiseTransactionContainers.Values)
                            {
                                newTransactionContainer.IncrementalTransaction =
                                    newTransactionContainer.NewTransaction.Clone();
                                incrementalTransactions.Add(newTransactionContainer.IncrementalTransaction);
                            }
                        }
                        else //new+old cdr co-existence
                        {
                            ++this._matchedOldCdrs;
                            foreach (KeyValuePair<long, TransactionContainerForSingleAccount> kv in newCdrExt
                                .AccWiseTransactionContainers)
                            {
                                long accountId = kv.Key;
                                TransactionContainerForSingleAccount newTransactionContainer = kv.Value;
                                TransactionContainerForSingleAccount oldTransactionContainer = null;
                                if (oldCdrExt.AccWiseTransactionContainers.TryGetValue(accountId,
                                        out oldTransactionContainer) == false)
                                {
                                    throw new Exception("OldTransaction container not found in old CdrExt instance.");
                                }
                                this._matchedOldTransactions += oldTransactionContainer.OldTransactions.Count;
                                var incrementalTransaction = newTransactionContainer.NewTransaction.Clone();
                                incrementalTransaction.amount =
                                    newTransactionContainer.NewTransaction.amount -
                                    oldTransactionContainer.OldTransactions.Sum(t => t.amount);
                                newTransactionContainer.IncrementalTransaction = incrementalTransaction;
                                incrementalTransactions.Add(newTransactionContainer.IncrementalTransaction);
                            }
                        }
                    }
                }
            }
            else // new cdr does not exist, cdrEraser case
            {
                if (this.CdrEraser == null)
                    throw new Exception("Both cdrProcessor & cdrEraser are found null while processing transactions.");
                foreach (var oldTransactionContainer in this.OldCdrExts.Values.SelectMany(
                    c => c.AccWiseTransactionContainers.Values))
                {
                    var incrementalTransaction = oldTransactionContainer.OldTransactions.Last().Clone();
                    incrementalTransaction.id =
                        this.CdrJobContext.AutoIncrementManager.GetNewCounter("acc_transaction");
                    incrementalTransaction.amount = (-1) * oldTransactionContainer.OldTransactions.Sum(t => t.amount);
                    incrementalTransactions.Add(incrementalTransaction);
                }
            }
            return incrementalTransactions;
        }

        public void ValidateTransactions(List<acc_transaction> incrementalTransactions)
        {
            if (this._matchedOldCdrs != this.OldCdrExts.Count)
                throw new Exception(
                    "Found number of oldCdr during transaction processing does not match initial count.");
            if (this._matchedOldTransactions != this.OldCdrExts.Values
                    .SelectMany(c => c.AccWiseTransactionContainers.Values).Count())
                throw new Exception(
                    "Found number of old transactions during transaction processing does not match initial count.");
            var sumOfNewTransactionAmounts = this.NewProcessedCdrExts
                .SelectMany(c => c.AccWiseTransactionContainers.Values)
                .Sum(t => t.NewTransaction.amount);
            var sumOfOldTransactionAmounts = this.OldCdrExts.Values.SelectMany(c => c.AccWiseTransactionContainers
                .Values
                .SelectMany(t => t.OldTransactions)).Sum(t => t.amount);
            var sumOfIncrementalTransactionAmounts = incrementalTransactions.Sum(t => t.amount);
            if (sumOfNewTransactionAmounts != sumOfOldTransactionAmounts + sumOfIncrementalTransactionAmounts)
                throw new Exception(
                    "Sum of new transctions amount does not match the sum of old & incremental amounts. ");
        }
    }
}
