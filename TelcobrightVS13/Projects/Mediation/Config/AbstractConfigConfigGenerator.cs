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
    public abstract class AbstractConfigConfigGenerator: IConfigGenerator
    {
        public override string ToString() => this.GetType().Name;
        public abstract TelcobrightConfig Tbc { get; }
        public abstract TelcobrightConfig GenerateConfig();
        public abstract List<QuartzTbDaemonConfig> GetSchedulerDaemonConfigs();
        public abstract DatabaseSetting GetDatabaseSettings();
    }
}






