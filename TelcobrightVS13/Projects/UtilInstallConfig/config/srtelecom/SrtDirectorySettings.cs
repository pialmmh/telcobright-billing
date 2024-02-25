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
    public partial class SrtAbstractConfigGenerator //quartz config part
    {
        private FileLocation vaultPrimary;
        private FileLocation vaultCataleya;
        private SyncPair Huawei_Vault;
        private SyncPair ipCAS;
        private SyncPair tdmCAS;
        public static Dictionary<string, string> SrtConfigHelperMap = new Dictionary<string, string>()
        {
            { "vaultName","vault"},
            //{}
        };
        public void PrepareDirectorySettings(TelcobrightConfig tbc)
        {
            DirectorySettings directorySetting = new DirectorySettings("Directory Settings", "");
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
                StartingPath = "C:/telcobright/Vault/Resources/cdr/huawei",
                User = "",
                Pass = "",
            };
            this.vaultCataleya = new FileLocation()
            {
                Name = "vaultCataleya",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/cdr/cataleya",
                User = "",
                Pass = "",
            };

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


            FileLocation cas_tdm = new FileLocation()//raw cdr archive
            {
                Name = "cas_zte",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/",
                ServerIp = "10.154.150.35", //server = "172.16.16.242",
                User = "srTelecom_tdm",
                Pass = "q9%0p3la#WO8",
                IgnoreZeroLenghFile = 1
            };

            FileLocation cas_ip = new FileLocation()//raw cdr archive
            {
                Name = "cas_huawei",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/",
                ServerIp = "10.255.200.35", 
                User = "srTelecom_ip",
                Pass = @"TYSDP*Fp@w\37a,;",
                IgnoreZeroLenghFile = 1
            };


            //add locations to directory settings
            tbc.DirectorySettings.FileLocations.Add(vaultCataleya.Name, vaultCataleya);
            tbc.DirectorySettings.FileLocations.Add(vaultPrimary.Name,vaultPrimary);

            this.Huawei_Vault = new SyncPair("Huawei:Vault")
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
                    Recursive = true,
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

            this.ipCAS = new SyncPair("ip:cas")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultCataleya
                },
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = cas_ip
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


            this.tdmCAS = new SyncPair("tdm:cas")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultPrimary
                },
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = cas_tdm
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
            directorySetting.SyncPairs.Add(ipCAS.Name, ipCAS);
            directorySetting.SyncPairs.Add(tdmCAS.Name, tdmCAS);


            //add archive locations to CdrSettings
            this.Tbc.CdrSetting.BackupSyncPairNames = new List<string>()
            {
                ipCAS.Name,
                tdmCAS.Name,
            };
        }
    }
}
