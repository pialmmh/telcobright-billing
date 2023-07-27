using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LibraryExtensions
{
    public static class StringExtensions
    {
        public const string MySqlDateTimeFormat= "yyyy-MM-dd HH:mm:ss";
        public static T? GetValueOrNull<T>(this string valueAsString)
            where T : struct
        {
            if (string.IsNullOrEmpty(valueAsString)||string.IsNullOrWhiteSpace(valueAsString))
                return null;
            return (T)Convert.ChangeType(valueAsString, typeof(T));
        }
        public static T GetValue<T>(this string valueAsString)
            where T : struct
        {
            if (string.IsNullOrEmpty(valueAsString) || string.IsNullOrWhiteSpace(valueAsString))
                return default(T) ;
            return (T)Convert.ChangeType(valueAsString, typeof(T));
        }
        public static String Left(this string input, int length)
        {
            var result = "";
            if ((input.Length <= 0)) return result;
            if ((length > input.Length))
            {
                length = input.Length;
            }
            result = input.Substring(0, length);
            return result;
        }

        public static String Mid(this string input, int start, int length)
        {
            var result = "";
            if (((input.Length <= 0) || (start >= input.Length))) return result;
            if ((start + length > input.Length))
            {
                length = (input.Length - start);
            }
            result = input.Substring(start, length);
            return result;
        }

        public static String Right(this string input, int length)
        {
            var result = "";
            if ((input.Length <= 0)) return result;
            if ((length > input.Length))
            {
                length = input.Length;
            }
            result = input.Substring((input.Length - length), length);
            return result;
        }
        public static String ReplaceLastOccurrence(string Source, string Find, string Replace)
        {
            int place = Source.LastIndexOf(Find);

            if (place == -1)
                return Source;

            string result = Source.Remove(place, Find.Length).Insert(place, Replace);
            return result;
        }
        public static bool IsNumeric(this string s)
        {
            double output;
            return double.TryParse(s, out output);
        }
        public static bool IsDateTime(this string s, string dateFormat)
        {
            DateTime tempdate = new DateTime();
            return DateTime.TryParseExact(s, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempdate);
        }
        public static bool IsMySqlDateTime(this string s)
        {
            DateTime tempdate = new DateTime();
            return DateTime.TryParseExact(s, MySqlDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempdate);
        }
        public static bool TryParseToDateTimeFromMySqlFormat(this string s,out DateTime targetDate)
        {
            targetDate = new DateTime();
            if(DateTime.TryParseExact(s, MySqlDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out targetDate)==true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public static DateTime ConvertToDateTimeFromCustomFormat(this string s, string dateFormat)
        {
            DateTime targetDate = new DateTime();
            if (DateTime.TryParseExact(s, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out targetDate) == true)
            {
                return targetDate;
            }
            else
            {
                throw new Exception("Format String is not a valid date time format.");
            }
        }
        public static DateTime ConvertToDateTimeFromMySqlFormat(this string s)
        {
            DateTime targetDate = new DateTime();
            if (DateTime.TryParseExact(s, MySqlDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out targetDate) == true)
            {
                return targetDate;
            }
            else
            {
                throw new Exception("String not in MySql format.");
            }
        }
        public static DateTime? ConvertToNullableDateTimeFromMySqlFormat(this string s)
        {
            DateTime targetDate = new DateTime();
            if (DateTime.TryParseExact(s, MySqlDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out targetDate) == true)
            {
                return targetDate;
            }
            else
            {
                return null;
            }
        }
        public static bool IsNullOrEmptyOrWhiteSpace(this string s)
        {
            return string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s);
        }
        public static bool ValueIn(this string s,IEnumerable<string> values)
        {
            return values.Contains(s);
        }
        public static bool ValueNotIn(this string s, IEnumerable<string> values)
        {
            return !values.Contains(s);
        }
    }

}
