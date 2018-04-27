using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using MediationModel;
using LibraryExtensions;
using TelcobrightMediation.Cache;
using TelcobrightMediation.Config;

namespace TelcobrightMediation.Accounting
{
    public class AccountingContext
    {
        private PartnerEntities Context { get; }
        public AccountCache AccountCache { get;}
        public SummaryCache<acc_ledger_summary, ValueTuple<long, DateTime>> LedgerSummaryCache { get;}
        public TransactionCache TransactionCache { get;}
        public ChargeableCache ChargeableCache { get; }
        public AutoIncrementManager AutoIncrementManager { get; }
        public int AccountingLevelOrDepth { get; set; }
        public List<DateTime> DatesInvolved { get; }
        private DbCommand Cmd { get; }
        private int SegmentSizeforDbWrite { get; }
        private AbstractSummaryFactory<acc_transaction> LedgerSummaryFactory { get; }
            =new LedgerSummaryFactory<acc_transaction>();
                
        public AccountingContext(PartnerEntities context, int accountingLevelOrDepth,
            AutoIncrementManager autoIncrementManager, List<DateTime> datesInvolved, int segmentSizeForDbWrite)
        {
            this.Context = context;
            this.Cmd = ConnectionManager.CreateCommandFromDbContext(this.Context);
            this.AccountingLevelOrDepth = accountingLevelOrDepth;
            this.AutoIncrementManager = autoIncrementManager;
            this.DatesInvolved = datesInvolved;
            this.SegmentSizeforDbWrite = segmentSizeForDbWrite;
            Func<acc_transaction, string> whereBuilderTransaction =
                transaction => $@" where uniquebillid={transaction.uniqueBillId.EncloseWithSingleQuotes()} 
                                              and id= {transaction.id.ToString()}
                                              and transactiontime= {
                        transaction.transactionTime.ToMySqlStyleDateTimeStrWithQuote()
                    }";
            this.TransactionCache = new TransactionCache(e => e.id.ToString(),
                e => e.GetExtInsertValues(),
                e => e.GetUpdateCommand(whereBuilderTransaction), e => e.GetDeleteCommand(whereBuilderTransaction));
            this.TransactionCache.PopulateCache(() => new Dictionary<string, acc_transaction>());
            this.AccountCache = new AccountCache(e => e.accountName,
                e => e.GetExtInsertValues(), acc=>acc.GetUpdateCommand(sameAcc=>$@" where id={sameAcc.id}"), null);
            this.AccountCache.PopulateCache(() => context.accounts.ToDictionary(c => c.accountName));
            this.ChargeableCache = new ChargeableCache(c => c.id.ToString(), c => c.GetExtInsertValues(), null,
                e=>e.GetDeleteCommand(c => $@"where uniquebillid={c.uniqueBillId.EncloseWithSingleQuotes()} 
                                     and id={c.id.ToString()} and transactionTime={
                        c.transactionTime.ToMySqlStyleDateTimeStrWithQuote()
                    }"));
            this.ChargeableCache.PopulateCache(() => new Dictionary<string, acc_chargeable>());
            this.LedgerSummaryCache=new SummaryCache<acc_ledger_summary, ValueTuple<long, DateTime>>(
                entityName: "acc_ledger_summary",dictionaryKeyGenerator: c=>c.GetTupleKey(),
                autoIncrementManager:this.AutoIncrementManager,
                insertCommandGenerator: c=>c.GetExtInsertValues(),
                updateCommandGenerator: c=>c.GetUpdateCommand(l=>$@" where id={l.id} 
                                    and transactiondate={l.transactionDate.ToMySqlStyleDateTimeStrWithQuote()}"),
                deleteCommandGenerator: null);
            
            if (this.AccountCache == null || this.LedgerSummaryCache == null)
                throw new Exception("Null Accounting or balance or ledgerSummaryCache!");
        }

        public void PopulatePrevLedgerSummary()
        {
            TimeWiseSummaryCachePopulator<acc_ledger_summary, ValueTuple<long, DateTime>> ledgerSummaryPopulator
                = new TimeWiseSummaryCachePopulator<acc_ledger_summary, ValueTuple<long, DateTime>>
                    (this.LedgerSummaryCache, this.Context, "transactionDate", this.DatesInvolved);
            ledgerSummaryPopulator.Populate();
        }
        public void WriteAllChanges()
        {
            this.ChargeableCache.WriteAllChanges(this.Cmd,this.SegmentSizeforDbWrite);
            this.AccountCache.WriteAllChanges(this.Cmd, this.SegmentSizeforDbWrite);
            this.TransactionCache.WriteAllChanges(this.Cmd, this.SegmentSizeforDbWrite);
            this.LedgerSummaryCache.WriteAllChanges(this.Cmd, this.SegmentSizeforDbWrite);
        }

        public void ExecuteTransactions(IEnumerable<acc_transaction> transactions)
        {
            foreach (var accTransaction in transactions)
            {
                this.ExecuteTransaction(accTransaction);
            }
        }

        public void ExecuteTransaction(acc_transaction transaction)
        {
            string accName = "";
            this.AccountCache.IndexById.TryGetValue(transaction.glAccountId.ToString(), out accName);
            if (string.IsNullOrEmpty(accName))
                throw new Exception("Target account id not found in IndexById in AccountCache.");
            CachedItem<string, account> cAccount = this.AccountCache.GetItemByKey(accName);
            account acc = cAccount?.Entity;
            if (cAccount == null || acc == null) throw new Exception("Target account not found for transaction.");

            this.TransactionCache.InsertWithKey(transaction, transaction.id.ToString(), trans => trans.id > 0);
            acc.ExecuteTransaction(transaction);
            this.AccountCache.AddExternallyUpdatedEntityToUpdatedItems(acc);
            this.TransactionCache.AddExternallyUpdatedEntityToUpdatedItems(transaction);
            UpdateLedgerSummary(transaction);
        }

        public void UpdateLedgerSummary(acc_transaction transaction)
        {
            acc_ledger_summary ledgerSummary =
                (acc_ledger_summary)this.LedgerSummaryFactory.CreateNewInstance(transaction);
            ledgerSummary.AMOUNT = transaction.amount;
            this.LedgerSummaryCache.Merge(ledgerSummary, SummaryMergeType.Add, sum => sum.id > 0);
        }
    }
}
