using System.Collections.Generic;

namespace TelcobrightMediation
{
    public class SqlMultiWhereClauseBuilder
    {
        //bracket enclosed multiple single where clause
        public List<SqlSingleWhereClauseBuilder> SingleParams { get; set; }
        public SqlWhereAndOrType AndOrType { get; set; }
        public string PrependWith { get; set; }
        public string ApendWith { get; set; }
        public SqlMultiWhereClauseBuilder(SqlWhereAndOrType andOrFirst)
        {
            this.AndOrType = andOrFirst;
            this.SingleParams = new List<SqlSingleWhereClauseBuilder>();
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
            List<string> singleClauses = new List<string>();
            foreach (SqlSingleWhereClauseBuilder sc in this.SingleParams)
            {
                singleClauses.Add(sc.GetWhereClauseAsString());
            }

            string combinedClauses = string.Join(" ", singleClauses);
            return this.AndOrType == SqlWhereAndOrType.FirstBeforeAndOr
                ? " (" + this.PrependWith + " " + combinedClauses + " " + this.ApendWith + " ) "
                : this.AndOrType + " (" + this.PrependWith + " " + combinedClauses + " " + this.ApendWith + " ) ";
        }
    }
}