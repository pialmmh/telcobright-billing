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
using LibraryExtensions;
using TelcobrightFileOperations;
using MediationModel;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Config;

namespace Jobs
{

    [Export("Job", typeof(ITelcobrightJob))]
    public class TransactionInvoicingJob : ITelcobrightJob
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        private AccountingJobInputData Input { get; set; }
        public string HelpText => "Cdr based invoice generation job";
        public int Id => 12;
        public JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            this.Input = (AccountingJobInputData) jobInputData;
            SegmentedCdrInvoicingJobProcessor segmentedInvoiceProcessor =
                new SegmentedCdrInvoicingJobProcessor(this.Input, "id", "transactiontime");
            if (this.Input.TelcobrightJob.Status != 2) //prepare job if not prepared already
                segmentedInvoiceProcessor.PrepareSegments();
            List<jobsegment> jobsegments =
                segmentedInvoiceProcessor.ExecuteIncompleteSegments();
            segmentedInvoiceProcessor.FinishJob(jobsegments, null); //mark job as complete
            return JobCompletionStatus.Complete;
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


