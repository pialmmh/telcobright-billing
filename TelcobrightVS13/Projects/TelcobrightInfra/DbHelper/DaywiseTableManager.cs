using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MySql.Data.MySqlClient;

namespace TelcobrightInfra
{
    public class DaywiseTableManager
    {
        public static List<string> getExistingTableNames(string databaseName, IEnumerable<string> tableNamesToCheckIfExists, MySqlConnection con)
        {
            List<string> existingTables = new List<string>();
            using (MySqlCommand cmd= new MySqlCommand("", con))
            {
                cmd.CommandText = $"show tables from {databaseName} where tables_in_{databaseName} in (" +
                                  $" {string.Join(",", tableNamesToCheckIfExists.Select(t => $"'{t}'"))});";
                cmd.CommandType = CommandType.Text;
                DbDataReader reader1 = cmd.ExecuteReader();
                try
                {
                    while (reader1.Read())
                    {
                        existingTables.Add(reader1[0].ToString());
                    }
                    reader1.Close();
                }
                catch (Exception e)
                {
                    reader1.Close();
                    Console.WriteLine(e);
                    throw;
                }
            }
            return existingTables;
        }
        public static void DeleteOldTables(string tablePrefix, int daysToRetainOldData, DbCommand cmd)
        {

            DateTime lastDateOfKeepingData = DateTime.Now.Date.AddDays(-1 * daysToRetainOldData);

            cmd.CommandText = $"show tables; ";
            DbDataReader reader = cmd.ExecuteReader();

            List<string> possibleTablesToDelete = new List<string>();
            while (reader.Read())
            {
                possibleTablesToDelete.Add(reader[0].ToString());
            }
            reader.Close();
            possibleTablesToDelete = possibleTablesToDelete.Where(t => t.StartsWith(tablePrefix)
                                                                       && t.Length > tablePrefix.Length).ToList();
            foreach (string existingTable in possibleTablesToDelete)
            {
                DateTime tableDate = existingTable.Substring(tablePrefix.Length)
                    .ConvertToDateTimeFromCustomFormat("yyyyMMdd");
                if (tableDate < lastDateOfKeepingData)
                {
                    cmd.CommandText = $"drop table {existingTable}";
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void CreateTables(string tablePrefix, string templateSql, List<DateTime>dateTimes,MySqlConnection con,
            bool partitionByHour,string partitionColName,string engine )
        {
            using (MySqlCommand cmd = new MySqlCommand("", con))
            {
                foreach (DateTime tableDate in dateTimes)
                {
                    string date = tableDate.ToMySqlFormatDateOnlyWithoutTimeAndQuote().Replace("-","");
                    string tableName = tablePrefix + "_" + date;
                    string sql = templateSql.Replace("<" + tablePrefix + ">", tableName);
                    if (partitionByHour)
                    {
                        sql += GetHourlytPartitionExpression(partitionColName, tableDate, engine) + ";";
                    }
                    cmd.CommandText = sql;//avoid using if not exists, that might have triggered the table definition has changed exception
                    cmd.ExecuteNonQuery();
                }
            } 
        }
        public static string GetHourlytPartitionExpression(string partitionColName, DateTime date, string engine)
        {
            int yr = date.Year;
            int mon = date.Month;
            int day = date.Day;
            DateTime partitionDay = new DateTime(yr, mon, day);
            
            Func<DateTime, string> getPartitionExpression = dateHr => 
                new StringBuilder("PARTITION p").Append((dateHr.Hour==0?24:dateHr.Hour).ToString()).Append(" VALUES LESS THAN ('")
                    .Append(dateHr.ToMySqlFormatWithoutQuote()).Append("') ENGINE = ").Append(engine).ToString();

            List<string> hourlyPartitionExpressions = Enumerable.Range(1, 23).Select(hr =>
            {
                var dateWithHour = partitionDay.AddHours(hr);
                return getPartitionExpression(dateWithHour);
            }).ToList();

            var nextDayZeroHour = partitionDay.AddDays(1);
            hourlyPartitionExpressions.Add(getPartitionExpression(nextDayZeroHour));
            return $"PARTITION BY RANGE  COLUMNS({partitionColName})\r\n" +
                    "(" + string.Join(",\r\n", hourlyPartitionExpressions) + ")";
        }
    }
}
