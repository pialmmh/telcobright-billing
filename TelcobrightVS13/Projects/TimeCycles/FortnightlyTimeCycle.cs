using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Itenso.TimePeriod;
using LibraryExtensions.TimeCycle;
using TelcobrightMediation;
using TelcobrightMediation.Accounting;

namespace TimeCycles
{
    [Export("TimeCycle", typeof(ITimeCycle))]
    public class FortnightlyTimeCycle : AbstractTimeCycleResolver, ITimeCycle
    {
        public string Name => GetType().Name;
        public override string ToString() => this.Name;
        public TimeCycleInterval? Interval { get; set; }
        public int Duration { get; set; }
        public TimeRange Resolve(Dictionary<string,object> data)
        {
            base.Initialize(data);
            DateTime timeRangeStart;
            if (base.NextTriggerDate.Day == 1)
                timeRangeStart = base.NextTriggerDate.AddMonths(-1).AddDays(base.CycleDuration);
            else
                timeRangeStart = base.NextTriggerDate.AddDays(-base.CycleDuration).AddDays(1);

            return base.ResolveWithMilliSecSubstraction(timeRangeStart);
        }
    }
}
