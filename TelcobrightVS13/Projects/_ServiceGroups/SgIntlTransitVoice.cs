using System.ComponentModel.Composition;
using System;
using MediationModel;
using System.Collections.Generic;
using LibraryExtensions;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Cdr;
using TransactionTuple = System.ValueTuple<int, int, long, int,long>;
namespace TelcobrightMediation
{

    [Export("ServiceGroup", typeof(IServiceGroup))]
    public class SgIntlTransitVoice : IServiceGroup
    {
        public override string ToString() => this.RuleName;
        public string RuleName => "Transit Calls [Wholesale Voice]";
        public string HelpText => "Service group for international transit voice.";
        public int Id => 100;
        private Dictionary<CdrSummaryType, Type> SummaryTargetTables { get; }
        public SgIntlTransitVoice()//constructor
        {
            this.SummaryTargetTables = new Dictionary<CdrSummaryType, Type>()
                {
                    { CdrSummaryType.sum_voice_day_01, typeof(sum_voice_day_01)},
                    { CdrSummaryType.sum_voice_hr_01 , typeof(sum_voice_hr_01) },
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

        public void Execute(cdr cdr, CdrProcessor cdrProcessor)
        {
            cdr.ServiceGroup = 100; 
        }

        public void SetServiceGroupWiseSummaryParams(CdrExt cdrExt, AbstractCdrSummary newSummary)
        {
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
            SetChargingSummaryInCustomerDirection(chargeableCust, newSummary);

            acc_chargeable chargeableSupp = null;
            cdrExt.Chargeables.TryGetValue(new ValueTuple<int, int, int>(this.Id, 1, 2), out chargeableSupp);
            if (chargeableSupp == null)
            {
                throw new Exception("Chargeable info not found for supplier direction.");
            }
            SetChargingSummaryInSupplierDirection(chargeableSupp, newSummary);
        }

        public InvoiceGenerationInputData ExecInvoicePreProcessing(InvoiceGenerationInputData invoiceGenerationInputData)
        {
            throw new NotImplementedException();
        }

        public InvoicePostProcessingData ExecInvoicePostProcessing(InvoicePostProcessingData invoicePostProcessingData)
        {
            throw new NotImplementedException();
        }


        public void SetChargingSummaryInCustomerDirection(acc_chargeable chargeableCust, AbstractCdrSummary newSummary)
        {
            newSummary.tup_matchedprefixcustomer = chargeableCust.Prefix;
            newSummary.tup_customerrate = chargeableCust.unitPriceOrCharge;
            newSummary.tup_customercurrency = chargeableCust.idBilledUom;
            newSummary.customercost = chargeableCust.BilledAmount;
        }

        public void SetChargingSummaryInSupplierDirection(acc_chargeable chargeableSupp, AbstractCdrSummary newSummary)
        {
            newSummary.tup_matchedprefixsupplier = chargeableSupp.Prefix;
            newSummary.tup_supplierrate = chargeableSupp.unitPriceOrCharge;
            newSummary.tup_suppliercurrency = chargeableSupp.idBilledUom;
            newSummary.suppliercost = chargeableSupp.BilledAmount;
        }

    }
}
