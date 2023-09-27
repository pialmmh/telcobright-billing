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
        int IdOperator { get; set; }
        string CustomerName { get; set; }
        string DatabaseName { get; set; }
        TelcobrightConfig GenerateFullConfig(InstanceConfig instanceConfig, int microserviceInstanceId);
        List<QuartzTbDaemonConfig> GetSchedulerDaemonConfigs();
        DatabaseSetting GetDatabaseConfigs();
        List<Server> GetServerConfigs();
    }
}