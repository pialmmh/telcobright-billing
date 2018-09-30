using System;
using System.Collections.Generic;
using Itenso.TimePeriod;

namespace LibraryExtensions.TimeCycle
{
    public interface ITimeCycle
    {
        string Name { get; }
        TimeRange Resolve(Dictionary<string,object> data);
        TimeCycleInterval? Interval { get; set; }
        int Duration { get; set; }
    }
}