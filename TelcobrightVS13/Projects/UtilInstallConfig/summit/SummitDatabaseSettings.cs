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
    public partial class SummitConfigGenerator //quartz config part
    {
        public DatabaseSetting OverrideDatabaseSettingsIfEnabled(TelcobrightConfig tbc)
        {
            DatabaseSetting dataBaseSetting = tbc.DatabaseSetting;
            dataBaseSetting.OverrideDatabaseSettingsFromAppConfig = true;
            dataBaseSetting.ServerName = "103.26.244.74";
            dataBaseSetting.DatabaseName = "summit";
            dataBaseSetting.AdminPassword = "Takay1#$ane";
            dataBaseSetting.AdminUserName = "root";
            dataBaseSetting.OperatorShortNameAliasToOverride = "summit";
            dataBaseSetting.DatabaseEngine = "innodb";
            dataBaseSetting.StorageEngineForPartitionedTables = "tokudb";
            dataBaseSetting.PartitionStartDate = new DateTime(2023, 1, 1);
            dataBaseSetting.PartitionLenInDays = 1;
            dataBaseSetting.ReadOnlyUserName = "dbreader";
            dataBaseSetting.ReadOnlyPassword = "Takay1takaane";
            return dataBaseSetting;
        }
    }
}
