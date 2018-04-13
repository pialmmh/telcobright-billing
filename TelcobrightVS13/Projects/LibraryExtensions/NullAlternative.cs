using System;

namespace LibraryExtensions
{
    public static class NullAlternative
    {
        public static string ReplaceNullWith(this string val, string nullAlternate)
        {
            return string.IsNullOrEmpty(val) ? nullAlternate : val;
        }
        public static string ReplaceNullWith(this char? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string ReplaceNullWith(this long? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string ReplaceNullWith(this double? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string ReplaceNullWith(this decimal? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string ReplaceNullWith(this Single? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string ReplaceNullWith(this int? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string ReplaceNullWith(this short? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string ReplaceNullWith(this sbyte? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string ReplaceNullWith(this byte? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string ReplaceNullWith(this DateTime? val, string nullAlternate,string dateFormat)
        {
            return val == null ? nullAlternate : (Convert.ToDateTime(val)).ToString(dateFormat);
        }
    }
}
