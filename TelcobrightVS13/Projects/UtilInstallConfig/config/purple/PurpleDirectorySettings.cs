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

    public partial class PurpleAbstractConfigGenerator//quartz config part
    {

        public static Dictionary<string, string> SrtConfigHelperMap = new Dictionary<string, string>()
        {
            { "vaultName","vault"},
            //{}
        };


        public void PrepareDirectorySettings(TelcobrightConfig tbc)
        {


            DirectorySettings directorySetting = new DirectorySettings("c:/telcobright", "");
            tbc.DirectorySettings = directorySetting;


            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaultPrimary = new FileLocation()
            {
                Name = "vault.huawei",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "c:/telcobright/Vault/Resources/cdr/tdm",
                User = "",
                Pass = "",
            };
            FileLocation vaultCataleya = new FileLocation()
            {
                Name = "vault.Cataleya",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/cdr/cataleya",
                User = "",
                Pass = "",
            };

            tbc.DirectorySettings.FileLocations.Add(vaultPrimary.Name, vaultPrimary);
            tbc.DirectorySettings.FileLocations.Add(vaultCataleya.Name, vaultCataleya);


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



        //    SyncPair huawei_Vault = new SyncPair("huawei:Vault")
        //    {
        //        SkipSourceFileListing = false,
        //        SrcSyncLocation = new SyncLocation()
        //        {
        //            FileLocation = new FileLocation()
        //            {
        //                Name = "huawei",
        //                LocationType = "ftp",
        //                OsType = "linux",
        //                UseActiveModeForFTP = false,
        //                PathSeparator = "/",
        //                StartingPath = "/",
        //                ServerIp = "123.176.59.19",
        //                User = "icxhuawei",
        //                Pass = "Icx2023@",
        //                //ExcludeBefore = new DateTime(2015, 6, 26, 0, 0, 0),
        //                IgnoreZeroLenghFile = 1,
        //                FtpSessionCloseAndReOpeningtervalByFleTransferCount = 1000
        //            },
        //            DescendingFileListByFileName = this.Tbc.CdrSetting.DescendingOrderWhileListingFiles
        //        },
        //        DstSyncLocation = new SyncLocation()
        //        {
        //            FileLocation = vaultPrimary
        //        },
        //        SrcSettings = new SyncSettingsSource()
        //        {
        //            SecondaryDirectory = "downloaded",
        //            MoveFilesToSecondaryAfterCopy = false,
        //            Recursive=false,
        //            ExpFileNameFilter = new SpringExpression(@"Name.StartsWith('b')
        //                                                        and
        //                                                        (Name.EndsWith('.dat'))
        //                                                        and Length>0")
        //        },
        //        DstSettings = new SyncSettingsDest()
        //        {
        //            FileExtensionForSafeCopyWithTempFile = ".tmp",//make sure when copying to vault always .tmp ext used
        //            Overwrite = true,
        //            ExpDestFileName = new SpringExpression(@"Name.Insert(0,'')"),
        //            CompressionType = CompressionType.None
        //        }
        //    };

            
        //    //sync pair Vault_S3:FileArchive1
        //    SyncPair vaultCAS = new SyncPair("Vault:CAS")
        //    {
        //        SkipCopyingToDestination = false,
        //        SkipSourceFileListing = true,
        //        SrcSyncLocation = new SyncLocation()
        //        {
        //            FileLocation = vaultPrimary
        //        },
        //        DstSyncLocation = new SyncLocation()
        //        {
        //            FileLocation = new FileLocation()//raw cdr archive
        //            {
        //                Name = "cas",
        //                LocationType = "ftp",
        //                OsType = "windows",
        //                PathSeparator = @"/",//backslash didn't work with winscp
        //                StartingPath = @"/",
        //                ServerIp = "192.168.100.161", //server = "172.16.16.242",
        //                User = "adminsrt",
        //                Pass = "srticx725",
        //                IgnoreZeroLenghFile = 1
        //            }
        //},
        //        SrcSettings = new SyncSettingsSource()
        //        {
        //            SecondaryDirectory = "downloaded",
        //            ExpFileNameFilter = null,
        //        },
        //        DstSettings = new SyncSettingsDest()
        //        {
        //            FileExtensionForSafeCopyWithTempFile = ".tmp",
        //            Overwrite = true,
        //            CompressionType = CompressionType.None,
        //        }
        //    };



            //add sync pairs to directory config
            //directorySetting.SyncPairs.Add(huawei_Vault.Name, huawei_Vault);
            ////directorySetting.SyncPairs.Add(vaultS3FileArchive1.Name, vaultS3FileArchive1);
            //directorySetting.SyncPairs.Add(vaultCAS.Name, vaultCAS);

            //add archive locations to CdrSettings
            this.Tbc.CdrSetting.BackupSyncPairNames = new List<string>()
            {
                //vaultS3FileArchive1.Name,
                //vaultCAS.Name
            };
            //directorySetting.FileLocations = directorySetting.SyncPairs.Values.SelectMany(sp =>
            //    new List<FileLocation>
            //    {
            //        sp.SrcSyncLocation.FileLocation,
            //        sp.DstSyncLocation.FileLocation
            //    }).ToDictionary(floc => floc.Name);
        }
    }
}
