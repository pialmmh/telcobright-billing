using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions.ConfigHelper;
using MySql.Data.MySqlClient;

namespace InstallConfig
{
    public static class DbUtil
    {
        public static string getDbConStr(DatabaseSetting databaseSetting, string operatorName)
        {
            return 
                "server=" + databaseSetting.ServerName + ";User Id=" + databaseSetting.AdminUserName +
                ";password=" + databaseSetting.AdminPassword + ";Persist Security Info=True; default command timeout=3600";
        }
        private static bool CreateDatabaseIfRequired(DatabaseSetting databaseSetting, ConfigPathHelper configPathHelper)
        {
            string constr =
                "server=" + databaseSetting.ServerName + ";User Id=" + databaseSetting.AdminUserName +
                ";password=" + databaseSetting.AdminPassword + ";Persist Security Info=True;";
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("", con))
                {
                    Func<bool> dbExists = () =>
                    {
                        cmd.CommandText = "show databases;";
                        MySqlDataReader reader = cmd.ExecuteReader();
                        List<string> databases = new List<string>();
                        while (reader.Read())
                        {
                            databases.Add(reader[0].ToString());
                        }
                        reader.Close();
                        return databases.Contains(databaseSetting.DatabaseName);
                    };
                    Action createDb = () =>
                    {
                        cmd.CommandText = $"CREATE SCHEMA `{databaseSetting.DatabaseName}` DEFAULT CHARACTER SET {databaseSetting.CharacterSet} collate {databaseSetting.Collate};";
                        cmd.ExecuteNonQuery();
                    };
                    Action createTables = () =>
                    {
                        cmd.CommandText = "use " + databaseSetting.DatabaseName;
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = File.ReadAllText(configPathHelper.GetSchedulerScriptPath()
                                                           + Path.DirectorySeparatorChar + "CreateTables.txt");
                        cmd.ExecuteNonQuery();
                    };
                    if (dbExists() == false) //not forced, create db only if doesn't exist
                    {
                        createDb();
                    }
                    createTables();

                }
            }
        }
    }

}
