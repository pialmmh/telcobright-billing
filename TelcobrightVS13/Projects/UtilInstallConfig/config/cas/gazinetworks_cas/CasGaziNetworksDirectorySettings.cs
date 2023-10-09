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

    public sealed partial class CasGaziNetworksAbstractConfigGenerator //quartz config part
    {
        public void PrepareDirectorySettings(TelcobrightConfig tbc)
        {
            DirectorySettings directorySetting = new DirectorySettings("d:/telcobright", @"cas");
            tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value

            FileLocation vaultGazi = new FileLocation()
            {
                Name = "Vault.Genband",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "D:/telcobright/vault/resources/cdr/gazinetworks/tdm",
                User = "",
                Pass = "",
            };

            FileLocation vaultKhlBogra = new FileLocation()
            {
                Name = "Vault.TelcobridgeBogra",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "D:/telcobright/vault/resources/cdr/gaziNetworks/tdmKhlBogra",
                User = "",
                Pass = "",
            };

           

            FileLocation vaultcataliya = new FileLocation()
            {
                Name = "Vault.cataleya",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "D:/telcobright/vault/resources/cdr/gazinetworks/ip",
                User = "",
                Pass = "",
            };

            this.Tbc.DirectorySettings.FileLocations.Add(vaultGazi.Name, vaultGazi);
            this.Tbc.DirectorySettings.FileLocations.Add(vaultKhlBogra.Name, vaultKhlBogra);
            this.Tbc.DirectorySettings.FileLocations.Add(vaultcataliya.Name, vaultcataliya);
           
           
           
        }
    }
}
