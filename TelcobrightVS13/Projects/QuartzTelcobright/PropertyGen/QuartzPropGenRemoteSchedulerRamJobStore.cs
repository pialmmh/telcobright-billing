using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuartzTelcobright.PropertyGen
{
    public class QuartzPropGenRemoteSchedulerRamJobStore : AbstractQuartzPropertyGenerator
    {
        public int TcpPortNumber { get; set; }

        public QuartzPropGenRemoteSchedulerRamJobStore(int tcpPortNumber)
        {
            this.TcpPortNumber = tcpPortNumber;
        }

        protected override NameValueCollection GenerateSchedulerProperties()
        {
            return new NameValueCollection()
            {
                {"quartz.scheduler.instanceName", "RemoteServerSchedulerClient"},
                {"quartz.threadPool.type", "Quartz.Simpl.SimpleThreadPool, Quartz"},
                {"quartz.threadPool.threadCount", "10"},
                {"quartz.threadPool.threadPriority", "Normal"},
                {"quartz.scheduler.exporter.type", "Quartz.Simpl.RemotingSchedulerExporter, Quartz"},
                {"quartz.scheduler.exporter.port", this.TcpPortNumber.ToString()},
                {"quartz.scheduler.exporter.bindName", "QuartzScheduler"},
                {"quartz.scheduler.exporter.channelType", "tcp"},
                {"quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz"},
                {"quartz.jobStore.useProperties", "true"},
            };
        }
    }
}
