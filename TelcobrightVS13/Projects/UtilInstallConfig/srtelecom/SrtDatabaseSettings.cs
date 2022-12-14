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
    public partial class SrtConfigGenerator //quartz config part
    {
        public DatabaseSetting OverrideDatabaseSettingsIfEnabled(TelcobrightConfig tbc)
        {
            DatabaseSetting dataBaseSetting = tbc.DatabaseSetting;
            dataBaseSetting.OverrideDatabaseSettingsFromAppConfig = true;
            dataBaseSetting.ServerName = "172.16.100.6";
            dataBaseSetting.DatabaseName = "srtelecom";
            dataBaseSetting.AdminPassword = "Takay1#$ane";
            dataBaseSetting.AdminUserName = "root";
            dataBaseSetting.OperatorShortNameAliasToOverride = "srtelecom";

            return dataBaseSetting;
        }
    }
}
