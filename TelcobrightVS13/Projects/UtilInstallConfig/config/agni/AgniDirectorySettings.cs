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
    public partial class AgniAbstractConfigGenerator //quartz config part
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
                StartingPath = "C:/telcobright/Vault/Resources/cdr/tdm",
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
                StartingPath = "C:/telcobright/Vault/Resources/cdr/ip",
                User = "",
                Pass = "",
            };


            //add locations to directory settings
            tbc.DirectorySettings.FileLocations.Add(vaultCataleya.Name, vaultCataleya);
            tbc.DirectorySettings.FileLocations.Add(vaultPrimary.Name,vaultPrimary);

         
            FileLocation cas_tdm = new FileLocation()//raw cdr archive
            {
                Name = "cas_huawei",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/",
                ServerIp = "10.154.150.11", //server = "172.16.16.242",
                User = "agni_tdm",
                Pass = "5D87zmZu0%jb",
                IgnoreZeroLenghFile = 1
            };

            FileLocation cas_ip = new FileLocation()//raw cdr archive
            {
                Name = "cas_cataleya",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/",
                ServerIp = "10.255.200.30",
                User = "agni_ip",
                Pass = @":LR(Z3zf-*2g-=Dt",
                IgnoreZeroLenghFile = 1
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
            //directorySetting.SyncPairs.Add(Huawei_Vault.Name, Huawei_Vault);
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
