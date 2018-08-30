using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using LibraryExtensions;
using MediationModel;
using Newtonsoft.Json;
using TelcobrightMediation.Accounting;

namespace InvoiceGenerationRules
{

    [Export("InvoiceGenerationRule", typeof(IInvoiceGenerationRule))]
    public class InvoiceGenerationByLedgerSummary : IInvoiceGenerationRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }

        public string RuleName => GetType().Name;
        public string HelpText => "Generate invoice from ledger summary.";
        public int Id => 2;

        public InvoicePostProcessingData Execute(object data)
        {
            InvoiceGenerationInputData input = (InvoiceGenerationInputData) data;
            PartnerEntities context = input.Context;
            job telcobrightJob = input.TelcobrightJob;
            Dictionary<string, string> jobParamsMap =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(telcobrightJob.jobsegments.Single()
                    .SegmentDetail);
            long serviceAccountId = Convert.ToInt64(jobParamsMap["serviceAccountId"]);
            DateTime startDate = Convert.ToDateTime(jobParamsMap["startDate"]);
            DateTime endDate = Convert.ToDateTime(jobParamsMap["endDate"]);
            int timeZoneOffsetSecForInvoicing = Convert.ToInt32(jobParamsMap["timeZoneOffsetSec"]);
            IServiceGroup serviceGroup = input.SelectedServiceGroup;
            if (serviceGroup == null)
                throw new Exception("Service group should be set already thus cannot be null while " +
                                    "executing invoice generation by ledger summary.");
            int timeZoneIdInTelcobrightConfig = input.Tbc.DefaultTimeZoneId;
            timezone configuredTimeZone = context.timezones.Where(c => c.id == timeZoneIdInTelcobrightConfig).ToList()
                .Single();
            if (timeZoneOffsetSecForInvoicing != configuredTimeZone.gmt_offset)
            {
                throw new Exception($"Timezone must be {configuredTimeZone.abbreviation}");
            }
            decimal ledgerSummaryAmount = context.acc_ledger_summary.Where(c => c.id == serviceAccountId)
                .Sum(c => c.AMOUNT);
            decimal tempTransactionAmount = context.acc_temp_transaction.Where(c => c.glAccountId == serviceAccountId)
                .Sum(c => c.amount);
            decimal invoiceAmount = ledgerSummaryAmount + tempTransactionAmount;
            if (invoiceAmount <= 0)
            {
                throw new Exception("Account balance [= ledger summary+temp transaction amount] " +
                                    " must be >0");
            }
            var cmd = input.Context.Database.Connection.CreateCommand();
            string invoiceDescription = serviceGroup.RuleName + $" [{startDate.ToMySqlStyleDateTimeStrWithoutQuote()}" +
                                        $"-{endDate.ToMySqlStyleDateTimeStrWithoutQuote()}]";
            cmd.CommandText = $"insert into invoice (billing_account_id,description) values(" +
                              $"{serviceAccountId.ToString()},'{invoiceDescription}');";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "last_insert_id();";
            long generatedInvoiceId = (long) cmd.ExecuteScalar();
            string uom = input.Context.accounts.Where(c => c.id == serviceAccountId).ToList().Single().uom;
            cmd.CommandText = $"insert into invoice_item " +
                              $"(invoice_id,product_id,uom_Id,amount) values (" +
                              $"{generatedInvoiceId},'{serviceGroup.RuleName}','{uom}'," +
                              $"{invoiceAmount})";
            cmd.ExecuteNonQuery();
            invoice generatedInvoiceWithItem = context.invoices.Where(c => c.INVOICE_ID == generatedInvoiceId)
                .Include(c => c.invoice_item).ToList().Single();
            InvoicePostProcessingData invoicePostProcessingData =
                new InvoicePostProcessingData(input, generatedInvoiceWithItem, new Dictionary<string, string>());
            return invoicePostProcessingData;
        }
    }
}
