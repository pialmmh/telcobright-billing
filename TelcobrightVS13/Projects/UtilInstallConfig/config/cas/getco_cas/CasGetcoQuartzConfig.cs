using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using TelcobrightMediation;
using TelcobrightMediation.Scheduler.Quartz;
using System.ComponentModel.Composition;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;
using QuartzTelcobright;
using TelcobrightMediation.Config;

namespace InstallConfig
{
    public sealed partial class CasGetcoAbstractConfigGenerator //quartz config part
    {
        private List<QuartzTbDaemonConfig> DaemonConfigurations { get; set; }
        public override List<QuartzTbDaemonConfig> GetSchedulerDaemonConfigs()
        {
            this.DaemonConfigurations = CasQuartzHelper.GetSchedulerDaemonConfigs(this.DatabaseName);
            return this.DaemonConfigurations;
        }
        
    }
}
