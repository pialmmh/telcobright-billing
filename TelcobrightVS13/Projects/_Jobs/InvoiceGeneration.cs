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
using TelcobrightMediation.Config;

namespace Jobs
{

    [Export("Job", typeof(ITelcobrightJob))]
    public class InvoiceGeneration : ITelcobrightJob
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        private CdrCollectorInputData CdrCollectorInputData { get; set; }
        public string HelpText =>
            "Cdr based invoice generation job";

        public int Id => 17;

        public JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            CdrJobInputData input = (CdrJobInputData) jobInputData;
            this.CdrCollectorInputData = new CdrCollectorInputData(input, "");
            SegmentedInvoiceGenerator segmentedInvoiceGenerator=
                new SegmentedInvoiceGenerator(this.CdrCollectorInputData,
                    input.CdrSetting.BatchSizeWhenPreparingLargeSqlJob, "id", "transactiontime");
            if (input.TelcobrightJob.Status != 2) //prepare job if not prepared already
                segmentedInvoiceGenerator.PrepareSegments();
            List<jobsegment> jobsegments = segmentedInvoiceGenerator.ExecuteIncompleteSegments(UpdateJobStateAfterSegment);
            segmentedInvoiceGenerator.FinishJob(jobsegments,null); //mark job as complete
            return JobCompletionStatus.Complete;
        }

        private void UpdateJobStateAfterSegment(object jobData)
        {
            Dictionary<string, string> jobDataAsMap = jobData as Dictionary<string, string>;
            Dictionary<string,string> newJobStateAsMap=new Dictionary<string, string>();
            if (jobDataAsMap != null)
            {
                int segmentNumber = Convert.ToInt32(jobDataAsMap["segmentNumber"]);
                if (segmentNumber<=0)
                {
                    throw new Exception("Segment number must be > 0.");
                }
                decimal invoicedAmountAfterLastSegment =
                    Convert.ToDecimal(jobDataAsMap["invoicedAmountAfterLastSegment"]);
                var cmd = this.CdrCollectorInputData.Context.Database.Connection.CreateCommand();
                newJobStateAsMap.Add("segmentNumber",segmentNumber.ToString());
                newJobStateAsMap.Add("invoicedAmountAfterLastSegment",invoicedAmountAfterLastSegment.ToString());
                newJobStateAsMap.Add("lastSegmentExecutedOn", DateTime.Now.ToMySqlStyleDateTimeStrWithoutQuote());
                cmd.CommandText = $"update job set jobstate='{JsonConvert.SerializeObject(newJobStateAsMap)}'";
            }
            else
            {
                throw new Exception("Job data cannot be null while " +
                                    "updating job state.");
            }
        }
    }
}


