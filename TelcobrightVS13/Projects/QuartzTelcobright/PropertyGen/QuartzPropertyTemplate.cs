using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuartzTelcobright.PropertyGen
{
    public static class QuartzPropertyTemplate
    {
        public static NameValueCollection GetAdoJobStoreTemplate(int tcpPortNumber,string connectionString)
        {
            return new NameValueCollection()
            {
                {"quartz.scheduler.instanceName", "RemoteServerSchedulerClient"},
                {"quartz.threadPool.type", "Quartz.Simpl.SimpleThreadPool, Quartz"},
                {"quartz.threadPool.threadCount", "10"},
                {"quartz.threadPool.threadPriority", "Normal"},
                {"quartz.scheduler.exporter.type", "Quartz.Simpl.RemotingSchedulerExporter, Quartz"},
                {"quartz.scheduler.exporter.port", tcpPortNumber.ToString()},
                {"quartz.scheduler.exporter.bindName", "QuartzScheduler"},
                {"quartz.scheduler.exporter.channelType", "tcp"},
                {"quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.MySQLDelegate, Quartz"},
                {"quartz.jobStore.dataSource", "default"},
                {"quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz"},
                {
                    "quartz.dataSource.default.connectionString",
                    connectionString //REPLACE THIS DYNAMICALLY WITH VALUE FROM APP.CONFIG
                },
                {"quartz.jobStore.tablePrefix", "qrtz_"},
                {"quartz.dataSource.default.provider", "MySql-65"}, //TILL NOW ONLY CONNECTOR.NET 6.5 SUPPORTED
                {"quartz.jobStore.useProperties", "true"},
            };
        }
        public static NameValueCollection GetRamJobStoreTemplate(int tcpPortNumber)
        {
            return new NameValueCollection()
            {
                {"quartz.scheduler.instanceName", "RemoteSchedulerRamJobStore"},
                {"quartz.threadPool.type", "Quartz.Simpl.SimpleThreadPool, Quartz"},
                {"quartz.threadPool.threadCount", "10"},
                {"quartz.threadPool.threadPriority", "Normal"},
                {"quartz.scheduler.exporter.type", "Quartz.Simpl.RemotingSchedulerExporter, Quartz"},
                {"quartz.scheduler.exporter.port", tcpPortNumber.ToString()},
                {"quartz.scheduler.exporter.bindName", "QuartzScheduler"},
                {"quartz.scheduler.exporter.channelType", "tcp"},
                {"quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.MySQLDelegate, Quartz"},
                {"quartz.jobStore.dataSource", "default"},
                {"quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz"},
                {"quartz.jobStore.useProperties", "true"},
            };
        }
    }
}
