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
    public static class CasQuartzHelper //quartz config part
    {
        public static List<QuartzTbDaemonConfig> GetSchedulerDaemonConfigs(string operatorName)
        {
            List<QuartzTbDaemonConfig> daemonConfigurations = new List<QuartzTbDaemonConfig>();
            daemonConfigurations.AddRange(GetLogFileJobCreatorInstances(operatorName));
            daemonConfigurations.AddRange(GetCdrJobProcessorInstances(operatorName));
            daemonConfigurations.AddRange(GetCdrPreProcessorInstances(operatorName));
            daemonConfigurations.AddRange(GetOptimizerInstances(operatorName));
            return daemonConfigurations;
        }

        private static List<QuartzTbDaemonConfig> GetLogFileJobCreatorInstances(string operatorName)
        {
            var telcobrightProcessInstances = new List<QuartzTbDaemonConfig>()
            {
                new QuartzTbDaemonConfig(
                    operatorName: operatorName,
                    identity: "CdrJobCreator" + " [" + operatorName+"]",
                    @group: operatorName,
                    fireOnceIfMissFired: false,
                    cronExpression: "/5 * * ? * *",
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "101"},
                        {"operatorName", operatorName}
                    }
                ),
                new QuartzTbDaemonConfig(
                    operatorName: operatorName,
                    identity: "CdrErrorJobCreator" + " [" + operatorName+"]",
                    @group: operatorName,
                    fireOnceIfMissFired: false,
                    cronExpression: "/5 * * ? * *",
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "110"},
                        {"operatorName", operatorName}
                    }
                )
            };
            return telcobrightProcessInstances;
        }
        private static List<QuartzTbDaemonConfig> GetCdrJobProcessorInstances(string operatorName)
        {
            var telcobrightProcessInstances = new List<QuartzTbDaemonConfig>()
            {
                new QuartzTbDaemonConfig(
                    operatorName: operatorName,
                    identity: "CdrJobProcessor" + " [" + operatorName+"]",
                    @group: operatorName,
                    fireOnceIfMissFired: false,
                    cronExpression: "/5 * * ? * *",
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "103"},
                        {"operatorName", operatorName}
                    }
                )
            };
            return telcobrightProcessInstances;

        }
        private static List<QuartzTbDaemonConfig> GetCdrPreProcessorInstances(string operatorName)
        {
            var telcobrightProcessInstances = new List<QuartzTbDaemonConfig>()
            {
                new QuartzTbDaemonConfig(
                    operatorName: operatorName,
                    identity: "CdrPreProcessor" + " [" + operatorName+"]",
                    @group: operatorName,
                    fireOnceIfMissFired: false,
                    cronExpression: "/5 * * ? * *",
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "120"},
                        {"operatorName", operatorName}
                    }
                )
            };
            return telcobrightProcessInstances;
        }

        private static List<QuartzTbDaemonConfig> GetOptimizerInstances(string operatorName)
        {
            //don't use foreach, do it manually for flixibility e.g. different repeating interval
            List<QuartzTbDaemonConfig> optimizerInstances = new List<QuartzTbDaemonConfig>()
            {
                new QuartzTbDaemonConfig
                (
                    operatorName: operatorName,
                    identity: "Optimizer" + " [" + operatorName+"]",
                    group: operatorName,
                    cronExpression: "/30 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "107"},
                        {"operatorName", operatorName},
                    }),
            };
            return optimizerInstances;
        }
        private static List<QuartzTbDaemonConfig> GetInvoiceGeneratorInstances(string operatorName)
        {
            var telcobrightProcessInstances = new List<QuartzTbDaemonConfig>()
            {
                //new QuartzTbDaemonConfig(
                //    operatorName: operatorName,
                //    identity: "InvoiceGenerator" + " [" + operatorName+"]",
                //    @group: operatorName,
                //    fireOnceIfMissFired: false,
                //    cronExpression: "/5 * * ? * *",
                //    jobDataMap: new Dictionary<string, string>()
                //    {
                //        {"telcobrightProcessId", "108"},
                //        {"operatorName", operatorName}
                //    }
                //)
            };
            return telcobrightProcessInstances;
        }
    }
}
