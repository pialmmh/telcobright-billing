using System;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using Itenso.TimePeriod;
using MediationModel;
using TelcobrightMediation.Accounting;

namespace PartnerRules
{

    [Export("BillingCycle", typeof(IBillingCycle))]
    public class Yearly : IBillingCycle
    {
        public DateInterval? BillingInterval { get; set; }
        public int BillDuration { get; set; }
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public TimeRange GetBillingCycleByBillableItemsDate(DateTime billablesDate)
        {
            throw new NotImplementedException();
        }
        public int Id => 1;
        public int Execute(cdr thisCdr, MefPartnerRulesContainer pData)
        {
            var key = new ValueTuple<int,string>(thisCdr.SwitchId, thisCdr.IncomingRoute);
            route thisRoute = null;
            pData.SwitchWiseRoutes.TryGetValue(key, out thisRoute);
            if (thisRoute != null)
            {
                thisCdr.InPartnerId = thisRoute.idPartner;
                return thisRoute.idPartner;
            }
            thisCdr.InPartnerId = 0;
            return 0;
        }
    }
}
