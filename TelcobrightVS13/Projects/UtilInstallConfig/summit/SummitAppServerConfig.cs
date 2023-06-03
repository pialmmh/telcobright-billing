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
    public partial class SummitAbstractConfigConfigGenerator //quartz config part
    {
        public override List<Server> GetApplicationServerConfigs()
        {
            return
                base.ValidateInstances(new List<Server>
                {
                    new Server
                    (
                        serverId: 1,
                        ipAddresses: new List<IpAddress>() { new IpAddress {Address = "172.18.0.3" } }
                    ),
                    new Server
                    (
                        serverId: 2,
                        ipAddresses: new List<IpAddress>() { new IpAddress {Address = "172.18.0.5" } }
                    )
                });
        }
    }
}


