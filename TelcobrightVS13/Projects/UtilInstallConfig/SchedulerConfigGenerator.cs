using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions.ConfigHelper;
using Newtonsoft.Json;
using System.IO;
using LibraryExtensions;

namespace InstallConfig
{
    public static class SchedulerConfigGenerator
    {
        static string databaseConfigFileName = new DirectoryInfo(FileAndPathHelper.GetBinPath()).Parent.Parent.FullName
                                               + Path.DirectorySeparatorChar + "Server.conf";


        public static SchedulerSetting GeneraterateSchedulerConfig(string operatorName="")
        {
            DatabaseSetting databaseSetting = new DatabaseSetting();
            Dictionary<string, string> settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                File.ReadAllText(databaseConfigFileName));

            databaseSetting.ServerName = settings["ServerName"];
            //databaseSetting.DatabaseName = settings["DatabaseName"];
            databaseSetting.AdminUserName = settings["AdminUserName"];
            databaseSetting.AdminPassword = settings["AdminPassword"];
            databaseSetting.ReadOnlyUserName = settings["ReadOnlyUserName"];
            databaseSetting.ReadOnlyPassword = settings["ReadOnlyPassword"];

            databaseSetting.DatabaseEngine = settings["DatabaseEngine"];
            databaseSetting.StorageEngineForPartitionedTables = settings["StorageEngineForPartitionedTables"];
            databaseSetting.PartitionStartDate = Convert.ToDateTime(settings["PartitionStartDate"]);
            databaseSetting.PartitionLenInDays = Convert.ToInt32(settings["PartitionLenInDays"]);
            
            if (!string.IsNullOrEmpty(operatorName)) {
                string key = "db_" + operatorName;
                string dbName = settings[key];
                databaseSetting.operatorWiseDatabaseNames.Add(key, dbName);
            }
            SchedulerSetting generaterateSchedulerConfig = new SchedulerSetting(
                schedulerType: "quartz",
                databaseSetting: databaseSetting);
            return generaterateSchedulerConfig;
        }
    }
}
