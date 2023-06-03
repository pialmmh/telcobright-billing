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
using TelcobrightMediation.Automation;
using TelcobrightMediation.Config;

namespace InstallConfig
{
    public partial class BtelAbstractConfigConfigGenerator //quartz config part
    {
        public override List<Server> GetApplicationServerConfigs()
        {
            return
                base.ValidateInstances(new List<Server>
                {
                    new Server
                    (
                        serverId: 1,
                        ipAddresses: new List<IpAddress>() { new IpAddress {Address = "10.0.0.5" } } 
                    ),
                    new Server
                    (
                        serverId: 2,
                        ipAddresses: new List<IpAddress>() { new IpAddress {Address = "10.0.0.7" } }
                    ),
                    new Server
                    (
                        serverId: 3,
                        ipAddresses: new List<IpAddress>() { new IpAddress {Address = "10.0.0.9" } }
                    ),
                    new Server
                    (
                        serverId: 4,
                        ipAddresses: new List<IpAddress>() { new IpAddress {Address = "10.0.0.11" } }
                    )
                });
        }
    }
}
