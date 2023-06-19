using Newtonsoft.Json;
using System;
using System.IO;
using TelcobrightFileOperations;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using LibraryExtensions.ConfigHelper;
using MediationModel;
using TelcobrightMediation;
using TelcobrightMediation.Config;
using TelcobrightMediation.Scheduler.Quartz;

namespace InstallConfig
{
    [Export(typeof(AbstractConfigConfigGenerator))]
    public partial class DblAbstractConfigConfigGeneratorHelper:AbstractConfigConfigGenerator
    {
        public override List<QuartzTbDaemonConfig> GetSchedulerDaemonConfigs()
        {
            throw new NotImplementedException();
        }
        public override TelcobrightConfig Tbc { get; }
        public DblAbstractConfigConfigGeneratorHelper()
        {
            int thisServerId = 1;
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Igw, thisServerId,
                new telcobrightpartner
                    {
                        idCustomer = 4,
                        CustomerName = "DBL Telecom Ltd.",
                        idOperatorType = 4,
                        databasename = "dbl",
                        NativeTimeZone = 3251,
                        IgwPrefix = "240",
                        RateDictionaryMaxRecords = 3000000,
                        MinMSForIntlOut = 100,
                        RawCdrKeepDurationDays = 90,
                        SummaryKeepDurationDays = 730,
                        AutoDeleteOldData = 1,
                        AutoDeleteStartHour = 4,
                        AutoDeleteEndHour = 6
                    },
                tcpPortNoForRemoteScheduler: 559
                );
        }
        public override TelcobrightConfig GenerateConfig()
        {
            
            this.Tbc.Nes = new List<ne>()
            {
                new ne
                {
                    idSwitch= 4,
                    idCustomer= 4,
                    idcdrformat= 1,
                    idMediationRule= 1,
                    SwitchName= "dhkS3",
                    CDRPrefix= "S3",
                    FileExtension= ".CDR",
                    Description= null,
                    SourceFileLocations= "Vault.S3",
                    BackupFileLocations= null,
                    LoadingStopFlag= null,
                    LoadingSpanCount= 100,
                    TransactionSizeForCDRLoading= 100,
                    DecodingSpanCount= 5000,
                    SkipAutoCreateJob= 1,
                    SkipCdrListed= 1,
                    SkipCdrReceived= 1,
                    SkipCdrDecoded= 1,
                    SkipCdrBackedup= 1,
                    KeepDecodedCDR= 0,
                    KeepReceivedCdrServer= 1,
                    CcrCauseCodeField= 23,
                    SwitchTimeZoneId= null,
                    CallConnectIndicator= "CT",
                    FieldNoForTimeSummary= 29,
                    EnableSummaryGeneration= "1",
                    ExistingSummaryCacheSpanHr= 6,
                    BatchToDecodeRatio= 3,
                    PrependLocationNumberToFileName= 1
                }
            };

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
            DirectorySettings directorySetting = new DirectorySettings("c:/Telcobright");
            this.Tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaultS3 = new FileLocation()
            {
                Name = "Vault.S3",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = "\\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/CDR/DBL/dhkS3",
                User = "",
                Pass = "",
            };
            FileLocation appServerFtp1 = new FileLocation()
            {
                Name = "AppServerFTP1",//
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                ServerIp = "172.16.16.207",
                StartingPath = "Resources/CDR/DBL/dhkS3",
                User = "ftpuser",
                Pass = "Takay1takaane",
            };
            FileLocation appServerFtp2 = new FileLocation()
            {
                Name = "AppServerFTP2",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                ServerIp = "172.16.16.209",
                StartingPath = "Resources/CDR/DBL/dhkS3",
                User = "ftpuser",
                Pass = "Takay1takaane",
                Skip = true
            };
            //VAULT PART
            FileLocation s31 = new FileLocation()
            {
                Name = "S3_1",
                LocationType = "sftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = "/var/cdrs",
                Sftphostkey = "ssh-rsa 1024 0d:fd:ac:4a:67:05:e9:76:ef:a0:d2:c0:f9:1a:55:c1",
                ServerIp = "172.16.16.3",
                User = "billing",
                Pass = "DBLbill!@#$%",
                ExcludeBefore = new DateTime(2015, 6, 26, 0, 0, 0),
                IgnoreZeroLenghFile = 1
            };
            FileLocation s32 = new FileLocation()
            {
                Name = "S3_2",
                LocationType = "sftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = "/var/cdrs",
                Sftphostkey = "ssh-rsa 1024 0d:fd:ac:4a:67:05:e9:76:ef:a0:d2:c0:f9:1a:55:c1",
                ServerIp = "172.16.16.4",
                User = "billing",
                Pass = "DBLbill!@#$%",
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
            //    //sftphostkey = "ssh-rsa 1024 0d:fd:ac:4a:67:05:e9:76:ef:a0:d2:c0:f9:1a:55:c1",
            //    server = "172.16.16.210",
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
                ServerIp = "172.16.16.210", //server = "172.16.16.242",
                User = "ftpuser",
                Pass = "Takay1takaane",
                IgnoreZeroLenghFile = 1
            };

            //FileLocation FileArchiveIOF = new FileLocation()//raw cdr archive
            //{
            //    Name = "FileArchiveIOF",
            //    locationType = "ftp",
            //    OSType = "windows",
            //    pathSeparator = @"/",//backslash didn't work with winscp
            //    startingPath = @"/iof",
            //    server = "172.16.16.210", //server = "172.16.16.242",
            //    user = "ftpuser",
            //    pass = "Takay1takaane",
            //    IgnoreZeroLenghFile = 1
            //};

            FileLocation fileArchiveIof = new FileLocation()//raw cdr archive
            {
                Name = "FileArchiveIOF",
                LocationType = "ftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = "",
                ServerIp = "172.22.21.11",
                User = "dbl",
                Pass = "usr_dbl3!2",
                IgnoreZeroLenghFile = 1,
            };
            //add locations to directory settings



            //sync pair S3_1:Vault
            SyncPair s31Vault = new SyncPair("S3_1:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = s31
                },
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultS3
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "Downloaded",
                    ExpFileNameFilter = new SpringExpression(@"(Name.StartsWith('T') or Name.StartsWith('T'))
                                                                and
                                                                Name.EndsWith('.CDR')
                                                                and
                                                                Length>0
                                                                and
                                                                !Name.EndsWith('.tmp')")
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",//make sure when copying to vault always .tmp ext used
                    Overwrite = true,
                    ExpDestFileName = new SpringExpression(@"Name.Insert(0,'S3_1_')"),
                    CompressionType = CompressionType.None
                }
            };
            //sync pair S3_2:Vault
            SyncPair s32Vault = new SyncPair("S3_2:Vault")
            {
                SkipSourceFileListing = false,
                SkipCopyingToDestination=false,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = s32,
                    DescendingFileListByFileName = this.Tbc.CdrSetting.DescendingOrderWhileListingFiles
                },
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultS3
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "Downloaded",
                    ExpFileNameFilter = new SpringExpression(@"(Name.StartsWith('T') or Name.StartsWith('T'))
                                                                and
                                                                Name.EndsWith('.CDR')
                                                                and
                                                                Length>0
                                                                and
                                                                !Name.EndsWith('.tmp')")
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",//make sure when copying to vault always .tmp ext used
                    Overwrite = true,
                    ExpDestFileName = new SpringExpression(@"Name.Insert(0,'S3_2_')"),
                    CompressionType = CompressionType.None
                }
            };

            //sync pair Vault_S3:FileArchive1
            SyncPair vaultS3FileArchive1 = new SyncPair("Vault:FileArchive1")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing=false,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultS3
                },
                DstSyncLocation = new SyncLocation()
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
                    SubDirRule=new SyncSettingsDstSubDirectoryRule
                    (
                        DateWiseSubDirCreationType.ByFileName, 
                        new SpringExpression(@"Name.Substring(6,8)"), //"S3_2_" is appended at vault
                        "yyyyMMdd",
                        true
                    )
                }
            };
            //sync pair Vault_S3:IOF
            SyncPair vaultS3Iof = new SyncPair("Vault:IOF")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing=false,
                SrcSyncLocation = new SyncLocation()
                {
                    FileLocation = vaultS3
                },
                DstSyncLocation = new SyncLocation()
                {
                    FileLocation = fileArchiveIof
                },
                SrcSettings = new SyncSettingsSource()
                {
                    ExpFileNameFilter = null,//source filter not required, job created after newcdr for this pair
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".Incomplete",
                    Overwrite = true,
                    ExpDestFileName = new SpringExpression(@"'DBLSBC'+Name.Substring(3, 1) +Name.Substring(5)"),
                    CompressionType = CompressionType.None,
                    SubDirRule = null
                }
            };

            //add sync pairs to directory config
            directorySetting.SyncPairs.Add(s31Vault.Name, s31Vault);
            directorySetting.SyncPairs.Add(s32Vault.Name, s32Vault);
            directorySetting.SyncPairs.Add(vaultS3FileArchive1.Name, vaultS3FileArchive1);
            directorySetting.SyncPairs.Add(vaultS3Iof.Name, vaultS3Iof);
            //load the syncpairs in dictioinary, first by source
            //add archive locations to CdrSettings
            this.Tbc.CdrSetting.BackupSyncPairNames = new List<string>();
            this.Tbc.CdrSetting.BackupSyncPairNames.Add(vaultS3FileArchive1.Name);
            this.Tbc.CdrSetting.BackupSyncPairNames.Add(vaultS3Iof.Name);


            //configuration for server1
            Server serverConfig1 = new Server(1, "db01");
            //configuration for server2
            Server serverConfig2 = new Server(1, "db02");

            this.Tbc.ApplicationServersConfig.Add(serverConfig1);
            this.Tbc.ApplicationServersConfig.Add(serverConfig2);

            this.Tbc.DatabaseSetting = this.GetDatabaseConfigs();

            PortalSettings portalSetting = new PortalSettings("Portal Settings")
            {
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
                        SiteName = this.Tbc.Telcobrightpartner.CustomerName,
                        SiteId = 1,
                        PhysicalPath = "C:/inetpub/wwwroot/" + this.Tbc.Telcobrightpartner.CustomerName,
                        BindAddress = "0.0.0.0:80",
                        TemplateFileName = "../../" + this.Tbc.Telcobrightpartner.CustomerName+ "/tmplPortalWebSite.txt"
                    },
                    new InternetSite(this.Tbc)
                    {
                        SiteType = "ftp",
                        SiteName = this.Tbc.Telcobrightpartner.CustomerName,
                        SiteId = 1,
                        PhysicalPath = "C:/sftp_root",
                        BindAddress = "0.0.0.0:21",
                        TemplateFileName = "../../" + this.Tbc.Telcobrightpartner.CustomerName + "/tmplPortalFtpSite.txt"
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
            
            //write config files
            //string templateFileName = Directory.GetParent((Directory.GetParent(Directory.GetCurrentDirectory())).FullName).FullName + Path.DirectorySeparatorChar + "tmpl_AllConfig.json";
            this.Tbc.PortalSettings = portalSetting;
            return this.Tbc;
        }
}
}
