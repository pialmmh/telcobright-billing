using System.Collections.Generic;

namespace TelcobrightMediation
{
    public class BatchSqlJobParamJson
    {
        public string TableName { get; set; }
        public int BatchSize { get; set; }
        public List<SqlSingleWhereClauseBuilder> LstWhereParamsSingle { get; set; }
        public List<SqlMultiWhereClauseBuilder> LstWhereParamsMultiple { get; set; }
        public List<string> ColumnExpressions { get; } 
        public BatchSqlJobParamJson(string tableName, int batchSize, List<SqlSingleWhereClauseBuilder> lstWhereParamsSingle,
            List<SqlMultiWhereClauseBuilder> lstWhereParamsMulti,List<string> columnExpressions)
        {
            this.TableName = tableName;
            this.BatchSize = batchSize;
            this.LstWhereParamsSingle = lstWhereParamsSingle;
            this.LstWhereParamsMultiple = lstWhereParamsMulti;
            this.ColumnExpressions = columnExpressions;
        }
        public BatchSqlJobParamJson GetCopy()
        {
            return new BatchSqlJobParamJson(this.TableName, this.BatchSize, this.LstWhereParamsSingle,
                this.LstWhereParamsMultiple, this.ColumnExpressions);
        }
        private string GetFullWhereClause()
        {
            List<string> whereLinesSingle = new List<string>();
            foreach (SqlSingleWhereClauseBuilder sp in this.LstWhereParamsSingle)
            {
                whereLinesSingle.Add(sp.GetWhereClauseAsString());
            }

            List<string> whereLinesMulti = new List<string>();
            foreach (SqlMultiWhereClauseBuilder mp in this.LstWhereParamsMultiple)
            {
                whereLinesMulti.Add(mp.GetWhereClauseAsString());
            }

            //determine whether single or multi clauses to put first, right after where
            bool singlesFirst = this.LstWhereParamsSingle[0].AndOrType == SqlWhereAndOrType.FirstBeforeAndOr;
            if (singlesFirst == true)
            {
                return " where " + string.Join(" ", whereLinesSingle) + string.Join(" ", whereLinesMulti);
            }
            else
            {
                return " where " + string.Join(" ", whereLinesMulti) + string.Join(" ", whereLinesSingle);
            }
        }

        public string GetSqlForRowIdAndDate()
        {
            //return " select idcall,date(starttime) as callDate from " + this.TableName + GetFullWhereClause();
            return " select "+string.Join(",",this.ColumnExpressions)+ " from " + this.TableName + GetFullWhereClause();
        }
    }
}