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
    public partial class SrtAbstractConfigConfigGenerator //quartz config part
    {
        public static Dictionary<string, string> SrtConfigHelperMap = new Dictionary<string, string>()
        {
            { "vaultName","vault"},
            //{}
        };
        public void PrepareDirectorySettings(TelcobrightConfig tbc)
        {
            DirectorySettings directorySetting = new DirectorySettings("c:/telcobright");
            tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            
            FileLocation cataleyaLocal = new FileLocation()
            {
                Name = "vault",//this is refered in ne table
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/cdr",
                User = "",
                Pass = "",
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

            FileLocation huaweiLocal = new FileLocation()
            {
                Name = "huaweiLocal",//this is refered in ne table
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/cdr/huawei",
                User = "",
                Pass = "",
            };
            SyncPair Huawei_Vault = new SyncPair("Huawei:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = new FileLocation()
                    {
                        Name = "huaweiRemote",
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
                    },
                    DescendingFileListByFileName = this.Tbc.CdrSetting.DescendingOrderWhileListingFiles
                },
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = huaweiLocal
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
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = huaweiLocal
                },
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = new FileLocation()//raw cdr archive
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
            directorySetting.SyncPairs.Add(Huawei_Vault.Name, Huawei_Vault);
            directorySetting.SyncPairs.Add(vaultCAS.Name, vaultCAS);

            //add archive locations to CdrSettings
            this.Tbc.CdrSetting.BackupSyncPairNames = new List<string>()
            {
                //vaultS3FileArchive1.Name,
                vaultCAS.Name
            };
        }
    }
}
