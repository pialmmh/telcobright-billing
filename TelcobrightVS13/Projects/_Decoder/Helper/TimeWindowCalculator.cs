using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Decoders.Helper
{
    public class TimeWindowCalculator
    {
        private TimeSpan windowSize;

        public TimeWindowCalculator(TimeSpan windowSize)
        {
            this.windowSize = windowSize;
        }

        public DateTime GetRoundedDownDateTime(DateTime dateTime)
        {
            // Check if the time is between 11:55 PM and 11:59 PM
            if (dateTime.TimeOfDay >= TimeSpan.Parse("23:55") && dateTime.TimeOfDay <= TimeSpan.Parse("23:59"))
            {
                // If so, return the next day's 12:00 AM
                return dateTime.Date.AddDays(1);
            }
            else
            {
                // Otherwise, calculate the rounded down time normally
                long ticks = dateTime.Ticks / windowSize.Ticks;
                return new DateTime(ticks * windowSize.Ticks);
            }
        }
    }
}
