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
    public partial class DblAbstractConfigConfigGeneratorHelper //quartz config part
    {
        public override List<Server> GetServerConfigs()
        {
            List<Server> servers = new List<Server>();
            Server server01 = new Server(1, "db01")
            {
                IpAddresses = new List<IpAddress>() { new IpAddress { Address = "114.130.82.54" } },
                ServerOs = ServerOs.Ubuntu,
                AutomationType = ServerAutomationType.LinuxMysqlAutomation,
                AdminUsername = "telcobright",
                AdminPassword = "Takay1#$ane%%",
                AutomationUsername = "telcobright",
                AutomationPassword = "Takay1#$ane%%",
            };
            Server server02 = new Server(2, "db02")
            {
                IpAddresses = new List<IpAddress>() { new IpAddress { Address = "192.168.1.122" } },
                ServerOs = ServerOs.Ubuntu,
                AutomationType = ServerAutomationType.LinuxMysqlAutomation,
                AdminUsername = "root",
                AdminPassword = "Btl@secdb2$",
                AutomationUsername = "root",
                AutomationPassword = "Btl@secdb2$"
            };
            servers.Add(server01);
            servers.Add(server02);
            base.ValidateServers(servers);

          


            return servers;
        }
    }
}
