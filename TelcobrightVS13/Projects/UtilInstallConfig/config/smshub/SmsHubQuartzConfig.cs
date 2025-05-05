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
    public partial class SmsHubAbstractConfigGenerator //quartz config part
    {
        private List<QuartzTbDaemonConfig> DaemonConfigurations { get; set; }
        public override List<QuartzTbDaemonConfig> GetSchedulerDaemonConfigs()
        {
            this.DaemonConfigurations = new List<QuartzTbDaemonConfig>();
            this.DaemonConfigurations.AddRange(GetFileListerInstances(this.Tbc.Telcobrightpartner.databasename));
            this.DaemonConfigurations.AddRange(GetDownloadMarkerInstances(this.Tbc.Telcobrightpartner.databasename));
            this.DaemonConfigurations.AddRange(GetCdrPreProcessorInstances(this.Tbc.Telcobrightpartner.databasename));
            this.DaemonConfigurations.AddRange(GetLogFileJobCreatorInstances(this.Tbc.Telcobrightpartner.databasename));
            this.DaemonConfigurations.AddRange(GetFileCopierInstances(this.Tbc.Telcobrightpartner.databasename));
            this.DaemonConfigurations.AddRange(GetCdrJobProcessorInstances(this.Tbc.Telcobrightpartner.databasename));
            this.DaemonConfigurations.AddRange(GetOptimizerInstances(this.Tbc.Telcobrightpartner.databasename));
            this.DaemonConfigurations.AddRange(GetInvoiceGeneratorInstances(this.Tbc.Telcobrightpartner.databasename));
            this.DaemonConfigurations.AddRange(GetRamDiskMounterInstances(this.Tbc.Telcobrightpartner.databasename));
            return this.DaemonConfigurations;
        }

        private List<QuartzTbDaemonConfig> GetFileListerInstances(string operatorName)
        {
            //don't use foreach, do it manually for flixibility e.g. different repeating interval
            List<QuartzTbDaemonConfig> fileListerInstances = new List<QuartzTbDaemonConfig>()
            {
                new QuartzTbDaemonConfig
                (
                    operatorName: operatorName,
                    identity: "FileLister [borak1:vault]" + " [" + operatorName + "]",
                    group: operatorName,
                    cronExpression: "/30 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "106"},
                        {"operatorName", operatorName},
                        {"syncPair", "borak1:Vault"}
                    }),

                new QuartzTbDaemonConfig
                (
                    operatorName: operatorName,
                    identity: "FileLister [borak2:vault]" + " [" + operatorName + "]",
                    group: operatorName,
                    cronExpression: "/30 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "106"},
                        {"operatorName", operatorName},
                        {"syncPair", "borak2:Vault"}
                    }),

                new QuartzTbDaemonConfig
                (
                    operatorName: operatorName,
                    identity: "FileLister [khaja1:vault]" + " [" + operatorName + "]",
                    group: operatorName,
                    cronExpression: "/30 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "106"},
                        {"operatorName", operatorName},
                        {"syncPair", "khaja1:Vault"}
                    }),

                new QuartzTbDaemonConfig
                (
                    operatorName: operatorName,
                    identity: "FileLister [khaja2:vault]" + " [" + operatorName + "]",
                    group: operatorName,
                    cronExpression: "/30 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "106"},
                        {"operatorName", operatorName},
                        {"syncPair", "khaja2:Vault"}
                    }),


            };
            return fileListerInstances;
        }

        private List<QuartzTbDaemonConfig> GetDownloadMarkerInstances(string operatorName)
        {
            //don't use foreach, do it manually for flixibility e.g. different repeating interval
            List<QuartzTbDaemonConfig> downloadMarkerInstances = new List<QuartzTbDaemonConfig>()
            {
                new QuartzTbDaemonConfig
                (
                    operatorName: operatorName,
                    identity: "DownloadMarker " + " [" + operatorName + "]",
                    group: operatorName,
                    cronExpression: "/30 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "114"},
                        {"operatorName", operatorName},
                        { "maxMarkingForDownlaod","500"}
                    }),

            };
            return downloadMarkerInstances;
        }

        private List<QuartzTbDaemonConfig> GetFileCopierInstances(string operatorName)
        {
            //don't use foreach, do it manually for flixibility e.g. different repeating interval
            List<QuartzTbDaemonConfig> fileCopierInstances = new List<QuartzTbDaemonConfig>()
            {
                new QuartzTbDaemonConfig
                (
                    operatorName: operatorName,
                    identity: "FileCopier [borak1:Vault]" + " [" + operatorName + "]",
                    group: operatorName,
                    cronExpression: "/5 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "104"},
                        {"operatorName", operatorName},
                        {"syncPair", "borak1:Vault"}
                    }),

                new QuartzTbDaemonConfig
                (
                    operatorName: operatorName,
                    identity: "FileCopier [borak2:Vault]" + " [" + operatorName + "]",
                    group: operatorName,
                    cronExpression: "/5 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "104"},
                        {"operatorName", operatorName},
                        {"syncPair", "borak2:Vault"}
                    }),

                new QuartzTbDaemonConfig
                (
                    operatorName: operatorName,
                    identity: "FileCopier [khaja1:Vault]" + " [" + operatorName + "]",
                    group: operatorName,
                    cronExpression: "/5 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "104"},
                        {"operatorName", operatorName},
                        {"syncPair", "khaja1:Vault"}
                    }),

                new QuartzTbDaemonConfig
                (
                    operatorName: operatorName,
                    identity: "FileCopier [khaja2:Vault]" + " [" + operatorName + "]",
                    group: operatorName,
                    cronExpression: "/5 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "104"},
                        {"operatorName", operatorName},
                        {"syncPair", "khaja2:Vault"}
                    }),
            };
            return fileCopierInstances;
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

        private List<QuartzTbDaemonConfig> GetLogFileJobCreatorInstances(string operatorName)
        {
            var telcobrightProcessInstances = new List<QuartzTbDaemonConfig>()
            {
                new QuartzTbDaemonConfig(
                    operatorName: operatorName,
                    identity: "MdrJobCreator" + " [" + operatorName+"]",
                    @group: operatorName,
                    fireOnceIfMissFired: false,
                    cronExpression: "/5 * * ? * *",
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "111"},
                        {"operatorName", operatorName}
                    }
                )
            };
            return telcobrightProcessInstances;
        }

        private List<QuartzTbDaemonConfig> GetCdrJobProcessorInstances(string operatorName)
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
        private List<QuartzTbDaemonConfig> GetOptimizerInstances(string operatorName)
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
        private List<QuartzTbDaemonConfig> GetInvoiceGeneratorInstances(string operatorName)
        {
            var telcobrightProcessInstances = new List<QuartzTbDaemonConfig>()
            {
                new QuartzTbDaemonConfig(
                    operatorName: operatorName,
                    identity: "InvoiceGenerator" + " [" + operatorName+"]",
                    @group: operatorName,
                    fireOnceIfMissFired: false,
                    cronExpression: "/5 * * ? * *",
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "108"},
                        {"operatorName", operatorName}
                    }
                )
            };
            return telcobrightProcessInstances;
        }

        private List<QuartzTbDaemonConfig> GetRamDiskMounterInstances(string operatorName)
        {
            List<QuartzTbDaemonConfig> telcobrightProcessInstances = new List<QuartzTbDaemonConfig>()
            {
                new QuartzTbDaemonConfig(
                    operatorName: operatorName,
                    identity: "RamDiskMounter" + " [" + operatorName+"]",
                    @group: operatorName,
                    fireOnceIfMissFired: false,
                    cronExpression: "/5 * * ? * *",
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "112"},
                        {"operatorName", operatorName}
                    }
                )
            };
            return telcobrightProcessInstances;
        }
    }
}
