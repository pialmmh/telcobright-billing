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
    public class InvoiceGenerationByLedgerSummary : IInvoiceGenerationRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }

        public string RuleName => GetType().Name;
        public string HelpText => "Generate invoice from ledger summary.";
        public int Id => 2;

        public invoice Execute(object data)
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

        }
    }
}
