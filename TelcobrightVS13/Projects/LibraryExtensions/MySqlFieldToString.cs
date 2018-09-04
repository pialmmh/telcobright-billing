using System;

namespace LibraryExtensions
{
    public static class MySqlFieldToString
    {
        private static string MySqldateFormat { get {return "yyyy-MM-dd HH:mm:ss"; } }
        public static string ToMySqlField(this DateTime? val)
        {
            return val.ToSqlField(MySqldateFormat);
        }
        public static string ToMySqlFormatWithoutQuote(this DateTime val)
        {
            return val.ToString(MySqldateFormat);
        }
        public static string ToMySqlFormatWithQuote(this DateTime val)
        {
            return val.ToString(MySqldateFormat).EncloseWith("'");
        }
        public static string ToMySqlField(this DateTime val)
        {
            return val.ToSqlField(MySqldateFormat);
        }
        public static string ToMySqlField(this char? val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlField(this string val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlField(this long? val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlField(this double? val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlField(this double val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlField(this bool val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlField(this decimal? val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlField(this decimal val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlField(this Single? val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlField(this long val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlField(this int? val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlField(this int val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlField(this short? val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlField(this short val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlField(this Single val)
        {
            return val.ToSqlField();
        }

        public static string ToMySqlField(this sbyte? val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlField(this sbyte val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlField(this byte? val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlField(this byte val)
        {
            return val.ToSqlField();
        }
        public static string ToMySqlWhereClauseStartOfDate(this DateTime date,string fieldNameInWhereClause)
        {
            return fieldNameInWhereClause + ">=" + date.ToMySqlField();
        }
        public static string ToMySqlWhereClauseEndOfDate(this DateTime date, string fieldNameInWhereClause)
        {
            return fieldNameInWhereClause + "<" + (date.AddDays(1)).ToMySqlField();
        }
        public static string ToMySqlWhereClauseForOneDay(this DateTime date, string fieldNameInWhereClause)
        {
            return ToMySqlWhereClauseStartOfDate(date,fieldNameInWhereClause) + " and "+
                    ToMySqlWhereClauseEndOfDate(date, fieldNameInWhereClause);
        }
    }
}
