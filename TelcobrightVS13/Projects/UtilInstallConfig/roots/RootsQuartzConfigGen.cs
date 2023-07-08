using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions.ConfigHelper;
using QuartzTelcobright;
using TelcobrightMediation;
using TelcobrightMediation.Config;
using TelcobrightMediation.Scheduler.Quartz;

namespace InstallConfig
{
    public partial class RootsConfigGenerator
    {
        private List<QuartzTbDaemonConfig> DaemonConfigurations { get; set; }
        public List<QuartzTbDaemonConfig> GetSchedulerDaemonConfigs()
        {
            //throw new NotImplementedException();
            return GetFileListerInstances("roots");
        }
        private List<QuartzTbDaemonConfig> GetFileListerInstances(string operatorName)
        {
            //don't use foreach, do it manually for flixibility e.g. different repeating interval
            List<QuartzTbDaemonConfig> fileListerInstances = new List<QuartzTbDaemonConfig>()
            {
                new QuartzTbDaemonConfig
                (
                    operatorName:operatorName,
                    identity: "FileLister [ROOTS:Vault]",
                    group: operatorName,
                    cronExpression: "/5 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap:new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "106"},
                        {"operatorName",operatorName}
                    }),

            };
            return fileListerInstances;
        }

    }
}
