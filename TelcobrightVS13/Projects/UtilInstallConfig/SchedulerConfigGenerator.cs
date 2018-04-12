using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions.ConfigHelper;

namespace InstallConfig
{
    public static class SchedulerConfigGenerator
    {
        public static SchedulerSetting GeneraterateSchedulerConfig()
        {
            return new SchedulerSetting(
                "quartz",
                new DatabaseSetting()
                {
                    ServerName = "127.0.0.1",
                    DatabaseName = "scheduler",
                    AdminUserName = "root",
                    AdminPassword = "Takay1#$ane",
                    ReadOnlyUserName = "dbreader",
                    ReadOnlyPassword = "Takay1takaane"
                });
        }
    }
}
