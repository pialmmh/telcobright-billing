using System;
using System.Collections.Generic;
using System.Linq;
using LibraryExtensions.ConfigHelper;
using TelcobrightInfra;
using TelcobrightMediation.Scheduler.Quartz;

namespace TelcobrightMediation.Config
{
    public abstract class AbstractConfigGenerator: IConfigGenerator
    {
        public override string ToString() => this.GetType().Name;
        public abstract TelcobrightConfig Tbc { get; set; }
        public abstract int IdOperator { get; set; }
        public abstract string CustomerName { get; set; }
        public abstract string DatabaseName { get; set; }
        public abstract TelcobrightConfig GenerateFullConfig(InstanceConfig instanceConfig,int microserviceInstanceId);
        public abstract List<QuartzTbDaemonConfig> GetSchedulerDaemonConfigs();
        public abstract DatabaseSetting GetDatabaseConfigs();
        public abstract List<Server> GetServerConfigs();
        public void ValidateServers(List<Server> servers)
        {
            bool ipAddressUnique = servers.Select(c => c.IpAddresses.Select(ip=>ip.Address)).Distinct().Count() == servers.Count;
            bool serverIdUnique = servers.Where(s=>s.ServerId>0).Select(c => c.ServerId)
                .Distinct().Count() == servers.Count(s => s.ServerId > 0);
            bool serverNameUnique = servers.Select(c => c.Name).Distinct().Count() == servers.Count;
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






