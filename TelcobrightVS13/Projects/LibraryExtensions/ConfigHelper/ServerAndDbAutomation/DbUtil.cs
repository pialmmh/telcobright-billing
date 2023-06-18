using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;
using MySql.Data.MySqlClient;

namespace LibraryExtensions.ConfigHelper
{
    public static class DbUtil
    {
        public static ConfigPathHelper configPathHelper { get; set; }

        public static string getDbConStrPipeWithoutDatabase(DatabaseSetting databaseSetting)
        {
            return
                $"server={databaseSetting.ServerName};Pipe={databaseSetting.SocketNameForNamedPipeConnection};Protocol=pipe;Host=.;User Id={databaseSetting.AdminUserName};password={databaseSetting.AdminPassword};" +
                $"Persist Security Info=True; default command timeout=3600;";
        }
        public static string getDbConStrWithoutDatabase(DatabaseSetting databaseSetting)
        {
            return
                $"server={databaseSetting.ServerName};User Id={databaseSetting.AdminUserName};password={databaseSetting.AdminPassword};" +
                $"Persist Security Info=True; default command timeout=3600;";
        }

        public static string getDbConStrWithDatabase(DatabaseSetting databaseSetting)
        {
            return getDbConStrWithoutDatabase(databaseSetting) + $"database={databaseSetting.DatabaseName};";
        }

        public static string execCommandAndGetOutput(MySqlConnection con, string commandText)
        {
            MySqlCommand cmd= new MySqlCommand();
            cmd.CommandText = commandText;
            MySqlDataReader reader = cmd.ExecuteReader();
            StringBuilder sb = new StringBuilder();
            while (reader.Read())
            {
                sb.Append(reader[0].ToString());
            }
            reader.Close();
            return sb.ToString();
        }

        public static void CreateDatabase(DatabaseSetting databaseSetting)
        {
            string constr = getDbConStrWithoutDatabase(databaseSetting);
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("", con);
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
                    cmd.CommandText =
                        $"CREATE SCHEMA `{databaseSetting.DatabaseName}` DEFAULT CHARACTER SET {databaseSetting.CharacterSet} collate {databaseSetting.Collate};";
                    cmd.ExecuteNonQuery();
                };
                Action createTables = () =>
                {
                    cmd.CommandText = "use " + databaseSetting.DatabaseName;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = File.ReadAllText(configPathHelper.GetDbScriptPath()
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

        public static void CreateOrOverwriteQuartzTables(DatabaseSetting databaseSetting)
        {
            string constr = getDbConStrWithDatabase(databaseSetting);
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("", con))
                {
                    cmd.CommandText = "use " + databaseSetting.DatabaseName;
                    cmd.ExecuteNonQuery();

                    List<string> commands = File.ReadAllText(configPathHelper.GetDbScriptPath()
                                                             + Path.DirectorySeparatorChar + "quartzTables.sql")
                        .Split(';').Select(c => c.Trim()).Where(c => !c.IsNullOrEmptyOrWhiteSpace()).ToList();
                    foreach (string command in commands)
                    {
                        cmd.CommandText = command;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
