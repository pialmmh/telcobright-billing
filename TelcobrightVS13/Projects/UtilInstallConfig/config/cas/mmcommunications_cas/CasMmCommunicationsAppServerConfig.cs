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
using TelcobrightInfra;
using TelcobrightMediation.Config;

namespace InstallConfig
{
    public sealed partial class CasMmCommunicationsAbstractConfigGenerator   //quartz config part
    {
        public override List<Server> GetServerConfigs()
        {
            List<Server> servers = new List<Server>();
            Server server01 = new Server("db01")
            {
                IpAddresses = new List<IpAddressOrHostName>() { new IpAddressOrHostName { Address = "10.0.0.5" } },
                ServerOs = ServerOs.Ubuntu,
                AutomationType = ServerAutomationType.LinuxMysqlAutomation,
                AdminUsername = "telcobright",
                AdminPassword = "Takay1#$ane%%",
                AutomationUsername = "telcobright",
                AutomationPassword = "Takay1#$ane%%",
            };
            Server server02 = new Server("db02")
            {
                IpAddresses = new List<IpAddressOrHostName>() { new IpAddressOrHostName { Address = "10.0.0.7" } },
                ServerOs = ServerOs.Ubuntu,
                AutomationType = ServerAutomationType.LinuxMysqlAutomation,
                AdminUsername = "telcobright",
                AdminPassword = "Takay1#$ane%%",
                AutomationUsername = "telcobright",
                AutomationPassword = "Takay1#$ane%%"
            };
            servers.Add(server01);
            servers.Add(server02);
            base.ValidateServers(servers);

        
            return servers;
        }
    }
}