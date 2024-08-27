using System;
using System.Collections.Generic;

namespace TelcobrightMediation
{
    public class MinuteWiseEventData<T>
    {
        public List<T> Events { get; }
        public DateTime MinuteOfTheDay { get; }
        public MinuteWiseEventData(List<T> events, DateTime minuteOfTheDay)
        {
            int second = minuteOfTheDay.Second;
            if (second != 0)
                throw new Exception("Seconds part must be 0 for minute of the day.");
            this.Events = events;
            this.MinuteOfTheDay = minuteOfTheDay;
        }
    }
}