using TelcobrightMediation;
using Newtonsoft.Json;
using System.ComponentModel.Composition;
using System.IO;
using TelcobrightFileOperations;
using WinSCP;
using System;
using System.Linq;
using System.Collections.Generic;
using MediationModel;
using TelcobrightMediation.Config;
using TelcobrightMediation.Accounting;

namespace Jobs
{

    [Export("Job", typeof(ITelcobrightJob))]
    public class TempTransactionPosting2 : ITelcobrightJob
    {
        public override string ToString() => this.RuleName;
        public string RuleName => "TransactionPosting";
        public string HelpText => "Posts a transaction to accounting context with automatic update of ledger summary";
        public int Id => 14;
        public JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            TransactionJobInputData input = (TransactionJobInputData) jobInputData;
            List<acc_temp_transaction> tempTransactions = input.TempTransactions;
            List<DateTime> datesInvolved = tempTransactions.Select(t => t.transactionTime.Date).Distinct().ToList();
            int segmentSizeForDbWrite = input.Tbc.CdrSetting.SegmentSizeForDbWrite;
            AutoIncrementManager autoIncrementManager = new AutoIncrementManager(
                    counter => (int) AutoIncrementTypeDictionary.EnumTypes[counter.tableName],
                    counter => counter.GetExtInsertValues(),
                    counter => counter.GetUpdateCommand(
                    c => $@" where tableName='{AutoIncrementTypeDictionary.EnumTypes[counter.tableName]}'"),
                    null, input.Context.Database.Connection.CreateCommand(), segmentSizeForDbWrite);
            AccountingContext accountingContext = new AccountingContext(input.Context,0,autoIncrementManager,
                datesInvolved,segmentSizeForDbWrite);
            accountingContext.ExecuteTransactions(
               tempTransactions.Select(TempTransactionHelper.ConvertTempTransactionToTransaction));
            return JobCompletionStatus.Complete;
        } 
    }
}
