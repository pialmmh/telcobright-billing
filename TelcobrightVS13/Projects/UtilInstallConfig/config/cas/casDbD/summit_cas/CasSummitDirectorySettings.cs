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
    public partial class CasSummitAbstractConfigGenerator //quartz config part
    {
        private FileLocation vaultPrimary;
        private FileLocation vaultDialogic;
        private SyncPair huawei_Vault;
        private SyncPair zteCAS;

        public static Dictionary<string, string> SrtConfigHelperMap = new Dictionary<string, string>()
        {
            { "vaultName","vault"},
            //{}
        };
        public void PrepareDirectorySettings(TelcobrightConfig tbc)
        {
            DirectorySettings directorySetting = new DirectorySettings("Directory Settings", @"cas\casDbD");
            
            tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            this.vaultPrimary = new FileLocation()
            {
                Name = "vault",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "D:/telcobright/vault/resources/cdr/summit/tdm",
                User = "",
                Pass = "",
            };

            this.vaultDialogic = new FileLocation()
            {
                Name = "vaultDialogic",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "D:/telcobright/vault/resources/cdr/summit/ip",
                User = "",
                Pass = "",
            };

            FileLocation huawei = new FileLocation()
            {
                Name = "huawei",
                LocationType = "ftp",
                OsType = "linux",
                UseActiveModeForFTP = true,
                PathSeparator = "/",
                StartingPath = "/home/zxss10_bsvr/data/bfile/bill/delete/delete_reve",
                ServerIp = "10.33.34.12",
                User = "icxreve",
                Pass = "icxreve123",
                //ExcludeBefore = new DateTime(2015, 6, 26, 0, 0, 0),
                IgnoreZeroLenghFile = 1,
                FtpSessionCloseAndReOpeningtervalByFleTransferCount = 1000
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
                ServerIp = "192.168.100.185", //server = "172.16.16.242",
                User = "adnvertx",
                Pass = "vtxicx296#",
                IgnoreZeroLenghFile = 1
            };

            //add locations to directory settings
            tbc.DirectorySettings.FileLocations.Add(vaultPrimary.Name, vaultPrimary);
            tbc.DirectorySettings.FileLocations.Add(vaultDialogic.Name, vaultDialogic);
            tbc.DirectorySettings.FileLocations.Add(huawei.Name, huawei);
            tbc.DirectorySettings.FileLocations.Add(fileArchive1.Name, fileArchive1);
            tbc.DirectorySettings.FileLocations.Add(fileArchiveCAS.Name, fileArchiveCAS);

            this.huawei_Vault = new SyncPair("huawei:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = huawei,
                    DescendingFileListByFileName = this.Tbc.CdrSetting.DescendingOrderWhileListingFiles
                },
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultPrimary
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "downloaded",
                    MoveFilesToSecondaryAfterCopy = false,
                    Recursive = false,
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
            this.zteCAS = new SyncPair("zte:cas")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultPrimary
                },
                DstSyncLocation = new SyncLocation()
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
            directorySetting.SyncPairs.Add(huawei_Vault.Name, huawei_Vault);
            //directorySetting.SyncPairs.Add(vaultS3FileArchive1.Name, vaultS3FileArchive1);
            directorySetting.SyncPairs.Add(zteCAS.Name, zteCAS);

            //add archive locations to CdrSettings
            this.Tbc.CdrSetting.BackupSyncPairNames = new List<string>()
            {
                //vaultS3FileArchive1.Name,
                zteCAS.Name
            };
        }
    }
}
