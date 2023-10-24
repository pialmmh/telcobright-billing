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
    public partial class SrtAbstractConfigGenerator //quartz config part
    {
        private List<QuartzTbDaemonConfig> DaemonConfigurations { get; set; }
        public override List<QuartzTbDaemonConfig> GetSchedulerDaemonConfigs()
        {
            this.DaemonConfigurations = new List<QuartzTbDaemonConfig>();
            this.DaemonConfigurations.AddRange(GetFileListerInstances(this.Tbc.Telcobrightpartner.databasename));
            this.DaemonConfigurations.AddRange(GetLogFileJobCreatorInstances(this.Tbc.Telcobrightpartner.databasename));
            this.DaemonConfigurations.AddRange(GetFileCopierInstances(this.Tbc.Telcobrightpartner.databasename));
            this.DaemonConfigurations.AddRange(GetCdrJobProcessorInstances(this.Tbc.Telcobrightpartner.databasename));
            this.DaemonConfigurations.AddRange(GetOptimizerInstances(this.Tbc.Telcobrightpartner.databasename));
            this.DaemonConfigurations.AddRange(GetInvoiceGeneratorInstances(this.Tbc.Telcobrightpartner.databasename));
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
                    identity: "FileLister [Huawei:Vault]" + " [" + operatorName+"]",
                    group: operatorName,
                    cronExpression: "/30 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "106"},
                        {"operatorName", operatorName},
                        {"syncPair", Huawei_Vault.Name}
                    }),
            };
            return fileListerInstances;
        }
        private List<QuartzTbDaemonConfig> GetFileCopierInstances(string operatorName)
        {
            //don't use foreach, do it manually for flixibility e.g. different repeating interval
            List<QuartzTbDaemonConfig> fileCopierInstances = new List<QuartzTbDaemonConfig>()
            {
                new QuartzTbDaemonConfig
                (
                    operatorName: operatorName,
                    identity: "FileCopier [Huawei:Vault]" + " [" + operatorName+"]",
                    group: operatorName,
                    cronExpression: "/2 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "104"},
                        {"operatorName", operatorName},
                        {"syncPair", Huawei_Vault.Name}
                    }),
                
                new QuartzTbDaemonConfig
                (
                    operatorName: operatorName,
                    identity: "FileCopier [Vault:CAS]" + " [" + operatorName+"]",
                    group: operatorName,
                    cronExpression: "/2 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "104"},
                        {"operatorName", operatorName},
                        {"syncPair", vaultCAS.Name}
                    }),
            };
            return fileCopierInstances;
        }
        private List<QuartzTbDaemonConfig> GetLogFileJobCreatorInstances(string operatorName)
        {
            var telcobrightProcessInstances = new List<QuartzTbDaemonConfig>()
            {
                new QuartzTbDaemonConfig(
                    operatorName: operatorName,
                    identity: "CdrJobCreator" + " [" + operatorName+"]",
                    @group: operatorName,
                    fireOnceIfMissFired: false,
                    cronExpression: "/2 * * ? * *",
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "101"},
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
                    cronExpression: "/2 * * ? * *",
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
                    cronExpression: "/2 * * ? * *",
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "108"},
                        {"operatorName", operatorName}
                    }
                )
            };
            return telcobrightProcessInstances;
        }
    }
}
