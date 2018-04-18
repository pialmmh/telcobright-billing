﻿using System;

namespace LibraryExtensions
{
    public static class ValueToNotNullSqlFieldConverter
    {
        public static string ToNotNullSqlField(this string val)
        {
            string str = val.ReplaceNullWith("");
            return str.EncloseWith("'");
        }
        public static string ToNotNullSqlField(this char? val)
        {
            throw new NotImplementedException();
        }

        public static string ToNotNullSqlField(this long? val)
        {
            return val.ReplaceNullWith("0");
        }
        public static string ToNotNullSqlField(this double? val)
        {
            return val.ReplaceNullWith("0");
        }
        public static string ToNotNullSqlField(this decimal? val)
        {
            return val.ReplaceNullWith("0");
        }

        public static string ToNotNullSqlField(this Single? val)
        {
            return val.ReplaceNullWith("0");
        }

        public static string ToNotNullSqlField(this int? val)
        {
            return val.ReplaceNullWith("0");
        }

        public static string ToNotNullSqlField(this short? val)
        {
            return val.ReplaceNullWith("0");
        }

        public static string ToNotNullSqlField(this sbyte? val)
        {
            return val.ReplaceNullWith("0");
        }

        public static string ToNotNullSqlField(this byte? val)
        {
            return val.ReplaceNullWith("0");
        }
    }
}
