using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using System.Linq;
using LibraryExtensions;
using MediationModel;
using Newtonsoft.Json;
using TelcobrightMediation.Accounting;

namespace InvoiceGenerationRules
{

    [Export("InvoiceGenerationRule", typeof(IInvoiceGenerationRule))]
    public class InvoiceGenerationByBatchJob : IInvoiceGenerationRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public string HelpText => "Generate invoice from raw transactions in batches.";
        public int Id => 1;
        public void Execute(object data)
        {
            InvoiceGenerationData invoiceData = (InvoiceGenerationData) data;
            InvoiceDataCollector invoiceDataCollector = invoiceData.InvoiceDataCollector;
            PartnerEntities context = invoiceData.Context;
            int batchSizeForJobSegments = invoiceData.BatchSizeForJobSegment;
            job invoicingJob = CreateInvoiceGenerationJob(invoiceDataCollector, context,
                batchSizeForJobSegments);
            context.jobs.Add(invoicingJob);
            context.SaveChanges();
        }
        protected job CreateInvoiceGenerationJob(InvoiceDataCollector invoiceDataCollector,
            PartnerEntities context, int batchSizeForJobSegment)
        {
            string serviceAccount = invoiceDataCollector.ServiceAccount;
            int jobDefinition = 12;
            int prevJobCountWithSameName =
                context.jobs.Count(j => j.idjobdefinition == jobDefinition && j.idjobdefinition == jobDefinition);
            string sourceTable = "acc_transaction";
            Dictionary<string, string> jobParamsMap = new Dictionary<string, string>();
            jobParamsMap.Add("sourceTable", sourceTable);
            jobParamsMap.Add("serviceAccountId", invoiceDataCollector.AccountId.ToString());
            if (prevJobCountWithSameName > 0)
            {
                serviceAccount = serviceAccount + "_" + prevJobCountWithSameName;
            }
            string jobName = invoiceDataCollector.PartnerName + "/" + serviceAccount
                             + "/" + invoiceDataCollector.StartDateTimeLocal.ToMySqlStyleDateTimeStrWithoutQuote()
                             + " to " +
                             invoiceDataCollector.EndDateTimeLocal.ToMySqlStyleDateTimeStrWithoutQuote();


            List<SqlSingleWhereClauseBuilder> singleWhereClauses = new List<SqlSingleWhereClauseBuilder>();
            List<SqlMultiWhereClauseBuilder> multipleWhereClauses = new List<SqlMultiWhereClauseBuilder>();
            SqlSingleWhereClauseBuilder newParam = null;
            newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.FirstBeforeAndOr);
            newParam.Expression = "transactionTime>=";
            newParam.ParamType = SqlWhereParamType.Datetime;
            newParam.ParamValue = invoiceDataCollector.StartDateTimeLocal.ToMySqlStyleDateTimeStrWithoutQuote();
            singleWhereClauses.Add(newParam);

            newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
            newParam.Expression = "transactionTime<=";
            newParam.ParamType = SqlWhereParamType.Datetime;
            newParam.ParamValue = invoiceDataCollector.EndDateTimeLocal.ToMySqlStyleDateTimeStrWithoutQuote();
            singleWhereClauses.Add(newParam);

            long glAccountId = invoiceDataCollector.AccountId;
            newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
            newParam.Expression = "glAccountId=";
            newParam.ParamType = SqlWhereParamType.Numeric;
            newParam.ParamValue = glAccountId.ToString();
            singleWhereClauses.Add(newParam);

            newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
            newParam.Expression = "isBillable=";
            newParam.ParamType = SqlWhereParamType.Numeric;
            newParam.ParamValue = "1";
            singleWhereClauses.Add(newParam);

            newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
            newParam.Expression = "isBilled is null or isBilled<>";
            newParam.ParamType = SqlWhereParamType.Numeric;
            newParam.ParamValue = "1";
            singleWhereClauses.Add(newParam);

            newParam = new SqlSingleWhereClauseBuilder(SqlWhereAndOrType.And);
            newParam.Expression = "cancelled is null or cancelled<>";
            newParam.ParamType = SqlWhereParamType.Numeric;
            newParam.ParamValue = "1";
            singleWhereClauses.Add(newParam);

            BatchSqlJobParamJson sqlParam = new BatchSqlJobParamJson
            (
                sourceTable,
                batchSizeForJobSegment,
                singleWhereClauses,
                multipleWhereClauses,
                columnExpressions: new List<string>() { "id as RowId", "transactionTime as RowDateTime" }
            );

            int jobPriority = context.enumjobdefinitions.Where(j => j.id == 12).Select(j => j.Priority).First();
            jobParamsMap.Add("sqlParam", JsonConvert.SerializeObject(sqlParam));
            job newjob = new job();
            newjob.Progress = 0;
            newjob.idjobdefinition = 12; //invoicing job
            newjob.Status = 6; //created
            newjob.JobName = jobName;
            newjob.idjobdefinition = jobDefinition;
            newjob.CreationTime = DateTime.Now;
            newjob.idNE = 0;
            newjob.JobParameter = JsonConvert.SerializeObject(jobParamsMap);
            newjob.priority = jobPriority;
            return newjob;
        }
    }
}
