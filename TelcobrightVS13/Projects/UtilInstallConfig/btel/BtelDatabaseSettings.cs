using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using TelcobrightMediation;
using TelcobrightMediation.Scheduler.Quartz;
using System.ComponentModel.Composition;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;
using QuartzTelcobright;
using TelcobrightFileOperations;
using TelcobrightMediation.Automation;
using TelcobrightMediation.Config;


namespace InstallConfig
{
    public partial class BtelConfigGenerator //quartz config part
    {
        public DatabaseSetting GetDatabaseSettings()
        {
            return new DatabaseSetting()
            {
                ServerName = "10.0.0.9",
                DatabaseName = "btel",
                AdminPassword = "Takay1#$ane",
                AdminUserName = "root",
                DatabaseEngine = "innodb",
                StorageEngineForPartitionedTables = "tokudb",
                PartitionStartDate = new DateTime(2023, 1, 1),
                PartitionLenInDays = 1,
                ReadOnlyUserName = "dbreader",
                ReadOnlyPassword = "Takay1takaane"
            };
        }
    }
}