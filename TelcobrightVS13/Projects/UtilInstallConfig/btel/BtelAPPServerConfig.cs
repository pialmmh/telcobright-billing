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
        public override List<ApplicationServerConfig> GetApplicationServerConfigs()
        {
            return
                base.ValidateInstances(new List<ApplicationServerConfig>
                {
                    new ApplicationServerConfig
                    (
                        serverId: 1,
                        ownIpAddress: "10.0.0.5"
                    ),
                    new ApplicationServerConfig
                    (
                        serverId: 2,
                        ownIpAddress: "10.0.0.7"
                    ),
                    new ApplicationServerConfig
                    (
                        serverId: 3,
                        ownIpAddress: "10.0.0.9"
                    ),
                    new ApplicationServerConfig
                    (
                        serverId: 4,
                        ownIpAddress: "10.0.0.11"
                    )
                });
        }
    }
}
