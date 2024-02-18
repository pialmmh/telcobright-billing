using TelcobrightMediation.Scheduler.Quartz;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;

namespace CrystalQuarts.Samples.Common
{
    using System;
    using System.Collections.Specialized;
    using Quartz;
    using System.Configuration;
    using System.Collections.Generic;
    using QuartzTelcobright;
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
        public static void RunSampleScheduler()
        {
            Console.WriteLine("Starting scheduler...");
            var map = new JobDataMap();
            map.Put("jobSays", "Hello World!");
            map.Put("myFloatValue", "3.04");
            NameValueCollection schedulerProperties = new NameValueCollection()
            {
                {"quartz.scheduler.instanceName", "RemoteServerSchedulerClient"},
                {"quartz.threadPool.type", "Quartz.Simpl.SimpleThreadPool, Quartz"},
                {"quartz.threadPool.threadCount", "10"},
                {"quartz.threadPool.threadPriority", "Normal"},
                {"quartz.scheduler.exporter.type", "Quartz.Simpl.RemotingSchedulerExporter, Quartz"},
                {"quartz.scheduler.exporter.port", "555"},
                {"quartz.scheduler.exporter.bindName", "QuartzScheduler"},
                {"quartz.scheduler.exporter.channelType", "tcp"},
                {"quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.MySQLDelegate, Quartz"},
                {"quartz.jobStore.dataSource", "default"},
                {"quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz"},
                {
                    "quartz.dataSource.default.connectionString",
                    "server = 127.0.0.1; User Id = root; password = Takay1#$ane; Persist Security Info = True;default command timeout = 3600; database = platinum"
                },
                {"quartz.jobStore.tablePrefix", "qrtz_"},
                {"quartz.dataSource.default.provider", "MySql-65"},
                {"quartz.jobStore.useProperties", "true"},
            };
            var telcobrightProcessInstances = new List<QuartzTbDaemonConfig>()
            {
                new QuartzTbDaemonConfig(
                    new DatabaseSetting("asdf")
                    {
                        WritePasswordForApplication = "Takay1#$ane",
                        WriteUserNameForApplication = "root",
                        DatabaseName = "platinum",
                        ReadOnlyPasswordForApplication = "adsf",
                        ReadOnlyUserNameForApplication = "adfadf",
                        SectionName = "adsf",
                        SectionOrder = 0
                    },
                    1, "01_FileLister", null, false, "/5 * * ? * *",
                    new Dictionary<string, string>()
                    {
                        {"jobSays", "HelloWorld"},
                    }
                )
                //here goes list item n, job=n
            };
            QuartzRunTimeConfig runTimeConfig = new QuartzRunTimeConfig(schedulerProperties,
                telcobrightProcessInstances);
            
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