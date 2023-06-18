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
    public partial class JslAbstractConfigConfigGenerator //quartz config part
    {
        public void PrepareDirectorySettings(TelcobrightConfig tbc)
        {
            DirectorySettings directorySetting = new DirectorySettings("Directory Settings")
            {
                ApplicationRootDirectory = "c:/Telcobright"
            };
            tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaultJslhuaweiDhk = new FileLocation()
            {
                Name = "Vault.JslhuaweiDhk",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/CDR/JSL/JslhuaweiDhk",
                User = "",
                Pass = "",
            };
            FileLocation appServerFtp1 = new FileLocation()
            {
                Name = "AppServerFTP1",//
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                ServerIp = "192.168.2.216",
                StartingPath = "Resources/CDR/JSL/JslhuaweiDhk",
                User = "ftpuser",
                Pass = "Takay1takaane",
            };
            FileLocation appServerFtp2 = new FileLocation()
            {
                Name = "AppServerFTP2",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                ServerIp = "192.168.2.224",
                StartingPath = "Resources/CDR/JSL/JslhuaweiDhk",
                User = "ftpuser",
                Pass = "Takay1takaane",
                Skip = true
            };
            //VAULT PART
            Vault JslhuaweiDhkvault = new Vault("Vault.JslhuaweiDhk", tbc);
            JslhuaweiDhkvault.LocalLocation = new SyncLocation(vaultJslhuaweiDhk.Name) { FileLocation = vaultJslhuaweiDhk };//don't pass this to constructor and set there, causes problem in json serialize
            tbc.DirectorySettings.Vaults.Add(JslhuaweiDhkvault);
            FileLocation JslhuaweiDhk = new FileLocation()
            {
                Name = "JslhuaweiDhk",
                LocationType = "sftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = "/home/zxss10_bsvr/data/bfile/bill",
                //StartingPath = "/home/zxss10_bsvr/data/bfile/bill/zsmart_media_bak",
                Sftphostkey = string.Empty,
                //Sftphostkey = "ssh-rsa 2048 44:56:0b:fa:3a:79:c2:ee:1c:95:d9:05:b5:9b:56:4a",
                ServerIp = "10.133.34.12",
                User = "icxbill",
                Pass = "icx123",
                //ServerIp = "192.168.0.105",
                //User = "ftpuser",
                //Pass = "Takay1takaane",
                ExcludeBefore = new DateTime(2015, 6, 26, 0, 0, 0),
                IgnoreZeroLenghFile = 1
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

            FileLocation fileArchiveCAS = new FileLocation()//raw cdr archive
            {
                Name = "fileArchiveCAS",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/ICX_CDR_TO_BTRC_CAS",
                ServerIp = "10.100.201.13", //server = "172.16.16.242",
                User = "iofcdr",
                Pass = "blt#.45",
                IgnoreZeroLenghFile = 1
            };

            //add locations to directory settings
            tbc.DirectorySettings.FileLocations.Add(vaultJslhuaweiDhk.Name, vaultJslhuaweiDhk);
            tbc.DirectorySettings.FileLocations.Add(appServerFtp1.Name, appServerFtp1);
            tbc.DirectorySettings.FileLocations.Add(appServerFtp2.Name, appServerFtp2);
            tbc.DirectorySettings.FileLocations.Add(JslhuaweiDhk.Name, JslhuaweiDhk);
            tbc.DirectorySettings.FileLocations.Add(fileArchive1.Name, fileArchive1);
            tbc.DirectorySettings.FileLocations.Add(fileArchiveCAS.Name, fileArchiveCAS);

            SyncPair jslhuaweiDhkVault = new SyncPair("JslhuaweiDhk:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation("JslhuaweiDhk")
                {
                    FileLocation = JslhuaweiDhk,
                    DescendingFileListByFileName = this.Tbc.CdrSetting.DescendingOrderWhileListingFiles
                },
                DstSyncLocation = new SyncLocation("Vault_JslhuaweiDhk")
                {
                    FileLocation = vaultJslhuaweiDhk
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "downloaded",
                    MoveFilesToSecondaryAfterCopy = true,
                    ExpFileNameFilter = new SpringExpression(@"Name.StartsWith('ICX')
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
            SyncPair vaultS3FileArchive1 = new SyncPair("Vault:FileArchive1")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation("Vault_JslhuaweiDhk")
                {
                    FileLocation = vaultJslhuaweiDhk
                },
                DstSyncLocation = new SyncLocation("FileArchive1")
                {
                    FileLocation = fileArchive1
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "downloaded",
                    ExpFileNameFilter = null,
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",
                    Overwrite = true,
                    CompressionType = CompressionType.None,
                }
            };

            //sync pair Vault_S3:FileArchive1
            SyncPair vaultCAS = new SyncPair("Vault:CAS")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation("Vault_JslhuaweiDhk")
                {
                    FileLocation = vaultJslhuaweiDhk
                },
                DstSyncLocation = new SyncLocation("fileArchiveCAS")
                {
                    FileLocation = fileArchiveCAS
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "downloaded",
                    ExpFileNameFilter = null,
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",
                    Overwrite = true,
                    CompressionType = CompressionType.None,
                }
            };

            //add sync pairs to directory config
            directorySetting.SyncPairs.Add(jslhuaweiDhkVault.Name, jslhuaweiDhkVault);
            directorySetting.SyncPairs.Add(vaultS3FileArchive1.Name, vaultS3FileArchive1);
            directorySetting.SyncPairs.Add(vaultCAS.Name, vaultCAS);

            //load the syncpairs in dictionary, first by source
            foreach (SyncPair sp in directorySetting.SyncPairs.Values)
            {
                if (directorySetting.SyncLocations.ContainsKey(sp.SrcSyncLocation.Name) == false)
                {
                    directorySetting.SyncLocations.Add(sp.SrcSyncLocation.Name, sp.SrcSyncLocation);
                }
            }
            foreach (SyncPair sp in directorySetting.SyncPairs.Values)
            {
                if (directorySetting.SyncLocations.ContainsKey(sp.DstSyncLocation.Name) == false)
                {
                    directorySetting.SyncLocations.Add(sp.DstSyncLocation.Name, sp.DstSyncLocation);
                }
            }
            //add archive locations to CdrSettings
            this.Tbc.CdrSetting.BackupSyncPairNames = new List<string>()
            {
                vaultS3FileArchive1.Name,
                vaultCAS.Name
            };
        }
    }
}
