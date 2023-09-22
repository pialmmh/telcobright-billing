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

    public sealed partial class CasParadiseAbstractConfigGenerator //quartz config part
    {
        public void PrepareDirectorySettings(TelcobrightConfig tbc)
        {
            DirectorySettings directorySetting = new DirectorySettings("g:/telcobright", @"cas");
            tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaultJslZteDhk = new FileLocation()
            {
                Name = "Vault.Huwaei",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "G:/telcobright/vault/resources/cdr/paradise/tdm",
                User = "",
                Pass = "",
            };

            FileLocation vaultJslcataliyaDhk = new FileLocation()
            {
                Name = "Vault.JslcataleyaDhk",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "G:/telcobright/vault/resources/cdr/paradise/ip",
                User = "",
                Pass = "",
            };

            tbc.DirectorySettings.FileLocations.Add(vaultJslZteDhk.Name, vaultJslZteDhk);
            tbc.DirectorySettings.FileLocations.Add(vaultJslcataliyaDhk.Name, vaultJslcataliyaDhk);
           
            //add archive locations to CdrSettings
            tbc.CdrSetting.BackupSyncPairNames = new List<string>()
            {
                //vaultS3FileArchive1.Name,
                //vaultCAS.Name
            };
           
        }
    }
}
