using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Quartz;
using TelcobrightMediation.Scheduler.Quartz;

namespace QuartzTelcobright
{
    public class QuartzTelcobrightManager
    {
        private IScheduler Scheduler { get; set; }
        public QuartzTelcobrightManager() { }//empty constructor, required when running quartz, not resetting jobstore

        public QuartzTelcobrightManager(IScheduler scheduler)
        {
            this.Scheduler = scheduler;
        }

        public void ResetJobStore(List<QuartzTbDaemonConfig> daemonConfigs)
        {
            ClearJobs();
            CreateJobs(daemonConfigs);
            this.Scheduler = null;
        }
        public void ResetJobStore<T>(List<QuartzTbDaemonConfig> daemonConfigs) where T:IJob
        {
            ClearJobs();
            CreateJobs<T>(daemonConfigs);
            this.Scheduler = null;
        }
        public void ClearJobs()
        {
            this.Scheduler.Clear();
        }
        public void CreateJobs(List<QuartzTbDaemonConfig> daemonConfigs)
        {
            foreach (QuartzTbDaemonConfig instance in daemonConfigs)
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
        public void CreateJobs<T>(List<QuartzTbDaemonConfig> daemonConfigs) where T:IJob
        {
            foreach (QuartzTbDaemonConfig instance in daemonConfigs)
            {
                var job = CreateJob<T>(instance);
                var trigger = instance.FireOnceIfMissFired == false ? CreateTrigger(instance)
                    : CreateTriggerWithMissFire(instance);
                if (this.Scheduler == null)
                {
                    throw new Exception("Scheduler Instance does not exist!");
                }
                this.Scheduler.ScheduleJob(job, trigger);
            }
        }
        
        private IJobDetail CreateJob(QuartzTbDaemonConfig daemonConfig)
        {
            return JobBuilder.Create<QuartzTelcobrightProcessWrapper>()
                .WithIdentity(daemonConfig.Identity, daemonConfig.Group)
                .UsingJobData(daemonConfig.JobDataMap)
                .Build();
        }
        private IJobDetail CreateJob<T>(QuartzTbDaemonConfig daemonConfig) where T:IJob
        {
            return JobBuilder.Create<T>()
                .WithIdentity(daemonConfig.Identity, daemonConfig.Group)
                .UsingJobData(daemonConfig.JobDataMap)
                .Build();
        }
        private ITrigger CreateTrigger(QuartzTbDaemonConfig qData)
        {
            return TriggerBuilder.Create()
                .WithIdentity(qData.Identity +" " +qData.Group, qData.Group)
                //.ForJob(jobKey)//it's an option to have individual trigger per job
                //.StartNow()
                .WithCronSchedule(qData.CronExpression)
                .Build();
        }
        private ITrigger CreateTriggerWithMissFire(QuartzTbDaemonConfig qData)
        {
            return TriggerBuilder
                .Create()
                .WithIdentity(qData.Identity +" " +qData.Group, qData.Group)
                .WithCronSchedule(qData.CronExpression, x => x.WithMisfireHandlingInstructionFireAndProceed())
                .Build();
        }
    }
}