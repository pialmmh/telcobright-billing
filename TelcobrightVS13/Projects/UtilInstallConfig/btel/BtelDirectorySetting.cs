﻿using System;
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
    public partial class BtelAbstractConfigConfigGenerator //quartz config part
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
            FileLocation vaultBtelhuaweiDhk = new FileLocation()
            {
                Name = "Vault.BtelhuaweiDhk",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/CDR/btel/BtelhuaweiDhk",
                User = "",
                Pass = "",
            };
            FileLocation vaultBtelCataleya = new FileLocation()
            {
                Name = "Vault.btelCataleya",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/CDR/btel/BtelhuaweiDhk",
                User = "",
                Pass = "",
            };
            FileLocation appServerFtp1 = new FileLocation()
            {
                Name = "AppServerFTP1",//
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                ServerIp = "10.0.0.10",
                StartingPath = "Resources/CDR/btel/BtelhuaweiDhk",
                User = "ftpuser",
                Pass = "Takay1takaane",
            };
            FileLocation appServerFtp2 = new FileLocation()
            {
                Name = "AppServerFTP2",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                ServerIp = "10.0.0.12",
                StartingPath = "Resources/CDR/btel/BtelhuaweiDhk",
                User = "ftpuser",
                Pass = "Takay1takaane",
                Skip = true
            };
            //VAULT PART
            List<FileLocation> ftpLocations = new List<FileLocation>();
            ftpLocations.Add(appServerFtp1);
            ftpLocations.Add(appServerFtp2);
            Vault BtelhuaweiDhkvault = new Vault("Vault.BtelhuaweiDhk", tbc, ftpLocations);
            Vault BtelCataleyaVault = new Vault("Vault.btelCataleya", tbc, ftpLocations);

            BtelhuaweiDhkvault.LocalLocation = new SyncLocation(vaultBtelhuaweiDhk.Name) { FileLocation = vaultBtelhuaweiDhk };//don't pass this to constructor and set there, causes problem in json serialize
            BtelCataleyaVault.LocalLocation = new SyncLocation(vaultBtelCataleya.Name) { FileLocation = vaultBtelCataleya };//don't pass this to constructor and set there, causes problem in json serialize
            directorySettings.Vaults.Add(BtelhuaweiDhkvault);
            directorySettings.Vaults.Add(BtelCataleyaVault);
            FileLocation BtelhuaweiDhk = new FileLocation()
            {
                Name = "BtelhuaweiDhk",
                LocationType = "sftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = "/home/zxss10_bsvr/data/bfile/bill",
                //StartingPath = "/home/zxss10_bsvr/data/bfile/bill/zsmart_media_bak",
                Sftphostkey = "",
                ServerIp = "10.33.34.12",
                User = "igwbill",
                Pass = "igw123",
                ExcludeBefore = new DateTime(2015, 6, 26, 0, 0, 0),
                IgnoreZeroLenghFile = 1
            };
            FileLocation btelCataleya = new FileLocation()
            {
                Name = "btelCataleya",
                LocationType = "sftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = "/sdr/incoming_sdr_bin/",
                //StartingPath = "/home/zxss10_bsvr/data/bfile/bill/zsmart_media_bak",
                Sftphostkey = "",
                ServerIp = "10.33.42.4",
                User = "banglatel_sdr",
                Pass = "B@ngL@TEL@2021!_1",
                ExcludeBefore = new DateTime(2015, 6, 26, 0, 0, 0),
                IgnoreZeroLenghFile = 1
            };
            FileLocation fileArchive1Zip = new FileLocation()//raw cdr archive
            {
                Name = "FileArchive1Zip",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/IGW_CDR_BK",
                ServerIp = "10.100.201.13", //server = "172.16.16.242",
                User = "iofcdr",
                Pass = "blt#.45",
                IgnoreZeroLenghFile = 1
            };
            FileLocation fileArchiveIof = new FileLocation()//raw cdr archive
            {
                Name = "FileArchiveIof",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/IGW_TO_IOF2",
                ServerIp = "10.100.201.13", //server = "172.16.16.242",
                User = "iofcdr",
                Pass = "blt#.45",
                IgnoreZeroLenghFile = 1
            };
            //add locations to directory settings
            directorySettings.FileLocations.Add(vaultBtelhuaweiDhk.Name, vaultBtelhuaweiDhk);
            directorySettings.FileLocations.Add(vaultBtelCataleya.Name, vaultBtelCataleya);
            directorySettings.FileLocations.Add(appServerFtp1.Name, appServerFtp1);
            directorySettings.FileLocations.Add(appServerFtp2.Name, appServerFtp2);
            directorySettings.FileLocations.Add(BtelhuaweiDhk.Name, BtelhuaweiDhk);

            directorySettings.FileLocations.Add(fileArchive1Zip.Name, fileArchive1Zip);
            directorySettings.FileLocations.Add(fileArchiveIof.Name, fileArchiveIof);


            SyncPair spBtelhuaweiDhkVault = new SyncPair("BtelhuaweiDhk:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation("BtelhuaweiDhk")
                {
                    FileLocation = BtelhuaweiDhk,
                    DescendingFileListByFileName = tbc.CdrSetting.DescendingOrderWhileListingFiles
                },
                DstSyncLocation = new SyncLocation("Vault_BtelhuaweiDhk")
                {
                    FileLocation = vaultBtelhuaweiDhk
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "downloaded",
                    MoveFilesToSecondaryAfterCopy = true,
                    ExpFileNameFilter = new SpringExpression(@"Name.StartsWith('IGW')
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
            //cataleya
            SyncPair spBtelCataleyaVault = new SyncPair("btelCataleya:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation("btelCataleya")
                {
                    FileLocation = btelCataleya,
                    DescendingFileListByFileName = tbc.CdrSetting.DescendingOrderWhileListingFiles
                },
                DstSyncLocation = new SyncLocation("Vault.btelCataleya")
                {
                    FileLocation = vaultBtelCataleya
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "incoming_sdr_bin_backup",
                    MoveFilesToSecondaryAfterCopy = true,
                    ExpFileNameFilter = new SpringExpression(@"Name.StartsWith('esdr')
                                                                and
                                                                (Name.EndsWith('.bin'))
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
            SyncPair vaultFileArchive1Zip = new SyncPair("Vault:FileArchive1")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation("Vault_BtelhuaweiDhk")
                {
                    FileLocation = vaultBtelhuaweiDhk
                },
                DstSyncLocation = new SyncLocation("FileArchive1Zip")
                {
                    FileLocation = fileArchive1Zip
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

            SyncPair vaultIof = new SyncPair("Vault:IOF")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation("Vault_BtelhuaweiDhk")
                {
                    FileLocation = vaultBtelhuaweiDhk
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
            directorySettings.SyncPairs.Add(spBtelhuaweiDhkVault.Name, spBtelhuaweiDhkVault);
            directorySettings.SyncPairs.Add(spBtelCataleyaVault.Name, spBtelCataleyaVault);
            directorySettings.SyncPairs.Add(vaultFileArchive1Zip.Name, vaultFileArchive1Zip);
            directorySettings.SyncPairs.Add(vaultIof.Name, vaultIof);
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
                vaultFileArchive1Zip.Name,
                vaultIof.Name,
            };

        }
    }
}
