using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Cdr
{
    public class CdrJob : ISegmentedJob
    {
        public CdrProcessor CdrProcessor { get; }
        public CdrEraser CdrEraser { get; }
        public int ActualStepsCount { get; }
        public CdrJobContext CdrJobContext { get; }

        public CdrJob(CdrProcessor cdrProcessor, CdrEraser cdrEraser, int actualStepsCount)
        {
            this.CdrJobContext = cdrProcessor?.CdrJobContext ?? cdrEraser.CdrJobContext;
            this.CdrProcessor = cdrProcessor;
            this.CdrEraser = cdrEraser;
            this.ActualStepsCount = actualStepsCount;
        }

        public void Execute()
        {
            this.CdrEraser?.UndoOldSummaries();
            this.CdrEraser?.UndoOldChargeables();
            this.CdrEraser?.DeleteOldCdrs();

            this.CdrProcessor?.Process();

            this.ProcessTransactionsIncrementally();

            this.CdrProcessor?.WriteCdrs();
            foreach (var summaryCache in this.CdrJobContext.CdrSummaryContext.TableWiseSummaryCache.Values)
            {
                summaryCache.WriteAllChanges(this.CdrJobContext.DbCmd, this.CdrJobContext.SegmentSizeForDbWrite);
            }
            this.CdrJobContext.AccountingContext.WriteAllChanges();
            this.CdrJobContext.AutoIncrementManager.WriteState();
        }

        public void ProcessTransactionsIncrementally()
        {
            var newProcessedCdrExts = this.CdrProcessor.CollectionResult.ProcessedCdrExts.ToList();
            var oldCdrExts = this.CdrEraser?.CollectionResult.ConcurrentCdrExts ??
                             new ConcurrentDictionary<string, CdrExt>();
            int matchedOldCdrs = 0;
            int matchedOldTransactions = 0;
            List<acc_transaction> incrementalTransactions = new List<acc_transaction>();
            if (this.CdrProcessor != null) //new cdr exists
            {
                if (this.CdrEraser == null) //no old cdrs
                {
                    newProcessedCdrExts.SelectMany(c=>c.AccWiseTransactionContainers.Values).ToList()
                        .ForEach(transactionContainer =>
                        {
                            var incrementalTransaction = transactionContainer.NewTransaction.Clone();
                            transactionContainer.IncrementalTransaction = incrementalTransaction;
                            incrementalTransactions.Add(incrementalTransaction);
                        });
                }
                else if (this.CdrEraser != null) //corresponding oldcdrs may exist for a newCdrExt
                {
                    foreach (var transactionContainer in newProcessedCdrExts.SelectMany(c => c
                        .AccWiseTransactionContainers.Values))
                    {
                        //var incrementalTransaction =
                        //    GetIncrementalTransactionsForNewOldCdrCoExistenceCase(c, oldCdrExts,
                        //        ref matchedOldCdrs,
                        //        ref matchedOldTransactions);
                        //incrementalTransactions.Add(incrementalTransaction);
                    }
                }
            }
            else // new cdr does not exist, cdrEraser case
            {
                if (this.CdrEraser == null)
                    throw new Exception("Both cdrProcessor & cdrEraser are found null while processing transactions.");
                foreach (TransactionContainerForSingleAccount oldTransactionContainer in oldCdrExts.Values.SelectMany(
                    c => c.AccWiseTransactionContainers.Values))
                {
                    var incrementalTransaction = oldTransactionContainer.OldTransactions.Last().Clone();
                    incrementalTransaction.id =
                        this.CdrJobContext.AutoIncrementManager.GetNewCounter("acc_transaction");
                    incrementalTransaction.amount = (-1) * oldTransactionContainer.OldTransactions.Sum(t => t.amount);
                    incrementalTransactions.Add(incrementalTransaction);
                }
            }
            ValidateTransactions(newProcessedCdrExts, oldCdrExts, matchedOldCdrs, matchedOldTransactions, incrementalTransactions);
            this.CdrJobContext.AccountingContext.ExecuteTransactions(incrementalTransactions);
        }

        private static acc_transaction GetIncrementalTransactionsForNewOldCdrCoExistenceCase(
            TransactionContainerForSingleAccount newTransactionContainer,
            TransactionContainerForSingleAccount oldTrasactionContainer, ref int matchedOldCdrs,
            ref int matchedOldTransactions)
        {
            //if (oldCdrExt == null) //old cdr doesn't exist
            //{
            //    var incrementalTransaction = transactionContainer.NewTransaction.Clone();
            //    transactionContainer.IncrementalTransaction = incrementalTransaction;
            //    return incrementalTransaction;
            //}
            //else //matched oldCdr found for this newcdrext
            //{
            //    ++matchedOldCdrs;
            //    long accountId = kv
            //    TransactionContainerForSingleAccount oldTransactionContainer = null;
            //    if (oldCdrExt.AccWiseTransactionContainers.TryGetValue(accountId,
            //            out oldTransactionContainer) == false)
            //    {
            //        throw new Exception("OldTransaction container not found in old CdrExt instance.");
            //    }
            //    ++matchedOldTransactions;
            //    var newTransaction = newTransactionContainer.NewTransaction.Clone();
            //    newTransactionContainer.IncrementalTransaction = newTransaction;
            //    newTransactionContainer.IncrementalTransaction.amount =
            //        newTransactionContainer.NewTransaction.amount -
            //        oldTransactionContainer.OldTransactions.Sum(t => t.amount);
            //}
            //return newProcessedCdrExts.SelectMany(c => c.AccWiseTransactionContainers.Values)
            //    .Select(t => t.IncrementalTransaction).ToList();
            return new acc_transaction();
        }

        private static void ValidateTransactions(List<CdrExt> newProcessedCdrExts, ConcurrentDictionary<string, CdrExt> oldCdrExts, int matchedOldCdrs, int matchedOldTransactions,
            List<acc_transaction> incrementalTransactions)
        {
            if (matchedOldCdrs != oldCdrExts.Count)
                throw new Exception(
                    "Found number of oldCdr during transaction processing does not match initial count.");
            if (matchedOldTransactions != oldCdrExts.Values.SelectMany(c => c.AccWiseTransactionContainers.Values).Count())
                throw new Exception("Found number of old transactions during transaction processing does not match initial count.");
            var sumOfNewTransactionAmounts = newProcessedCdrExts.SelectMany(c => c.AccWiseTransactionContainers.Values)
                .Sum(t => t.NewTransaction.amount);
            var sumOfOldTransactionAmounts = oldCdrExts.Values.SelectMany(c => c.AccWiseTransactionContainers.Values
                .SelectMany(t => t.OldTransactions)).Sum(t => t.amount);
            var sumOfIncrementalTransactionAmounts = incrementalTransactions.Sum(t => t.amount);
            if (sumOfNewTransactionAmounts != sumOfOldTransactionAmounts + sumOfIncrementalTransactionAmounts)
                throw new Exception(
                    "Sum of new transctions amount does not match the sum of old & incremental amounts. ");
        }
    }
}
