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
    public sealed partial class CasCrossWorldAbstractConfigGenerator//quartz config part
    {
        public static Dictionary<string, string> SrtConfigHelperMap = new Dictionary<string, string>()
        {
            { "vaultName","vault"},
            //{}
        };
        public void PrepareDirectorySettings(TelcobrightConfig tbc)
        {
            DirectorySettings directorySetting = new DirectorySettings("h:/telcobright", @"cas");
            tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaultCataleya = new FileLocation()
            {
                Name = "vault.Cataleya",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "f:/telcobright/vault/resources/cdr/crossworld/ip",
                User = "",
                Pass = "",
            };
            FileLocation vaultGenband = new FileLocation()
            {
                Name = "vault.Genband",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "f:/telcobright/vault/resources/cdr/crossworld/tdm",
                User = "",
                Pass = "",
            };
            FileLocation vaultTelcobridgeCTG = new FileLocation()
            {
                Name = "vault",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "f:/telcobright/vault/resources/cdr/crossworld/tdm1/CTG",
                User = "",
                Pass = "",
            };


            FileLocation vaultTelcobridgeKHL = new FileLocation()
            {
                Name = "vault",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "f:/telcobright/vault/resources/cdr/crossworld/tdm1/KHL",
                User = "",
                Pass = "",
            };


            
            this.Tbc.DirectorySettings.FileLocations.Add(vaultCataleya.Name,vaultCataleya);
            this.Tbc.DirectorySettings.FileLocations.Add(vaultGenband.Name, vaultGenband);
          
            
        }
    }
}
