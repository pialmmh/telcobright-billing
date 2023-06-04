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
        public abstract DatabaseSetting GetDatabaseSettings();
        public abstract List<Server> GetApplicationServerConfigs();
        public void ValidateServers(List<Server> servers)
        {
            bool ipAddressUnique = servers.Select(c => c.IpAddresses.Select(ip=>ip.Address)).Distinct().Count() == servers.Count;
            bool serverIdUnique = servers.Select(c => c.ServerId).Distinct().Count() == servers.Count;
            if (!ipAddressUnique)
            {
                throw new Exception("Application server ip addresses must be unique.");
            }
            if (!serverIdUnique)
            {
                throw new Exception("Application Server ids be unique.");
            }
        }
    }
}






