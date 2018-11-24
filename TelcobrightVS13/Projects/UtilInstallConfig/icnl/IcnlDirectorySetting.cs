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
    public partial class IcnlConfigGenerator //quartz config part
    {
        public void PrepareDirectorySetting(TelcobrightConfig tbc)
        {
            DirectorySettings directorySettings = new DirectorySettings("Directory Settings")
            {
                ApplicationRootDirectory = "C:/Telcobright"
            };
            
            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaultIcnl = new FileLocation()
            {
                Name = "Vault.Icnl",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/CDR/Icnl/dialogic",
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
                StartingPath = "Resources/CDR/btel/Icnl",
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
                StartingPath = "Resources/CDR/btel/Icnl",
                User = "ftpuser",
                Pass = "Takay1takaane",
                Skip = true
            };
            //VAULT PART
            List<FileLocation> ftpLocations = new List<FileLocation>();
            ftpLocations.Add(appServerFtp1);
            ftpLocations.Add(appServerFtp2);
            Vault Icnlvault = new Vault("Vault.Icnl",tbc , ftpLocations);
            Icnlvault.LocalLocation = new SyncLocation(vaultIcnl.Name) { FileLocation = vaultIcnl };//don't pass this to constructor and set there, causes problem in json serialize
            directorySettings.Vaults.Add(Icnlvault);
            FileLocation Icnl = new FileLocation()
            {
                Name = "Icnl",
                LocationType = "sftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = "/home/zxss10_bsvr/data/bfile/bill/zsmart_media_bak",
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
                StartingPath = @"/RAW_DATA/IGW_CDR",
                ServerIp = "10.100.201.20", //server = "172.16.16.242",
                User = "cdr",
                Pass = "cdr13531",
                IgnoreZeroLenghFile = 1
            };
            FileLocation fileArchive2 = new FileLocation()//raw cdr archive
            {
                Name = "FileArchive2",
                LocationType = "ftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = @"/medi/data/igw_src/igw_src_bk",
                ServerIp = "10.100.201.10", //server = "172.16.16.242",
                User = "medi",
                Pass = "medi",
                IgnoreZeroLenghFile = 1,
            };
            FileLocation fileArchiveIof = new FileLocation()//raw cdr archive
            {
                Name = "FileArchiveIof",
                LocationType = "ftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = @"/medi/data/igw_src/igw_src_bk",
                ServerIp = "10.100.201.13",
                User = "iofcdr",
                Pass = "blt#.45",
                IgnoreZeroLenghFile = 1,
            };
            //add locations to directory settings
            directorySettings.FileLocations.Add(vaultIcnl.Name, vaultIcnl);
            directorySettings.FileLocations.Add(appServerFtp1.Name, appServerFtp1);
            directorySettings.FileLocations.Add(appServerFtp2.Name, appServerFtp2);
            directorySettings.FileLocations.Add(Icnl.Name, Icnl);
            
            directorySettings.FileLocations.Add(fileArchive1Zip.Name, fileArchive1Zip);
            directorySettings.FileLocations.Add(fileArchive2.Name, fileArchive2);
            directorySettings.FileLocations.Add(fileArchiveIof.Name, fileArchiveIof);


            //sync pair platinum:Vault
            SyncPair IcnlVault = new SyncPair("Icnl:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation("Icnl")
                {
                    FileLocation = Icnl,
                    DescendingFileListByFileName = tbc.CdrSetting.DescendingOrderWhileListingFiles
                },
                DstSyncLocation = new SyncLocation("Vault_Icnl")
                {
                    FileLocation = vaultIcnl
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "Downloaded",
                    ExpFileNameFilter = new SpringExpression(@"Name.StartsWith('icdr')")
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
            SyncPair vaultFileArchive1Zip = new SyncPair("Vault:FileArchive1Zip")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation("Vault_Icnl")
                {
                    FileLocation = vaultIcnl
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
                    CompressionType = CompressionType.Sevenzip,
                    SubDirRule = new SyncSettingsDstSubDirectoryRule
                    (
                        DateWiseSubDirCreationType.ByFileName,
                        new SpringExpression(@"Name.Substring(3,8)"), //"S3_2_" is appended at vault
                        "yyyyMMdd", true
                    )
                }
            };
            SyncPair vaultFileArchive2 = new SyncPair("Vault:FileArchive2")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation("Vault_Icnl")
                {
                    FileLocation = vaultIcnl
                },
                DstSyncLocation = new SyncLocation("FileArchive2")
                {
                    FileLocation = fileArchive2
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
                    ExpDestFileName = new SpringExpression(""),
                    CompressionType = CompressionType.None,
                    SubDirRule = null
                }
            };
            SyncPair vaultIof = new SyncPair("Vault:IOF")
            {
                SkipCopyingToDestination = true,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation("Vault_Icnl")
                {
                    FileLocation = vaultIcnl
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
            directorySettings.SyncPairs.Add(IcnlVault.Name, IcnlVault);
            directorySettings.SyncPairs.Add(vaultFileArchive1Zip.Name, vaultFileArchive1Zip);
            directorySettings.SyncPairs.Add(vaultFileArchive2.Name, vaultFileArchive2);
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
                vaultFileArchive1Zip.Name, vaultIof.Name
            };
            tbc.CdrSetting.DisableCdrPostProcessingJobCreationForAutomation = true;
        }
    }
}
