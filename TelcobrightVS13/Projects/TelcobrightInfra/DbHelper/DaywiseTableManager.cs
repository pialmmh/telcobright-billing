using System;
using System.Collections.Generic;
using System.Data;
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
    class DaywiseTableManager
    {

        public static void DeleteOldTables(string tablePrefix, int daysToRetainOldData, string connectionString)
        {

            DateTime lastDateOfKeepingData = DateTime.Now.Date.AddDays(-1 * daysToRetainOldData);
            //string databasename = "testdb";
            //string tablename = tablePrefix+lastDateOfKeepingData.ToString("yyyy MMMM dd");
            // string connectionString = $"Server = localhost; User Id = root; Password = '';";



            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                if (connection.State != ConnectionState.Open) connection.Open();
                string queury = $"show tables; ";
                MySqlCommand command = new MySqlCommand(queury, connection);
                MySqlDataReader reader = command.ExecuteReader();

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
                        string droptablequeury = $"drop table {existingTable}";
                        MySqlCommand dropCommand = new MySqlCommand(droptablequeury, connection);
                        dropCommand.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
        }



        public static void CreateTable(string tablePrefix, string createTableTemplateSql, List<DateTime>dateTimes,
            string conStr )
        {
            using (MySqlConnection connection = new MySqlConnection(conStr))
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                foreach (DateTime dateTime in dateTimes)
                {
                    string date = dateTime.ToString("yyyyMMdd");
                    //string sql =
                    //$"create table {tablePrefix}{date} (col1 varchar(50), col2 varchar(50), col3 varchar(50));"; // Replace with your actual table name

                    //string s = $"create table {tablePrefix}";
                    string tableName = tablePrefix + date;

                    string sql = "create table" + tableName +
                                 createTableTemplateSql.Remove(0, createTableTemplateSql.IndexOf('('));

                    MySqlCommand command = new MySqlCommand(sql, connection);
                    command.ExecuteNonQuery();
                }


            }
        }
    }
}
