using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            this.CdrEraser?.WriteChangesExceptContext();

            this.CdrJobContext.CreateIncrementalTransactions();
            accountingContext.ExecuteTransactions(processedCdrExt.Transactions);

            this.CdrProcessor?.Process();
            this.CdrProcessor?.WriteChangesExceptContext();
            this.CdrJobContext.WriteChanges();
        }

        void CreateIncrementalTransactions()
        {
            var billIdAccountIdWiseOldTransactions = this.CdrEraser?.CollectionResult.ConcurrentCdrExts.Values
                                      .ToDictionary(c => c.UniqueBillId,
                                          c => c.Transactions.ToDictionary(t => t.glAccountId)) ??
                                        new Dictionary<string, Dictionary<long, acc_transaction>>();
                                  
            var billIdAccountIdWiseNewTransactions = this.CdrProcessor?.CollectionResult.ConcurrentCdrExts.Values
                                      .ToDictionary(c => c.UniqueBillId,
                                          c => c.Transactions.ToDictionary(t => t.glAccountId)) ??
                                        new Dictionary<string, Dictionary<long, acc_transaction>>();

            if (this.CdrProcessor!=null)//new cdr exists
            {
                if(this.CdrEraser == null)//no old cdrs
                {
                    foreach (var newCdrExt in this.CdrProcessor.CollectionResult.ConcurrentCdrExts.Values)
                    {
                        newCdrExt.Transactions.ForEach(t=> newCdrExt.IncrementalTransactions.Add(t));
                    }
                }
                else if (this.CdrEraser!=null)//oldcdrs exist
                {
                    foreach (var newCdrExt in this.CdrProcessor.CollectionResult.ConcurrentCdrExts.Values)
                    {
                        var oldCdrExt = this.CdrEraser.CollectionResult.ConcurrentCdrExts[newCdrExt.UniqueBillId];
                        foreach (var newTransaction in newCdrExt.Transactions)
                        {
                            
                        }
                    }
                }
            }

            foreach (var transaction in oldCdrExt.Transactions)
            {
                acc_transaction cancelledTransaction = this.MarkPrevTransactionAsCancelled(transaction);
                this.CdrJobContext.AccountingContext.TransactionCache
                    .AddExternallyUpdatedEntityToUpdatedItems(cancelledTransaction);
                acc_transaction reversedTransaction = this.CreateReversedTransactions(
                    transaction, this.CdrJobContext.TelcobrightJob.id);
                this.CdrJobContext.AccountingContext.ExecuteTransaction(reversedTransaction);
            }
        }
    }
}
