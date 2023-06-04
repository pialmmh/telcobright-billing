﻿using System;
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
    public partial class SrtAbstractConfigConfigGenerator  //quartz config part
    {
        public override List<Server> GetApplicationServerConfigs()
        {
            List<Server> servers = new List<Server>();
            Server server01 = new Server(1, "db01")
            {
                IpAddresses = new List<IpAddress>() { new IpAddress { Address = "172.16.100.6" } },
                ServerOs = ServerOs.Ubuntu,
                AutomationType = ServerAutomationType.LinuxMysqlAutomation,
                AdminUsername = "telcobright",
                AdminPassword = "Takay1#$ane%%",
                AutomationUsername = "telcobright",
                AutomationPassword = "Takay1#$ane%%",
            };
            Server server02 = new Server(2, "db02")
            {
                IpAddresses = new List<IpAddress>() { new IpAddress { Address = "172.16.100.12" } },
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
