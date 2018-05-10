using System.ComponentModel.Composition;
using System;
using MediationModel;
using System.Collections.Generic;
using System.Linq;
using LibraryExtensions;
using TelcobrightMediation.Cdr;
using TransactionTuple = System.ValueTuple<int, int, long, int, long>;

namespace TelcobrightMediation
{
    [Export("ServiceGroup", typeof(IServiceGroup))]
    public class SgLtfsIcx : IServiceGroup
    {
        private readonly SgIntlTransitVoice sgIntlTransitVoice = new SgIntlTransitVoice();
        public override string ToString() => this.RuleName;
        public string RuleName => "LTFS Calls [ICX]";
        public string HelpText => "Service group LTFS for BD ICX.";
        public int Id => 6;
        private Dictionary<string, Type> SummaryTargetTables { get; }
        private List<string> PrefixesOrderedByMaxLenFirst { get; set; }

        public SgLtfsIcx() //constructor
        {
            this.SummaryTargetTables = new Dictionary<string, Type>()
            {
                {"sum_voice_day_04", typeof(sum_voice_day_04)},
                {"sum_voice_hr_04", typeof(sum_voice_hr_04)},
            };
        }

        public Dictionary<string, Type> GetSummaryTargetTables()
        {
            return this.SummaryTargetTables;
        }

        public void ExecutePostRatingActions(CdrExt cdrExt, object postRatingData)
        {

        }

        public void SetAdditionalParams(Dictionary<string, string> additionalParams)
        {
            if (additionalParams.ContainsKey("prefixes"))
            {
                this.PrefixesOrderedByMaxLenFirst = additionalParams["prefixes"].Split(',')
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
            var key = new ValueTuple<int, string>(cdr.SwitchId, cdr.IncomingRoute);
            route thisRoute = null;
            dicRoutes.TryGetValue(key, out thisRoute);
            if (thisRoute != null)
            {
                if (thisRoute.partner.PartnerType == IcxPartnerType.ANS &&
                    thisRoute.NationalOrInternational == RouteLocalityType.National
                ) //ANS and route=national
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
    }
}
