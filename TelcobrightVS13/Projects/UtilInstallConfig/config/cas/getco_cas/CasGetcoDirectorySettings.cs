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
using TelcobrightMediation.Config;

namespace InstallConfig
{

    public partial class CasGetcoAbstractConfigGenerator //quartz config part
    {
        public void PrepareDirectorySettings(TelcobrightConfig tbc)
        {
            DirectorySettings directorySetting = new DirectorySettings("F:/telcobright", @"cas");
            tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaultHuawei = new FileLocation()
            {
                Name = "Vault.Huawei",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "F:/telcobright/vault/resources/cdr/getco/tdm",
                User = "",
                Pass = "",
            };

            FileLocation vaultReve= new FileLocation()
            {
                Name = "Vault.REVE",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "F:/telcobright/vault/resources/cdr/getco/ip",
                User = "",
                Pass = "",
            };

            this.Tbc.DirectorySettings.FileLocations.Add(vaultHuawei.Name, vaultHuawei);
            this.Tbc.DirectorySettings.FileLocations.Add(vaultReve.Name, vaultReve);
           
            //add archive locations to CdrSettings
            this.Tbc.CdrSetting.BackupSyncPairNames = new List<string>()
            {
                //vaultS3FileArchive1.Name,
                //vaultCAS.Name
            };
           
        }
    }
}
