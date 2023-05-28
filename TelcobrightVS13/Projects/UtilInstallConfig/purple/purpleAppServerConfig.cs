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
    public partial class PurpleAbstractConfigConfigGenerator //quartz config part
    {
        public override List<ApplicationServerConfig> GetApplicationServerConfigs()
        {
            return
                base.ValidateInstances(new List<ApplicationServerConfig>
                {
                    new ApplicationServerConfig
                    (
                        serverId: 1,
                        ownIpAddress: "172.16.100.7"
                    ),
                    //new ApplicationServerConfig
                    //(
                    //    serverId: 2,
                    //    ownIpAddress: "192.168.0.231"
                    //)
                });
        }
    }
}
