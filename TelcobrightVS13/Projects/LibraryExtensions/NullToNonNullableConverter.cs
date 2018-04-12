using System;

namespace LibraryExtensions
{
    public static class NullToNonNullableConverter
    {
        public static sbyte ConvertToNonNullableValueOrZeroIfNull(this sbyte? val)
        {
            return Convert.ToSByte(val);
        }
        public static Int16 ConvertToNonNullableValueOrZeroIfNull(this Int16? val)
        {
            return Convert.ToInt16(val);
        }
        public static int ConvertToNonNullableValueOrZeroIfNull(this int? val)
        {
            return Convert.ToInt32(val);
        }
        public static long ConvertToNonNullableValueOrZeroIfNull(this long? val)
        {
            return Convert.ToInt64(val);
        }
        public static double ConvertToNonNullableValueOrZeroIfNull(this double? val)
        {
            return Convert.ToDouble(val);
        }
        public static Single ConvertToNonNullableValueOrZeroIfNull(this Single? val)
        {
            return Convert.ToSingle(val);
        }
    }
}
