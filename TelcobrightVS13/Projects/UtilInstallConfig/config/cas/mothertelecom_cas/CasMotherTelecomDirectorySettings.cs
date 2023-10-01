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
    public sealed partial class CasMotherTelecomAbstractConfigGenerator //quartz config part
    {
        private FileLocation vaultNokia;
        private FileLocation vaultDialogic;
        private SyncPair huawei_Vault;
        private SyncPair zteCAS;

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
            this.vaultNokia = new FileLocation()
            {
                Name = "vault.Nokia",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "e:/telcobright/vault/resources/cdr/mothertelecom/tdm",
                User = "",
                Pass = "",
            };

            this.vaultDialogic = new FileLocation()
            {
                Name = "vaultDialogic",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "e:/telcobright/vault/resources/cdr/mothertelecom/ip",
                User = "",
                Pass = "",
            };

          

            //add locations to directory settings
            tbc.DirectorySettings.FileLocations.Add(vaultNokia.Name, vaultNokia);
            tbc.DirectorySettings.FileLocations.Add(vaultDialogic.Name, vaultDialogic);
           
            
            
            //add archive locations to CdrSettings
            this.Tbc.CdrSetting.BackupSyncPairNames = new List<string>()
            {
               
            };
        }
    }
}
