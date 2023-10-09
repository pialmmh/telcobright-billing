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
                StartingPath = "H:/telcobright/vault/resources/cdr/ringtech/tdm",
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
                StartingPath = "H:/telcobright/vault/resources/cdr/ringtech/ip",
                User = "",
                Pass = "",
            };



            FileLocation fileArchive1 = new FileLocation()//raw cdr archive
            {
                Name = "FileArchive1Zip",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/ICX_CDR_BK",
                ServerIp = "10.100.201.13", //server = "172.16.16.242",
                User = "iofcdr",
                Pass = "blt#.45",
                IgnoreZeroLenghFile = 1
            };

            this.Tbc.DirectorySettings.FileLocations.Add(vaultPrimary.Name, vaultPrimary);
            this.Tbc.DirectorySettings.FileLocations.Add(vaultGNEW.Name, vaultGNEW);
 
        }
    }
}
