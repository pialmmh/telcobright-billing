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

    public sealed partial class CasMnHAbstractConfigGenerator //quartz config part
    {
        public void PrepareDirectorySettings(TelcobrightConfig tbc)
        {
            DirectorySettings directorySetting = new DirectorySettings("g:/telcobright", @"cas");
            tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaultHuwaei = new FileLocation()
            {
                Name = "Vault.Huwaei",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "e:/telcobright/vault/resources/cdr/mnh/tdm",
                User = "",
                Pass = "",
            };

            FileLocation vaultCataliya = new FileLocation()
            {
                Name = "Vault.cataleya",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "e:/telcobright/vault/resources/cdr/mnh/ip",
                User = "",
                Pass = "",
            };

            this.Tbc.DirectorySettings.FileLocations.Add(vaultHuwaei.Name, vaultHuwaei);
            this.Tbc.DirectorySettings.FileLocations.Add(vaultCataliya.Name, vaultCataliya);
           
        }
    }
}
