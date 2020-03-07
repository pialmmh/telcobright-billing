﻿using TelcobrightMediation;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using Topshelf;
using MediationModel;
using Quartz;
using QuartzTelcobright;
using TelcobrightMediation.Scheduler.Quartz;
using System.Configuration;
using System.Diagnostics;
using CrystalQuartz.Core.SchedulerProviders;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using QuartzTelcobright.MefComposers;
using QuartzTelcobright.PropertyGen;
using Spring.Context;
using Spring.Context.Support;
using TelcobrightMediation.Config;
using LibraryExtensions;
using Quartz.Util;

namespace WS_Telcobright_Topshelf
{
    public class TopShelf
    {
        public MefCollectiveAssemblyComposer MefColllectiveAssemblyComposer { get; set; }

        static void Main(string[] args)
        {
            //todo: remove test code
            RemoteSchedulerProvider provider= new RemoteSchedulerProvider
            {
                SchedulerHost = "tcp://localhost:555/QuartzScheduler"
            };
            provider.Init();
            //

            ProcessParamater procParam = new ProcessParamater(1000);
            string json = JsonConvert.SerializeObject(procParam);
            HostFactory.Run(hostConfigurator =>
            {
                hostConfigurator.Service<TelcobrightService>(serviceConfigurator =>
                {
                    serviceConfigurator.ConstructUsing(() => new TelcobrightService());
                    serviceConfigurator.WhenStarted(myService => myService.Start());
                    serviceConfigurator.WhenStopped(myService => myService.Stop());
                });

                hostConfigurator.RunAsLocalSystem();

                hostConfigurator.SetDisplayName("Telcobright");
                hostConfigurator.SetDescription("Telcobright Windows Service");
                hostConfigurator.SetServiceName("Telcobright");
            });
        }
    }

    enum SchedulerRunTimeType
    {
        Runtime,
        Debug
    }

    public class TelcobrightService
    {
        public TelcobrightService()
        {
            try
            {
                Console.WriteLine("Starting Telcobright Scheduler.");
                var timer = new Timer(1000) {AutoReset = false, Enabled = true};
                timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
                //TimerAction();
                Console.WriteLine("Program Exited.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        static void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                TimerAction();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }

        }

        static void TimerAction()
        {
            try
            {
                IApplicationContext
                    springContext = ContextRegistry.GetContext(); //don't use "using", it auto disposes spring context
                MefProcessContainer mefProcessContainer = GetMefProcessContainerFromIoC(springContext);
                Dictionary<string, TelcobrightConfig> operatorWiseConfigs = GetTelcobrightConfigs();
                IScheduler runtimeScheduler = null;
                try
                {
                    runtimeScheduler = GetScheduler(SchedulerRunTimeType.Runtime, springContext);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Unable to bind"))
                    {
                        Console.WriteLine("Unable to start debug scheduler, " +
                                          "telcobright service needs to be turned off.");
                    }
                    Console.Read();
                    Environment.Exit(1);
                }

#if DEBUG
                if (Debugger.IsAttached)
                {
                    Console.WriteLine("Starting RAMJobStore based scheduler in debug mode....");
                    runtimeScheduler.Standby();
                    IScheduler debugScheduler = GetScheduler(SchedulerRunTimeType.Debug, springContext);
                    ScheduleDebugJobsThroughMenu(runtimeScheduler, debugScheduler);
                    debugScheduler.Context.Put("processes", mefProcessContainer);
                    debugScheduler.Context.Put("configs", operatorWiseConfigs);
                    debugScheduler.Start();
                    Console.WriteLine("Telcobright Scheduler has been started in debug mode.");
                    return;
                }
#endif
                Console.WriteLine("Starting Scheduler in runtime mode...");
                runtimeScheduler.ResumeTriggers(GroupMatcher<TriggerKey>.AnyGroup());
                PauseNonSelectedTrigggersThroughMenu(runtimeScheduler);
                runtimeScheduler.Context.Put("processes", mefProcessContainer);
                runtimeScheduler.Context.Put("configs", operatorWiseConfigs);
                runtimeScheduler.Start();
                Console.WriteLine("Telcobright Scheduler has been started in runtime mode.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static MefProcessContainer GetMefProcessContainerFromIoC(IApplicationContext springContext)
        {
            var mefProcessContainer = (MefProcessContainer) springContext.GetObject("mefProcessContainer");
            return mefProcessContainer;
        }

        private static Dictionary<string, TelcobrightConfig> GetTelcobrightConfigs()
        {
            bool disableParallelMediationForDebug =
                Convert.ToBoolean(ConfigurationManager.AppSettings["disableParallelMediationForDebug"]);
            DirectoryInfo dir =
                new DirectoryInfo(ExecutablePathFinder.GetBinPath() + Path.DirectorySeparatorChar + "config");
            Dictionary<string, TelcobrightConfig> operatorWiseConfigs = new Dictionary<string, TelcobrightConfig>();
            foreach (FileInfo configFile in dir.GetFiles())
            {
                string configFileNameAsOperatorName = Path.GetFileNameWithoutExtension(configFile.Name);
                TelcobrightConfig tbc = ConfigFactory.GetConfigFromFile(configFile.FullName);
                //populate tbCustomer name
                string entityConStr =
                    ConnectionManager.GetEntityConnectionStringByOperator(tbc.DatabaseSetting.DatabaseName);
                using (PartnerEntities context = new PartnerEntities(entityConStr))
                {
                    telcobrightpartner tbPartner = context.telcobrightpartners
                        .Where(p => p.databasename == tbc.DatabaseSetting.DatabaseName).ToList().First();
                }
                if (Debugger.IsAttached)
                {
                    tbc.CdrSetting.DisableParallelMediation = disableParallelMediationForDebug;
                }
                operatorWiseConfigs.Add(configFileNameAsOperatorName, tbc);
            }
            return operatorWiseConfigs;
        }

        static IScheduler GetScheduler(SchedulerRunTimeType runTimeType, IApplicationContext springContext)
        {
            QuartzPropertyFactory quartzPropertyFactoryRuntime;
            QuartzPropertyFactory quartzPropertyFactoryDebug;
            NameValueCollection schedulerProperties = null;
            var mefProcessContainer = (MefProcessContainer) springContext.GetObject("mefProcessContainer");
            if (runTimeType == SchedulerRunTimeType.Runtime)
            {
                quartzPropertyFactoryRuntime =
                    (QuartzPropertyFactory) springContext.GetObject("quartzPropertyFactoryRuntime");
                schedulerProperties = quartzPropertyFactoryRuntime.GetProperties();
                IScheduler scheduler = QuartzSchedulerFactory.CreateSchedulerInstance(schedulerProperties);
                return scheduler;
            }
            else if (runTimeType == SchedulerRunTimeType.Debug)
            {
                //quartzPropertyFactoryDebug =
                //    (QuartzPropertyFactory)springContext.GetObject("quartzPropertyFactoryDebug");
                //schedulerProperties = quartzPropertyFactoryDebug.GetProperties();
                ISchedulerFactory schedFact = new StdSchedulerFactory();
                IScheduler debugScheduler = schedFact.GetScheduler();
                return debugScheduler;
            }
            return null;
        }

        private static void ScheduleDebugJobsThroughMenu(IScheduler runtimeScheduler, IScheduler debugScheduler)
        {
            List<TriggerKey> triggerKeysForDebug = GetSelectedTriggerKeysFromMenu(runtimeScheduler, selectToPause: false)
                .OrderBy(t => t.Name).ToList();
            ScheduleDebugJobs(runtimeScheduler, debugScheduler, triggerKeysForDebug);
        }
        private static void PauseNonSelectedTrigggersThroughMenu(IScheduler runtimeScheduler)
        {
            List<TriggerKey> triggerKeysForDebug = GetSelectedTriggerKeysFromMenu(runtimeScheduler,selectToPause: true);
            PauseNonSelectedTriggers(runtimeScheduler, triggerKeysForDebug);
        }
        static List<TriggerKey> GetSelectedTriggerKeysFromMenu(IScheduler runtimeScheduler,bool selectToPause)
        {
            List<TriggerKey> triggersKeys = runtimeScheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup())
                                .ToList();
            List<ITrigger> triggers = triggersKeys.Select(tk => runtimeScheduler.GetTrigger(tk)).ToList();
            List<int> selectedtriggerNumbersFromConsole = DisplayMenu(triggers);
            if (selectedtriggerNumbersFromConsole.Count > 0)
            {
                if (selectToPause == false)
                {
                    triggersKeys = triggersKeys
                        .Where((item, index) => selectedtriggerNumbersFromConsole.Contains(index))
                        .Select(t => t).ToList();
                }
                else
                {
                    triggersKeys = triggersKeys.Where((item, index) => !selectedtriggerNumbersFromConsole.Contains(index))
                        .Select(t => t).ToList();
                }
            }
            return triggersKeys;
        }

        static int ScheduleDebugJobs(IScheduler runtimeScheduler, IScheduler debugScheduler,
            List<TriggerKey> triggerKeysForDebug)
        {
            int jobCount = 0;
            if (triggerKeysForDebug.Count > 0)
            {
                List<string> groupNames = runtimeScheduler.GetJobGroupNames().ToList();
                foreach (string groupName in groupNames)
                {
                    List<JobKey> jobKeys = runtimeScheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName))
                        .ToList();
                    List<IJobDetail> jobDetails = jobKeys.Select(c => runtimeScheduler.GetJobDetail(c)).ToList();
                    foreach (IJobDetail jobDetail in jobDetails)
                    {
                        //var builtJob=ReBuildJob<QuartzTelcobrightProcessWrapper>(jobDetail, groupName);
                        ITrigger trigger = runtimeScheduler.GetTriggersOfJob(jobDetail.Key).First();
                        if (triggerKeysForDebug.Contains(trigger.Key))
                        {
                            debugScheduler.ScheduleJob(jobDetail, trigger);
                            jobCount++;
                        }
                    }
                }
            }
            if (debugScheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()).Count == jobCount)
            {
                return jobCount;
            }
            else throw new Exception("Scheduled job count did not match expected job count for debug scheduler.");
        }

        static void PauseNonSelectedTriggers(IScheduler runtimeScheduler, List<TriggerKey> triggerKeysToPause)
        {
            if (triggerKeysToPause.Count > 0)
            {
                List<string> groupNames = runtimeScheduler.GetJobGroupNames().ToList();
                foreach (string groupName in groupNames)
                {
                    List<JobKey> jobKeys = runtimeScheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName))
                        .ToList();
                    List<IJobDetail> jobDetails = jobKeys.Select(c => runtimeScheduler.GetJobDetail(c)).ToList();
                    foreach (IJobDetail jobDetail in jobDetails)
                    {
                        //var builtJob=ReBuildJob<QuartzTelcobrightProcessWrapper>(jobDetail, groupName);
                        ITrigger trigger = runtimeScheduler.GetTriggersOfJob(jobDetail.Key).First();
                        if (triggerKeysToPause.Contains(trigger.Key))
                        {
                            runtimeScheduler.PauseTrigger(trigger.Key);
                        }
                    }
                }
            }
        }

        //static IJobDetail ReBuildJob<T>(IJobDetail jobDetail,string groupName) where T: Quartz.IJob
        //{
        //    return JobBuilder.Create<T>()
        //        .WithIdentity(jobDetail.Key.Name, groupName)
        //        .UsingJobData(jobDetail.JobDataMap)
        //        .Build();
        //}
        private static List<int> DisplayMenu(List<ITrigger> triggers)
        {
            Console.Clear();
            Console.WriteLine("Enter trigger numbers to debug with scheduler, separated by comma...");
            Console.WriteLine("Press Enter to resume all...");
            for (var index = 0; index < triggers.Count; index++)
            {
                ITrigger t = triggers[index];
                Console.WriteLine((index + 1) + ". " + t.Key); //display from 1, keep 0 for all
            }
            var readLine = Console.ReadLine();
            if (readLine.IsNullOrEmptyOrWhiteSpace())
            {
                return new List<int>();
            }
            return readLine.Split(',').Select(c => Convert.ToInt32(c))
                .Select(c => c - 1).ToList().ToList(); //displayed menu items are 1 based, change to 0 based choise
        }

        static ConsoleKeyInfo WaitForkeyPressForDebugMode()
        {
            ConsoleKeyInfo k = new ConsoleKeyInfo();
            Console.WriteLine("Press 'D' in the next 3 seconds for entering scheduler debug mode...");
            for (int cnt = 3; cnt > 0; cnt--)
            {
                if (Console.KeyAvailable == true)
                {
                    k = Console.ReadKey();
                    break;
                }
                else
                {
                    Console.WriteLine(cnt.ToString());
                    System.Threading.Thread.Sleep(1000);
                }
            }
            Console.WriteLine();
            Console.WriteLine("The key pressed was " + k.Key);
            return k;
        }

        public void Stop()
        {

        }

        public void Start()
        {
            try
            {

            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.ToString());
                var logFileName = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "telcobright.log";
                File.WriteAllText(logFileName,
                    e1.Message + Environment.NewLine + (e1.InnerException != null ? e1.InnerException.ToString() : ""));
            }
        }
    }
}
