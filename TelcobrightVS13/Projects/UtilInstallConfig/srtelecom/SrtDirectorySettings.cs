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
    public partial class SrtConfigGenerator //quartz config part
    {
        public static Dictionary<string, string> SrtConfigHelperMap = new Dictionary<string, string>()
        {
            { "vaultName","vault"},
            //{}
        };
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
            FileLocation vaultPrimary = new FileLocation()
            {
                Name = "vault",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/cdr",
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
                StartingPath = "Resources/cdr",
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
                StartingPath = "Resources/cdr",
                User = "ftpuser",
                Pass = "Takay1takaane",
                Skip = true
            };
            //VAULT PART
            List<FileLocation> ftpLocations = new List<FileLocation>();
            ftpLocations.Add(appServerFtp1);
            ftpLocations.Add(appServerFtp2);
            Vault vault = new Vault("vault", tbc, ftpLocations);
            vault.LocalLocation = new SyncLocation(vault.Name) { FileLocation = vaultPrimary };//don't pass this to constructor and set there, causes problem in json serialize
            tbc.DirectorySettings.Vaults.Add(vault);
            FileLocation huawei = new FileLocation()
            {
                Name = "huawei",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                StartingPath = "/",
                //StartingPath = "/home/zxss10_bsvr/data/bfile/bill/zsmart_media_bak",
                Sftphostkey = string.Empty,
                //Sftphostkey = "ssh-rsa 2048 44:56:0b:fa:3a:79:c2:ee:1c:95:d9:05:b5:9b:56:4a",
                ServerIp = "10.0.30.50",
                User = "ftpuser",
                Pass = "ftpuser",
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
                Name = "cas",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/",
                ServerIp = "192.168.100.161", //server = "172.16.16.242",
                User = "adminsrt",
                Pass = "srticx725",
                IgnoreZeroLenghFile = 1
            };

            //add locations to directory settings
            tbc.DirectorySettings.FileLocations.Add(vault.Name, vaultPrimary);
            tbc.DirectorySettings.FileLocations.Add(appServerFtp1.Name, appServerFtp1);
            tbc.DirectorySettings.FileLocations.Add(appServerFtp2.Name, appServerFtp2);
            tbc.DirectorySettings.FileLocations.Add(huawei.Name, huawei);
            tbc.DirectorySettings.FileLocations.Add(fileArchive1.Name, fileArchive1);
            tbc.DirectorySettings.FileLocations.Add(fileArchiveCAS.Name, fileArchiveCAS);

            SyncPair Huawei_Vault = new SyncPair("Huawei:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation("Huawei")
                {
                    FileLocation = huawei,
                    DescendingFileListByFileName = this.Tbc.CdrSetting.DescendingOrderWhileListingFiles
                },
                DstSyncLocation = new SyncLocation("Vault_Huawei")
                {
                    FileLocation = vaultPrimary
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "downloaded",
                    MoveFilesToSecondaryAfterCopy = false,
                    Recursive=true,
                    ExpFileNameFilter = new SpringExpression(@"Name.StartsWith('SRT')
                                                                and
                                                                (Name.EndsWith('.dat'))
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
            SyncPair vaultCAS = new SyncPair("Vault:CAS")
            {
                SkipCopyingToDestination = true,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation("Vault")
                {
                    FileLocation = vaultPrimary
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
            directorySetting.SyncPairs.Add(Huawei_Vault.Name, Huawei_Vault);
            //directorySetting.SyncPairs.Add(vaultS3FileArchive1.Name, vaultS3FileArchive1);
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
                //vaultS3FileArchive1.Name,
                //vaultCAS.Name
            };
        }
    }
}
