﻿using Newtonsoft.Json;
using System;
using System.IO;
using TelcobrightFileOperations;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using TelcobrightMediation;
using LibraryExtensions.ConfigHelper;
using TelcobrightMediation.Config;
using TelcobrightMediation.Scheduler.Quartz;

namespace InstallConfig
{
    [Export(typeof(IConfigGenerator))]
    public class BiglConfigGenerator:IConfigGenerator
    {
        public List<QuartzTbDaemonConfig> GetSchedulerDaemonConfigs()
        {
            throw new NotImplementedException();
        }

        public string OperatorName { get;}
        public TelcobrightConfig Tbc { get; }
        public BiglConfigGenerator()
        {
            int ThisServerId = 1;
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Igw, ThisServerId);
            this.OperatorName = "bigl";
        }
        public TelcobrightConfig GenerateConfig(DatabaseSetting schedulerDatabaseSetting)
        {
            if (string.IsNullOrWhiteSpace(this.OperatorName))
                throw new Exception("Operator name not configured in Config Generator");
            this.Tbc.CdrSetting = new CdrSetting()
            {
                DescendingOrderWhileListingFiles = true,
                
            };
            //write all configuration first in ws_topshelf / bin/debug, for production
            //also, in tester/bin/debug
            List<KeyValuePair<Regex, string>> serviceAliases = new List<KeyValuePair<Regex, string>>
            {
                new KeyValuePair<Regex, string>(new Regex(@".*/sg5/.*/sf4/.*"), "International Outgoing"),
                new KeyValuePair<Regex, string>(new Regex(@".*/sg4/.*/sf1/.*"), "AZ Voice")
            };
            this.Tbc.ServiceAliasesRegex = serviceAliases;

            //temp use of ne, remove later


            //database part
            DirectorySettings directorySetting = new DirectorySettings("Directory Settings")
            {
                ApplicationRootDirectory = "c:/Telcobright"
            };
            this.Tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaultDhkHwBigl = new FileLocation()
            {
                Name = "Vault.dhkHwBigl",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = "\\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/CDR/Bangladesh International Gateway Limited/dhkHwBigl",
                User = "",
                Pass = "",
            };
            FileLocation appServerFtp1 = new FileLocation()
            {
                Name = "AppServerFTP1",//
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                ServerIp = "103.209.83.226",
                StartingPath = "Resources/CDR/Bangladesh International Gateway Limited/dhkHwBigl",
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
                StartingPath = "Resources/CDR/Bangladesh International Gateway Limited/dhkHwBigl",
                User = "ftpuser",
                Pass = "Takay1takaane",
                Skip = true
            };
            //VAULT PART
            List<FileLocation> ftpLocations = new List<FileLocation>();
            ftpLocations.Add(appServerFtp1);
            ftpLocations.Add(appServerFtp2);
            Vault dhkHwBiglvault = new Vault("Vault.dhkHwBigl", this.Tbc, ftpLocations);
            dhkHwBiglvault.LocalLocation = new SyncLocation(vaultDhkHwBigl.Name) { FileLocation = vaultDhkHwBigl };//don't pass this to constructor and set there, causes problem in json serialize
            this.Tbc.DirectorySettings.Vaults.Add(dhkHwBiglvault);
            FileLocation dhkHwBigl = new FileLocation()
            {
                Name = "dhkHwBigl",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                StartingPath = "/set/later",
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
                ServerIp = "127.0.0.1", //server = "172.16.16.242",
                User = "ftpuser",
                Pass = "Takay1takaane",
                IgnoreZeroLenghFile = 1
            };
            FileLocation fileArchiveIof = new FileLocation()//raw cdr archive
            {
                Name = "FileArchiveIOF",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/iof",
                ServerIp = "127.0.0.1", //server = "172.16.16.242",
                User = "ftpuser",
                Pass = "Takay1takaane",
                IgnoreZeroLenghFile = 1
            };
            //add locations to directory settings
            this.Tbc.DirectorySettings.FileLocations.Add(vaultDhkHwBigl.Name, vaultDhkHwBigl);
            this.Tbc.DirectorySettings.FileLocations.Add(appServerFtp1.Name, appServerFtp1);
            this.Tbc.DirectorySettings.FileLocations.Add(appServerFtp2.Name, appServerFtp2);
            this.Tbc.DirectorySettings.FileLocations.Add(dhkHwBigl.Name, dhkHwBigl);

            this.Tbc.DirectorySettings.FileLocations.Add(fileArchive1.Name, fileArchive1);
            this.Tbc.DirectorySettings.FileLocations.Add(fileArchiveIof.Name, fileArchiveIof);


            //sync pair platinum:Vault
            SyncPair dhkHwBiglVault = new SyncPair("dhkHwBigl:Vault")
            {
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation("dhkHwBigl")
                {
                    FileLocation = dhkHwBigl,
                    DescendingFileListByFileName = this.Tbc.CdrSetting.DescendingOrderWhileListingFiles
                },
                DstSyncLocation = new SyncLocation("Vault_dhkHwBigl")
                {
                    FileLocation = vaultDhkHwBigl
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
                SrcSyncLocation = new SyncLocation("Vault_dhkHwBigl")
                {
                    FileLocation = vaultDhkHwBigl
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
                    FileLocation = vaultDhkHwBigl
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
            directorySetting.SyncPairs.Add(dhkHwBiglVault.Name, dhkHwBiglVault);
            directorySetting.SyncPairs.Add(vaultS3FileArchive1.Name, vaultS3FileArchive1);
            directorySetting.SyncPairs.Add(vaultS3Iof.Name, vaultS3Iof);
            //load the syncpairs in dictioinary, first by source
            foreach (SyncPair sp in directorySetting.SyncPairs.Values)
            {
                if (directorySetting.SyncLocations.ContainsKey(sp.SrcSyncLocation.Name) == false)
                {
                    directorySetting.SyncLocations.Add(sp.SrcSyncLocation.Name, sp.SrcSyncLocation);
                }
            }
            foreach (SyncPair sp in directorySetting.SyncPairs.Values)
            {
                if (directorySetting.SyncLocations.ContainsKey(sp.DstSyncLocation.Name) == false)
                {
                    directorySetting.SyncLocations.Add(sp.DstSyncLocation.Name, sp.DstSyncLocation);
                }
            }
            //add archive locations to CdrSettings
            this.Tbc.CdrSetting.BackupSyncPairNames = new List<string>();
            this.Tbc.CdrSetting.BackupSyncPairNames.Add(vaultS3FileArchive1.Name);
            this.Tbc.CdrSetting.BackupSyncPairNames.Add(vaultS3Iof.Name);


            //configuration for server1
            ApplicationServerConfig serverConfig1 = new ApplicationServerConfig(this.Tbc) { ServerId = 1, OwnIpAddress = "192.168.101.1" };
            //configuration for server2
            ApplicationServerConfig serverConfig2 = new ApplicationServerConfig(this.Tbc) { ServerId = 2 };

            this.Tbc.ApplicationServersConfig.Add(serverConfig1.ServerId.ToString(), serverConfig1);
            this.Tbc.ApplicationServersConfig.Add(serverConfig2.ServerId.ToString(), serverConfig1);

            DatabaseSetting databaseSetting = schedulerDatabaseSetting;
            databaseSetting.DatabaseName = this.OperatorName;
            this.Tbc.DatabaseSetting = databaseSetting;

            PortalSettings portalSetting = new PortalSettings("Portal Settings")
            {
                HomePageUrl = "~/Dashboard.aspx",
                RouteTypeEnums = new Dictionary<string, int>()
                {
                    {"Select",-1 },
                    { "International",2},
                },
                PortalSites = new List<InternetSite>()
                {
                    new InternetSite(this.Tbc)//make sure that first one always the http portal
                    {
                        SiteType = "http",
                        SiteName = databaseSetting.DatabaseName,
                        SiteId = 1,
                        PhysicalPath = "C:/inetpub/wwwroot/" + this.Tbc.DatabaseSetting.DatabaseName,
                        BindAddress = this.Tbc.DirectorySettings.FileLocations["AppServerFTP" + this.Tbc.ServerId].ServerIp + ":80",
                        TemplateFileName = "../../" + this.Tbc.DatabaseSetting.DatabaseName + "/tmplPortalWebSite.txt",
                        ApplicationPool=new IisApplicationPool()
                        {
                            AppPoolName = this.Tbc.DatabaseSetting.DatabaseName,
                            TemplateFileName = "../../" + this.Tbc.DatabaseSetting.DatabaseName + "/tmplPortalAppPools.txt",
                        },
                        ImpersonateUserName="administrator",
                        ImpersonatePassword="Habib321"

                    },
                    new InternetSite(this.Tbc)
                    {
                        SiteType = "ftp",
                        SiteName = databaseSetting.DatabaseName,
                        SiteId = 2,
                        PhysicalPath = "C:/sftp_root",
                        BindAddress = this.Tbc.DirectorySettings.FileLocations["AppServerFTP" + this.Tbc.ServerId].ServerIp + ":21",
                        TemplateFileName = "../../" + this.Tbc.DatabaseSetting.DatabaseName + "/tmplPortalFtpSite.txt"
                    }
                },
                DicConfigObjects = new Dictionary<string, object>()
                {
                    { "CdrFieldTemplate",new List<CdrFieldTemplate>()//the list is the object
                        {
                           new CdrFieldTemplate()
                           {
                               FieldTemplateName="Basic",
                               Fields=new List<string>()
                                    {
                                        "starttime as `Start Time`",
                                        "AnswerTime as `Answer Time`",
                                        "endtime as `End Time`",
                                        "inpartner.partnername as `In Partner`",
                                        "outpartner.partnername as `Out Partner`",
                                        "concat(switchid,'-',IncomingRoute) as `Ingress Route`",
                                        "concat(switchid,'-',OutgoingRoute)  as `Egress Route`",
                                        "OriginatingCallingNumber as `Ingress Calling Number`",
                                        "OriginatingCalledNumber as `Ingress Called Number`",
                                        "TerminatingCallingNumber as `Egress Calling Number`",
                                        "TerminatingCalledNumber as `Egress Called Number`",
                                        "DurationSec as `Actual Duration`",
                                        "roundedduration as `Rounded Duration`",
                                        "duration1 as Duration1",
                                        "duration2 as Duration2",
                                        "Duration3 as Duration3",
                                        "releasecauseingress as `Ingress CauseCode`",
                                    }
                           },
                           new CdrFieldTemplate()
                           {
                               FieldTemplateName="All",
                               Fields=new List<string>()
                                    {
                                        "*"
                                    }
                           },
                           new CdrFieldTemplate()
                           {
                                FieldTemplateName="Basic_Error",
                                Fields=new List<string>()
                                    {
                                        "c.errorcode as `Error Code`",
                                        "starttime as `Start Time`",
                                        "AnswerTime as `Answer Time`",
                                        "endtime as `End Time`",
                                        "c.customerid",
                                        "c.supplierid",
                                        "concat(switchid,'-',IncomingRoute) as `Ingress Route`",
                                        "concat(switchid,'-',OutgoingRoute)  as `Egress Route`",
                                        "OriginatingCallingNumber as `Ingress Calling Number`",
                                        "OriginatingCalledNumber as `Ingress Called Number`",
                                        "TerminatingCallingNumber as `Egress Calling Number`",
                                        "TerminatingCalledNumber as `Egress Called Number`",
                                        "DurationSec as `Actual Duration`",
                                        "roundedduration as `Rounded Duration`",
                                        "duration1 as Duration1",
                                        "duration2 as Duration2",
                                        "Duration3 as Duration3",
                                        "releasecauseingress as `Ingress CauseCode`",
                                        "ReleaseCauseEgress as `Egress Cause Code`"
                                    }
                           } ,
                           new CdrFieldTemplate()
                           {
                               FieldTemplateName="All_Error",
                               Fields=new List<string>()
                                   {
                                       "c.errorcode as `Error Code`",
                                       "c.*"
                                   }
                           }
                        }//list of cdr templates
                    }//one config object
                }
            };//portalSettings

            //settings for report pages, same settings to be copied...
            List<SettingByRoles> settingIntlIn = new List<SettingByRoles>()
            {
                {
                    new SettingByRoles()
                    {
                        RoleNames = new List<string>()
                        {
                            "admin","billing"
                        },
                        SpringExpressionIfRole = new List<string>()
                        {
                            "CheckBoxShowCost.Enabled=true",
                            "GridView1.Columns[7].Visible=true",//actual duration
                            "GridView1.Columns[9].Visible=true",//duration1
                            "GridView1.Columns[6].Visible=false",//Connect count
                            "GridView1.Columns[19].Visible=false",//CCR
                        },
                        SpringExpressionIfNotRole = new List<string>()
                        {
                            "CheckBoxShowCost.Enabled=false",
                            "GridView1.Columns[6].Visible=false",//Connect count
                            "GridView1.Columns[7].Visible=false",
                            "GridView1.Columns[9].Visible=false",
                            "GridView1.Columns[8].HeaderText=Duration",
                            "GridView1.Columns[19].Visible=false",//CCR
                        }
                    }
                }
            };//settings for one role within a page
            List<SettingByRoles> settingIntlInRoute = new List<SettingByRoles>()
            {
                {
                    new SettingByRoles()
                    {
                        RoleNames = new List<string>()
                        {
                            "admin","billing"
                        },
                        SpringExpressionIfRole = new List<string>()
                        {
                            "CheckBoxShowCost.Enabled=true",
                            "GridView1.Columns[6].Visible=true",//actual duration
                            "GridView1.Columns[8].Visible=true",//duration1
                            "GridView1.Columns[19].Visible=false",//CCR
                        },
                        SpringExpressionIfNotRole = new List<string>()
                        {
                            "CheckBoxShowCost.Enabled=false",
                            "GridView1.Columns[6].Visible=false",
                            "GridView1.Columns[8].Visible=false",
                            "GridView1.Columns[7].HeaderText=Duration",
                            "GridView1.Columns[19].Visible=false",//CCR
                        }
                    }
                }
            };//settings for one role within a page
            List<SettingByRoles> settingIntlOut = new List<SettingByRoles>()
            {
                {
                    new SettingByRoles()
                    {
                        RoleNames = new List<string>()
                        {
                            "admin","billing"
                        },
                        SpringExpressionIfRole = new List<string>()
                        {
                            "CheckBoxShowCost.Enabled=true",
                            "GridView1.Columns[8].Visible=false",//connected calls
                            "GridView1.Columns[9].Visible=true",//actual duration
                            "GridView1.Columns[12].Visible=true",//supplier duration
                            "GridView1.Columns[16].Visible=false",//CCR
                        },
                        SpringExpressionIfNotRole = new List<string>()
                        {
                            "CheckBoxShowCost.Enabled=false",
                            "GridView1.Columns[8].Visible=false",//connected calls
                            "GridView1.Columns[9].Visible=false",//actual duration
                            "GridView1.Columns[12].Visible=false",//supplier duration
                            "GridView1.Columns[16].Visible=false",//CCR
                        }
                    }
                }
            };//settings for one role within a page
            portalSetting.PageSettings = new PortalPageSettings()
            {
                DicPageSettingsByRole = new Dictionary<string, List<SettingByRoles>>()
                {
                    { "~/site.master",//settings for master page, //use tolower for master because got different results during execution
                        new List<SettingByRoles>()
                        {
                            new SettingByRoles()
                           {
                               RoleNames = new List<string>()
                               {
                                    "admin","billing"
                               },
                               SpringExpressionIfRole = new List<string>()
                                       {
                                            "nodes['Configuration'].Expanded=true",
                                            "nodes['Mediation'].Expanded=true",
                                            "nodes['Settings'].Expanded=true",
                                            "nodes['Billing Reports'].Expanded=true"
                                       },
                               SpringExpressionIfNotRole = new List<string>()
                                       {
                                            "nodes['Configuration'].Expanded=false",
                                            "nodes['Mediation'].Expanded=false",
                                            "nodes['Settings'].Expanded=false",
                                            "nodes['Billing Reports'].Expanded=false"
                                        }
                           }
                        }//list of settings by Roles
                    },
                    { "~/reports/InternationalIn.aspx",//settings for report pages
                        settingIntlIn
                    },
                    { "~/reports/InternationalOut.aspx",
                        settingIntlOut
                    },
                    { "~/reports/RouteInternationalIn.aspx",
                        settingIntlInRoute
                    }
                },//dictionary of page settings

            };
            this.Tbc.PortalSettings = portalSetting;
            return this.Tbc;
        }
    }
}
