using System.ComponentModel.Composition;
using System;
using MediationModel;
using System.Collections.Generic;
using System.Linq;
using LibraryExtensions;
using Newtonsoft.Json;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Accounting.Invoice;
using TelcobrightMediation.Cdr;
using TransactionTuple = System.ValueTuple<int, int, long, int,long>;

namespace TelcobrightMediation
{
    [Export("ServiceGroup", typeof(IServiceGroup))]
    public class SgIntlInIcx : IServiceGroup
    {
        private readonly SgIntlTransitVoice sgIntlTransitVoice = new SgIntlTransitVoice();
        public override string ToString() => this.RuleName;
        public string RuleName => "International Incoming Calls [ICX]";
        public string HelpText { get; } = "Service group Intl In for BD ICX.";
        public int Id => 3;
        private Dictionary<CdrSummaryType, Type> SummaryTargetTables { get; }

        public SgIntlInIcx() //constructor
        {
            this.SummaryTargetTables = new Dictionary<CdrSummaryType, Type>()
            {
                {CdrSummaryType.sum_voice_day_03, typeof(sum_voice_day_03)},
                {CdrSummaryType.sum_voice_hr_03, typeof(sum_voice_hr_03)},
            };
        }

        public Dictionary<CdrSummaryType, Type> GetSummaryTargetTables()
        {
            return this.SummaryTargetTables;
        }

        public void ExecutePostRatingActions(CdrExt cdrExt, object postRatingData)
        {
        }

        public void SetAdditionalParams(Dictionary<string, object> additionalParams)
        {
            
        }

        public void Execute(cdr thisCdr, CdrProcessor cdrProcessor)
        {
            //international in call direction/service group
            var dicRoutes = cdrProcessor.CdrJobContext.MediationContext.MefServiceGroupContainer.SwitchWiseRoutes;
            var key = new ValueTuple<int, string>(thisCdr.SwitchId, thisCdr.IncomingRoute);
            route thisRoute = null;
            dicRoutes.TryGetValue(key, out thisRoute);
            if (thisRoute != null)
            {
                if (thisRoute.partner.PartnerType == IcxPartnerType.IOS
                    && thisRoute.NationalOrInternational == RouteLocalityType.International
                ) //IGW and route=international
                {
                    thisCdr.ServiceGroup = 3; //Intl in ICX
                }
            }
        }

        public void SetServiceGroupWiseSummaryParams(CdrExt cdrExt, AbstractCdrSummary newSummary)
        {
            //this._sgIntlTransitVoice.SetServiceGroupWiseSummaryParams(cdrExt, newSummary);
            newSummary.tup_countryorareacode = cdrExt.Cdr.CountryCode;
            newSummary.tup_matchedprefixcustomer = cdrExt.Cdr.MatchedPrefixCustomer;
            newSummary.tup_matchedprefixsupplier = cdrExt.Cdr.MatchedPrefixSupplier;
            if (cdrExt.Cdr.ChargingStatus != 1) return;

            acc_chargeable chargeableCust = null;
            cdrExt.Chargeables.TryGetValue(new ValueTuple<int, int, int>(this.Id, 1, 1), out chargeableCust);
            if (chargeableCust == null)
            {
                throw new Exception("Chargeable info not found for customer direction.");
            }
            this.sgIntlTransitVoice.SetChargingSummaryInCustomerDirection(chargeableCust, newSummary);
            newSummary.tax1 = Convert.ToDecimal(chargeableCust.TaxAmount1);
        }

        public void ValidateInvoiceGenerationParams(object validationInput)
        {
            
        }

        public InvoiceGenerationInputData ExecInvoicePreProcessing(InvoiceGenerationInputData invoiceGenerationInputData)
        {
            Dictionary<string, string> jobParamsMap = invoiceGenerationInputData.JsonDetail;
            DateTime startDate = Convert.ToDateTime(jobParamsMap["startDate"]);
            var context = invoiceGenerationInputData.Context;
            DateTime lastSecondOfPrevMonth = startDate.AddSeconds(-1);
            uom_conversion_dated usdConversionDated = context.uom_conversion_dated.Where(
                    c => c.PURPOSE_ENUM_ID == "INTERNAL_CONVERSION"
                         && c.UOM_ID == "USD" && c.UOM_ID_TO == "BDT" && c.FROM_DATE == lastSecondOfPrevMonth)
                         .ToList().FirstOrDefault();
            if (usdConversionDated == null)
                throw new Exception("Usd conversion rate not found.");
            invoiceGenerationInputData.JsonDetail = jobParamsMap;
            invoiceGenerationInputData.JsonDetail.Add("usdRate",
                usdConversionDated.CONVERSION_FACTOR.ToString());
            return invoiceGenerationInputData;
        }

        public InvoicePostProcessingData ExecInvoicePostProcessing(InvoicePostProcessingData invoicePostProcessingData)
        {
            invoice invoice = invoicePostProcessingData.Invoice;
            invoice_item invoiceItem = invoicePostProcessingData.InvoiceItem;
            Dictionary<string, string> jsonDetail = JsonConvert.DeserializeObject<Dictionary<string, string>>
                (invoiceItem.JSON_DETAIL);
            decimal usdRateTtClean = Convert.ToDecimal(jsonDetail["usdRate"]);
            invoice.currencyConversionFactor = usdRateTtClean;
            invoice.convertedFinalCurrency = "BDT";
            invoice.convertedFinalAmount = usdRateTtClean * invoice.originalAmount;
            string cdrOrSummarytableName = this.SummaryTargetTables.Single(t => t.Key.ToString().Contains("day"))
                .Key.ToString();
            CommonInvoicePostProcessor commonInvoicePostProcessor
                = new CommonInvoicePostProcessor(invoicePostProcessingData, cdrOrSummarytableName, jsonDetail);
            return commonInvoicePostProcessor.Process();
        }
        
    }
}
