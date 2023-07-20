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
    public partial class CasJslAbstractConfigGenerator //quartz config part
    {
        public void PrepareDirectorySettings(TelcobrightConfig tbc)
        {
            DirectorySettings directorySetting = new DirectorySettings("c:/telcobright", @"cas\casDbF");
            tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaultJslZteDhk = new FileLocation()
            {
                Name = "Vault.JslZteDhk",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "F:/telcobright/vault/resources/cdr/jibonDhara/tdm",
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
                StartingPath = "F:/telcobright/vault/resources/cdr/jibonDhara/ip",
                User = "",
                Pass = "",
            };

            this.Tbc.DirectorySettings.FileLocations.Add(vaultJslZteDhk.Name, vaultJslZteDhk);
            this.Tbc.DirectorySettings.FileLocations.Add(vaultJslcataliyaDhk.Name, vaultJslcataliyaDhk);

            FileLocation appServerFtp1 = new FileLocation()
            {
                Name = "AppServerFTP1",//
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                ServerIp = "192.168.2.216",
                StartingPath = "C:/telcobright/Vault/Resources/CDR/JSL/JslhuaweiDhk",
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

            SyncPair jslhuaweiDhkVault = new SyncPair("JslhuaweiDhk:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = new FileLocation()
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
                    },
            DescendingFileListByFileName = this.Tbc.CdrSetting.DescendingOrderWhileListingFiles
                },
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultJslZteDhk
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
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultJslZteDhk
                },
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = new FileLocation()//raw cdr archive
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
                    }
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
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultJslZteDhk
                },
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = new FileLocation()//raw cdr archive
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
                    }
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

            //add archive locations to CdrSettings
            this.Tbc.CdrSetting.BackupSyncPairNames = new List<string>()
            {
                vaultS3FileArchive1.Name,
                vaultCAS.Name
            };
            directorySetting.FileLocations = directorySetting.SyncPairs.Values.SelectMany(sp =>
                new List<FileLocation>
                {
                    //sp.SrcSyncLocation.FileLocation,
                    //sp.DstSyncLocation.FileLocation
                }).ToDictionary(floc => floc.Name);
        }
    }
}
