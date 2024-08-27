using System;
using System.Collections.Generic;

namespace TelcobrightMediation
{
    public class HourlyEventData<T>
    {
        public List<T> Events { get; }
        public DateTime HourOfTheDay { get; }
        public DateTime Date { get; private set; }
        public HourlyEventData(List<T> events, DateTime hourOfTheDay)
        {
            DateTime date = hourOfTheDay.Date;
            int hour = this.HourOfTheDay.Hour;
            int minute = this.HourOfTheDay.Minute;
            int second = this.HourOfTheDay.Second;
            if (minute != 0 || second != 0)
                throw new Exception("Hour of the day must be 0-23 and minutes and seconds parts must be 0.");
            this.Events = events;
            this.HourOfTheDay = hourOfTheDay;
        }
    }
}