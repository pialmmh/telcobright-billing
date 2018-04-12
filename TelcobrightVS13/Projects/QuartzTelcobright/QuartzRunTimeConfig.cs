using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using TelcobrightMediation.Scheduler.Quartz;

namespace QuartzTelcobright
{
    public class QuartzRunTimeConfig
    {
        public IDictionary<string, string> SchedulerPropertiesDictionary { get; set; }//namevaluecollection could not be serialized with newtonsoft json

        private NameValueCollection SchedulerProperties => this.SchedulerPropertiesDictionary.ToNameValueCollection();

        public List<QuartzTbDaemonConfig> DaemonConfigs { get; }

        public QuartzRunTimeConfig()//default for serializing
        {
        }

        public QuartzRunTimeConfig(NameValueCollection schedulerProperties, List<QuartzTbDaemonConfig> daemonConfigs)
        {
            this.SchedulerPropertiesDictionary = schedulerProperties.ToDictionary();
            this.DaemonConfigs = daemonConfigs;
        }

    }
}
