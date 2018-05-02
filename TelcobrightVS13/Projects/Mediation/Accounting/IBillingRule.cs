
using System;
using Itenso.TimePeriod;
namespace TelcobrightMediation.Accounting
{
    public interface IBillingRule
    {
        string RuleName { get; }
        string Description { get; }
        bool IsPrepaid { get; set; }
        TimeRange GetBillingCycleByBillableItemsDate(DateTime billablesDate);
    }
}