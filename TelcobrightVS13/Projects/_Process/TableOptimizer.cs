using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Data;
using MediationModel;
using TelcobrightMediation.Config;

namespace Process
{
    class TableOptimizer
    {
        private static TableOptimizer tableOptimizer;
        private TableOptimizer()
        {

        }
        public static TableOptimizer creatDeleteTableObject()
        {
            if (tableOptimizer == null)
            {
                tableOptimizer = new TableOptimizer();
            }
            return tableOptimizer;
        }

        public  void deleteTables(PartnerEntities context)
        {
            DbConnection connection = context.Database.Connection;
            string query = "";
            List<string> tableNames = new List<string>();

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                tableNames = context.Database
                        .SqlQuery<string>($@"SHOW TABLES;").ToList();
                int cnt = 0;
                foreach (string tableName in tableNames)
                {
                    if (tableName.Length > 8 && IsValidTable(tableName) && isDateOver(tableName))
                    {

                        using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(context))
                        {
                            try
                            {
                                String sql = $"drop table {tableName};";
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();
                                Console.WriteLine($" {tableName} Table deleted successfully");
                                cnt++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"An error occurred: {ex.Message}");
                            }

                        }
                    }
                }
                Console.WriteLine("{0} Tables deleted", cnt);
            }
        }
        private bool IsValidTable(string name)
        {
            string dateTime =  name.Substring(name.Length - 8); 
            if (name[0] == 'z' && name[1] == 'z' && isDatetimeFormate(dateTime)) return true;
            return false;
        }

        private bool isDatetimeFormate(string name)
        {
            DateTime date;
            bool flag = DateTime.TryParseExact(name, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None,out  date);
            return flag;
        }

        private bool isDateOver (string name)
        {
            string dateTime = name.Substring(name.Length - 8);
            DateTime today = DateTime.Now;
            DateTime strDate;
            bool flag = DateTime.TryParseExact(dateTime, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out strDate);
            TimeSpan dateDifference = today - strDate;
            if (dateDifference.Days > 7) return true;
            return false;
        }
    }
}
