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
        public void PrepareApplicationServerConfig()
        {
            ApplicationServerConfig serverConfig1 = new ApplicationServerConfig(this.Tbc) { ServerId = 1, OwnIpAddress = "192.168.101.1" };
            ApplicationServerConfig serverConfig2 = new ApplicationServerConfig(this.Tbc) { ServerId = 2 };

            this.Tbc.ApplicationServersConfig.Add(serverConfig1.ServerId.ToString(), serverConfig1);
            this.Tbc.ApplicationServersConfig.Add(serverConfig2.ServerId.ToString(), serverConfig1);
        }
    }
}
