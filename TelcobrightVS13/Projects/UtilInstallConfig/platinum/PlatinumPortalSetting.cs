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
using TelcobrightMediation.Config;

namespace InstallConfig
{
    public partial class PlatinumConfigGenerator //quartz config part
    {
        public PortalSettings GetPortalSettings(TelcobrightConfig tbc)
        {
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
                        SiteName = tbc.DatabaseSetting.DatabaseName,
                        SiteId = 1,
                        PhysicalPath = "C:/inetpub/wwwroot/" + this.Tbc.DatabaseSetting.DatabaseName,
                        BindAddress = this.Tbc.DirectorySettings.FileLocations["AppServerFTP" + this.Tbc.ServerId].ServerIp + ":80",
                        TemplateFileName = "../../" + this.Tbc.DatabaseSetting.DatabaseName + "/tmplPortalWebSite.txt",
                        ApplicationPool=new IisApplicationPool()
                        {
                            AppPoolName = this.Tbc.DatabaseSetting.DatabaseName,
                            TemplateFileName = "../../" + this.Tbc.DatabaseSetting.DatabaseName + "/tmplPortalAppPools.txt",
                        },
                        ImpersonateUserName="Mustafa",
                        ImpersonatePassword="Habib321"
                    },
                    new InternetSite(this.Tbc)
                    {
                        SiteType = "ftp",
                        SiteName = tbc.DatabaseSetting.DatabaseName,
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
                                    "c.InPartnerId",
                                    "c.OutPartnerId",
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
                            "CheckBoxShowCost.Checked=true",
                            "GridView1.Columns[7].Visible=true",//actual duration
                            "GridView1.Columns[9].Visible=true",//duration1
                            "GridView1.Columns[6].Visible=true",//Connect count
                            "GridView1.Columns[19].Visible=true",//CCR
                            "GridView1.Columns[20].Visible=false",//Connect count by cc
                            "GridView1.Columns[21].Visible=false",//CCR by cc
                        },
                        SpringExpressionIfNotRole = new List<string>()
                        {
                            "CheckBoxShowCost.Enabled=false",
                            "GridView1.Columns[6].Visible=true",//Connect count
                            "GridView1.Columns[7].Visible=false",
                            "GridView1.Columns[9].Visible=false",
                            "GridView1.Columns[8].HeaderText=Duration",
                            "GridView1.Columns[19].Visible=true",//CCR
                            "GridView1.Columns[20].Visible=false",//Connect count by cc
                            "GridView1.Columns[21].Visible=false",//CCR by cc
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
                            "GridView1.Columns[6].Visible=true",//connect count
                            "GridView1.Columns[8].Visible=true",//duration1
                            "GridView1.Columns[19].Visible=true",//CCR
                            "GridView1.Columns[20].Visible=false",//connect by cc
                            "GridView1.Columns[21].Visible=false",//CCR by cc
                        },
                        SpringExpressionIfNotRole = new List<string>()
                        {
                            "CheckBoxShowCost.Enabled=false",
                            "GridView1.Columns[6].Visible=true",
                            "GridView1.Columns[8].Visible=false",
                            "GridView1.Columns[7].HeaderText=Duration",
                            "GridView1.Columns[19].Visible=true",//CCR
                            "GridView1.Columns[20].Visible=false",//connect by cc
                            "GridView1.Columns[21].Visible=false",//CCR by cc
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
                            "GridView1.Columns[8].Visible=true",//connected calls
                            "GridView1.Columns[9].Visible=true",//actual duration
                            "GridView1.Columns[12].Visible=true",//supplier duration
                            "GridView1.Columns[16].Visible=true",//CCR
                            "GridView1.Columns[17].Visible=false",//connect by cc
                            "GridView1.Columns[18].Visible=false",//CCR by cc
                        },
                        SpringExpressionIfNotRole = new List<string>()
                        {
                            "CheckBoxShowCost.Enabled=false",
                            "GridView1.Columns[8].Visible=true",//connected calls
                            "GridView1.Columns[9].Visible=false",//actual duration
                            "GridView1.Columns[12].Visible=false",//supplier duration
                            "GridView1.Columns[16].Visible=true",//CCR
                            "GridView1.Columns[17].Visible=false",//connect by cc
                            "GridView1.Columns[18].Visible=false",//CCR by cc
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
                                    "admin"
                                },
                                SpringExpressionIfRole = new List<string>()
                                {
                                    "nodes['Configuration'].Expanded=true",
                                    "nodes['Mediation'].Expanded=true",
                                    "nodes['Settings'].Expanded=true",
                                    "nodes['Billing Reports'].Expanded=true",
                                    "nodes['Reports/ICX'].Expanded=false",
                                    "nodes['Reports/IGW'].Expanded=true"
                                },
                                SpringExpressionIfNotRole = new List<string>()
                                {
                                    "nodes['Configuration'].Expanded=false",
                                    "nodes['Mediation'].Expanded=false",
                                    "nodes['Settings'].Expanded=false",
                                    "nodes['Billing Reports'].Expanded=false",
                                    "nodes['Reports/ICX'].Expanded=false",
                                    "nodes['Reports/IGW'].Expanded=true"
                                }
                            },
                            new SettingByRoles()
                            {
                                RoleNames = new List<string>()
                                {
                                    "billing"
                                },
                                SpringExpressionIfRole = new List<string>()
                                {
                                    "nodes['Configuration'].Expanded=true",
                                    "nodes['Mediation'].Expanded=true",
                                    "nodes['Settings'].Expanded=true",
                                    "nodes['Billing Reports'].Expanded=true",
                                    "nodes['Reports/ICX'].Expanded=false",
                                    "nodes['Reports/IGW'].Expanded=true",
                                    "nodes['Settings/Manage Users'].Expanded=false"
                                },
                                SpringExpressionIfNotRole = new List<string>()
                                {
                                    "nodes['Configuration'].Expanded=false",
                                    "nodes['Mediation'].Expanded=false",
                                    "nodes['Settings'].Expanded=false",
                                    "nodes['Billing Reports'].Expanded=false",
                                    "nodes['Reports/ICX'].Expanded=false",
                                    "nodes['Reports/IGW'].Expanded=true"
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
                    },
                    { "~/reports/icx/InternationalIn_ICX.aspx",//settings for report pages
                        settingIntlIn
                    },
                    { "~/reports/icx/InternationalOut _ICX.aspx",
                        settingIntlOut
                    },
                    { "~/reports/icx/Domestic.aspx",
                        settingIntlIn
                    }
                },//dictionary of page settings

            };
            return portalSetting;
        }
    }
}
