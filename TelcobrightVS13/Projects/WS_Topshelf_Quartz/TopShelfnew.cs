using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using MediationModel;
using Quartz;
using QuartzTelcobright;
using System.Configuration;
using System.Diagnostics;
using CrystalQuartz.Core.SchedulerProviders;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using QuartzTelcobright.MefComposers;
using QuartzTelcobright.PropertyGen;
using TelcobrightMediation.Config;
using LibraryExtensions;

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
        private static string getLogFileName()
        {
            var binPath = System.IO.Path.GetDirectoryName(
                            System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            binPath = binPath.Substring(6);
            string logFileName = binPath + Path.DirectorySeparatorChar + "telcobright.log";
            return logFileName;
        }
        static void Main(string[] args)
        {
            string logFileName=getLogFileName();
            mefColllectiveAssemblyComposer = new MefCollectiveAssemblyComposer("..//..//bin//Extensions//");
            RemoteSchedulerProvider provider = new RemoteSchedulerProvider
            {
                SchedulerHost = "tcp://localhost:555/QuartzScheduler"
            };
            provider.Init();
            File.WriteAllLines(logFileName, new string[] { DateTime.Now.ToMySqlFormatWithoutQuote() + ": Telcobright started at " + provider.SchedulerHost } );
            try
            {
                Console.WriteLine("Starting Telcobright Scheduler.");
                mefProcessContainer = new MefProcessContainer(mefColllectiveAssemblyComposer);
                Dictionary<string, TelcobrightConfig> operatorWiseConfigs = GetTelcobrightConfigs();
                IScheduler runtimeScheduler = null;
                try
                {
                    runtimeScheduler = GetScheduler(SchedulerRunTimeType.Runtime,operatorWiseConfigs.First().Value);
                }
                catch (Exception e1)
                {
                    if (e1.Message.Contains("Unable to bind"))
                    {
                        Console.WriteLine("Unable to start debug scheduler, " +
                                          "telcobright service needs to be turned off.");
                    }
                    throw (e1);
                }
                Console.WriteLine("Starting RAMJobStore based scheduler....");
                runtimeScheduler.Standby();
                IScheduler debugScheduler = GetScheduler(SchedulerRunTimeType.Debug, operatorWiseConfigs.First().Value);
                ScheduleDebugJobsThroughMenu(runtimeScheduler, debugScheduler);
                debugScheduler.Context.Put("processes", mefProcessContainer);
                debugScheduler.Context.Put("configs", operatorWiseConfigs);
                debugScheduler.Start();
                Console.WriteLine("Telcobright Scheduler has been started.");
                Console.ReadLine();
                Console.WriteLine("Program Exited.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

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
                string entityConStr =
                    ConnectionManager.GetEntityConnectionStringByOperator(configFileNameAsOperatorName, tbc);
                using (PartnerEntities context = new PartnerEntities(entityConStr))
                {
                    var operatorShortName = tbc.Telcobrightpartner.databasename;
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

        static IScheduler GetScheduler(SchedulerRunTimeType runTimeType,TelcobrightConfig tbc)// IApplicationContext springContext)
        {
            QuartzPropertyFactory quartzPropertyFactoryRuntime;
            QuartzPropertyFactory quartzPropertyFactoryDebug;
            NameValueCollection schedulerProperties = null;
            if (runTimeType == SchedulerRunTimeType.Runtime)
            {
                
                QuartzPropGenRemoteSchedulerAdoRuntime quartzPropGenRemoteSchedulerAdoRuntime =
                    new QuartzTelcobright.PropertyGen.QuartzPropGenRemoteSchedulerAdoRuntime(555, tbc.DatabaseSetting.DatabaseName);

                quartzPropertyFactoryRuntime =
                    new QuartzPropertyFactory(quartzPropGenRemoteSchedulerAdoRuntime);
                schedulerProperties = quartzPropertyFactoryRuntime.GetProperties();
                IScheduler scheduler = QuartzSchedulerFactory.CreateSchedulerInstance(schedulerProperties);
                return scheduler;
            }
            else if (runTimeType == SchedulerRunTimeType.Debug)
            {
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
            string choices = "";
            for (var index = 0; index < triggers.Count; index++)
            {
                ITrigger t = triggers[index];
                Console.WriteLine((index + 1) + ". " + t.Key); //display from 1, keep 0 for all
            }
            bool keyWasPressedForMenu = WaitForkeyPressForDebugMode();
            if (keyWasPressedForMenu)
            {
                Console.Write("\r{0}   ", "Enter trigger numbers to start process selectively, separated by comma...    ");
                Console.WriteLine("Press Enter to start all...");
                choices = Console.ReadLine();
                if (choices.IsNullOrEmptyOrWhiteSpace())
                {
                    choices = string.Join(",", Enumerable.Range(1, triggers.Count).Select(num => num.ToString()));
                }
            }
            else
            {//no key was pressed...
                Console.WriteLine(Environment.NewLine + "No keys were pressed, starting all process...");
                choices = string.Join(",", Enumerable.Range(1, triggers.Count).Select(num => num.ToString()));
            }
            return choices.Split(',').Select(keyWithArgs =>
            {
                var arr = keyWithArgs.Split(null).Select(item => item.Trim()).ToArray();
                int key = Convert.ToInt32(arr[0]) - 1;//displayed menu items are 1 based, change to 0 based choise
                string args = arr.Length > 1 ? arr[1] : "";
                return new KeyValuePair<int, string>(key, args);
            }).ToDictionary(kv => kv.Key, kv => kv.Value);
        }
        static bool WaitForkeyPressForDebugMode()
        {
            ConsoleKeyInfo k= new ConsoleKeyInfo();
            int pressMenuWithinSec = 5;
            Console.Write("\r{0}   ", $"Press any key within {pressMenuWithinSec} seconds to selectively run processes from menu...");
            for (int cnt = pressMenuWithinSec; cnt > -1; cnt--)
            {
                if (Console.KeyAvailable == true)
                {
                    k = Console.ReadKey();
                    return true;
                }
                else
                {
                    Console.Write("\r{0}   ", $"Press any key within {cnt} seconds to selectively run processes from menu...");
                    System.Threading.Thread.Sleep(1000);
                }
            }
            return false;
        }
    }
}
