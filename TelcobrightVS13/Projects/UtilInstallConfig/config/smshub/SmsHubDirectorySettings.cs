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

    public partial class SmsHubAbstractConfigGenerator//quartz config part
    {
        private FileLocation vaultPrimary { get; set; }

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
            FileLocation vaultBorak1 = new FileLocation()
            {
                Name = "vault.borak1",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/cdr/borak1",
                User = "",
                Pass = "",
            };
            FileLocation vaultBorak2 = new FileLocation()
            {
                Name = "vault.borak2",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/cdr/borak2",
                User = "",
                Pass = "",
            };
            FileLocation vaultKhaja1 = new FileLocation()
            {
                Name = "vault.khaja1",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/cdr/khaja1",
                User = "",
                Pass = "",
            };
            FileLocation vaultKhaja2 = new FileLocation()
            {
                Name = "vault.khaja2",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/cdr/khaja2",
                User = "",
                Pass = "",
            };

            this.vaultPrimary = new FileLocation()
            {
                Name = "vault.primary",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/cdr/dialogic",
                User = "",
                Pass = "",
            };

            tbc.DirectorySettings.FileLocations.Add(vaultBorak1.Name, vaultBorak1);
            tbc.DirectorySettings.FileLocations.Add(vaultBorak2.Name, vaultBorak2);
            tbc.DirectorySettings.FileLocations.Add(vaultKhaja1.Name, vaultKhaja1);
            tbc.DirectorySettings.FileLocations.Add(vaultKhaja2.Name, vaultKhaja2);
            tbc.DirectorySettings.FileLocations.Add(vaultPrimary.Name, vaultPrimary);



            SyncPair borak1_Vault = new SyncPair("borak1:Vault")
            {
                SkipSourceFileListing = false,
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultBorak1
                },
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = new FileLocation()
                    {
                        Name = "borak1",
                        LocationType = "sftp",
                        OsType = "linux",
                        UseActiveModeForFTP = false,
                        PathSeparator = "/",
                        StartingPath = "/ftpuser/syslog/mdr",
                        ServerIp = "172.20.23.101",
                        User = "Telcobright",
                        Pass = "duJg$n7a",
                        //ExcludeBefore = new DateTime(2015, 6, 26, 0, 0, 0),
                        IgnoreZeroLenghFile = 1,
                        FtpSessionCloseAndReOpeningtervalByFleTransferCount = 1000
                    },
                    DescendingFileListByFileName = this.Tbc.CdrSetting.DescendingOrderWhileListingFiles
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "",
                    MoveFilesToSecondaryAfterCopy = false,
                    Recursive = true,
                    ExpFileNameFilter = new SpringExpression(@"Name.StartsWith('mdr')
                                                                and
                                                                (Name.EndsWith('.gz'))
                                                                and Length>0")
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",//make sure when copying to vault always .tmp ext used
                    Overwrite = true,
                    PrefixForUniqueName = "",
                    RecursiveFileStore=true,
                    ExpDestFileName = new SpringExpression(@"Name.Insert(0,'')"),
                    CompressionType = CompressionType.None
                }
            };


            SyncPair borak2_Vault = new SyncPair("borak2:Vault")
            {
                SkipSourceFileListing = false,
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultBorak2
                },
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = new FileLocation()
                    {
                        Name = "borak2",
                        LocationType = "sftp",
                        OsType = "linux",
                        UseActiveModeForFTP = false,
                        PathSeparator = "/",
                        StartingPath = "/ftpuser/syslog/mdr",
                        ServerIp = "172.20.23.102",
                        User = "Telcobright",
                        Pass = "duJg$n7a",
                        //ExcludeBefore = new DateTime(2015, 6, 26, 0, 0, 0),
                        IgnoreZeroLenghFile = 1,
                        FtpSessionCloseAndReOpeningtervalByFleTransferCount = 1000
                    },
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "",
                    MoveFilesToSecondaryAfterCopy = false,
                    Recursive = true,
                    ExpFileNameFilter = new SpringExpression(@"Name.StartsWith('mdr')
                                                                and
                                                                (Name.EndsWith('.gz'))
                                                                and Length>0")
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",//make sure when copying to vault always .tmp ext used
                    Overwrite = true,
                    PrefixForUniqueName = "",
                    ExpDestFileName = new SpringExpression(@"Name.Insert(0,'')"),
                    CompressionType = CompressionType.None
                }
            };

            SyncPair khaja1_Vault = new SyncPair("khaja1:Vault")
            {
                SkipSourceFileListing = false,
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultKhaja1
                },
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = new FileLocation()
                    {
                        Name = "khaja1",
                        LocationType = "sftp",
                        OsType = "linux",
                        UseActiveModeForFTP = false,
                        PathSeparator = "/",
                        StartingPath = "/ftpuser/syslog/mdr",
                        ServerIp = "172.30.23.101",
                        User = "Telcobright",
                        Pass = "duJg$n7a",
                        //ExcludeBefore = new DateTime(2015, 6, 26, 0, 0, 0),
                        IgnoreZeroLenghFile = 1,
                        FtpSessionCloseAndReOpeningtervalByFleTransferCount = 1000
                    },
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "",
                    MoveFilesToSecondaryAfterCopy = false,
                    Recursive = true,
                    ExpFileNameFilter = new SpringExpression(@"Name.StartsWith('mdr')
                                                                and
                                                                (Name.EndsWith('.gz'))
                                                                and Length>0")
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",//make sure when copying to vault always .tmp ext used
                    Overwrite = true,
                    PrefixForUniqueName = "",
                    ExpDestFileName = new SpringExpression(@"Name.Insert(0,'')"),
                    CompressionType = CompressionType.None
                }
            };

            SyncPair khaja2_Vault = new SyncPair("khaja2:Vault")
            {
                SkipSourceFileListing = false,
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultKhaja2
                },
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = new FileLocation()
                    {
                        Name = "khaja2",
                        LocationType = "sftp",
                        OsType = "linux",
                        UseActiveModeForFTP = false,
                        PathSeparator = "/",
                        StartingPath = "/ftpuser/syslog/mdr",
                        ServerIp = "172.30.23.102",
                        User = "Telcobright",
                        Pass = "duJg$n7a",
                        //ExcludeBefore = new DateTime(2015, 6, 26, 0, 0, 0),
                        IgnoreZeroLenghFile = 1,
                        FtpSessionCloseAndReOpeningtervalByFleTransferCount = 1000
                    },
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "",
                    MoveFilesToSecondaryAfterCopy = false,
                    Recursive = true,
                    ExpFileNameFilter = new SpringExpression(@"Name.StartsWith('mdr')
                                                                and
                                                                (Name.EndsWith('.gz'))
                                                                and Length>0")
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",//make sure when copying to vault always .tmp ext used
                    Overwrite = true,
                    PrefixForUniqueName = "",
                    ExpDestFileName = new SpringExpression(@"Name.Insert(0,'')"),
                    CompressionType = CompressionType.None
                }
            };



            //add sync pairs to directory config
            directorySetting.SyncPairs.Add(borak1_Vault.Name, borak1_Vault);
            directorySetting.SyncPairs.Add(borak2_Vault.Name, borak2_Vault);
            directorySetting.SyncPairs.Add(khaja1_Vault.Name, khaja1_Vault);
            directorySetting.SyncPairs.Add(khaja2_Vault.Name, khaja2_Vault);

            //add archive locations to CdrSettings
            this.Tbc.CdrSetting.BackupSyncPairNames = new List<string>()
            {
                borak2_Vault.Name,
                borak1_Vault.Name,
                khaja1_Vault.Name,
                khaja2_Vault.Name,
            };
            directorySetting.FileLocations = directorySetting.SyncPairs.Values.SelectMany(sp =>
                new List<FileLocation>
                {
                    sp.SrcSyncLocation.FileLocation,
                    sp.DstSyncLocation.FileLocation
                }).ToDictionary(floc => floc.Name);
        }
    }
}
