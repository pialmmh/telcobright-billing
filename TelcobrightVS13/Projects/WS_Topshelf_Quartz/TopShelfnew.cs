using TelcobrightMediation;
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
    enum SchedulerRunTimeType
    {
        Runtime,
        Debug
    }

    public class TelcobrightService
    {
        public static MefCollectiveAssemblyComposer mefColllectiveAssemblyComposer { get; set; }
        public static MefProcessContainer mefProcessContainer { get; set; }
        static void Main(string[] args)
        {
            //todo: remove test code
            mefColllectiveAssemblyComposer = new MefCollectiveAssemblyComposer("..//..//bin//Extensions//");
            RemoteSchedulerProvider provider = new RemoteSchedulerProvider
            {
                SchedulerHost = "tcp://localhost:555/QuartzScheduler"
            };
            provider.Init();

            try
            {
                Console.WriteLine("Starting Telcobright Scheduler.");
                //var timer = new Timer(1000) { AutoReset = false, Enabled = true };
                //timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
                TimerAction();
                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();
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
                //IApplicationContext
                //  springContext = ContextRegistry.GetContext(); //don't use "using", it auto disposes spring context
                //MefProcessContainer mefProcessContainer = GetMefProcessContainerFromIoC(springContext);
                mefProcessContainer = new MefProcessContainer(mefColllectiveAssemblyComposer);
                //     < object name = "mefProcessContainer" type = "QuartzTelcobright.MefComposers.MefProcessContainer,QuartzTelcobright" singleton = "true" >
                //< constructor - arg name = "mefCollectiveAssemblyComposer" ref= "mefCollectiveAssemblyComposer" />
                //   </ object >

                Dictionary<string, TelcobrightConfig> operatorWiseConfigs = GetTelcobrightConfigs();
                IScheduler runtimeScheduler = null;
                try
                {
                    runtimeScheduler = GetScheduler(SchedulerRunTimeType.Runtime);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Unable to bind"))
                    {
                        Console.WriteLine("Unable to start debug scheduler, " +
                                          "telcobright service needs to be turned off.");
                    }
                    throw (e);
                }
                Console.WriteLine("Starting RAMJobStore based scheduler....");
                runtimeScheduler.Standby();
                IScheduler debugScheduler = GetScheduler(SchedulerRunTimeType.Debug);
                ScheduleDebugJobsThroughMenu(runtimeScheduler, debugScheduler);
                debugScheduler.Context.Put("processes", mefProcessContainer);
                debugScheduler.Context.Put("configs", operatorWiseConfigs);
                debugScheduler.Start();
                Console.WriteLine("Telcobright Scheduler has been started.");
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static MefProcessContainer GetMefProcessContainerFromIoC(IApplicationContext springContext)
        {
            var mefProcessContainer = (MefProcessContainer)springContext.GetObject("mefProcessContainer");
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
                //string entityConStr =
                //  ConnectionManager.GetEntityConnectionStringByOperator(tbc.DatabaseSetting.DatabaseName, tbc);
                string entityConStr =
                    ConnectionManager.GetEntityConnectionStringByOperator(configFileNameAsOperatorName, tbc);
                using (PartnerEntities context = new PartnerEntities(entityConStr))
                {
                    var operatorShortName = tbc.DatabaseSetting.GetOperatorName;
                    telcobrightpartner tbPartner = context.telcobrightpartners
                        .Where(p => p.databasename == operatorShortName).ToList().First();
                }
                if (Debugger.IsAttached)
                {
                    tbc.CdrSetting.DisableParallelMediation = disableParallelMediationForDebug;
                }
                operatorWiseConfigs.Add(configFileNameAsOperatorName, tbc);
            }
            return operatorWiseConfigs;
        }

        static IScheduler GetScheduler(SchedulerRunTimeType runTimeType)// IApplicationContext springContext)
        {
            QuartzPropertyFactory quartzPropertyFactoryRuntime;
            QuartzPropertyFactory quartzPropertyFactoryDebug;
            NameValueCollection schedulerProperties = null;
            if (runTimeType == SchedulerRunTimeType.Runtime)
            {
                //quartzPropertyFactoryRuntime =
                //    (QuartzPropertyFactory) springContext.GetObject("quartzPropertyFactoryRuntime");
                QuartzPropGenRemoteSchedulerAdoRuntime quartzPropGenRemoteSchedulerAdoRuntime =
                    new QuartzTelcobright.PropertyGen.QuartzPropGenRemoteSchedulerAdoRuntime(555, "scheduler");

                quartzPropertyFactoryRuntime =
                    new QuartzPropertyFactory(quartzPropGenRemoteSchedulerAdoRuntime);


                //quartzPropertyFactoryRuntime =
                //  (QuartzPropertyFactory)springContext.GetObject("quartzPropertyFactoryRuntime");

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
            List<TriggerKeyExt> triggerKeysForDebug = GetSelectedTriggerKeysFromMenu(runtimeScheduler)
                .OrderBy(t => t.TriggerKey.Name).ToList();
            ScheduleDebugJobs(runtimeScheduler, debugScheduler, triggerKeysForDebug);
        }
        private static void PauseNonSelectedTrigggersThroughMenu(IScheduler runtimeScheduler)
        {
            List<TriggerKeyExt> triggerKeysForDebug = GetSelectedTriggerKeysFromMenu(runtimeScheduler);
            PauseNonSelectedTriggers(runtimeScheduler, triggerKeysForDebug);
        }
        static List<TriggerKeyExt> GetSelectedTriggerKeysFromMenu(IScheduler runtimeScheduler)
        {
            List<TriggerKey> triggersKeys = runtimeScheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup()).ToList();
            List<ITrigger> triggers = triggersKeys.Select(tk => runtimeScheduler.GetTrigger(tk)).ToList();
            Dictionary<int, string> selectedtriggersFromConsoleWithArgs = DisplayMenu(triggers);
            List<TriggerKeyExt> selectedTriggerKeysFinal = new List<TriggerKeyExt>();
            if (selectedtriggersFromConsoleWithArgs.Count > 0)
            {
                selectedTriggerKeysFinal = triggersKeys
                        .Where((item, index) => selectedtriggersFromConsoleWithArgs.Select(kv => kv.Key).Contains(index))
                        .Select((t, index) => {
                            string argsStr = "";
                            selectedtriggersFromConsoleWithArgs.TryGetValue(index, out argsStr);
                            return new TriggerKeyExt(t, argsStr);
                        }).ToList();

            }
            return selectedTriggerKeysFinal;
        }

        static int ScheduleDebugJobs(IScheduler runtimeScheduler, IScheduler debugScheduler,
            List<TriggerKeyExt> triggerKeysForDebug)
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
                        if (triggerKeysForDebug.Select(t => t.TriggerKey).Contains(trigger.Key))
                        {
                            Dictionary<string, string> argsTelcobright = triggerKeysForDebug.Where(t => t.TriggerKey == trigger.Key)
                                .FirstOrDefault()?.ArgsTelcobright;
                            if (argsTelcobright != null)
                            {
                                jobDetail.JobDataMap.Put("args", argsTelcobright);
                            }
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

        static void PauseNonSelectedTriggers(IScheduler runtimeScheduler, List<TriggerKeyExt> triggerKeysToPause)
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
                        if (triggerKeysToPause.Select(t => t.TriggerKey).Contains(trigger.Key))
                        {
                            runtimeScheduler.PauseTrigger(trigger.Key);
                        }
                    }
                }
            }
        }
        
        private static Dictionary<int, string> DisplayMenu(List<ITrigger> triggers)
        {
            Console.Clear();
            Console.WriteLine("Enter trigger numbers to debug with scheduler, separated by comma...");
            Console.WriteLine("Press Enter to resume all...");
            string choices = "";
            for (var index = 0; index < triggers.Count; index++)
            {
                ITrigger t = triggers[index];
                Console.WriteLine((index + 1) + ". " + t.Key); //display from 1, keep 0 for all
            }
            choices = Console.ReadLine();
            if (choices.IsNullOrEmptyOrWhiteSpace())
            {
                choices = string.Join(",", Enumerable.Range(1, triggers.Count).Select(num => num.ToString()));
            }
            return choices.Split(',').Select(keyWithArgs =>
            {
                var arr = keyWithArgs.Split(null).Select(item => item.Trim()).ToArray();
                int key = Convert.ToInt32(arr[0]) - 1;//displayed menu items are 1 based, change to 0 based choise
                string args = arr.Length > 1 ? arr[1] : "";
                return new KeyValuePair<int, string>(key, args);
            }).ToDictionary(kv => kv.Key, kv => kv.Value);
            //        .Select(c => c - 1).ToList().ToList(); //displayed menu items are 1 based, change to 0 based choise
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
