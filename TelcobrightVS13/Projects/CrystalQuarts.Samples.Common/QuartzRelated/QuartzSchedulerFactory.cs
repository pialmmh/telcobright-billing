using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Quartz;
using Quartz.Impl;

namespace CrystalQuarts.Samples.Common
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