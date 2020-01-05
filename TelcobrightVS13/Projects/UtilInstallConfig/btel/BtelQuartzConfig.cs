﻿using System;
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
    public partial class BtelConfigGenerator //quartz config part
    {
        private List<QuartzTbDaemonConfig> DaemonConfigurations { get; set; }
        public List<QuartzTbDaemonConfig> GetSchedulerDaemonConfigs()
        {
            this.DaemonConfigurations = new List<QuartzTbDaemonConfig>();
            this.DaemonConfigurations.AddRange(GetFileListerInstances(this.Tbc.DatabaseSetting.DatabaseName));
            this.DaemonConfigurations.AddRange(GetLogFileJobCreatorInstances(this.Tbc.DatabaseSetting.DatabaseName));
            this.DaemonConfigurations.AddRange(GetFileCopierInstances(this.Tbc.DatabaseSetting.DatabaseName));
            this.DaemonConfigurations.AddRange(GetCdrJobProcessorInstances(this.Tbc.DatabaseSetting.DatabaseName));
            this.DaemonConfigurations.AddRange(GetInvoiceGeneratorInstances(this.Tbc.DatabaseSetting.DatabaseName));
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
                    identity: "FileLister [BtelZteDhk:Vault]" + " [" + operatorName+"]",
                    group: operatorName,
                    cronExpression: "/30 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "106"},
                        {"operatorName", operatorName},
                        {"syncPair", "BtelZteDhk:Vault"}
                    }),
            };
            return fileListerInstances;
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
                    cronExpression: "/5 * * ? * *",
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
        private List<QuartzTbDaemonConfig> GetFileCopierInstances(string operatorName)
        {
            //don't use foreach, do it manually for flixibility e.g. different repeating interval
            List<QuartzTbDaemonConfig> fileCopierInstances = new List<QuartzTbDaemonConfig>()
            {
                new QuartzTbDaemonConfig
                (
                    operatorName: operatorName,
                    identity: "FileCopier [BtelZteDhk:Vault]" + " [" + operatorName+"]",
                    group: operatorName,
                    cronExpression: "/5 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "104"},
                        {"operatorName", operatorName},
                        {"syncPair", "BtelZteDhk:Vault"}
                    }),
                new QuartzTbDaemonConfig
                (
                    operatorName: operatorName,
                    identity: "FileCopier [Vault:FileArchive1]" + " [" + operatorName+"]",
                    group: operatorName,
                    cronExpression: "/5 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "104"},
                        {"operatorName", operatorName},
                        {"syncPair", "Vault:FileArchive1"}
                    }),
                new QuartzTbDaemonConfig
                (
                    operatorName: operatorName,
                    identity: "FileCopier [Vault:IOF]" + " [" + operatorName+"]",
                    group: operatorName,
                    cronExpression: "/5 * * ? * *",
                    fireOnceIfMissFired: false,
                    jobDataMap: new Dictionary<string, string>()
                    {
                        {"telcobrightProcessId", "104"},
                        {"operatorName", operatorName},
                        {"syncPair","Vault:IOF" }
                    })

            };
            return fileCopierInstances;
        }
    }
}
