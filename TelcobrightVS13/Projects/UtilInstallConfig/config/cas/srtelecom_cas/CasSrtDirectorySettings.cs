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
    public sealed partial class CasSrtAbstractConfigGenerator //quartz config part
    {
        private FileLocation vaultPrimary;
        private FileLocation vaultCataleya;
        private SyncPair Huawei_Vault;
        private SyncPair vaultCAS;
        public static Dictionary<string, string> SrtConfigHelperMap = new Dictionary<string, string>()
        {
            { "vaultName","vault"},
            //{}
        };
        public void PrepareDirectorySettings(TelcobrightConfig tbc)
        {
            DirectorySettings directorySetting = new DirectorySettings("Directory Settings", @"cas");
            tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            this.vaultPrimary = new FileLocation()
            {
                Name = "vault",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "d:/telcobright/vault/resources/cdr/srTelecom/tdm",
                User = "",
                Pass = "",
            };
            this.vaultCataleya = new FileLocation()
            {
                Name = "vaultCataleya",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "d:/telcobright/vault/resources/cdr/srTelecom/ip/Aug-Oct",
                User = "",
                Pass = "",
            };

            
            //add locations to directory settings
            tbc.DirectorySettings.FileLocations.Add(vaultCataleya.Name, vaultCataleya);
            tbc.DirectorySettings.FileLocations.Add(vaultPrimary.Name,vaultPrimary);

            
        }
    }
}
