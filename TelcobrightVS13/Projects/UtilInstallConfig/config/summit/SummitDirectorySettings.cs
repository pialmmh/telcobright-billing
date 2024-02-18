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
    public partial class SummitAbstractConfigGenerator //quartz config part
    {
        private FileLocation vaultPrimary;
        private FileLocation vaultDialogic;
        private SyncPair zte_Vault;
        private SyncPair reveCAS;
        private SyncPair ipCAS;
        private SyncPair tdmCAS;
        private SyncPair SummitFtpForIp;
        private SyncPair SummitFtpForTdm;

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
                StartingPath = "C:/telcobright/Vault/Resources/cdr/zte",
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
                StartingPath = "c:/telcobright/Vault/Resources/cdr/dialogic",
                User = "",
                Pass = "",
            };

            FileLocation zte = new FileLocation()
            {
                Name = "zte",
                LocationType = "ftp",
                OsType = "linux",
                UseActiveModeForFTP = true,
                PathSeparator = "/",
                StartingPath = "/home/zxss10_bsvr/data/bfile/bill/delete",
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

            FileLocation cas_tdm = new FileLocation()//raw cdr archive
            {
                Name = "cas_zte",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/",
                ServerIp = "10.154.150.57", //server = "172.16.16.242",
                User = "summit_tdm",
                Pass = "g*GVC$HG1110",
                IgnoreZeroLenghFile = 1
            };

            FileLocation cas_ip = new FileLocation()//raw cdr archive
            {
                Name = "cas_dialogic",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/",
                ServerIp = "10.255.200.57", //server = "172.16.16.242",
                User = "summit_ip",
                Pass = "MdReMz!rm$&KJh7{",
                IgnoreZeroLenghFile = 1
            };

            FileLocation cas_reve = new FileLocation()//raw cdr archive
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

            FileLocation summitFtpForTdm = new FileLocation()//raw cdr archive
            {
                Name = "summitftp",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/1 TDM",
                ServerIp = "172.18.0.10", //server = "172.16.16.242",
                User = "ftpuser",
                Pass = "Takay1takaane",
                IgnoreZeroLenghFile = 1
            };
            FileLocation summitFtpForIp = new FileLocation()//raw cdr archive
            {
                Name = "summitftp",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/2 IP",
                ServerIp = "172.18.0.10", //server = "172.16.16.242",
                User = "ftpuser",
                Pass = "Takay1takaane",
                IgnoreZeroLenghFile = 1
            };

            //add locations to directory settings
            tbc.DirectorySettings.FileLocations.Add(vaultPrimary.Name, vaultPrimary);
            tbc.DirectorySettings.FileLocations.Add(vaultDialogic.Name, vaultDialogic);
            tbc.DirectorySettings.FileLocations.Add(zte.Name, zte);
            tbc.DirectorySettings.FileLocations.Add(fileArchive1.Name, fileArchive1);
            tbc.DirectorySettings.FileLocations.Add(cas_reve.Name, cas_reve);
            tbc.DirectorySettings.FileLocations.Add(cas_ip.Name, cas_ip);
            tbc.DirectorySettings.FileLocations.Add(cas_tdm.Name, cas_tdm);

            this.zte_Vault = new SyncPair("zte:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = zte,
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
            this.reveCAS = new SyncPair("zte:cas")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultPrimary
                },
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = cas_reve
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


            this.ipCAS = new SyncPair("ip:cas")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultDialogic
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

            this.SummitFtpForTdm = new SyncPair("zte:summitftpfortdm")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultPrimary
                },
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = summitFtpForTdm
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

            this.SummitFtpForIp = new SyncPair("dlg:summitftpforip")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultDialogic
                },
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = summitFtpForIp
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
            directorySetting.SyncPairs.Add(zte_Vault.Name, zte_Vault);
            directorySetting.SyncPairs.Add(reveCAS.Name, reveCAS);
            directorySetting.SyncPairs.Add(ipCAS.Name, ipCAS);
            directorySetting.SyncPairs.Add(tdmCAS.Name, tdmCAS);
            directorySetting.SyncPairs.Add(SummitFtpForTdm.Name, SummitFtpForTdm);
            directorySetting.SyncPairs.Add(SummitFtpForIp.Name, SummitFtpForIp);

            //add archive locations to CdrSettings
            this.Tbc.CdrSetting.BackupSyncPairNames = new List<string>()
            {
                reveCAS.Name,
                ipCAS.Name,
                tdmCAS.Name,
                SummitFtpForTdm.Name,
                SummitFtpForIp.Name
            };
        }
    }
}
