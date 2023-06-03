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
        public abstract List<ServerInstance> GetApplicationServerConfigs();

        public List<ServerInstance> ValidateInstances(List<ServerInstance> configs)
        {
            bool ipAddressUnique = configs.Select(c => c.IpAddresses.Select(ip=>ip.Address)).Distinct().Count() == configs.Count;
            bool serverIdUnique = configs.Select(c => c.ServerId).Distinct().Count() == configs.Count;
            if (!ipAddressUnique)
            {
                throw new Exception("Application server ip addresses must be unique.");
            }
            if (!serverIdUnique)
            {
                throw new Exception("Application Server ids be unique.");
            }
            return configs;
        }
    }
}






