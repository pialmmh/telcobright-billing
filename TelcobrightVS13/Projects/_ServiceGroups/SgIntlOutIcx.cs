using System.ComponentModel.Composition;
using System;
using MediationModel;
using System.Collections.Generic;
using System.Linq;
using LibraryExtensions;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Cdr;
using TransactionTuple = System.ValueTuple<int, int, long, int,long>;

namespace TelcobrightMediation
{

    [Export("ServiceGroup", typeof(IServiceGroup))]
    public class SgIntlOutIcx : IServiceGroup
    {
        private readonly SgIntlTransitVoice sgIntlTransitVoice = new SgIntlTransitVoice();
        public override string ToString() => this.RuleName;
        public string RuleName => "International Outgoing Calls [ICX]";
        public string HelpText => "Service group International Outgoing for BD ICX.";
        public int Id => 2;
        private Dictionary<CdrSummaryType, Type> SummaryTargetTables { get; }
        private List<ICdrRule> CdrRules { get; set; }=new List<ICdrRule>();
        public SgIntlOutIcx() //constructor
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
            var cdrRules = additionalParams["cdrRules"] as List<ICdrRule>;
            this.CdrRules = cdrRules;
        }

        public void Execute(cdr thisCdr, CdrProcessor cdrProcessor)
        {
            
            foreach (ICdrRule cdrRule in this.CdrRules)
            {
                if (cdrRule.CheckIfTrue(thisCdr))
                {
                    thisCdr.ServiceGroup = 2; //international Out in IGW 
                    break;
                }
            }
        }

        public void SetServiceGroupWiseSummaryParams(CdrExt cdrExt, AbstractCdrSummary newSummary)
        {
            newSummary.tup_matchedprefixcustomer = cdrExt.Cdr.MatchedPrefixY;
            newSummary.tup_countryorareacode = cdrExt.Cdr.CountryCode;
            newSummary.tup_sourceId = cdrExt.Cdr.InPartnerId.ToString();
            newSummary.tup_inpartnerid = Convert.ToInt32(cdrExt.Cdr.InPartnerId);
            if (cdrExt.Cdr.ChargingStatus != 1) return;
            acc_chargeable chargeableCust = null;
            cdrExt.Chargeables.TryGetValue(new ValueTuple<int, int, int>(this.Id, 7, 1), out chargeableCust);
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
            newSummary.tax1 = Convert.ToDecimal(chargeableCust.TaxAmount1);//btrc rev share
        }

        public void ExecInvoicePreProcessing(InvoiceGenerationInputData invoiceGenerationInputData)
        {
            throw new NotImplementedException();
        }

        public InvoicePostProcessingData ExecInvoicePostProcessing(InvoicePostProcessingData invoicePostProcessingData)
        {
            throw new NotImplementedException();
        }
    }
}
