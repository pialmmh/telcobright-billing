using System;
using Itenso.TimePeriod;

namespace TelcobrightMediation.Accounting
{
    public interface ITimeRangeImplementer
    {
        TimeRange TimeRangeAsBillingPeriod { get; set; }
        ITimeRangePopulator TimeRangePopulator { get; set; }
    }
}