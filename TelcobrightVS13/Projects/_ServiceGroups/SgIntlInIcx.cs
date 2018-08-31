using System.ComponentModel.Composition;
using System;
using MediationModel;
using System.Collections.Generic;
using System.Linq;
using LibraryExtensions;
using Newtonsoft.Json;
using TelcobrightMediation.Accounting;
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

        public InvoiceGenerationInputData ExecInvoicePreProcessing(InvoiceGenerationInputData invoiceGenerationInputData)
        {
            job telcobrightJob = invoiceGenerationInputData.TelcobrightJob;
            Dictionary<string, string> jobParamsMap =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(telcobrightJob.jobsegments.Single()
                    .SegmentDetail);
            DateTime startDate = Convert.ToDateTime(jobParamsMap["startDate"]);
            DateTime endDate = Convert.ToDateTime(jobParamsMap["endDate"]);
            if (startDate.Day != 1 || endDate.Day != DateTime.DaysInMonth(startDate.Year, startDate.Month)
                || startDate.Hour != 0 || startDate.Minute != 0 || startDate.Second != 0 ||
                endDate.Hour != 23 || endDate.Minute != 59 || endDate.Second != 59)
            {
                throw new Exception("Start date & end date must be first & last day of a month.");
            }
            var context = invoiceGenerationInputData.Context;
            DateTime lastHourOfPrevMonth = startDate.AddSeconds(-1);
            uom_conversion_dated usdConversionDated = context.uom_conversion_dated.Where(
                    c => c.PURPOSE_ENUM_ID == "EXTERNAL_CONVERSION"
                         && c.UOM_ID == "USD" && c.UOM_ID_TO == "BDT" && c.FROM_DATE == lastHourOfPrevMonth).ToList()
                .FirstOrDefault();
            if (usdConversionDated == null)
                throw new Exception("Usd rate not found in uom_conversion_dated table.");
            List<acc_chargeable> sampleChargeablesForThisPeriod = context.acc_chargeable.Where(c => c.transactionTime >= startDate
                                                                && c.transactionTime < endDate).Take(20).ToList();

            if (sampleChargeablesForThisPeriod.Any(c=>c.OtherDecAmount3!=usdConversionDated.CONVERSION_FACTOR))
            {
                throw new Exception(
                    $"Usd rates for outgoing calls for this period must be {usdConversionDated.CONVERSION_FACTOR}");
            }
            invoiceGenerationInputData.OtherDataAsObjectMap.Add("usdRate",
                usdConversionDated.CONVERSION_FACTOR.ToString());
            return invoiceGenerationInputData;
        }

        public InvoicePostProcessingData ExecInvoicePostProcessing(InvoicePostProcessingData invoicePostProcessingData)
        {
            invoice invoiceWithItem = invoicePostProcessingData.Invoice;
            invoice_item invoiceItem = invoiceWithItem.invoice_item.Single();
            PartnerEntities context = invoicePostProcessingData.InvoiceGenerationInputData.Context;
            job telcobrightJob = invoicePostProcessingData.InvoiceGenerationInputData.TelcobrightJob;
            Dictionary<string, string> jobParamsMap =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(telcobrightJob.jobsegments.Single()
                    .SegmentDetail);
            long serviceAccountId = Convert.ToInt64(jobParamsMap["serviceAccountId"]);
            DateTime startDate = Convert.ToDateTime(jobParamsMap["startDate"]);
            DateTime endDate = Convert.ToDateTime(jobParamsMap["endDate"]);
            account acc =context.accounts.Where(c=>c.id==serviceAccountId).ToList().Single();
            int inPartnerId = acc.idPartner;
            List<XyzInvoiceDataRow> xyzInvoiceDataRows = context.Database.SqlQuery<XyzInvoiceDataRow>(
                $@"select 
                sum(successfulcalls 	)	as successfulcalls,   
                sum(roundedduration   )  as roundedduration,   
                sum(longDecimalAmount1)  as longDecimalAmount1,
                sum(longDecimalAmount2)  as longDecimalAmount2,
                sum(longDecimalAmount3)  as longDecimalAmount3,
                sum(customercost      )  as customercost      
                from sum_voice_day_02
                where tup_starttime>={startDate.ToMySqlStyleDateTimeStrWithQuote()} 
                and tup_starttime < {endDate.ToMySqlStyleDateTimeStrWithQuote()}
                group by tup_outpartnerid;"
                ).ToList();
            jobParamsMap.Add("summaryRows",JsonConvert.SerializeObject(xyzInvoiceDataRows));
            invoiceItem.JSON_DETAIL = JsonConvert.SerializeObject(jobParamsMap);
            return invoicePostProcessingData;
        }
    }
}
