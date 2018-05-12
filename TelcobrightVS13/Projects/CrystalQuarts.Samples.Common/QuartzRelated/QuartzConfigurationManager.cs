using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Quartz;
using Quartz.Impl;

namespace CrystalQuarts.Samples.Common
{
    public class QuartzSchedulerManager
    {
        public NameValueCollection SchedulerProperties { get; set; }
        private IScheduler Scheduler { get; set; }
        public QuartzSchedulerManager(NameValueCollection schedulerProperties)
        {
            this.SchedulerProperties = schedulerProperties;
        }
        public IScheduler GetScheduler()
        {
            if (this.Scheduler == null)
            {
                this.Scheduler= QuartzSchedulerFactory.CreateSchedulerInstance(this.SchedulerProperties);
            }
            return this.Scheduler;
        }
        public void ClearJobs()
        {
            if (this.Scheduler == null)
            {
                this.Scheduler= QuartzSchedulerFactory.CreateSchedulerInstance(this.SchedulerProperties);
            }
            this.Scheduler.Clear();
        }
        public void CreateJobs(List<QuartzTelcobrightProcessInstance> jobPrototypes)
        {
            foreach (QuartzTelcobrightProcessInstance instance in jobPrototypes)
            {
                var job = CreateJob(instance);
                var trigger = instance.FireOnceIfMissFired == false ? CreateTrigger(instance)
                    : CreateTriggerWithMissFire(instance);
                if (this.Scheduler == null)
                {
                    throw new Exception("Scheduler Instance does not exist!");
                }
                this.Scheduler.ScheduleJob(job, trigger);
            }
        }
        
        private IJobDetail CreateJob(QuartzTelcobrightProcessInstance qData)
        {
            return JobBuilder.Create<TBProcess>()
                .WithIdentity(qData.Identity, qData.Group)
                .UsingJobData(qData.JobDataMap)
                .Build();
        }
        private ITrigger CreateTrigger(QuartzTelcobrightProcessInstance qData)
        {
            return TriggerBuilder.Create()
                .WithIdentity(qData.Identity + "_trigger", qData.Group)
                .WithCronSchedule(qData.CronExpression)
                .Build();
        }
        private ITrigger CreateTriggerWithMissFire(QuartzTelcobrightProcessInstance qData)
        {
            return TriggerBuilder
                .Create()
                .WithIdentity(qData.Identity + "_trigger", qData.Group)
                .WithCronSchedule(qData.CronExpression, x => x.WithMisfireHandlingInstructionFireAndProceed())
                .Build();
        }
    }
}