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
                                               + Path.DirectorySeparatorChar + "Database.conf";


        public static SchedulerSetting GeneraterateSchedulerConfig()
        {
            DatabaseSetting databaseSetting = JsonConvert.DeserializeObject<DatabaseSetting>(
                File.ReadAllText(databaseConfigFileName));

            SchedulerSetting generaterateSchedulerConfig = new SchedulerSetting(
                schedulerType: "quartz",
                databaseSetting: databaseSetting);
            return generaterateSchedulerConfig;
        }
    }
}
