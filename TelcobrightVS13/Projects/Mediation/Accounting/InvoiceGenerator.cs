using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using Newtonsoft.Json;

namespace TelcobrightMediation.Accounting
{
    public class InvoiceGenerator
    {
        protected InvoiceGenerationInputData InvoiceGenerationInputData { get; set; }
        protected invoice GeneratedInvoice { get; private set; }
        protected Action<InvoiceGenerationInputData> InvoicePreProcessingAction { get; set; }
        protected Action<InvoiceGenerationInputData> InvoicePostProcessingAction { get; set; }
        protected DbCommand Cmd { get; set; }

        public InvoiceGenerator(InvoiceGenerationInputData invoiceGenerationInputData,
            Action<InvoiceGenerationInputData> invoicePreProcessingAction, 
            Action<InvoiceGenerationInputData> invoicePostProcessingAction)
        {
            this.InvoiceGenerationInputData = invoiceGenerationInputData;
            this.InvoicePreProcessingAction = invoicePreProcessingAction;
            this.InvoicePostProcessingAction = invoicePostProcessingAction;
            this.Cmd = this.InvoiceGenerationInputData.Context.Database.Connection.CreateCommand();
        }
        public virtual invoice GenerateInvoice()
        {
            InvoiceDataCollector invoiceDataCollector = this.InvoiceGenerationInputData.InvoiceDataCollector;
            PartnerEntities context = this.InvoiceGenerationInputData.Context;
            int batchSizeForJobSegments = this.InvoiceGenerationInputData.BatchSizeForJobSegment;
            job telcobrightJob = this.InvoiceGenerationInputData.TelcobrightJob;
            Dictionary<string, string> jobParamsMap =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(telcobrightJob.jobsegments.Single()
                    .SegmentDetail);
            long serviceAccountId = Convert.ToInt64(jobParamsMap["serviceAccountId"]);
            DateTime startDate = Convert.ToDateTime(jobParamsMap["startDate"]);
            DateTime endDate = Convert.ToDateTime(jobParamsMap["endDate"]);
            int timeZoneOffsetSecForInvoicing = Convert.ToInt32(jobParamsMap["timeZoneOffsetSec"]);
            int timeZoneIdInTelcobrightConfig = this.InvoiceGenerationInputData.Tbc.DefaultTimeZoneId;
            timezone configuredTimeZone = context.timezones.Where(c => c.id == timeZoneIdInTelcobrightConfig).ToList()
                .Single();
            if (timeZoneOffsetSecForInvoicing != configuredTimeZone.gmt_offset)
            {
                throw new Exception($"Timezone must be {configuredTimeZone.abbreviation}");
            }
            decimal ledgerSummaryAmount = context.acc_ledger_summary.Where(c => c.id == serviceAccountId).Sum(c => c.AMOUNT);
            decimal tempTransactionAmount = context.acc_temp_transaction.Where(c => c.glAccountId == serviceAccountId)
                .Sum(c => c.amount);
            decimal invoiceAmount = ledgerSummaryAmount + tempTransactionAmount;
            if (invoiceAmount <= 0)
            {
                throw new Exception("Account balance [= ledger summary+temp transaction amount] " +
                                    " must be >0");
            }

            invoice newInvoice = new invoice()
            {
                BILLING_ACCOUNT_ID = serviceAccountId,
            };
            //this.Cmd.CommandText = $"insert into invoice (billing_account_id,description) values(" +
            //                       $"{this.InvoiceGenerationInputData.ServiceAccountId.ToString()},'{this.Description}');";
            //this.Cmd.ExecuteNonQuery();
            //this.Cmd.CommandText = "last_insert_id();";
            //long generatedInvoiceId = (long)Cmd.ExecuteScalar();

            //this.Cmd.CommandText = $"insert into invoice_item " +
            //                       $"(invoice_id,product_id,uom_Id,quantity,amount) values (" +
            //                       $"{generatedInvoiceId},'{this.ProductId}','{this.UomId}'," +
            //                       $"{this.Quantity},{this.Amount})";
            this.Cmd.ExecuteNonQuery();
            this.GeneratedInvoice = newInvoice;
            return this.GeneratedInvoice;
        }
        public virtual void ExecInvoicePreProcessing()
        {
            this.InvoicePreProcessingAction.Invoke(this.InvoiceGenerationInputData);
        }
        public virtual void ExecInvoicePostProcessing(Action<InvoiceGenerationInputData> invoicePostProcessingAction)
        {
            this.InvoicePostProcessingAction.Invoke(this.InvoiceGenerationInputData);
        }
    }
}
