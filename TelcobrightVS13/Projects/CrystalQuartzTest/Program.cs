using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using LibraryExtensions.ConfigHelper;
using Quartz;
using QuartzTelcobright;
using TelcobrightMediation.Scheduler.Quartz;

namespace CrystalQuartzTest
{
    class Program
    {
        static void Main()
        {

            //RemoteSchedulerRunner.RunSampleScheduler();
        }
    }
    public class TBProcess : IJob
    {
        public void Execute(IJobExecutionContext context)
        {

            JobKey key = context.JobDetail.Key;

            JobDataMap dataMap = context.JobDetail.JobDataMap;

            string jobSays = dataMap.GetString("jobSays");
            float myFloatValue = dataMap.GetFloat("myFloatValue");

            Console.Error.WriteLine("Instance " + key + " of DumbJob says: " + jobSays + ", and val is: " + myFloatValue + " "
                                    + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }

    public static class RemoteSchedulerRunner
    {
        /// <summary>
        /// Creates a scheduler, expose it as a Remote Scheduler 
        /// and initializes it with some sample jobs.
        /// </summary>
        public static void RunSampleScheduler(List<QuartzTbDaemonConfig> instances,string telcobrightClientName,
            NameValueCollection schedulerProperties)
        {
            Console.WriteLine("Starting scheduler...");
            //var map = new JobDataMap();
            QuartzRunTimeConfig runTimeConfig = new QuartzRunTimeConfig(schedulerProperties,
                instances);

            QuartzTelcobrightManager quartzSchedulerManager = new QuartzTelcobrightManager(runTimeConfig);
            IScheduler scheduler = quartzSchedulerManager.GetScheduler(); //get scheduler
            quartzSchedulerManager.ResetJobStore<QuartzTelcobrightProcessWrapper>(); //reset, this will instantiate scheduler too
            scheduler = quartzSchedulerManager.GetScheduler();
            //ITrigger t = scheduler.GetTrigger(new TriggerKey("01_FileLister_trigger", "FileSync"));
            //DateTime nextFireDate = t.GetNextFireTimeUtc().Value.DateTime;
            scheduler.Start();
            Console.WriteLine("Scheduler has been started.");
            Console.WriteLine("Press [Enter] to stop and exit.");
            Console.ReadLine();

            Console.WriteLine("Stopping scheduler...");

            scheduler.Shutdown();
        }
    }
}