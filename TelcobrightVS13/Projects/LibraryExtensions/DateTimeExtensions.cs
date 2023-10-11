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
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
        public static DateTime GetLastDayOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
        }
        public static string GetSqlWhereExpressionForHourlyCollection(this DateTime hourOfTheDay, string dateColName)
        {
            int minute = hourOfTheDay.Minute;
            int second = hourOfTheDay.Second;
            if (minute != 0 || second != 0)
                throw new Exception("Hour of the day must be 0-23 and minutes and seconds parts must be 0.");
            DateTime nextHour = hourOfTheDay.AddHours(1);
            return $" {dateColName}>='{hourOfTheDay.ToMySqlFormatWithoutQuote()}' " +
                   $" and {dateColName}<'{nextHour.ToMySqlFormatWithoutQuote()}' ";
        }
    }
}
