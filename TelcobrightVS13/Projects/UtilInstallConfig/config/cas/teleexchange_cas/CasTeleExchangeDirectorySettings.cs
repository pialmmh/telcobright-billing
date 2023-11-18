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

    public sealed partial class CasTeleExchangeAbstractConfigGenerator //quartz config part
    {
        public void PrepareDirectorySettings(TelcobrightConfig tbc)
        {
            DirectorySettings directorySetting = new DirectorySettings("I:/telcobright", @"cas");
            tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaulHuawei = new FileLocation()
            {
                Name = "Vault.Huawei",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "f:/telcobright/vault/resources/cdr/teleExchange/tdm",
                User = "",Pass = "",
            };
            FileLocation vaultTelcobridge = new FileLocation()
            {
                Name = "Vault.Telcobridge",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "i:/telcobright/vault/resources/cdr/teleExchange/tdm1",
                User = "",
                Pass = "",
            };

            FileLocation vaultCataleya= new FileLocation()
            {
                Name = "Vault.Cataleya",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "f:/telcobright/vault/resources/cdr/teleExchange/ip",
                User = "",
                Pass = "",
            };


            this.Tbc.DirectorySettings.FileLocations.Add(vaulHuawei.Name, vaulHuawei);
            this.Tbc.DirectorySettings.FileLocations.Add(vaultTelcobridge.Name, vaultTelcobridge);
            this.Tbc.DirectorySettings.FileLocations.Add(vaultCataleya.Name, vaultCataleya);
           
            //add archive locations to CdrSettings
            this.Tbc.CdrSetting.BackupSyncPairNames = new List<string>()
            {
                //vaultS3FileArchive1.Name,
                //vaultCAS.Name
            };
           
        }
    }
}
