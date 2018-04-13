using System;

namespace LibraryExtensions
{
    public static class ValueToSqlFieldConverter
    {
        public static string ToSqlField(this DateTime? val,string dateFormat)
        {
            string str = val.ReplaceNullWith("null", dateFormat);
            return string.Equals(str, "null", StringComparison.InvariantCultureIgnoreCase) ? str : str.EncloseWith("'");
        }
        public static string ToSqlField(this DateTime val, string dateFormat)
        {
            return val.ToString(dateFormat).EncloseWith("'");
        }
        public static string ToSqlField(this string val)
        {
            string str = val.ReplaceNullWith("null");
            return string.Equals(str,"null", StringComparison.InvariantCultureIgnoreCase)?str : str.EncloseWith("'");
        }
        public static string ToSqlField(this char? val)
        {
            return val.ReplaceNullWith("null");
        }
        public static string ToSqlField(this long? val)
        {
            return val.ReplaceNullWith("null");
        }
        public static string ToSqlField(this double? val)
        {
            return val.ReplaceNullWith("null");
        }
        public static string ToSqlField(this double val)
        {
            return val.ToString();
        }
        public static string ToSqlField(this bool val)
        {
            return val == true ? "1" : "0";
        }
        public static string ToSqlField(this decimal? val)
        {
            return val.ReplaceNullWith("null");
        }
        public static string ToSqlField(this decimal val)
        {
            return val.ToString();
        }
        public static string ToSqlField(this Single val)
        {
            return val.ToString();
        }
        public static string ToSqlField(this Single? val)
        {
            return val.ReplaceNullWith("null");
        }
        public static string ToSqlField(this long val)
        {
            return val.ToString();
        }

        public static string ToSqlField(this int? val)
        {
            return val.ReplaceNullWith("null");
        }
        public static string ToSqlField(this short? val)
        {
            return val.ReplaceNullWith("null");
        }
        public static string ToSqlField(this int val)
        {
            return val.ToString();
        }
        public static string ToSqlField(this short val)
        {
            return val.ToString();
        }
        
        public static string ToSqlField(this sbyte? val)
        {
            return val.ReplaceNullWith("null");
        }
        public static string ToSqlField(this sbyte val)
        {
            return val.ToString();
        }
        public static string ToSqlField(this byte? val)
        {
            return val.ReplaceNullWith("null");
        }
        public static string ToSqlField(this byte val)
        {
            return val.ToString();
        }
    }
}
