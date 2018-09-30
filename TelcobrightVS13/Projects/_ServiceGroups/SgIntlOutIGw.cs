using System;
using System.ComponentModel.Composition;
using MediationModel;
using System.Collections.Generic;
using LibraryExtensions;
using MediationModel.enums;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Cdr;
using TransactionTuple = System.ValueTuple<int, int, long, int, long>;

namespace TelcobrightMediation
{

    [Export("ServiceGroup", typeof(IServiceGroup))]
    public class SgIntlOutIGw : IServiceGroup
    {
        private readonly SgIntlTransitVoice sgIntlTransitVoice = new SgIntlTransitVoice();
        public override string ToString() => this.RuleName;
        public string RuleName => "International Outgoing Calls [IGW]";

        public string HelpText =>
            "Service group International Outgoing for BD IGW. Old common mediation codes for IGW, could not separate in short time, covers both intl in/out";

        public int Id => 5;
        private Dictionary<CdrSummaryType, Type> SummaryTargetTables { get; }

        public SgIntlOutIGw() //constructor
        {
            this.SummaryTargetTables = new Dictionary<CdrSummaryType, Type>()
            {
                {CdrSummaryType.sum_voice_day_02, typeof(sum_voice_day_02)},
                {CdrSummaryType.sum_voice_hr_02, typeof(sum_voice_hr_02)},
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
                if (thisRoute.partner.PartnerType == 2) //ICX
                {
                    thisCdr.ServiceGroup = 5; //international Out in IGW
                    //set ansidorig if roaming ans flag is on
                    if (thisRoute.field1 != null && Convert.ToInt32(thisRoute.field1) > 0) //roaming ANS=field1
                    {
                        thisCdr.AnsIdOrig = thisRoute.field1;
                    }
                    string originatingCallingNumber = (thisCdr.OriginatingCallingNumber.StartsWith("+") == false
                        ? thisCdr.OriginatingCallingNumber
                        : thisCdr.OriginatingCallingNumber.Substring(1, thisCdr.OriginatingCallingNumber.Length - 1));
                    //cli may not be present in some case...
                    if (originatingCallingNumber != "")
                    {
                        //normalize orig calling number from 00880, 880, 01x...
                        if (originatingCallingNumber.Substring(0, 1) == "0")
                        {
                            //first 0 removed next line
                            originatingCallingNumber =
                                originatingCallingNumber.Substring(1, originatingCallingNumber.Length - 1);
                            //check for 00880
                            if (originatingCallingNumber.Length >= 4)
                            {
                                if (originatingCallingNumber.Substring(0, 4) == "0880") //00880
                                {
                                    originatingCallingNumber =
                                        originatingCallingNumber.Substring(4, originatingCallingNumber.Length - 4);
                                }
                            }
                        }
                        else if (originatingCallingNumber.Length >= 3)
                        {
                            //bad rule for teletalk originated calls...
                            if (originatingCallingNumber.Length >= 6 &&
                                originatingCallingNumber.Substring(0, 6) == "880880")
                            {
                                originatingCallingNumber =
                                    originatingCallingNumber.Substring(6, originatingCallingNumber.Length - 6);
                            }
                            //regular 880 based rule...
                            else if (originatingCallingNumber.Substring(0, 3) == "880")
                            {
                                originatingCallingNumber =
                                    originatingCallingNumber.Substring(3, originatingCallingNumber.Length - 3);
                            }

                        }
                        int iteration = 0;
                        //ansidorig might already be set for roaming calls..in that case ignore
                        int tempInt = -1;
                        int.TryParse(thisCdr.AnsIdOrig.ToString(), out tempInt);
                        if (tempInt <= 0) //call wasn't roaming and ansidorig was not set before
                        {
                            string ansPrefixOrig = "";
                            int? ansIdOrig = null;
                            for (iteration = 0; iteration < originatingCallingNumber.Length; iteration++)
                            {
                                partnerprefix thisPrefix = null;
                                string matchStr = originatingCallingNumber.Substring(0, iteration + 1);
                                cdrProcessor.CdrJobContext.MediationContext.DictAnsOrig.TryGetValue(matchStr,
                                    out thisPrefix);
                                if (thisPrefix != null)
                                {
                                    ansPrefixOrig = thisPrefix.Prefix;
                                    ansIdOrig = thisPrefix.idPartner;
                                    continue;
                                }
                            }
                            thisCdr.AnsPrefixOrig = ansPrefixOrig;
                            thisCdr.AnsIdOrig = ansIdOrig;
                        }
                    } //if originatingcallingnumber present
                }
            }
        }


        public void SetServiceGroupWiseSummaryParams(CdrExt cdrExt, AbstractCdrSummary newSummary)
        {
            newSummary.tup_matchedprefixcustomer = cdrExt.Cdr.MatchedPrefixY;
            newSummary.tup_countryorareacode = cdrExt.Cdr.CountryCode;
            newSummary.tup_sourceId = cdrExt.Cdr.AnsIdOrig.ToString();
            if (cdrExt.Cdr.ChargingStatus != 1) return;

            acc_chargeable chargeableCust = null;
            cdrExt.Chargeables.TryGetValue(new ValueTuple<int, int, int>(this.Id, 4, 1), out chargeableCust);
            if (chargeableCust == null)
            {
                throw new Exception("Chargeable not found for customer direction.");
            }

            newSummary.customercost = Convert.ToDecimal(chargeableCust.BilledAmount); //invoice amount
            newSummary.tup_customerrate = Convert.ToDecimal(chargeableCust.OtherDecAmount1); //x rate
            newSummary.tup_supplierrate = Convert.ToDecimal(chargeableCust.OtherDecAmount2); //y rate
            newSummary.tup_customercurrency = Convert.ToDecimal(chargeableCust.OtherDecAmount3).ToString(); //usd rate
            newSummary.longDecimalAmount1 = Convert.ToDecimal(chargeableCust.OtherAmount1); //x amount
            newSummary.longDecimalAmount2 = Convert.ToDecimal(chargeableCust.OtherAmount2); //y amount
            newSummary.longDecimalAmount3 = Convert.ToDecimal(chargeableCust.OtherAmount3); //z amount
            newSummary.tax1 = Convert.ToDecimal(chargeableCust.TaxAmount1); //btrc
            acc_chargeable chargeableSupp = null;
            cdrExt.Chargeables.TryGetValue(new ValueTuple<int, int, int>(this.Id, 1, 2), out chargeableSupp);
            if (chargeableSupp == null)
            {
                throw new Exception("Chargeable info not found for supplier direction.");
            }
            this.sgIntlTransitVoice.SetChargingSummaryInSupplierDirection(chargeableSupp, newSummary);
        }

        public void ValidateInvoiceGenerationParams(object validationInput)
        {
            
        }

        public InvoiceGenerationInputData ExecInvoicePreProcessing(
            InvoiceGenerationInputData invoiceGenerationInputData)
        {
            return XyzRuleHelper.ExecInvoicePreProcessing(invoiceGenerationInputData);
        }

        public InvoicePostProcessingData ExecInvoicePostProcessing(InvoicePostProcessingData invoicePostProcessingData)
        {
            return XyzRuleHelper.ExecInvoicePostProcessing(invoicePostProcessingData);
        }
    }
}
