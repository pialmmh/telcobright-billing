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
                                               + Path.DirectorySeparatorChar + "LocalMachine.conf";


        public static SchedulerSetting GeneraterateSchedulerConfig()
        {
            DatabaseSetting databaseSetting = new DatabaseSetting();
            Dictionary<string, string> settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                File.ReadAllText(databaseConfigFileName));

            databaseSetting.ServerName = settings["ServerName"];
            databaseSetting.DatabaseName = settings["DatabaseName"];
            databaseSetting.AdminUserName = settings["AdminUserName"];
            databaseSetting.AdminPassword = settings["AdminPassword"];
            databaseSetting.ReadOnlyUserName = settings["ReadOnlyUserName"];
            databaseSetting.ReadOnlyPassword = settings["ReadOnlyPassword"];

            SchedulerSetting generaterateSchedulerConfig = new SchedulerSetting(
                schedulerType: "quartz",
                databaseSetting: databaseSetting);
            return generaterateSchedulerConfig;
        }
    }
}
