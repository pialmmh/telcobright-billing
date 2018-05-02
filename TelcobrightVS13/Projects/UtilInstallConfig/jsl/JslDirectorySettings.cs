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
    public partial class JslConfigGenerator //quartz config part
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
            FileLocation vaultJslZteDhk = new FileLocation()
            {
                Name = "Vault.JslZteDhk",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/CDR/JSL/JslZteDhk",
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
                StartingPath = "Resources/CDR/JSL/JslZteDhk",
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
                StartingPath = "Resources/CDR/JSL/JslZteDhk",
                User = "ftpuser",
                Pass = "Takay1takaane",
                Skip = true
            };
            //VAULT PART
            List<FileLocation> ftpLocations = new List<FileLocation>();
            ftpLocations.Add(appServerFtp1);
            ftpLocations.Add(appServerFtp2);
            Vault JslZteDhkvault = new Vault("Vault.JslZteDhk", tbc, ftpLocations);
            JslZteDhkvault.LocalLocation = new SyncLocation(vaultJslZteDhk.Name) { FileLocation = vaultJslZteDhk };//don't pass this to constructor and set there, causes problem in json serialize
            tbc.DirectorySettings.Vaults.Add(JslZteDhkvault);
            FileLocation JslZteDhk = new FileLocation()
            {
                Name = "JslZteDhk",
                LocationType = "ftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = "/home/zxss10_bsvr/data/bfile/bill/zsmart_media_bak",
                Sftphostkey = "",
                ServerIp = "10.133.34.12",
                User = "icxbill",
                Pass = "icx123",
                ExcludeBefore = new DateTime(2015, 6, 26, 0, 0, 0),
                IgnoreZeroLenghFile = 1
            };

            //FileLocation S3_2 = new FileLocation()
            //{
            //    Name = "S3_2",
            //    locationType = "ftp",
            //    OSType = "windows",
            //    pathSeparator = "/",
            //    startingPath = "/oldcdr",
            //    sftphostkey = "ssh-rsa 1024 0d:fd:ac:4a:67:05:e9:76:ef:a0:d2:c0:f9:1a:55:c1",
            //    server = "172.16.16.242",
            //    user = "ftpuser",
            //    pass = "Takay1takaane",
            //    ExcludeBefore = new DateTime(2015, 6, 26, 0, 0, 0),
            //    IgnoreZeroLenghFile = 1
            //};

            FileLocation fileArchive1 = new FileLocation()//raw cdr archive
            {
                Name = "FileArchive1",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/archive",
                ServerIp = "10.100.201.20",
                User = "cdr",
                Pass = "cdr13531",
                IgnoreZeroLenghFile = 1
            };
            FileLocation fileArchiveCAS = new FileLocation()//raw cdr archive
            {
                Name = "FileArchiveCAS",
                LocationType = "ftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = @"/",
                ServerIp = "192.168.100.83", //server = "172.16.16.242",
                User = "adminjibon",
                Pass = "jibondhara35#",
                IgnoreZeroLenghFile = 1,
            };
            //add locations to directory settings
            tbc.DirectorySettings.FileLocations.Add(vaultJslZteDhk.Name, vaultJslZteDhk);
            tbc.DirectorySettings.FileLocations.Add(appServerFtp1.Name, appServerFtp1);
            tbc.DirectorySettings.FileLocations.Add(appServerFtp2.Name, appServerFtp2);
            tbc.DirectorySettings.FileLocations.Add(JslZteDhk.Name, JslZteDhk);
            tbc.DirectorySettings.FileLocations.Add(fileArchive1.Name, fileArchive1);
            tbc.DirectorySettings.FileLocations.Add(fileArchiveCAS.Name, fileArchiveCAS);


            //sync pair platinum:Vault
            SyncPair JslZteDhkVault = new SyncPair("JslZteDhk:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation("JslZteDhk")
                {
                    FileLocation = JslZteDhk,
                    DescendingFileListByFileName = this.Tbc.CdrSetting.DescendingOrderWhileListingFiles
                },
                DstSyncLocation = new SyncLocation("Vault_JslZteDhk")
                {
                    FileLocation = vaultJslZteDhk
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "Downloaded",
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
                SrcSyncLocation = new SyncLocation("Vault_JslZteDhk")
                {
                    FileLocation = vaultJslZteDhk
                },
                DstSyncLocation = new SyncLocation("FileArchive1")
                {
                    FileLocation = fileArchive1
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
                        new SpringExpression(@"Name.Substring(16,8)"), //"S3_2_" is appended at vault
                        "yyyyMMdd",
                        true
                    )
                }
            };
            //sync pair vault:CAS
            SyncPair vaultS3CAS = new SyncPair("Vault:CAS")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation("Vault_JslZteDhk")
                {
                    FileLocation = vaultJslZteDhk
                },
                DstSyncLocation = new SyncLocation("CAS")
                {
                    FileLocation = fileArchiveCAS
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
            directorySetting.SyncPairs.Add(JslZteDhkVault.Name, JslZteDhkVault);
            directorySetting.SyncPairs.Add(vaultS3FileArchive1.Name, vaultS3FileArchive1);
            directorySetting.SyncPairs.Add(vaultS3CAS.Name, vaultS3CAS);
            //load the syncpairs in dictioinary, first by source
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
            this.Tbc.CdrSetting.BackupSyncPairNames = new List<string>
            {
                vaultS3FileArchive1.Name, vaultS3CAS.Name
            };
        }
    }
}
