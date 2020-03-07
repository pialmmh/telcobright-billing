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
using TelcobrightMediation.Automation;
using TelcobrightMediation.Config;

namespace InstallConfig
{
    public partial class RgrnConfigGenerator //quartz config part
    {
        public void PrepareDirectorySetting(TelcobrightConfig tbc)
        {
            DirectorySettings directorySettings = new DirectorySettings("Directory Settings")
            {
                ApplicationRootDirectory = "c:/Telcobright"
            };
            
            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaultRgrn = new FileLocation()
            {
                Name = "Vault.Rgrn",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "c:/telcobright/Vault/Resources/CDR/Rgrn/Sansay",
                User = "",
                Pass = "",
            };
            FileLocation appServerFtp1 = new FileLocation()
            {
                Name = "AppServerFTP1",//
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                ServerIp = "10.21.21.12",
                StartingPath = "Resources/CDR/Sansay",
                User = "ftpuser",
                Pass = "Takay1takaane",
            };
            FileLocation appServerFtp2 = new FileLocation()
            {
                Name = "AppServerFTP2",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                ServerIp = "10.21.21.13",
                StartingPath = "Resources/CDR/Sansay",
                User = "ftpuser",
                Pass = "Takay1takaane",
                Skip = true
            };
            //VAULT PART
            List<FileLocation> ftpLocations = new List<FileLocation>();
            ftpLocations.Add(appServerFtp1);
            ftpLocations.Add(appServerFtp2);
            Vault Rgrnvault = new Vault("Vault.Rgrn",tbc , ftpLocations);
            Rgrnvault.LocalLocation = new SyncLocation(vaultRgrn.Name) { FileLocation = vaultRgrn };//don't pass this to constructor and set there, causes problem in json serialize
            directorySettings.Vaults.Add(Rgrnvault);
            FileLocation SansayVIP = new FileLocation()
            {
                Name = "SansayVIP",
                LocationType = "sftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = "/home/cdr/CDR",
                Sftphostkey = "",
                ServerIp = "10.21.21.6",
                User = "cdr",
                Pass = "cdrpass",
                IgnoreZeroLenghFile = 1
            };
            
            FileLocation fileArchive1Zip = new FileLocation()//raw cdr archive
            {
                Name = "FileArchive1Zip",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/RAW_DATA/IGW_CDR",
                ServerIp = "10.100.201.20", //server = "172.16.16.242",
                User = "cdr",
                Pass = "cdr13531",
                IgnoreZeroLenghFile = 1
            };
            
            //add locations to directory settings
            directorySettings.FileLocations.Add(vaultRgrn.Name, vaultRgrn);
            directorySettings.FileLocations.Add(appServerFtp1.Name, appServerFtp1);
            directorySettings.FileLocations.Add(appServerFtp2.Name, appServerFtp2);
            directorySettings.FileLocations.Add(SansayVIP.Name, SansayVIP);
            
            SyncPair RgrnVault = new SyncPair("SansayVip:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation("SansayVip")
                {
                    FileLocation = SansayVIP,
                    DescendingFileListByFileName = tbc.CdrSetting.DescendingOrderWhileListingFiles
                },
                DstSyncLocation = new SyncLocation("Vault_Rgrn")
                {
                    FileLocation = vaultRgrn
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "Downloaded",
                    ExpFileNameFilter = new SpringExpression(@"Name.EndsWith('.cdr')")
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",//make sure when copying to vault always .tmp ext used
                    Overwrite = true,
                    ExpDestFileName = new SpringExpression(@"Name.Insert(0,'')"),
                    CompressionType = CompressionType.None
                }
            };

            
            //add sync pairs to directory config
            directorySettings.SyncPairs.Add(RgrnVault.Name, RgrnVault);
            //directorySettings.SyncPairs.Add(vaultFileArchive1Zip.Name, vaultFileArchive1Zip);
            //load the syncpairs in dictioinary, first by source
            foreach (SyncPair sp in directorySettings.SyncPairs.Values)
            {
                if (directorySettings.SyncLocations.ContainsKey(sp.SrcSyncLocation.Name) == false)
                {
                    directorySettings.SyncLocations.Add(sp.SrcSyncLocation.Name, sp.SrcSyncLocation);
                }
            }
            foreach (SyncPair sp in directorySettings.SyncPairs.Values)
            {
                if (directorySettings.SyncLocations.ContainsKey(sp.DstSyncLocation.Name) == false)
                {
                    directorySettings.SyncLocations.Add(sp.DstSyncLocation.Name, sp.DstSyncLocation);
                }
            }
            tbc.DirectorySettings = directorySettings;
            //add archive locations to CdrSettings
            tbc.CdrSetting.BackupSyncPairNames = new List<string>
            {
                //vaultFileArchive1Zip.Name, vaultFileArchive1Zip.Name
            };
            tbc.CdrSetting.DisableCdrPostProcessingJobCreationForAutomation = true;
        }
    }
}
