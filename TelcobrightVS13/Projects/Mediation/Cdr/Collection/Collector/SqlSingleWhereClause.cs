using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation
{
    public class SqlSingleWhereClause
    {
        public string Expression { get; set; }//e.g. startdate >=
        public SqlWhereParamType ParamType { get; set; }
        public SqlWhereAndOrType AndOrType { get; set; }
        public string ParamValue { get; set; }
        public string PrependWith { get; set; }
        public string ApendWith { get; set; }

        public SqlSingleWhereClause(SqlWhereAndOrType andOrFirst)
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
            return AndOrType == SqlWhereAndOrType.FirstBeforeAndOr
                ? " " + PrependWith + " " + Expression + ParamValWithQuoteIfRequired(ParamValue, ParamType) + " " + ApendWith + " "
                : AndOrType + " " + PrependWith + " " + Expression + ParamValWithQuoteIfRequired(ParamValue, ParamType) + " " + ApendWith + " ";
        }
    }
}
