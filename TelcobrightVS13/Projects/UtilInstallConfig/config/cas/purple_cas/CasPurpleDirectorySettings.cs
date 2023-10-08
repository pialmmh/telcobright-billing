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
    public sealed partial class CasPurpleAbstractConfigGenerator//quartz config part
    {
        public static Dictionary<string, string> SrtConfigHelperMap = new Dictionary<string, string>()
        {
            { "vaultName","vault"},
            //{}
        };
        public void PrepareDirectorySettings(TelcobrightConfig tbc)
        {
            DirectorySettings directorySetting = new DirectorySettings("c:/telcobright", @"cas");
            tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaultHuwaei = new FileLocation()
            {
                Name = "vault.Huwaei",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "H:/telcobright/vault/resources/cdr/purple/tdm",
                User = "",
                Pass = "",
            };

            FileLocation vaultCataleya = new FileLocation()
            {
                Name = "vault.Cataleya",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "h:/telcobright/Vault/Resources/cdr/purple/ip",
                User = "",
                Pass = "",
            };

            //add sync pairs to directory config
            //directorySetting.SyncPairs.Add(huawei_Vault.Name, huawei_Vault);
            //directorySetting.SyncPairs.Add(vaultS3FileArchive1.Name, vaultS3FileArchive1);
            //directorySetting.SyncPairs.Add(vaultCAS.Name, vaultCAS);
            this.Tbc.DirectorySettings.FileLocations.Add(vaultHuwaei.Name, vaultHuwaei);
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
