using System.ComponentModel.Composition;
using System;
using MediationModel;
using System.Collections.Generic;
using LibraryExtensions;
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
        private Dictionary<string, Type> SummaryTargetTables { get; }
        public SgDomesticIcx()//constructor
        {
            this.SummaryTargetTables = new Dictionary<string, Type>()
                {
                    { "sum_voice_day_01",typeof(sum_voice_day_01)},
                    { "sum_voice_hr_01" ,typeof(sum_voice_hr_01) },
                };
        }
        public Dictionary<string, Type> GetSummaryTargetTables()
        {
            return this.SummaryTargetTables;
        }

        public void ExecutePostRatingActions(CdrExt cdrExt, CdrProcessor cdrProcessor)
        {
            cdrExt.Cdr.roundedduration = cdrExt.Cdr.Duration1;
            cdrExt.Cdr.CostANSIn = cdrExt.Cdr.CustomerCost;
        }
        public void Execute(cdr thisCdr, CdrProcessor cdrProcessor)
        {
            //Domestic call direction/service group
            var dicRoutes = cdrProcessor.CdrJobContext.MediationContext.MefServiceGroupContainer.DicRouteIncludePartner;
            string key = thisCdr.SwitchId + "-" + thisCdr.incomingroute;
            route thisRoute = null;
            dicRoutes.TryGetValue(key, out thisRoute);
            if (thisRoute != null)
            {
                if (thisRoute.partner.PartnerType == IcxPartnerType.ANS &&
                    thisRoute.NationalOrInternational == RouteLocalityType.National) //ANS and route=national
                {
                    thisCdr.CallDirection = 1; //Domestic in ICX
                }
            }
        }
        public void SetServiceGroupWiseSummaryParams(CdrExt cdrExt, AbstractCdrSummary newSummary)
        {
            //this._sgIntlTransitVoice.SetServiceGroupWiseSummaryParams(cdrExt, newSummary);
            newSummary.tup_countryorareacode = cdrExt.Cdr.CountryCode;
            newSummary.tup_matchedprefixcustomer = cdrExt.Cdr.matchedprefixcustomer;
            newSummary.tup_matchedprefixsupplier = cdrExt.Cdr.matchedprefixsupplier;
            if (cdrExt.Cdr.ChargingStatus != 1) return;

            acc_chargeable chargeableCust = null;
            cdrExt.Chargeables.TryGetValue(new ValueTuple<int, int, int>(this.Id, 1, 1), out chargeableCust);
            if (chargeableCust == null)
            {
                throw new Exception("Chargeable info not found for customer direction.");
            }
            this._sgIntlTransitVoice.SetChargingSummaryInCustomerDirection(chargeableCust, newSummary);
            
            newSummary.tax1 = 0;
            newSummary.tax2 = 0;
            newSummary.vat = 0;
            newSummary.intAmount1 = 0;
            newSummary.intAmount2 = 0;
            newSummary.longAmount1 = 0;
            newSummary.longAmount2 = 0;
            newSummary.doubleAmount1 = 0;
            newSummary.doubleAmount2 = 0;
        }
    }
}
