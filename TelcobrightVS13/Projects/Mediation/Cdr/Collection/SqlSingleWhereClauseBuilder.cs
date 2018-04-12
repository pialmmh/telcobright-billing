namespace TelcobrightMediation
{
    public class SqlSingleWhereClauseBuilder
    {
        public string Expression { get; set; }//e.g. startdate >=
        public SqlWhereParamType ParamType { get; set; }
        public SqlWhereAndOrType AndOrType { get; set; }
        public string ParamValue { get; set; }
        public string PrependWith { get; set; }
        public string ApendWith { get; set; }

        public SqlSingleWhereClauseBuilder(SqlWhereAndOrType andOrFirst)
        {
            this.AndOrType = andOrFirst;
        }
        private string ParamValWithQuoteIfRequired(string str, SqlWhereParamType paramType)
        {
            switch (paramType)
            {
                case SqlWhereParamType.Text:
                case SqlWhereParamType.Datetime:
                    return "'" + str + "'";
                case SqlWhereParamType.Null:
                    return "";
                default:
                    return str;
            }
        }
        public string GetWhereClauseAsString()
        {
            return this.AndOrType == SqlWhereAndOrType.FirstBeforeAndOr
                ? " " + this.PrependWith + " " + this.Expression + ParamValWithQuoteIfRequired(this.ParamValue, this.ParamType) + " " + this.ApendWith + " "
                : this.AndOrType + " " + this.PrependWith + " " + this.Expression + ParamValWithQuoteIfRequired(this.ParamValue, this.ParamType) + " " + this.ApendWith + " ";
        }
    }
}