using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using LibraryExtensions.ConfigHelper;
using QuartzTelcobright;
using TelcobrightMediation.Automation;
using TelcobrightMediation.Config;
using TelcobrightMediation.Scheduler.Quartz;

namespace TelcobrightMediation
{
    public class TelcobrightConfig
    {
        //temporarily keep connection strings with help of ne, change later
        public int DefaultTimeZoneId { get; set; } = 3251;
        public TelecomOperatortype TelecomOperatortype { get;}
        public List<QuartzTbDaemonConfig> SchedulerDaemonConfigs { get; set; }
        public ResourcePool ResourcePool{ get; set; }
        public SimpleCacheSettings SimpleCacheSettings { get; set; }
        public CdrSetting CdrSetting { get; set; }
        public AutomationSetting AutomationSetting { get; set; }
        public int ServerId { get; set; }
        public DirectorySettings DirectorySettings { get; set; }
        public DatabaseSetting DatabaseSetting { get; set; } = new DatabaseSetting();
        public PortalSettings PortalSettings { get; set; }
        public Dictionary<string, ApplicationServerConfig> ApplicationServersConfig { get; set; }//server id as string
        public int IdTelcobrightPartner { get; set; }
        public Dictionary<int,AutomatedNetworkElementCli> AutomatedNetworkElementClis { get; set; }
        public EmailSenderConfig EmailSenderConfig { get; set; }
        public SmsSenderConfig SmsSenderConfig { get; set; }
        public TelcobrightConfig() { }//required by some non cdr process e.g. directorySYnc

        public List<KeyValuePair<Regex, string>> ServiceAliasesRegex { get; set; } =
            new List<KeyValuePair<Regex, string>>();
        public TelcobrightConfig(TelecomOperatortype telecomOperatortype,
            int thisServerId)
        {
            this.ServerId = thisServerId;
            this.ApplicationServersConfig = new Dictionary<string, ApplicationServerConfig>();
            this.ResourcePool = new ResourcePool();
            this.TelecomOperatortype = telecomOperatortype;
        }

        public string GetPathIndependentApplicationDirectory()
        {
            return this.DirectorySettings.ApplicationRootDirectory.Replace("/", Path.DirectorySeparatorChar.ToString());
        }
    }
}
