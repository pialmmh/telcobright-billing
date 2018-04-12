using System;

namespace LibraryExtensions
{
    public static class NullAlternative
    {
        public static string AlternateStringIfNull(this string val, string nullAlternate)
        {
            return string.IsNullOrEmpty(val) ? nullAlternate : val;
        }
        public static string AlternateStringIfNull(this char? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string AlternateStringIfNull(this long? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string AlternateStringIfNull(this double? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string AlternateStringIfNull(this decimal? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string AlternateStringIfNull(this Single? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string AlternateStringIfNull(this int? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string AlternateStringIfNull(this short? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string AlternateStringIfNull(this sbyte? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string AlternateStringIfNull(this byte? val, string nullAlternate)
        {
            return val == null ? nullAlternate : val.ToString();
        }
        public static string AlternateStringIfNull(this DateTime? val, string nullAlternate,string dateFormat)
        {
            return val == null ? nullAlternate : (Convert.ToDateTime(val)).ToString(dateFormat);
        }
    }
}
