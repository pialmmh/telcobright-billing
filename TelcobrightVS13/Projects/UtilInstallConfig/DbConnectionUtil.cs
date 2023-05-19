using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions.ConfigHelper;
using MySql.Data.MySqlClient;

namespace InstallConfig
{
    public static class DbConnectionUtil
    {
        public static MySqlConnection getDbConnection(DatabaseSetting databaseSetting, string operatorName)
        {
            string operatorDatabaseName = databaseSetting.operatorWiseDatabaseNames["db_" + operatorName];
            List<string> partitionedTables = databaseSetting.PartitionedTables;
            string constr =
                "server=" + databaseSetting.ServerName + ";User Id=" + databaseSetting.AdminUserName +
                ";password=" + databaseSetting.AdminPassword + ";Persist Security Info=True; default command timeout=3600";
            MySqlConnection con= new MySqlConnection(constr);
            return con;
        }
    }
}
