
using System;
using Itenso.TimePeriod;
namespace TelcobrightMediation.Accounting
{
    public interface IBillingCycle
    {
        string RuleName { get; }
        TimeRange GetBillingCycleByBillableItemsDate(DateTime billablesDate);
        DateInterval? BillingInterval { get; set; }
        int BillDuration { get; set; }
    }
}