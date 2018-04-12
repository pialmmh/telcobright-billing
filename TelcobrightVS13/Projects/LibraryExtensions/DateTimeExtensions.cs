using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public static class DateTimeExtensions
    {
        public static DateTime RoundDownToDay(this DateTime dt)
        {
            return dt.Date;
        }
        public static DateTime RoundDownToHour(this DateTime dt)
        {
            return dt.RoundDownToMinutes().AddMinutes(-1 * dt.Minute);
        }
        public static DateTime RoundDownToMinutes(this DateTime dt)
        {
            return dt.AddSeconds(-1 * dt.Second);
        }
    }
}
