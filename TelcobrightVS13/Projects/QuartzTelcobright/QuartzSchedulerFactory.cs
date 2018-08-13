using System.Collections.Specialized;
using System.Linq;
using Quartz;
using Quartz.Impl;
namespace QuartzTelcobright
{
    public static class QuartzSchedulerFactory
    {
        public static IScheduler CreateSchedulerInstance(NameValueCollection schedulerProperties)
        {
            var schedularFactory = GetSchedulerFactory(schedulerProperties);
            
            return schedularFactory.GetScheduler();
            
        }

        private static StdSchedulerFactory GetSchedulerFactory(NameValueCollection schedulerProperties)
        {
            return new StdSchedulerFactory(schedulerProperties);
        }
    }
}