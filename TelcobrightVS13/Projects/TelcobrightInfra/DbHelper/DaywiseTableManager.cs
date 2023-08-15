using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MySql.Data.MySqlClient;

namespace TelcobrightInfra
{
    public class DaywiseTableManager
    {

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



        public static void CreateTables(string tablePrefix, string createTableTemplateSql, List<DateTime>dateTimes,MySqlConnection con)
        {
            using (MySqlCommand cmd = new MySqlCommand("", con))
            {
                foreach (DateTime dateTime in dateTimes)
                {
                    string date = dateTime.ToString("yyyyMMdd");
                    string tableName = tablePrefix + date;

                    string sql = "create table if not exists " + tableName + " " +
                                 createTableTemplateSql.Remove(0, createTableTemplateSql.IndexOf('('));
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            } 
            
        }
    }
}
