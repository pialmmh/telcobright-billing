using System.Collections.Generic;
using LibraryExtensions.ConfigHelper;
using TelcobrightMediation.Scheduler.Quartz;
using LibraryExtensions.ConfigHelper;
namespace TelcobrightMediation.Config
{
    public interface IConfigGenerator
    {
        TelcobrightConfig Tbc { get; }
        TelcobrightConfig GenerateConfig();
        List<QuartzTbDaemonConfig> GetSchedulerDaemonConfigs();
        DatabaseSetting GetDatabaseConfigs();
        List<Server> GetServerConfigs();
    }
}