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
using TelcobrightMediation.Automation;
using TelcobrightMediation.Config;

namespace InstallConfig
{
    public partial class PlatinumConfigGenerator //quartz config part
    {
        public void PrepareDirectorySetting(TelcobrightConfig tbc)
        {
            DirectorySettings directorySettings = new DirectorySettings("Directory Settings")
            {
                ApplicationRootDirectory = "c:/Telcobright"
            };
            
            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaultPltDhkDl = new FileLocation()
            {
                Name = "Vault.PltDhkDL",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/CDR/platinum/PltDhkDL",
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
                StartingPath = "Resources/CDR/platinum/PltDhkDL",
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
                StartingPath = "Resources/CDR/platinum/PltDhkDL",
                User = "ftpuser",
                Pass = "Takay1takaane",
                Skip = true
            };
            //VAULT PART
            List<FileLocation> ftpLocations = new List<FileLocation>();
            ftpLocations.Add(appServerFtp1);
            ftpLocations.Add(appServerFtp2);
            Vault pltDhkDLvault = new Vault("Vault.PltDhkDL",tbc , ftpLocations);
            pltDhkDLvault.LocalLocation = new SyncLocation(vaultPltDhkDl.Name) { FileLocation = vaultPltDhkDl };//don't pass this to constructor and set there, causes problem in json serialize
            directorySettings.Vaults.Add(pltDhkDLvault);
            FileLocation pltDhkDl = new FileLocation()
            {
                Name = "PltDhkDL",
                LocationType = "ftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = "/cdr/ICDR/primary",
                Sftphostkey = "",
                ServerIp = "10.10.10.10",
                User = "ics",
                Pass = "icsveraz",
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
                ServerIp = "192.168.2.225", //server = "172.16.16.242",
                User = "ftpuser",
                Pass = "Takay1takaane",
                IgnoreZeroLenghFile = 1
            };
            FileLocation fileArchiveIof = new FileLocation()//raw cdr archive
            {
                Name = "FileArchiveIOF",
                LocationType = "ftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = @"/",
                ServerIp = "172.22.21.11", //server = "172.16.16.242",
                User = "platinum",
                Pass = "usr_platinum3!2",
                IgnoreZeroLenghFile = 1,
            };
            //add locations to directory settings
            directorySettings.FileLocations.Add(vaultPltDhkDl.Name, vaultPltDhkDl);
            directorySettings.FileLocations.Add(appServerFtp1.Name, appServerFtp1);
            directorySettings.FileLocations.Add(appServerFtp2.Name, appServerFtp2);
            directorySettings.FileLocations.Add(pltDhkDl.Name, pltDhkDl);
            
            directorySettings.FileLocations.Add(fileArchive1.Name, fileArchive1);
            directorySettings.FileLocations.Add(fileArchiveIof.Name, fileArchiveIof);


            //sync pair platinum:Vault
            SyncPair pltDhkDlVault = new SyncPair("PltDhkDL:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation("PltDhkDL")
                {
                    FileLocation = pltDhkDl,
                    DescendingFileListByFileName = tbc.CdrSetting.DescendingOrderWhileListingFiles
                },
                DstSyncLocation = new SyncLocation("Vault_PltDhkDL")
                {
                    FileLocation = vaultPltDhkDl
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "Downloaded",
                    ExpFileNameFilter = new SpringExpression(@"Name.StartsWith('icdr')
                                                                and
                                                                (Name.EndsWith('.0') or Name.EndsWith('.1'))
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
                SrcSyncLocation = new SyncLocation("Vault_PltDhkDL")
                {
                    FileLocation = vaultPltDhkDl
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
            //sync pair Vault_S3:IOF
            SyncPair vaultS3Iof = new SyncPair("Vault:IOF")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation("Vault_S3")
                {
                    FileLocation = vaultPltDhkDl
                },
                DstSyncLocation = new SyncLocation("IOF")
                {
                    FileLocation = fileArchiveIof
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
            directorySettings.SyncPairs.Add(pltDhkDlVault.Name, pltDhkDlVault);
            directorySettings.SyncPairs.Add(vaultS3FileArchive1.Name, vaultS3FileArchive1);
            directorySettings.SyncPairs.Add(vaultS3Iof.Name, vaultS3Iof);
            //load the syncpairs in dictioinary, first by source
            foreach (SyncPair sp in directorySettings.SyncPairs.Values)
            {
                if (directorySettings.SyncLocations.ContainsKey(sp.SrcSyncLocation.Name) == false)
                {
                    directorySettings.SyncLocations.Add(sp.SrcSyncLocation.Name, sp.SrcSyncLocation);
                }
            }
            foreach (SyncPair sp in directorySettings.SyncPairs.Values)
            {
                if (directorySettings.SyncLocations.ContainsKey(sp.DstSyncLocation.Name) == false)
                {
                    directorySettings.SyncLocations.Add(sp.DstSyncLocation.Name, sp.DstSyncLocation);
                }
            }
            tbc.DirectorySettings = directorySettings;
            
            //add archive locations to CdrSettings
            tbc.CdrSetting.BackupSyncPairNames = new List<string>
            {
                vaultS3FileArchive1.Name, vaultS3Iof.Name
            };
            tbc.CdrSetting.DisableCdrPostProcessingJobCreationForAutomation = true;
        }
    }
}
