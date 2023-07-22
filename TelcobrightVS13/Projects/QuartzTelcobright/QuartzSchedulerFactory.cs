using System.Collections.Specialized;
using System.Linq;
using Quartz;
using Quartz.Impl;
namespace QuartzTelcobright
{
    public class QuartzSchedulerFactory
    {
        private NameValueCollection schedulerProperties;

        public QuartzSchedulerFactory(NameValueCollection schedulerProperties)
        {
            this.schedulerProperties = schedulerProperties;
        }
        public IScheduler CreateSchedulerInstance()
        {
            var schedularFactory = new StdSchedulerFactory(this.schedulerProperties);
            return schedularFactory.GetScheduler();
            
        }
    }
}