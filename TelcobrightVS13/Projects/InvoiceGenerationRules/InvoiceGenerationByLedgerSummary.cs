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
            InvoiceGenerationInputData input = (InvoiceGenerationInputData)data;
            PartnerEntities context = input.Context;
            Dictionary<string, string> invoiceJsonDetail = input.JsonDetail;
            long serviceAccountId = Convert.ToInt64(invoiceJsonDetail["serviceAccountId"]);
            ValidateIfLocalTimeZoneUsed(input, context, invoiceJsonDetail);
            DateTime startDate = Convert.ToDateTime(invoiceJsonDetail["startDate"]);
            DateTime endDate = Convert.ToDateTime(invoiceJsonDetail["endDate"]);
            Dictionary<DateTime,acc_ledger_summary> dayWiseLedgerSummaries = context.acc_ledger_summary
                .Where(c => c.idAccount == serviceAccountId && c.transactionDate >= startDate
                && c.transactionDate <= endDate).OrderBy(c=>c.transactionDate).ToDictionary(c=>c.transactionDate);
            decimal ledgerSummaryAmount = dayWiseLedgerSummaries.Values.Sum(c => c.AMOUNT);
            Dictionary<DateTime, acc_ledger_summary_billed> dayWisetempTransactionAmount = context.acc_ledger_summary_billed
                .Where(c => c.idAccount == serviceAccountId && c.transactionDate >= startDate
                            && c.transactionDate <= endDate).OrderBy(c => c.transactionDate).ToDictionary(c => c.transactionDate);
            decimal tempTransactionAmount = dayWisetempTransactionAmount.Values.Sum(c => c.billedAmount);
            //decimal? tempTransactionAmount = context.acc_temp_transaction.Where(c => c.glAccountId == serviceAccountId)
            //    .Sum(c => (decimal?)c.amount);
            decimal invoiceAmount = -1*(ledgerSummaryAmount + Convert.ToDecimal(tempTransactionAmount));
            if (invoiceAmount <= 0)
            {
                throw new Exception("Account balance [= ledger summary+temp transaction amount] " +
                                    " must be negative.");
            }
            IServiceGroup serviceGroup = null;
            int idServiceGroup = Convert.ToInt32(invoiceJsonDetail["idServiceGroup"]);
            input.ServiceGroups.TryGetValue(idServiceGroup, out serviceGroup);
            if (serviceGroup == null)
                throw new Exception("Service group should be set already thus cannot be null while " +
                                    "executing invoice generation by ledger summary.");
            string invoiceDescription = serviceGroup.RuleName + $" [{startDate.ToMySqlFormatWithoutQuote()}" +
                                        $"-{endDate.ToMySqlFormatWithoutQuote()}]";
            string uom = invoiceJsonDetail["uom"];
            invoice newInvoice = CreateInvoiceWithItem(invoiceJsonDetail, serviceAccountId, invoiceAmount, serviceGroup,
                invoiceDescription, uom);
            InvoicePostProcessingData invoicePostProcessingData =
                new InvoicePostProcessingData(input, newInvoice, serviceAccountId, startDate, endDate,
                dayWiseLedgerSummaries);
            return invoicePostProcessingData;
        }

        public static invoice CreateInvoiceWithItem(Dictionary<string, string> invoiceJsonDetail, 
            long serviceAccountId, decimal invoiceAmount, IServiceGroup serviceGroup, string invoiceDescription, 
            string uom)
        {
            return new invoice()
            {
                BILLING_ACCOUNT_ID = serviceAccountId,
                DESCRIPTION = invoiceDescription,
                INVOICE_DATE = DateTime.Now.Date,
                originalAmount = invoiceAmount,
                convertedFinalAmount = invoiceAmount,
                originalCurrency = uom,
                convertedFinalCurrency = uom,
                invoice_item = new List<invoice_item>()
                {
                    new invoice_item()
                    {
                        PRODUCT_ID = serviceGroup.RuleName,
                        UOM_ID = uom,
                        AMOUNT = invoiceAmount,
                        JSON_DETAIL = JsonConvert.SerializeObject(invoiceJsonDetail)
                    }
                }
            };
        }

        private static void ValidateIfLocalTimeZoneUsed(InvoiceGenerationInputData input, PartnerEntities context, Dictionary<string, string> invoiceJsonDetail)
        {
            int timeZoneOffsetSecForInvoicing = Convert.ToInt32(invoiceJsonDetail["timeZoneOffsetSec"]);
            int timeZoneIdInTelcobrightConfig = input.Tbc.DefaultTimeZoneId;
            timezone configuredTimeZone = context.timezones.Where(c => c.id == timeZoneIdInTelcobrightConfig).ToList()
                .Single();
            if (timeZoneOffsetSecForInvoicing != configuredTimeZone.gmt_offset)
            {
                throw new Exception($"Timezone must be {configuredTimeZone.abbreviation}");
            }
        }
        }
}
