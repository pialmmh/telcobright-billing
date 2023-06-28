using System.Collections.Generic;
using LibraryExtensions.ConfigHelper;
using TelcobrightMediation.Scheduler.Quartz;
using LibraryExtensions.ConfigHelper;
using TelcobrightInfra;

namespace TelcobrightMediation.Config
{
    public interface IConfigGenerator
    {
        TelcobrightConfig Tbc { get; set; }
        TelcobrightConfig GenerateConfig(InstanceConfig instanceConfig, int microserviceInstanceId);
        List<QuartzTbDaemonConfig> GetSchedulerDaemonConfigs();
        DatabaseSetting GetDatabaseConfigs();
        List<Server> GetServerConfigs();
    }
}