using System.Collections.Generic;
using System.IO;
using LibraryExtensions.ConfigHelper;
using QuartzTelcobright;
using TelcobrightMediation.Scheduler.Quartz;

namespace TelcobrightMediation
{
    public interface IAutomationConfig
    {
        int IdTelcobrigtPartner { get; }
    }
    public class TelcobrightConfig
    {
        //temporarily keep connection strings with help of ne, change later
        public List<QuartzTbDaemonConfig> SchedulerDaemonConfigs { get; set; }
        public ResourcePool ResourcePool{ get; set; }
        public SimpleCacheSettings SimpleCacheSettings { get; set; }
        public CdrSetting CdrSetting { get; set; }
        public List<Vault> Vaults { get; set; }
        public int ServerId { get; set; }
        public DirectorySettings DirectorySettings { get; set; }
        public DatabaseSetting DatabaseSetting { get; set; }
        public PortalSettings PortalSettings { get; set; }
        public Dictionary<string, ApplicationServerConfig> ApplicationServersConfig { get; set; }//server id as string
        public int IdTelcobrightPartner { get; set; }
        public TelcobrightConfig() { }//required by some non cdr process e.g. directorySYnc
        public TelcobrightConfig(int thisServerId)
        {
            this.Vaults = new List<Vault>();
            this.ServerId = thisServerId;
            this.ApplicationServersConfig = new Dictionary<string, ApplicationServerConfig>();
            this.ResourcePool = new ResourcePool();
        }

        public string GetPathIndependentApplicationDirectory()
        {
            return this.DirectorySettings.ApplicationRootDirectory.Replace("/", Path.DirectorySeparatorChar.ToString());
        }
    }
}
