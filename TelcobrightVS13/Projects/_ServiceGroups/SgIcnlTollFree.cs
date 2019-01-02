using System.ComponentModel.Composition;
using System;
using MediationModel;
using System.Collections.Generic;
using System.Linq;
using LibraryExtensions;
using MediationModel.enums;
using Newtonsoft.Json;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Accounting.Invoice;
using TelcobrightMediation.Cdr;
using TransactionTuple = System.ValueTuple<int, int, long, int, long>;

namespace TelcobrightMediation
{
    [Export("ServiceGroup", typeof(IServiceGroup))]
    public class SgIcnlTollFree : IServiceGroup
    {
        private readonly SgIntlTransitVoice sgIntlTransitVoice = new SgIntlTransitVoice();
        public override string ToString() => this.RuleName;
        public string RuleName => "Toll Free";
        public string HelpText => "Icnl toll free";
        public int Id => 23;
        private List<ICdrRule> CdrRules { get; set; } = new List<ICdrRule>();
        private Dictionary<CdrSummaryType, Type> SummaryTargetTables { get; }
        private List<string> PrefixesOrderedByMaxLenFirst { get; set; }

        public SgIcnlTollFree() //constructor
        {
            this.SummaryTargetTables = new Dictionary<CdrSummaryType, Type>()
            {
                {CdrSummaryType.sum_voice_day_04, typeof(sum_voice_day_04)},
                {CdrSummaryType.sum_voice_hr_04 , typeof(sum_voice_hr_04)},
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
            var cdrRules = additionalParams["cdrRules"] as List<ICdrRule>;
            this.CdrRules = cdrRules;
            if (additionalParams.ContainsKey("prefixes"))
            {
                this.PrefixesOrderedByMaxLenFirst = ((string)additionalParams["prefixes"]).Split(',')
                    .OrderByDescending(c => c.Length).ToList();
            }
            else
            {
                this.PrefixesOrderedByMaxLenFirst = new List<string>();
            }
        }

        public void Execute(cdr cdr, CdrProcessor cdrProcessor)
        {
            var dicRoutes = cdrProcessor.CdrJobContext.MediationContext.MefServiceGroupContainer.SwitchWiseRoutes;
            var key = new ValueTuple<int, string>(cdr.SwitchId, cdr.OutgoingRoute);
                        route outgoingRoute = null;
            dicRoutes.TryGetValue(key, out outgoingRoute);
            if (outgoingRoute != null)
            {
                if (outgoingRoute.NationalOrInternational == RouteLocalityType.National)
                {
                    foreach (string prefix in this.PrefixesOrderedByMaxLenFirst)
                    {
                        if (cdr.OriginatingCalledNumber.StartsWith(prefix))
                        {
                            cdr.ServiceGroup = this.Id; //LTFS in ICX           
                            break;
                        }
                    }
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
            cdrExt.Chargeables.TryGetValue(new ValueTuple<int, int, int>(this.Id, 5, 1), out chargeableCust);
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
            invoiceGenerationInputData.JsonDetail = jobParamsMap;
            invoiceGenerationInputData.JsonDetail.Add("vat", "0");//todo: for now harcode
            return invoiceGenerationInputData;
        }

        public InvoicePostProcessingData ExecInvoicePostProcessing(InvoicePostProcessingData invoicePostProcessingData)
        {
            invoice_item invoiceItem = invoicePostProcessingData.InvoiceItem;
            Dictionary<string, string> jsonDetail = JsonConvert.DeserializeObject<Dictionary<string, string>>
                (invoiceItem.JSON_DETAIL);
            string cdrOrSummarytableName = this.SummaryTargetTables.Single(t => t.Key.ToString().Contains("day"))
                .Key.ToString();
            CommonInvoicePostProcessor commonInvoicePostProcessor
                = new CommonInvoicePostProcessor(invoicePostProcessingData, cdrOrSummarytableName, jsonDetail);
            return commonInvoicePostProcessor.Process();
        }
    }
}
