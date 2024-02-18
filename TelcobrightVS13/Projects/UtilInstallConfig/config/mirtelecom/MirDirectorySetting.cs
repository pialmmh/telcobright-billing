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
    public partial class MirAbstractConfigGenerator //quartz config part
    {
        private FileLocation vaultDialogic;
        public void PrepareDirectorySetting(TelcobrightConfig tbc)
        {
            DirectorySettings directorySettings = new DirectorySettings("c:/telcobright", "");
            tbc.DirectorySettings = directorySettings;
            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            this.vaultDialogic = new FileLocation()
            {
                Name = "Vault.MirDialogic",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/CDR/mir/dialogic",
                User = "",
                Pass = "",
            };
            

            tbc.DirectorySettings.FileLocations.Add(vaultDialogic.Name,vaultDialogic);

            FileLocation dialogic = new FileLocation()
            {
                Name = "dialogic",
                LocationType = "ftp",
                OsType = "linux",
                UseActiveModeForFTP = false,
                PathSeparator = "/",
                StartingPath = "/",
                ServerIp = "123.176.59.19",
                User = "igw_dialogic",
                Pass = "Abc@123456#",
                //ExcludeBefore = new DateTime(2015, 6, 26, 0, 0, 0),
                IgnoreZeroLenghFile = 1,
                FtpSessionCloseAndReOpeningtervalByFleTransferCount = 1000
            };

            // dialogic

            SyncPair MirDialogicVault = new SyncPair("mirDialogic:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = dialogic,
                    DescendingFileListByFileName = tbc.CdrSetting.DescendingOrderWhileListingFiles
                },
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultDialogic,
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "incoming_sdr_bin_backup",
                    MoveFilesToSecondaryAfterCopy = true,
                    ExpFileNameFilter = new SpringExpression(@"Name.StartsWith('sdr')
                                                                    and
                                                                    (Name.EndsWith('.gz'))
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


            //add sync pairs to directory config
            directorySettings.SyncPairs.Add(MirDialogicVault.Name, MirDialogicVault);

            tbc.DirectorySettings = directorySettings;

            //add archive locations to CdrSettings
            tbc.CdrSetting.BackupSyncPairNames = new List<string>
            {
                //vaultFileArchive1Zip.Name,
                //vaultIof.Name,
            };

            //directorySettings.FileLocations = directorySettings.SyncPairs.Values.SelectMany(sp =>
            //    new List<FileLocation>
            //    {
            //        sp.SrcSyncLocation.FileLocation,
            //        sp.DstSyncLocation.FileLocation
            //    }).ToDictionary(floc => floc.Name);
        }
    }
}
