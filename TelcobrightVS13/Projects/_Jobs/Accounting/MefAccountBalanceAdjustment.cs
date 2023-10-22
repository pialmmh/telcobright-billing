using TelcobrightMediation;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using LibraryExtensions;
using TelcobrightFileOperations;
using MediationModel;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Config;
using TelcobrightMediation;
namespace Jobs
{

    [Export("Job", typeof(ITelcobrightJob))]
    public class MefAccountBalanceAdjustment : ITelcobrightJob
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "Account balance adjustment job";
        public int Id => 13;
        public object Execute(ITelcobrightJobInput jobInputData)
        {
            InvoiceGenerationInputData invoiceGenerationInputData = (InvoiceGenerationInputData)jobInputData;
            BalanceAdjustmentHelper balanceAdjustmentHelper = new BalanceAdjustmentHelper(invoiceGenerationInputData);
            BalanceAdjustmentPostProcessingData adjustmentPostProcessingData = balanceAdjustmentHelper.Process();
            WriteToDb(adjustmentPostProcessingData);
            return JobCompletionStatus.Complete;
        }

        public object PreprocessJob(ITelcobrightJobInput jobInputData)
        {
            throw new NotImplementedException();
        }

        public object PostprocessJob(ITelcobrightJobInput jobInputData)
        {
            throw new NotImplementedException();
        }

        private void WriteToDb(BalanceAdjustmentPostProcessingData adjustmentPostProcessingData)
        {
            InvoiceGenerationInputData invoiceGenerationInputData =
                adjustmentPostProcessingData.InvoiceGenerationInputData;
            var context = invoiceGenerationInputData.Context;
            var cmd = context.Database.Connection.CreateCommand();
            cmd.CommandText = $"update account set balanceBefore = {adjustmentPostProcessingData.BalanceBefore}, " +
                              $"lastAmount = null, balanceAfter = {adjustmentPostProcessingData.BalanceAfter} " +
                              $"where id = {adjustmentPostProcessingData.AccountId}; ";
            cmd.ExecuteNonQuery();
        }

        private Action<object> actionOnFinish = jobInput =>
        {
            var input = (AccountingJobInputData) jobInput;
            var context = input.Context;
            var cmd = context.Database.Connection.CreateCommand();
            cmd.CommandText = $"select jobstate from job where id={input.TelcobrightJob.id}";
            string json = (string) cmd.ExecuteScalar();
            var jobStateMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            decimal finalInvoicedAmount = Convert.ToDecimal(jobStateMap["invoicedAmountAfterLastSegment"]);
            //Create Account posting job here
        };
    }
}


