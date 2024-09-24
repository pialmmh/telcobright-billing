using Spring.Expressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Data;
using System.Text;
using WinSCP;

namespace LibraryExtensions
{
    public static class TelcobrightClassExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static void SetColumnsOrder(this DataTable table, params String[] columnNames)
        {
            int columnIndex = 0;
            foreach (var columnName in columnNames)
            {
                table.Columns[columnName].SetOrdinal(columnIndex);
                columnIndex++;
            }
        }
        /// <summary>
        /// Converts a DataTable to a list with generic objects
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="table">DataTable</param>
        /// <returns>List with generic objects</returns>
        public static List<T> DataTableToList<T>(this DataTable table) where T : class, new()
        {
            try
            {
                List<T> list = new List<T>();

                foreach (var row in table.AsEnumerable())
                {
                    T obj = new T();

                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        try
                        {
                            PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
                            propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    list.Add(obj);
                }

                return list;
            }
            catch
            {
                return null;
            }
        }
        public static string EncloseWithDoubleQuotes(this string text)
        {
            return EncloseWith(text, "\"");
        }
        public static string EncloseWithSingleQuotes(this string text)
        {
            return EncloseWith(text, "'");
        }

        public static string EncloseWith(this string text, string encloseWith)
        {
            return new StringBuilder(encloseWith).Append(text).Append(encloseWith).ToString();
        }

        //extend RemoteFileInfo
        public static bool GetBoolByExpression(this RemoteFileInfo remoteFileInfo, IExpression exp)
        {
            if (exp == null) return true;
            return (bool)exp.GetValue(remoteFileInfo);
        }
        public static string GetStringByExpression(this RemoteFileInfo remoteFileInfo, IExpression exp)
        {
            if (exp == null) return "";
            return (string)exp.GetValue(remoteFileInfo);
        }
        public static DateTime? GetDateTimeByExpression(this RemoteFileInfo remoteFileInfo, string dateStr,string dateFormat)
        {
            DateTime tempdate = new DateTime();
            if(DateTime.TryParseExact(dateStr,dateFormat,CultureInfo.InvariantCulture, DateTimeStyles.None,out tempdate))
            {
                return tempdate;
            }
            else
            {
                return null;
            }
        }
        //extend FileInfo
        public static bool GetBoolByExpression(this FileInfo fileInfo, IExpression exp)
        {
            if (exp == null) return true;
            return (bool)exp.GetValue(fileInfo);
        }
        public static string GetStringByExpression(this FileInfo fileInfo, IExpression exp)
        {
            if (exp == null) return "";
            return (string)exp.GetValue(fileInfo);
        }
        public static DateTime? GetDateTimeByExpression(this FileInfo fileInfo, string dateStr, string dateFormat)
        {
            DateTime tempdate = new DateTime();
            if (DateTime.TryParseExact(dateStr, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempdate))
            {
                return tempdate;
            }
            else
            {
                return null;
            }
        }

    }
    public class Util
    {


        public static DateRange DateIntersection(DateRange compareMe, DateRange compareWith)
        {
            DateRange intersection = null;

            //                  |2jan-----------------|5 feb------------|28 feb
            //----|10 Dec--|---------7jan
            //-------------|1jan-----------------|30 Jan  [*****Compare With*******]
            //|2dec-----------5 Jan-----------|
            if ((compareMe.StartDate <= compareWith.StartDate) && (compareMe.EndDate > compareWith.StartDate))
            {
                intersection = new DateRange();
                intersection.StartDate = compareWith.StartDate;
                if (compareMe.EndDate > compareWith.EndDate)
                {
                    intersection.EndDate = compareWith.EndDate;
                }
                else if (compareMe.EndDate <= compareWith.EndDate)
                {
                    intersection.EndDate = compareMe.EndDate;
                }
                return intersection;
            }
            else if (compareMe.StartDate >= compareWith.StartDate && compareMe.StartDate < compareWith.EndDate)
            {
                intersection = new DateRange();
                intersection.StartDate = compareMe.StartDate;
                if (compareMe.EndDate >= compareWith.EndDate)
                {
                    intersection.EndDate = compareWith.EndDate;
                }
                else if (compareMe.EndDate < compareWith.EndDate)
                {
                    intersection.EndDate = compareMe.EndDate;
                }
            }
            return intersection;//will return null if does not intersect
        }
    }
   
    
    //customized way to handle return value and exception in functions 
    public class FunctionError
    {
        public Exception Exception = null;
    }
    public class Frint : FunctionError
    {
        public int Val = 0;
    }
    //public class frObj : FunctionError
    //{
    //    public object obj = new object();
    //}
    public enum ServiceAssignmentDirection
    {
        None = 0,
        Customer = 1,
        Supplier = 2
    }
    public enum RateChangeType
    {
        All = -1,
        Increase = 3,
        Decrease = 4,
        Unchanged = 5,
        New = 2
    }

    public class DateAndHour : IEquatable<DateAndHour>
    {
        public DateTime Date { get; }
        public int Hour { get; }

        public DateAndHour(DateTime date, int hour)
        {
            this.Date = date;
            this.Hour = hour;
        }

        // Override Equals to compare DateAndHour objects
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is DateAndHour))
                return false;

            return Equals((DateAndHour)obj);
        }

        // Implement IEquatable<DateAndHour>.Equals method
        public bool Equals(DateAndHour other)
        {
            if (other == null)
                return false;

            return this.Date.Date == other.Date.Date && this.Hour == other.Hour;
        }

        // Override GetHashCode to provide a unique hash code for the object
        public override int GetHashCode()
        {
            // Combine the hash codes of Date and Hour
            int hashDate = this.Date.Date.GetHashCode();
            int hashHour = this.Hour.GetHashCode();

            return hashDate ^ hashHour;
        }
    }

    public class DateRange : IEquatable<DateRange>
    {
        public DateTime StartDate = new DateTime(1, 1, 1);
        public DateTime EndDate = new DateTime(1, 1, 1);

        public DateRange()
        {
        }

        public DateRange(DateTime startDateTime, DateTime endDateTime)
        {
            this.StartDate = startDateTime;
            this.EndDate = endDateTime;
        }

        public override string ToString()
        {
            return this.StartDate.ToString("yyyy-MM-dd HH:mm:ss") + " to " + this.EndDate.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public List<DateTime> GetInvolvedHours()
        {
            var start = this.StartDate;
            var end = this.EndDate;
            List<DateTime> hours = new List<DateTime>();

            // Round down the start time to the nearest hour
            DateTime current = new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0);

            // Add hours to the list while within the range
            while (current <= end)
            {
                hours.Add(current);
                current = current.AddHours(1);
            }

            return hours;
        }

        public bool WithinRange(DateTime dateTime)
        {
            return dateTime >= this.StartDate && dateTime < this.EndDate;
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 29 + this.StartDate.GetHashCode();
                hash = hash * 29 + this.EndDate.GetHashCode();
                return hash;
            }
        }
        public DateRange NewRangeByAddingDays(int daysToAdd)
        {
            DateRange newRange = new DateRange();
            newRange.StartDate = this.StartDate.AddDays(daysToAdd);
            newRange.EndDate = this.EndDate.AddDays(daysToAdd);
            return newRange;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as DateRange);
        }
        public bool Equals(DateRange obj)
        {
            return obj != null &&
                (
                obj.StartDate == this.StartDate &&
                obj.EndDate == this.EndDate);
        }
        public class EqualityComparer : IEqualityComparer<DateRange>
        {
            public bool Equals(DateRange x, DateRange y)
            {
                return
                    x != null
                    && y != null
                    && (x.StartDate == y.StartDate && x.EndDate == y.EndDate);
            }

            public int GetHashCode(DateRange x)
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    // Suitable nullity checks etc, of course :)
                    hash = hash * 29 + x.StartDate.GetHashCode();
                    hash = hash * 29 + x.EndDate.GetHashCode();
                    return hash;
                }
            }

        }
        public string GetWhereExpressionRates(string startfieldname, string endfieldname)
        {
            return " (( " + startfieldname + " <= '" + this.StartDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and ifnull(" + endfieldname + ",'9999-12-31 23:59:59') > '" + this.StartDate.ToString("yyyy-MM-dd HH:mm:ss") + "') " +
                "  or " +
                " ( " + startfieldname + " >= '" + this.StartDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and " + startfieldname + " < '" + this.EndDate.ToString("yyyy-MM-dd HH:mm:ss") + "')) ";
        }
        public string GetWhereExpressionForTimeField(string timeFieldName)
        {
            return timeFieldName + " >= '" + this.StartDate.ToString("yyyy-MM-dd HH:mm:ss") +
                   "  and " + timeFieldName + " <= '" + this.EndDate.ToString("yyyy-MM-dd HH:mm:ss");
        }
        //public override int GetHashCode()
        //{
        //    return
        //        (StartDate == null ? 0 : StartDate.GetHashCode()) ^
        //        (EndDate == null ? 0 : EndDate.GetHashCode());
        //}

    }
}
