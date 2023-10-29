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
        public static List<Deploymentprofile> getDeploymentprofiles()
        {
            TelcobrightSeed seed = new TelcobrightSeed();
            List<Deploymentprofile> allProfiles =
                new List<Deploymentprofile>
                {
                    new Deploymentprofile
                    {
                        profileName = "srtelecom",
                        type = DeploymentProfileType.TelcoBilling,
                        instances = new List<InstanceConfig>
                        {
                            new InstanceConfig
                            {
                                Name = "srtelecom",
                                SchedulerPortNo = 555,
                            },
                        }
                    },
                    new Deploymentprofile
                    {
                        profileName = "summit",
                        type = DeploymentProfileType.TelcoBilling,
                        instances = new List<InstanceConfig>
                        {
                            new InstanceConfig
                            {
                                Name = "summit",
                                SchedulerPortNo = 556
                            },
                        },
                    },
                    new Deploymentprofile
                    {
                        profileName = "banglatelecom",
                        type = DeploymentProfileType.TelcoBilling,
                        instances = new List<InstanceConfig>
                        {
                            new InstanceConfig
                            {
                                Name = "banglatelecom",
                                SchedulerPortNo = 557
                            },
                        }
                    },
                    new Deploymentprofile
                    {
                        profileName = "jsl",
                        type = DeploymentProfileType.TelcoBilling,
                        instances = new List<InstanceConfig>
                        {
                            new InstanceConfig
                            {
                                Name = "jsl",
                                SchedulerPortNo = 558
                            },
                        }
                    },
                    new Deploymentprofile
                    {
                        profileName = "purple",
                        type = DeploymentProfileType.TelcoBilling,
                        instances = new List<InstanceConfig>
                        {
                            new InstanceConfig
                            {
                                Name = "purple",
                                SchedulerPortNo = 559
                            },
                        }
                    },
                    new Deploymentprofile
                    {
                        profileName = "btel",
                        type = DeploymentProfileType.TelcoBilling,
                        instances = new List<InstanceConfig>
                        {
                            new InstanceConfig
                            {
                                Name = "btel",
                                SchedulerPortNo = 560
                            },
                        }
                    },
                    new Deploymentprofile
                    {
                        profileName = "dbl",
                        type = DeploymentProfileType.TelcoBilling,
                        instances = new List<InstanceConfig>
                        {
                            new InstanceConfig
                            {
                                Name = "dbl",
                                SchedulerPortNo = 561
                            },
                        }
                    }
                };

            var casProfile = new Deploymentprofile
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
            };
            Dictionary<string,MySqlCluster> mySqlClusters =new Dictionary<string, MySqlCluster>();
            mySqlClusters.Add("AgniBanglatelBantel", AllDeploymenProfiles.getAgniBanglatelBantel());
            mySqlClusters.Add("NewSoftShebaTeleex",getNewSoftShebaTeleex());
            mySqlClusters.Add("MotherTelePlusVoice",getMotherTelePlusVoice());
            mySqlClusters.Add("MnhSrtel",getMnhSrtel());
            mySqlClusters.Add("JibonParaPurple",getJibonParaPurple());
            mySqlClusters.Add("GaziSummitRing",getGaziSummitRing());
            mySqlClusters.Add("BanIcxCross",getBanIcxCross());
            mySqlClusters.Add("BTCLImam",getBTCLImam());


            casProfile.MySqlClusters = mySqlClusters;
            allProfiles.Add(casProfile);
            var tcpPortWisecount = allProfiles.SelectMany(p => p.instances.Select(i => i.SchedulerPortNo))
                .GroupBy(portNo => portNo)
                .Select(g => new
                {
                    tcpPort = g.Key,
                    count = g.Count()
                }).Where(a => a.count > 1).ToList();

            foreach (var a in tcpPortWisecount)
            {
                if (a.count > 1)
                {
                    throw new Exception("Scheduler port numbers must be unique, duplicate port no: " + a.tcpPort);
                }
            }
            var profileNameCount= allProfiles.Select(p=>p.profileName)
                .GroupBy(name=> name)
                .Select(g => new
                {
                    name=g.Key,
                    count = g.Count()
                }).Where(a => a.count > 1);

            foreach (var a in profileNameCount)
            {
                if (a.count > 1)
                {
                    throw new Exception("Profile names must be unique, duplicate name: " + a.name);
                }
            }

            var instanceOrDatabaseName = allProfiles.SelectMany(p => p.instances.Select(i=>i.Name))
                .GroupBy(name => name)
                .Select(g => new
                {
                    name = g.Key,
                    count = g.Count()
                }).Where(a => a.count > 1);

            foreach (var a in instanceOrDatabaseName)
            {
                if (a.count > 1)
                {
                    throw new Exception("Instance or database names must be unique, duplicate name: " + a.name);
                }
            }
            return allProfiles;
        }
    }
}

