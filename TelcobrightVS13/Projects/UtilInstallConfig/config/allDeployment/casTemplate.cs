using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstallConfig._generator;
using TelcobrightInfra;

namespace InstallConfig
{
    public static partial class AllDeploymenProfiles
    {
        public static List<Deploymentprofile> getLinux01DeploymentProfile()
        {
            TelcobrightSeed seed = new TelcobrightSeed();
            List<Deploymentprofile> allProfiles =
                new List<Deploymentprofile>
                {
                    
                };

            var deploymentprofile = new Deploymentprofile
            {
                profileName = "cas",
                type = DeploymentProfileType.TelcoBilling,
                UserVsDbName = new Dictionary<string, string>()
                {
                    {"admin@telcobright.com", "btrc_cas"},
                    {"btrcadmin@telcobright.com", "btrc_cas"},
                    {"btrc@telcobright.com", "btrc_cas"},
                    //{"agni@telcobright.com", "agni_cas"},
                    //{"banglatelecom@telcobright.com", "banglatelecom_cas"},
                    {"banglaicx@telcobright.com", "banglaicx_cas"},
                    //{"bantel@telcobright.com", "bantel_cas"},
                    //{"gazinetworks@telcobright.com", "gazinetworks_cas"},
                    //{"imamnetwork@telcobright.com", "imamnetwork_cas"},
                    {"jibondhara@telcobright.com", "jibondhara_cas"},
                    {"mnh@telcobright.com", "mnh_cas"},
                    //{"btcl@telcobright.com", "btcl_cas"},
                    //{"paradise@telcobright.com", "paradise_cas"},
                    //{"purple@telcobright.com", "purple_cas"},
                    //{"ringtech@telcobright.com", "ringtech_cas"},
                    //{"crossworld@telcobright.com", "crossworld_cas"},
                    {"srtelecom@telcobright.com", "srtelecom_cas"},
                    {"sheba@telcobright.com", "sheba_cas"},
                    //{"softex@telcobright.com", "softex_cas"},
                    {"teleexchange@telcobright.com", "teleexchange_cas"},
                    {"newgenerationtelecom@telcobright.com", "newgenerationtelecom_cas"},
                    //{"mothertelecom@telcobright.com", "mothertelecom_cas"},
                    //{"teleplusnewyork@telcobright.com", "teleplusnewyork_cas"},
                    //{"summit@telcobright.com", "summit_cas"},
                    //{"voicetel@telcobright.com", "voicetel_cas"}
                },
                instances = new List<InstanceConfig>
                {
                    new InstanceConfig
                    {
                        Name = "btrc_cas",
                        SchedulerPortNo = 572,
                    },
                    new InstanceConfig
                    {
                        Name = "srtelecom_cas",
                        SchedulerPortNo = 570,
                    },
                    new InstanceConfig
                    {
                        Name = "summit_cas",
                        SchedulerPortNo = 571,
                    },
                    new InstanceConfig
                    {
                        Name = "jibondhara_cas",
                        SchedulerPortNo = 573
                    },
                    new InstanceConfig
                    {
                        Name = "purple_cas",
                        SchedulerPortNo = 574,
                    },
                    new InstanceConfig
                    {
                        Name = "agni_cas",
                        SchedulerPortNo = 575,
                    },
                    new InstanceConfig
                    {
                        Name = "gazinetworks_cas",
                        SchedulerPortNo = 576,
                    },
                    new InstanceConfig
                    {
                        Name = "mothertelecom_cas",
                        SchedulerPortNo = 577
                    },
                    new InstanceConfig
                    {
                        Name = "banglatelecom_cas",
                        SchedulerPortNo = 578
                    },
                    new InstanceConfig
                    {
                        Name = "crossworld_cas",
                        SchedulerPortNo = 579
                    },
                    new InstanceConfig
                    {
                        Name = "bantel_cas",
                        SchedulerPortNo = 580
                    },
                    new InstanceConfig
                    {
                        Name = "teleexchange_cas",
                        SchedulerPortNo = 581
                    },
                    new InstanceConfig
                    {
                        Name = "ringtech_cas",
                        SchedulerPortNo = 582
                    },
                    new InstanceConfig
                    {
                        Name = "voicetel_cas",
                        SchedulerPortNo = 583
                    },
                    new InstanceConfig
                    {
                        Name = "mnh_cas",
                        SchedulerPortNo = 584
                    },
                    new InstanceConfig
                    {
                        Name = "softex_cas",
                        SchedulerPortNo = 585
                    },
                    new InstanceConfig
                    {
                        Name = "imamnetwork_cas",
                        SchedulerPortNo = 586
                    },
                    new InstanceConfig
                    {
                        Name = "teleplusnewyork_cas",
                        SchedulerPortNo = 587
                    },
                    new InstanceConfig
                    {
                        Name = "sheba_cas",
                        SchedulerPortNo = 588
                    },
                    new InstanceConfig
                    {
                        Name = "paradise_cas",
                        SchedulerPortNo = 589
                    },
                    new InstanceConfig
                    {
                        Name = "banglaicx_cas",
                        SchedulerPortNo = 590
                    },
                    new InstanceConfig
                    {
                        Name = "btcl_cas",
                        SchedulerPortNo = 591
                    },
                    new InstanceConfig
                    {
                        Name = "newgenerationtelecom_cas",
                        SchedulerPortNo = 592
                    }
                    
                },
                MySqlCluster = new MySqlCluster
                {
                    Master = new MySqlServer("linux01docker20")
                    {
                        MySqlVersion = MySqlVersion.MySql57,
                        BindAddressForAutomation = new BindAddress
                        {
                            IpAddressOrHostName = new IpAddressOrHostName {Address = "localhost"},//10.12.1.1
                            Port = 3306
                        },
                        RootUserForAutomation = "btrc",
                        RootPasswordForAutomation = "Takay1takaane",
                        Users = new List<MySqlUser>()
                        {
                            new MySqlUser(username: CasConfigHelper.Db.AdminUserName,
                                password: CasConfigHelper.Db.AdminPassword,
                                hostnameOrIpAddresses: new List<string> {"localhost", "10.0.0.29"},
                                permissions: new List<MySqlPermission>
                                {
                                    new MySqlPermission(
                                        new List<MySqlPermissionType> {MySqlPermissionType.all,}, "*"),
                                }),
                            new MySqlUser(username: "dbreader",
                                password: "Takay1takaane",
                                hostnameOrIpAddresses: new List<string> {"localhost", "10.0.0.29"},
                                permissions: new List<MySqlPermission>
                                {
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "agni_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "banglaicx_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "banglatelecom_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "bantel_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "btcl_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "btrc_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "crossworld_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "gazinetworks_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "getco_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "imamnetwork_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "jibondhara_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "mmcommunications_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "mnh_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "mothertelecom_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "newgenerationtelecom_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "paradise_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "purple_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "ringtech_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "sheba_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "softex_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "srtelecom_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "summit_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "teleexchange_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "teleplusnewyork_cas"),
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.execute,
                                            MySqlPermissionType.@select
                                        }, "voicetel_cas")
                                })
                        }
                    },
                    Slaves = new List<MySqlServer>()
                }
            };
            
            return allProfiles;
        }
    }
}

