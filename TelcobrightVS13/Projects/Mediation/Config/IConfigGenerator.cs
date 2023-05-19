using System.Collections.Generic;
using LibraryExtensions.ConfigHelper;
using TelcobrightMediation.Scheduler.Quartz;

namespace TelcobrightMediation.Config
{
    public interface IConfigGenerator
    {
        TelcobrightConfig Tbc { get; }
        TelcobrightConfig GenerateConfig();
        List<QuartzTbDaemonConfig> GetSchedulerDaemonConfigs();
        DatabaseSetting GetDatabaseSettings();
    }
}






