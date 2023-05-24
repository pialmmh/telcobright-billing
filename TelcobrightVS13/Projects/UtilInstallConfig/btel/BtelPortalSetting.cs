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
using System.IO;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;
using Newtonsoft.Json;
using QuartzTelcobright;
using TelcobrightMediation.Config;

namespace InstallConfig
{
    public partial class BtelAbstractConfigConfigGenerator //quartz config part
    {
        //static string databaseConfigFileName = new DirectoryInfo(FileAndPathHelper.GetBinPath()).Parent.Parent.FullName
        //                                       + Path.DirectorySeparatorChar + "Server.conf";
        public PortalSettings GetPortalSettings(TelcobrightConfig tbc)
        {
            string portalLocalAccountNameAdministrator = "Administrator";
            string portalLocalAccountPassword = "Takay1#$ane%%";
            PortalSettings portalSetting = new PortalSettings("Portal Settings")
            {
                PortalLocalAccountNameAdministrator = portalLocalAccountNameAdministrator,
                PortalLocalAccountPassword = portalLocalAccountPassword,
                HomePageUrl = "~/Dashboard.aspx",
                AlternateDisplayName = "IGW Manager",
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
                        SiteName = tbc.Telcobrightpartner.CustomerName,
                        SiteId = 1,
                        PhysicalPath = "C:/inetpub/wwwroot/" + this.Tbc.Telcobrightpartner.CustomerName,
                        BindAddress = this.Tbc.DirectorySettings.FileLocations["AppServerFTP" + this.Tbc.ServerId].ServerIp + ":80",
                        TemplateFileName = "../../" + this.Tbc.Telcobrightpartner.CustomerName + "/tmplPortalWebSite.txt",
                        ApplicationPool=new IisApplicationPool()
                        {
                            AppPoolName = this.Tbc.Telcobrightpartner.CustomerName,
                            TemplateFileName = "../../" + this.Tbc.Telcobrightpartner.CustomerName + "/tmplPortalAppPools.txt",
                        },
                        ImpersonateUserName =portalLocalAccountNameAdministrator,
                        ImpersonatePassword =portalLocalAccountPassword
                    },
                   
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
                                    "c.InPartnerId",
                                    "c.OutPartnerId",
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
                            "CheckBoxShowCost.Checked=true",
                            "GridView1.Columns[7].Visible=true",//actual duration
                            "GridView1.Columns[9].Visible=true",//duration1
                            "GridView1.Columns[6].Visible=true",//Connect count
                            "GridView1.Columns[19].Visible=true",//CCR
                            "GridView1.Columns[20].Visible=false",//Connect count by cc
                            "GridView1.Columns[21].Visible=true",//CCR by cc
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
                            "GridView1.Columns[21].Visible=true",//CCR by cc
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
                            "GridView1.Columns[8].Visible=false",//connected calls
                            "GridView1.Columns[9].Visible=false",//actual duration
                            "GridView1.Columns[12].Visible=true",//supplier duration
                            "GridView1.Columns[16].Visible=false",//CCR
                            "GridView1.Columns[17].Visible=false",//connect by cc
                            "GridView1.Columns[18].Visible=true",//CCR by cc
                        },
                        SpringExpressionIfNotRole = new List<string>()
                        {
                            "CheckBoxShowCost.Enabled=false",
                            "GridView1.Columns[8].Visible=false",//connected calls
                            "GridView1.Columns[9].Visible=false",//actual duration
                            "GridView1.Columns[12].Visible=false",//supplier duration
                            "GridView1.Columns[16].Visible=false",//CCR
                            "GridView1.Columns[17].Visible=false",//connect by cc
                            "GridView1.Columns[18].Visible=true",//CCR by cc
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
                                    "nodes['Billing'].Expanded=true",
                                    "nodes['Mediation'].Expanded=true",
                                    "nodes['Settings'].Expanded=true",
                                    "nodes['Billing Reports'].Expanded=true",
                                    "nodes['Reports/ICX'].Expanded=false",
                                    "nodes['Reports/IGW'].Expanded=true",
                                    "nodes['Reports/Transit'].Expanded=false"
                                },
                                SpringExpressionIfNotRole = new List<string>()
                                {
                                    "nodes['Configuration'].Expanded=false",
                                    "nodes['Billing'].Expanded=false",
                                    "nodes['Mediation'].Expanded=false",
                                    "nodes['Settings'].Expanded=false",
                                    "nodes['Billing Reports'].Expanded=false",
                                    "nodes['Reports/ICX'].Expanded=false",
                                    "nodes['Reports/IGW'].Expanded=true",
                                    "nodes['Reports/Transit'].Expanded=false"
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
                                    "nodes['Billing'].Expanded=true",
                                    "nodes['Mediation'].Expanded=true",
                                    "nodes['Settings'].Expanded=true",
                                    "nodes['Billing Reports'].Expanded=true",
                                    "nodes['Reports/ICX'].Expanded=false",
                                    "nodes['Reports/IGW'].Expanded=true",
                                    "nodes['Reports/Transit'].Expanded=false",
                                    "nodes['Settings/Manage Users'].Expanded=false"
                                },
                                SpringExpressionIfNotRole = new List<string>()
                                {
                                    "nodes['Configuration'].Expanded=false",
                                    "nodes['Billing'].Expanded=false",
                                    "nodes['Mediation'].Expanded=false",
                                    "nodes['Settings'].Expanded=false",
                                    "nodes['Billing Reports'].Expanded=false",
                                    "nodes['Reports/ICX'].Expanded=false",
                                    "nodes['Reports/IGW'].Expanded=true",
                                    "nodes['Reports/Transit'].Expanded=false"
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
                },//dictionary of page settings

            };
            return portalSetting;
        }
    }
}
