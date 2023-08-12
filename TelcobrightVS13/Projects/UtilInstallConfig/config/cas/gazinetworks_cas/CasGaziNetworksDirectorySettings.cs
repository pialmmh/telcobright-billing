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

    public partial class CasGaziNetworksAbstractConfigGenerator //quartz config part
    {
        public void PrepareDirectorySettings(TelcobrightConfig tbc)
        {
            DirectorySettings directorySetting = new DirectorySettings("d:/telcobright", @"cas");
            tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaultGenbandDhk = new FileLocation()
            {
                Name = "Vault.GenbandDhk",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "D:/telcobright/vault/resources/cdr/gazinetworks/tdm",
                User = "",
                Pass = "",
            };

            FileLocation vaultTelcobridgeDhk = new FileLocation()
            {
                Name = "Vault.TelcobridgeDhk",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "D:/telcobright/vault/resources/cdr/gazinetworks/tdm_2",
                User = "",
                Pass = "",
            };

            FileLocation vaultcataliyaDhk = new FileLocation()
            {
                Name = "Vault.cataleyaDhk",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "D:/telcobright/vault/resources/cdr/gazinetworks/ip",
                User = "",
                Pass = "",
            };

            this.Tbc.DirectorySettings.FileLocations.Add(vaultGenbandDhk.Name, vaultGenbandDhk);
            this.Tbc.DirectorySettings.FileLocations.Add(vaultTelcobridgeDhk.Name, vaultTelcobridgeDhk);
            this.Tbc.DirectorySettings.FileLocations.Add(vaultcataliyaDhk.Name, vaultcataliyaDhk);
           
            //add archive locations to CdrSettings
            this.Tbc.CdrSetting.BackupSyncPairNames = new List<string>()
            {
                //vaultS3FileArchive1.Name,
                //vaultCAS.Name
            };
           
        }
    }
}
