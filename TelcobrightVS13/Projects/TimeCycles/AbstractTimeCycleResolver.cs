using System;
using System.Collections.Generic;
using Itenso.TimePeriod;

namespace TimeCycles
{
    public abstract class AbstractTimeCycleResolver
    {
        protected DateTime NextTriggerDate { get; set; }
        protected int CycleDuration { get; set; }
        protected AbstractTimeCycleResolver()
        {
            this.NextTriggerDate = new DateTime();
        }
        protected void Initialize(Dictionary<string, object> data)
        {
            this.NextTriggerDate = (DateTime) data["nextTriggerDate"];
            this.CycleDuration = (int) data["cycleDuration"];
            if (CycleDuration<=0)
                throw new Exception("Time cycle duration must be >=0.");
        }
        protected TimeRange ResolveWithMilliSecSubstraction(DateTime timeRangeStart)
        {
            DateTime timeRangeEnd = this.NextTriggerDate.AddMilliseconds(-1);
            return new TimeRange(timeRangeStart, timeRangeEnd);
        }
    }
}