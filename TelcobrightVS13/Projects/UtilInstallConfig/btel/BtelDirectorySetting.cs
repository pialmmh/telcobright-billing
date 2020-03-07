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
    public partial class BtelConfigGenerator //quartz config part
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
            FileLocation vaultBtelZteDhk = new FileLocation()
            {
                Name = "Vault.BtelZteDhk",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/CDR/btel/BtelZteDhk",
                User = "",
                Pass = "",
            };
            FileLocation appServerFtp1 = new FileLocation()
            {
                Name = "AppServerFTP1",//
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                ServerIp = "10.0.0.10",
                StartingPath = "Resources/CDR/btel/BtelZteDhk",
                User = "ftpuser",
                Pass = "Takay1takaane",
            };
            FileLocation appServerFtp2 = new FileLocation()
            {
                Name = "AppServerFTP2",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                ServerIp = "10.0.0.12",
                StartingPath = "Resources/CDR/btel/BtelZteDhk",
                User = "ftpuser",
                Pass = "Takay1takaane",
                Skip = true
            };
            //VAULT PART
            List<FileLocation> ftpLocations = new List<FileLocation>();
            ftpLocations.Add(appServerFtp1);
            ftpLocations.Add(appServerFtp2);
            Vault BtelZteDhkvault = new Vault("Vault.BtelZteDhk",tbc , ftpLocations);
            BtelZteDhkvault.LocalLocation = new SyncLocation(vaultBtelZteDhk.Name) { FileLocation = vaultBtelZteDhk };//don't pass this to constructor and set there, causes problem in json serialize
            directorySettings.Vaults.Add(BtelZteDhkvault);
            FileLocation BtelZteDhk = new FileLocation()
            {
                Name = "BtelZteDhk",
                LocationType = "sftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = "/home/zxss10_bsvr/data/bfile/bill",
                //StartingPath = "/home/zxss10_bsvr/data/bfile/bill/zsmart_media_bak",
                Sftphostkey = "",
                ServerIp = "10.33.34.12",
                User = "igwbill",
                Pass = "igw123",
                ExcludeBefore = new DateTime(2015, 6, 26, 0, 0, 0),
                IgnoreZeroLenghFile = 1
            };
            FileLocation fileArchive1Zip = new FileLocation()//raw cdr archive
            {
                Name = "FileArchive1Zip",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/IGW_CDR_BK",
                ServerIp = "10.100.201.13", //server = "172.16.16.242",
                User = "iofcdr",
                Pass = "blt#.45",
                IgnoreZeroLenghFile = 1
            };
            FileLocation fileArchiveIof = new FileLocation()//raw cdr archive
            {
                Name = "FileArchiveIof",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/IGW_TO_IOF2",
                ServerIp = "10.100.201.13", //server = "172.16.16.242",
                User = "iofcdr",
                Pass = "blt#.45",
                IgnoreZeroLenghFile = 1
            };
            //add locations to directory settings
            directorySettings.FileLocations.Add(vaultBtelZteDhk.Name, vaultBtelZteDhk);
            directorySettings.FileLocations.Add(appServerFtp1.Name, appServerFtp1);
            directorySettings.FileLocations.Add(appServerFtp2.Name, appServerFtp2);
            directorySettings.FileLocations.Add(BtelZteDhk.Name, BtelZteDhk);
            
            directorySettings.FileLocations.Add(fileArchive1Zip.Name, fileArchive1Zip);
            directorySettings.FileLocations.Add(fileArchiveIof.Name, fileArchiveIof);


            SyncPair BtelZteDhkVault = new SyncPair("BtelZteDhk:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation("BtelZteDhk")
                {
                    FileLocation = BtelZteDhk,
                    DescendingFileListByFileName = tbc.CdrSetting.DescendingOrderWhileListingFiles
                },
                DstSyncLocation = new SyncLocation("Vault_BtelZteDhk")
                {
                    FileLocation = vaultBtelZteDhk
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "Downloaded",
                    ExpFileNameFilter = new SpringExpression(@"Name.StartsWith('IGW')
                                                                and
                                                                (Name.EndsWith('.DAT'))
                                                                and Length>0")
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",//make sure when copying to vault always .tmp ext used
                    Overwrite = true,
                    ExpDestFileName = new SpringExpression(@"Name.Insert(0,'')"),
                    CompressionType = CompressionType.None
                }
            };


            //sync pair Vault_S3:FileArchive1
            SyncPair vaultFileArchive1Zip = new SyncPair("Vault:FileArchive1")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation("Vault_BtelZteDhk")
                {
                    FileLocation = vaultBtelZteDhk
                },
                DstSyncLocation = new SyncLocation("FileArchive1Zip")
                {
                    FileLocation = fileArchive1Zip
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "Downloaded",
                    ExpFileNameFilter = null,
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",
                    Overwrite = true,
                    CompressionType = CompressionType.None,
                }
            };
            
            SyncPair vaultIof = new SyncPair("Vault:IOF")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation("Vault_BtelZteDhk")
                {
                    FileLocation = vaultBtelZteDhk
                },
                DstSyncLocation = new SyncLocation("IOF")
                {
                    FileLocation = fileArchiveIof
                },
                SrcSettings = new SyncSettingsSource()
                {
                    ExpFileNameFilter = null,//source filter not required, job created after newcdr for this pair
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",
                    Overwrite = true,
                    ExpDestFileName = new SpringExpression(""),
                    CompressionType = CompressionType.None,
                    SubDirRule = null
                }
            };

            //add sync pairs to directory config
            directorySettings.SyncPairs.Add(BtelZteDhkVault.Name, BtelZteDhkVault);
            directorySettings.SyncPairs.Add(vaultFileArchive1Zip.Name, vaultFileArchive1Zip);
            directorySettings.SyncPairs.Add(vaultIof.Name, vaultIof);
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
                vaultFileArchive1Zip.Name,
                vaultIof.Name,
            };
            
        }
    }
}
