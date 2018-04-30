using System.ComponentModel.Composition;
using System;
using MediationModel;
using System.Collections.Generic;
using LibraryExtensions;
using TelcobrightMediation.Cdr;
using TransactionTuple = System.ValueTuple<int, int, long, int,long>;

namespace TelcobrightMediation
{

    [Export("ServiceGroup", typeof(IServiceGroup))]
    public class SgIntlInIcx : IServiceGroup
    {
        private readonly SgIntlTransitVoice _sgIntlTransitVoice = new SgIntlTransitVoice();
        public override string ToString() => this.RuleName;
        public string RuleName => "International Incoming Calls [ICX]";
        public string HelpText { get; } = "Service group Intl In for BD ICX.";
        public int Id => 3;
        private Dictionary<string, Type> SummaryTargetTables { get; }

        public SgIntlInIcx() //constructor
        {
            this.SummaryTargetTables = new Dictionary<string, Type>()
            {
                {"sum_voice_day_03", typeof(sum_voice_day_03)},
                {"sum_voice_hr_03", typeof(sum_voice_hr_03)},
            };
        }

        public Dictionary<string, Type> GetSummaryTargetTables()
        {
            return this.SummaryTargetTables;
        }

        public void ExecutePostRatingActions(CdrExt cdrExt, object postRatingData)
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
            this._sgIntlTransitVoice.SetChargingSummaryInCustomerDirection(chargeableCust, newSummary);
            newSummary.tax1 = Convert.ToDecimal(chargeableCust.TaxAmount1);
        }
    }
}
