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
using TelcobrightFileOperations;
using TelcobrightMediation.Config;

namespace InstallConfig
{
    public partial class CCLAbstractConfigGenerator //quartz config part
    {
        private FileLocation vaultPrimary;
        private SyncPair telcobright_Vault;

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
                StartingPath = "C:/telcobright/Vault/Resources/cdr/telcobright",
                User = "",
                Pass = "",
            };


            FileLocation telcobright = new FileLocation()
            {
                Name = "telcobright",
                LocationType = "sftp",
                OsType = "linux",
                UseActiveModeForFTP = true,
                PathSeparator = "/",
                StartingPath = "/home/telcobright/Desktop/RTC-Manager/FreeSwitch/src/main/resources/cdrFiles",
                ServerIp = "103.95.96.98",
                User = "telcobright",
                Pass = "Takay1#$ane%%",
                ExcludeBefore = new DateTime(2024, 7, 1, 0, 0, 0),
                IgnoreZeroLenghFile = 1,
                FtpSessionCloseAndReOpeningtervalByFleTransferCount = 1000
            };



            //add locations to directory settings
            tbc.DirectorySettings.FileLocations.Add(vaultPrimary.Name, vaultPrimary);
            tbc.DirectorySettings.FileLocations.Add(telcobright.Name, telcobright);

            this.telcobright_Vault = new SyncPair("telcobright:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = telcobright,
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
                    ExpFileNameFilter = new SpringExpression(@"Name.StartsWith('cdr_')
                                                                and
                                                                (Name.EndsWith('.csv'))
                                                                and Length>0")
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",//make sure when copying to vault always .tmp ext used
                    Overwrite = true,
                    ExpDestFileName = new SpringExpression(@"Name.Insert(0,'')"),
                    CompressionType = CompressionType.None,
                }
            };

            


            //add sync pairs to directory config
            directorySetting.SyncPairs.Add(telcobright_Vault.Name, telcobright_Vault);

            //add archive locations to CdrSettings
            this.Tbc.CdrSetting.BackupSyncPairNames = new List<string>()
            {
            };
        }
    }
}
