using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstallConfig._generator;
using TelcobrightInfra;

namespace InstallConfig
{
    public static class AllDeploymenProfiles
    {
        public static List<Deploymentprofile> getDeploymentprofiles()
        {
            TelcobrightSeed seed = new TelcobrightSeed();
            List<Deploymentprofile> allProfiles =
                new List<Deploymentprofile>()
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
                        }
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
                    },
                    new Deploymentprofile
                    {
                        profileName = "cas",
                        type = DeploymentProfileType.TelcoBilling,
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
                                Skip =  true
                            },
                            new InstanceConfig
                            {
                                Name = "summit_cas",
                                SchedulerPortNo = 571,
                                Skip =  true
                            },
                            new InstanceConfig
                            {
                                Name = "jibondhara_cas",
                                SchedulerPortNo = 573,
                                Skip =  true
                            },
                            new InstanceConfig
                            {
                                Name = "purple_cas",
                                SchedulerPortNo = 574,
                                Skip =  true
                            },
                            new InstanceConfig
                            {
                                Name = "agni_cas",
                                SchedulerPortNo = 575,
                                Skip =  true
                            }
                        }
                    },
                };
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

