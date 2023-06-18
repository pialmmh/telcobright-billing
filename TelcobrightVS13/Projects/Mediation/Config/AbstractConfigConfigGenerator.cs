using System;
using System.Collections.Generic;
using System.Linq;
using LibraryExtensions.ConfigHelper;
using TelcobrightMediation.Scheduler.Quartz;

namespace TelcobrightMediation.Config
{
    public abstract class AbstractConfigConfigGenerator: IConfigGenerator
    {
        public override string ToString() => this.GetType().Name;
        public abstract TelcobrightConfig Tbc { get; }
        public abstract TelcobrightConfig GenerateConfig();
        public abstract List<QuartzTbDaemonConfig> GetSchedulerDaemonConfigs();
        public abstract DatabaseSetting GetDatabaseConfigs();
        public abstract List<Server> GetServerConfigs();
        public void ValidateServers(List<Server> servers)
        {
            bool ipAddressUnique = servers.Select(c => c.IpAddresses.Select(ip=>ip.Address)).Distinct().Count() == servers.Count;
            bool serverIdUnique = servers.Select(c => c.ServerId).Distinct().Count() == servers.Count;
            bool serverNameUnique = servers.Select(c => c.ServerName).Distinct().Count() == servers.Count;
            if (!ipAddressUnique)
            {
                throw new Exception("Application server ip addresses must be unique.");
            }
            if (!serverIdUnique)
            {
                throw new Exception("Application Server ids must be unique.");
            }
            if (!serverNameUnique)
            {
                throw new Exception("Application Server names must be unique.");
            }
        }
    }
}






