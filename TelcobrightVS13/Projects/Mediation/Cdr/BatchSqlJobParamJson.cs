using System;
using System.Collections.Generic;

namespace TelcobrightMediation
{
    public class BatchSqlJobParamJson
    {
        public string TableName { get; set; }
        public int BatchSize { get; set; }
        public DateTime StartPartitionDate { get; set; }
        public DateTime EndPartitionDate { get; set; }
        public string PartitionColName { get; set; }
        public string RowIdColName { get; set; }
        public List<SqlSingleWhereClauseBuilder> LstWhereParamsSingle { get; set; }
        public List<SqlMultiWhereClauseBuilder> LstWhereParamsMultiple { get; set; }
        public List<string> ColumnExpressions { get; } 
        public BatchSqlJobParamJson(string tableName, int batchSize, List<SqlSingleWhereClauseBuilder> lstWhereParamsSingle,
            List<SqlMultiWhereClauseBuilder> lstWhereParamsMulti,List<string> columnExpressions,
            DateTime startPartitionDate,DateTime endPartitionDate,string partitionColName,string rowIdColName)
        {
            this.TableName = tableName;
            this.BatchSize = batchSize;
            this.LstWhereParamsSingle = lstWhereParamsSingle;
            this.LstWhereParamsMultiple = lstWhereParamsMulti;
            this.ColumnExpressions = columnExpressions;
            this.StartPartitionDate = startPartitionDate;
            this.EndPartitionDate = endPartitionDate;
            this.PartitionColName = partitionColName;
            this.RowIdColName = rowIdColName;
        }
        public BatchSqlJobParamJson GetCopy()
        {
            List<SqlSingleWhereClauseBuilder> whereParams= new List<SqlSingleWhereClauseBuilder>();
            foreach (var item in this.LstWhereParamsSingle)
            {
                whereParams.Add(item);
            }
            return new BatchSqlJobParamJson(this.TableName, this.BatchSize, whereParams,
                this.LstWhereParamsMultiple, this.ColumnExpressions,this.StartPartitionDate,this.EndPartitionDate,
                this.PartitionColName,this.RowIdColName);
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
            //return " select IdCall,date(starttime) as callDate from " + this.TableName + GetFullWhereClause();
            return " select "+string.Join(",",this.ColumnExpressions)+ " from " + this.TableName + GetFullWhereClause();
        }
    }
}