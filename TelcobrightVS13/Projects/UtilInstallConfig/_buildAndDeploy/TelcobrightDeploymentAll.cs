using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstallConfig._generator;
namespace InstallConfig
{
    public static class TelcobrightDeploymentAll
    {
        public static List<Deploymentprofile> getDeploymentprofiles()
        {
            TelcobrightSeed seed= new TelcobrightSeed();
            return new List<Deploymentprofile>()
            {
                new Deploymentprofile
                {
                    profileName = "cas",
                    type = "telcobilling",
                    instances = new List<InstanceConfig>
                    {
                        new InstanceConfig
                        {
                            name = "srtelecom",
                            otherParams = new Dictionary<string, string>
                            {
                                {"schedulerPortNo", seed.getNextSchedulerPort()}
                            }
                        },
                        new InstanceConfig
                        {
                            name = "summit",
                            otherParams = new Dictionary<string, string>
                            {
                                {"schedulerPortNo", seed.getNextSchedulerPort()}
                            }
                        }
                    }
                },
                new Deploymentprofile
                {
                    profileName = "srtelecom",
                    type = "telcobilling",
                    instances = new List<InstanceConfig>
                    {
                        new InstanceConfig
                        {
                            name = "srtelecom",
                            otherParams = new Dictionary<string, string>
                            {
                                {"schedulerPortNo", seed.getNextSchedulerPort()}
                            }
                        },
                    }
                },
                new Deploymentprofile
                {
                    profileName = "srtelecom",
                    type = "telcobilling",
                    instances = new List<InstanceConfig>
                    {
                        new InstanceConfig
                        {
                            name = "srtelecom",
                            otherParams = new Dictionary<string, string>
                            {
                                {"schedulerPortNo", seed.getNextSchedulerPort()}
                            }
                        },
                    }
                },
                new Deploymentprofile
                {
                    profileName = "summit",
                    type = "telcobilling",
                    instances = new List<InstanceConfig>
                    {
                        new InstanceConfig
                        {
                            name = "summit",
                            otherParams = new Dictionary<string, string>()
                        },
                    }
                }
            };
        }
    }
}
