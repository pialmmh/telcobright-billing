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
    public sealed partial class CasRingTechAbstractConfigGenerator//quartz config part
    {
        public static Dictionary<string, string> SrtConfigHelperMap = new Dictionary<string, string>()
        {
            { "vaultName","vault"},
            //{}
        };
        public void PrepareDirectorySettings(TelcobrightConfig tbc)
        {
            DirectorySettings directorySetting = new DirectorySettings("H:/telcobright", @"cas");
            tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaultPrimary = new FileLocation()
            {
                Name = "vaultZte",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "D:/telcobright/vault/resources/cdr/ringTech/tdm",
                User = "",
                Pass = "",
            };

            FileLocation vaultGNEW = new FileLocation()
            {
                Name = "vault.GNEW",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "D:/telcobright/vault/resources/cdr/ringTech/ip",
                User = "",
                Pass = "",
            };
            this.Tbc.DirectorySettings.FileLocations.Add(vaultPrimary.Name, vaultPrimary);
            this.Tbc.DirectorySettings.FileLocations.Add(vaultGNEW.Name, vaultGNEW);
 
        }
    }
}
