using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using LibraryExtensions.ConfigHelper;
using TelcobrightFileOperations;
using TelcobrightMediation;
using TelcobrightMediation.Config;
using TelcobrightMediation.Scheduler.Quartz;

namespace InstallConfig
{
    [Export(typeof(IConfigGenerator))]
    public partial class RootsConfigGenerator:IConfigGenerator
    {
        public string OperatorName { get;}
        public TelcobrightConfig Tbc { get; }

        public RootsConfigGenerator()
        {
            int thisServerId = 1;
            this.OperatorName = "roots";
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Igw, thisServerId);
        }
        public TelcobrightConfig GenerateConfig(DatabaseSetting schedulerDatabaseSetting)
        {
            if (string.IsNullOrWhiteSpace(this.OperatorName))
                throw new Exception("Operator name not configured in Config Generator");
            this.Tbc.CdrSetting = new CdrSetting()
            {
                SummaryTimeField = SummaryTimeFieldEnum.StartTime,
                DescendingOrderWhileListingFiles = false,
                DescendingOrderWhileProcessingListedFiles = false,
                IllegalStrToRemoveFromFields = new List<string>()
                {
                    "\\",
                }
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
                ApplicationRootDirectory = "e:/Telcobright"
            };
            this.Tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaultSansay = new FileLocation()
            {
                Name = "Vault.Sansay",//this is refered in ne table
                LocationType = "vault",
                OsType = "windows",
                PathSeparator = "\\",
                ServerIp = "",
                StartingPath = "E:/telcobright/Vault/Resources/CDR/roots/dhkSansay",
                User = "",
                Pass = "",
            };
            FileLocation appServerFtp1 = new FileLocation()
            {
                Name = "AppServerFTP1",//
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                ServerIp = "10.33.40.26",
                StartingPath = "E:/telcobright/Vault/Resources/CDR/roots/dhkSansay",
                User = "ftpuser",
                Pass = "Takay1takaane",
            };
            FileLocation appServerFtp2 = new FileLocation()
            {
                Name = "AppServerFTP2",//
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                ServerIp = "10.33.40.27",
                StartingPath = "E:/telcobright/Vault/Resources/CDR/roots/dhkSansay",
                User = "ftpuser",
                Pass = "Takay1takaane",
                Skip = true
            };
            //VAULT PART
            List<FileLocation> ftpLocations = new List<FileLocation>();
            ftpLocations.Add(appServerFtp1);
            ftpLocations.Add(appServerFtp2);
            Vault sansayVault = new Vault("Vault.Sansay", this.Tbc, ftpLocations);
            sansayVault.LocalLocation = new SyncLocation(vaultSansay.Name) { FileLocation = vaultSansay };//don't pass this to constructor and set there, causes problem in json serialize
            this.Tbc.Vaults.Add(sansayVault);
            FileLocation sansay = new FileLocation()
            {
                Name = "Sansay",
                LocationType = "ftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = "/2tbspace/TELCOBRITE",
                ServerIp = "10.33.40.20",
                User = "telcobrite",
                Pass = ".E21..11325yy",
                IgnoreZeroLenghFile = 0
            };
            
            //FileLocation FileArchive1 = new FileLocation()//raw cdr archive
            //{
            //    Name = "FileArchive1",
            //    locationType = "ftp",
            //    OSType = "windows",
            //    pathSeparator = "/",
            //    startingPath = "/var/cdrs/downloaded",
            //    server = "192.168.4.179", //server = "172.16.16.242",
            //    user = "ftpuser",
            //    pass = "Takay1takaane",
            //    IgnoreZeroLenghFile = 1
            //};
            //FileLocation FileArchiveBTRC = new FileLocation()//raw cdr archive
            //{
            //    Name = "FileArchiveBTRC",
            //    locationType = "ftp",
            //    OSType = "linux",
            //    pathSeparator = "/",
            //    startingPath = "/var/cdrs/downloaded",
            //    server = "192.168.4.180",
            //    user = "ftpuser",
            //    pass = "Takay1takaane",
            //    IgnoreZeroLenghFile = 1
            //};
            //add locations to directory settings
            this.Tbc.DirectorySettings.FileLocations.Add(vaultSansay.Name, vaultSansay);
            this.Tbc.DirectorySettings.FileLocations.Add(appServerFtp1.Name, appServerFtp1);
            this.Tbc.DirectorySettings.FileLocations.Add(appServerFtp2.Name, appServerFtp2);
            this.Tbc.DirectorySettings.FileLocations.Add(sansay.Name, sansay);
            //Tbc.directorySettings.fileLocations.Add(S3_2.Name, S3_2);
            //Tbc.directorySettings.fileLocations.Add(FileArchive1.Name, FileArchive1);
            //Tbc.directorySettings.fileLocations.Add(FileArchiveBTRC.Name, FileArchiveBTRC);



            //sync pair S3_1:Vault
            SyncPair Sansay_Vault = new SyncPair("Sansay:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation("Sansay")
                {
                    FileLocation = sansay
                },
                DstSyncLocation = new SyncLocation("Vault_Sansay")
                {
                    FileLocation = vaultSansay
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "",
                    ExpFileNameFilter = new SpringExpression(@"Name.StartsWith('2')
                                                                and
                                                                Name.EndsWith('.cdr')
                                                                and
                                                                Length>0
                                                                and
                                                                !Name.EndsWith('.tmp')")
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",//make sure when copying to vault always .tmp ext used
                    Overwrite = true,
                    ExpDestFileName = new SpringExpression(""),
                    CompressionType = CompressionType.None
                }
            };
            
            directorySetting.SyncPairs.Add(Sansay_Vault.Name, Sansay_Vault);
            
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
            //configuration for server1
            ApplicationServerConfig serverConfig1 = new ApplicationServerConfig(this.Tbc) { ServerId = 1, OwnIpAddress = "10.33.40.26" };
            //configuration for server2
            //ApplicationServerConfig ServerConfig2 = new ApplicationServerConfig(Tbc) { ServerId = 2 };

            this.Tbc.ApplicationServersConfig.Add(serverConfig1.ServerId.ToString(), serverConfig1);
            //Tbc.applicationServersConfig.Add(ServerConfig2.ServerId.ToString(), ServerConfig1);
            DatabaseSetting databaseSetting = schedulerDatabaseSetting.GetCopy();
            databaseSetting.DatabaseName = "roots";
            this.Tbc.DatabaseSetting = databaseSetting;

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
                        SiteName = databaseSetting.DatabaseName,
                        SiteId = 1,
                        PhysicalPath = "C:/inetpub/wwwroot/" + this.Tbc.DatabaseSetting.DatabaseName,
                        BindAddress = this.Tbc.DirectorySettings.FileLocations["AppServerFTP" + this.Tbc.ServerId].ServerIp + ":80",
                        TemplateFileName = "../../" + this.Tbc.DatabaseSetting.DatabaseName + "/tmplPortalWebSite.txt"
                    },
                    new InternetSite(this.Tbc)
                    {
                        SiteType = "ftp",
                        SiteName = databaseSetting.DatabaseName,
                        SiteId = 1,
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
                                        "durationsec as `Actual Duration`",
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
                                        "c.field4 as `Error Reason`",
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
                                        "durationsec as `Actual Duration`",
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
                                       "c.field4 as `Error Reason`",
                                       "c.*"
                                   }
                           }
                        }//list of cdr templates
                    }//one config object
                }
            };
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
                            "GridView1.Columns[19].Visible=false",//CCR
                        },
                        SpringExpressionIfNotRole = new List<string>()
                        {
                            "CheckBoxShowCost.Enabled=false",
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
                                            "nodes['Settings'].Expanded=true"
                                       },
                               SpringExpressionIfNotRole = new List<string>()
                                       {
                                            "nodes['Configuration'].Expanded=false",
                                            "nodes['Mediation'].Expanded=false",
                                            "nodes['Settings'].Expanded=false"
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
