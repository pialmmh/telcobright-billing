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
using TelcobrightMediation;
namespace Jobs
{

    [Export("Job", typeof(ITelcobrightJob))]
    public class MefInvoicingJob : ITelcobrightJob
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        private InvoiceGenerationInputData Input { get; set; }
        public string HelpText => "Invoice generation job";
        public int Id => 12;
        public JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            this.Input = (InvoiceGenerationInputData)jobInputData;
            IServiceGroup serviceGroup = GetServiceGroup();
            InvoiceGenerationHelper invoiceGenerationHelper = new InvoiceGenerationHelper(this.Input,
                serviceGroup.ExecInvoicePreProcessing, serviceGroup.ExecInvoicePostProcessing);
            invoiceGenerationHelper.ExecInvoicePreProcessing();
            InvoicePostProcessingData invoicePostProcessingData= invoiceGenerationHelper.GenerateInvoice();
            invoiceGenerationHelper.ExecInvoicePostProcessing(invoicePostProcessingData);
            return JobCompletionStatus.Complete;
        }

        private IServiceGroup GetServiceGroup()
        {
            PartnerEntities context = this.Input.Context;
            job telcobrightJob = this.Input.TelcobrightJob;
            Dictionary<string, string> jobParamsMap =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(telcobrightJob.jobsegments.Single()
                    .SegmentDetail);
            long serviceAccountId = Convert.ToInt64(jobParamsMap["serviceAccountId"]);
            int idServiceGroup = context.accounts.Where(c => c.id == serviceAccountId)
                .ToList().Single().serviceGroup;
            IServiceGroup serviceGroup = null;
            this.Input.ServiceGroups.TryGetValue(idServiceGroup, out serviceGroup);
            if (serviceGroup==null)
            {
                throw new Exception("Could not find service group.");
            }
            return serviceGroup;
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


