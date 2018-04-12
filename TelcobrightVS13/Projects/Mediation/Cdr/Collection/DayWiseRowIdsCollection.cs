using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;

namespace TelcobrightMediation
{
    public class DayWiseRowIdsCollection
    {
        //both get;set; are required for json deserialization
        public Dictionary<DateTime, List<string>> DayWiseRowIds { get; set; }
        public string IndexedRowIdColumnName { get; set; }

        public string DateColumnName { get; set; }
        public string SourceTable { get; set; }
        private string QuoteCharToEncloseNonNumericRowIdValues { get; set; }
        public DayWiseRowIdsCollection() { }//default constructor for json serialization
        public DayWiseRowIdsCollection(Dictionary<DateTime, List<string>> dayWiseRowIds, string sourceTable,
            string indexedRowIdColName, string dateColName, string quoteCharToEncloseNonNumericRowIdValues)
        {
            this.QuoteCharToEncloseNonNumericRowIdValues = quoteCharToEncloseNonNumericRowIdValues;
            this.DayWiseRowIds = dayWiseRowIds;
            this.SourceTable = sourceTable;
            this.IndexedRowIdColumnName = indexedRowIdColName;
            this.DateColumnName = dateColName;
        }
        public string GetSelectSql()
        {
            if (this.DayWiseRowIds.Any())
            {
                string sql = string.Join(" union all ",
                    this.DayWiseRowIds.Select(kv =>
                        $@"select * from {this.SourceTable} where 
                       {kv.Key.ToMySqlWhereClauseForOneDay(this.DateColumnName)} 
                       and {this.IndexedRowIdColumnName} 
                       in ({string.Join(",",
                            kv.Value.Select(rowId => rowId.EncloseWith(this.QuoteCharToEncloseNonNumericRowIdValues)))
                            })").ToList());
                return sql;
            }
            return string.Empty;
        }

        public string GetDeleteSql()
        {
            if (this.DayWiseRowIds.Any())
            {
                string sql = string.Join(";",
                    this.DayWiseRowIds.Select(kv =>
                        $@"delete from {this.SourceTable} where 
                   {kv.Key.ToMySqlWhereClauseForOneDay(this.DateColumnName)}                 
                   and {this.IndexedRowIdColumnName}
                   in ({string.Join(",", kv.Value.Select(rowId => rowId.EncloseWith(this.QuoteCharToEncloseNonNumericRowIdValues)))})"));
                return sql;
            }
            return string.Empty;
        }
    }
}
