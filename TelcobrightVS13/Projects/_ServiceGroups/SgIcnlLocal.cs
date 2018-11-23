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
    public class SgIcnlLocal : IServiceGroup
    {
        private readonly SgIntlTransitVoice _sgIntlTransitVoice = new SgIntlTransitVoice();
        public override string ToString() => this.RuleName;
        public string RuleName => "Local";
        public string HelpText => "Service group Domestic for ICNL";
        public int Id => 20;
        private Dictionary<CdrSummaryType, Type> SummaryTargetTables { get; }
        private List<ICdrRule> CdrRules { get; set; } = new List<ICdrRule>();
        public SgIcnlLocal() //constructor
        {
            this.SummaryTargetTables = new Dictionary<CdrSummaryType, Type>()
            {
                {CdrSummaryType.sum_voice_day_01, typeof(sum_voice_day_01)},
                {CdrSummaryType.sum_voice_hr_01, typeof(sum_voice_hr_01)},
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
            var trueForAll = true;
            foreach (var r in this.CdrRules)
            {
                if (r.CheckIfTrue(thisCdr) == false)
                {
                    trueForAll = false;
                    break;
                }
            }
            if (trueForAll)
            {
                thisCdr.ServiceGroup = this.Id;
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
            this._sgIntlTransitVoice.SetChargingSummaryInCustomerDirection(chargeableCust, newSummary);
            newSummary.tax1 = Convert.ToDecimal(chargeableCust.TaxAmount1);
        }

        public void ValidateInvoiceGenerationParams(object validationInput)
        {
            
        }

        public InvoiceGenerationInputData ExecInvoicePreProcessing(InvoiceGenerationInputData invoiceGenerationInputData)
        {
            Dictionary<string, string> jobParamsMap = invoiceGenerationInputData.JsonDetail;
            invoiceGenerationInputData.JsonDetail = jobParamsMap;
            invoiceGenerationInputData.JsonDetail.Add("vat",".15");//todo: for now harcode
            return invoiceGenerationInputData;
        }

        public InvoicePostProcessingData ExecInvoicePostProcessing(InvoicePostProcessingData invoicePostProcessingData)
        {
            invoice_item invoiceItem = invoicePostProcessingData.InvoiceItem;
            Dictionary<string, string> jsonDetail = JsonConvert.DeserializeObject<Dictionary<string, string>>
                (invoiceItem.JSON_DETAIL);
            string cdrOrSummarytableName = this.SummaryTargetTables.Single(t=>t.Key.ToString().Contains("day"))
                .Key.ToString();
            CommonInvoicePostProcessor commonInvoicePostProcessor
                = new CommonInvoicePostProcessor(invoicePostProcessingData, cdrOrSummarytableName, jsonDetail);
            return commonInvoicePostProcessor.Process();
        }
    }
}
