﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;

namespace TelcobrightMediation
{
    public class RowIdsCollectionForSingleDay
    {
        //both get;set; are required for json deserialization
        public DateTime Date { get; set; }
        public List<string> RowIds { get; set; }
        public string IndexedRowIdColumnName { get; set; }
        public string DateColumnName { get; set; }
        public string SourceTable { get; set; }
        public string QuoteCharToEncloseNonNumericRowIdValues { get; set; }
        public RowIdsCollectionForSingleDay() { }//default constructor for json serialization
        public RowIdsCollectionForSingleDay(DateTime date, List<string> rowIds, string sourceTable,
            string indexedRowIdColName, string dateColName, string quoteCharToEncloseNonNumericRowIdValues)
        {
            if (rowIds.Any()==false)
            {
                throw new Exception("Ther is no row id in day wise collection, probably erroneous call.");
            }
            this.Date = date.Date;
            this.RowIds = rowIds;
            this.QuoteCharToEncloseNonNumericRowIdValues = quoteCharToEncloseNonNumericRowIdValues;
            this.SourceTable = sourceTable;
            this.IndexedRowIdColumnName = indexedRowIdColName;
            this.DateColumnName = dateColName;
        }

        public string GetSelectSql()
        {
            string commaSepRowids = string.Join(",",
                this.RowIds.Select(id => id.EncloseWith(this.QuoteCharToEncloseNonNumericRowIdValues)));
            string sql =
                $@"select * from {this.SourceTable} where 
                    {this.Date.ToMySqlWhereClauseForOneDay(this.DateColumnName)} 
                    and {this.IndexedRowIdColumnName} in ({
                    commaSepRowids
                    })";
            return sql;
        }

        public string GetDeleteSql()
        {
            if (this.RowIds.Any())
            {
                string sql = $@"delete from {this.SourceTable} where 
                   {this.Date.ToMySqlWhereClauseForOneDay(this.DateColumnName)}                 
                   and {this.IndexedRowIdColumnName}
                   in ({string.Join(",", this.RowIds.Select(id=>id.EncloseWith(this.QuoteCharToEncloseNonNumericRowIdValues)))})";
                return sql;
            }
            return string.Empty;
        }
    }
}
