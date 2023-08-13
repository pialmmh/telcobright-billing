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
    public class SgDomesticIcx : IServiceGroup
    {
        private readonly SgIntlTransitVoice _sgIntlTransitVoice = new SgIntlTransitVoice();
        public override string ToString() => this.RuleName;
        public string RuleName => "Domestic Calls [ICX]";
        public string HelpText => "Service group Domestic for BD ICX.";
        public int Id => 1;
        private Dictionary<CdrSummaryType, Type> SummaryTargetTables { get; }

        public SgDomesticIcx() //constructor
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
            //Domestic call direction/service group
            var dicRoutes = cdrProcessor.CdrJobContext.MediationContext.MefServiceGroupContainer.SwitchWiseRoutes;
            var key = new ValueTuple<int, string>(thisCdr.SwitchId, thisCdr.IncomingRoute);
            route incomingRoute = null;
            dicRoutes.TryGetValue(key, out incomingRoute);
            if (incomingRoute != null)
            {
                bool useCasStyleProcessing = cdrProcessor.CdrJobContext.CdrjobInputData.CdrSetting.useCasStyleProcessing;

                if(!useCasStyleProcessing)
                {
                    if (incomingRoute.partner.PartnerType == IcxPartnerType.ANS &&
                    incomingRoute.NationalOrInternational == RouteLocalityType.National) //ANS and route=national
                    {
                        thisCdr.ServiceGroup = 1; //Domestic in ICX
                    }
                }
                else
                {
                    key = new ValueTuple<int, string>(thisCdr.SwitchId, thisCdr.OutgoingRoute);
                    route outGoingRoute = null;
                    dicRoutes.TryGetValue(key, out outGoingRoute);


                    if (outGoingRoute.partner.PartnerType == IcxPartnerType.ANS &&
                        incomingRoute.partner.PartnerType == IcxPartnerType.ANS) //ANS and route=national
                    {
                        thisCdr.ServiceGroup = 1; //Domestic call
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
            if (invoiceGenerationInputData.JsonDetail.ContainsKey("vat") == false)
            {
                invoiceGenerationInputData.JsonDetail.Add("vat", ".15");//todo: for now harcode
            }
            else {
                invoiceGenerationInputData.JsonDetail["vat"]= ".15";//todo: for now harcode
            }
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
