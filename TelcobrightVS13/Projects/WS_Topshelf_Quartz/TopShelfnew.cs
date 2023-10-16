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
using TelcobrightInfra;

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
            string deploymentRoot =
                @"C:\sftproot\TelcobrightProject\TelcobrightVS13\Projects\WS_Topshelf_Quartz\deployedInstances";
            string configFileName = "";

            if (configFileName == "")
            {
                ConfigPathHelper configPathHelper = new ConfigPathHelper(
                    "WS_Topshelf_Quartz",
                    "portal",
                    "UtilInstallConfig",
                    "generators", "");
                string deployedInstancesPath = configPathHelper.GetDeployedInstancesDir();
                DirectoryInfo deployDir = new DirectoryInfo(deployedInstancesPath);
                Dictionary<string, string> operatorNameVsConfigFile = new Dictionary<string, string>();
                foreach (DirectoryInfo dir in deployDir.GetDirectories())
                {
                    string operatorName = dir.Name;
                    string cnfFileName = dir.FullName + Path.DirectorySeparatorChar +
                                         dir.Name + ".conf";
                    operatorNameVsConfigFile.Add(operatorName, cnfFileName);
                }
                Menu menu = new Menu(operatorNameVsConfigFile.Keys.ToList(),
                    "Select an Operatorname to debug.", "");
                string selectedOpName = menu.getSingleChoice();
                configFileName = operatorNameVsConfigFile[selectedOpName];
            }



            //Telcobright2 tb = new Telcobright2($"{deploymentRoot}\\mothertelecom_cas\\mothertelecom_cas.conf", null);
            Telcobright2 tb = new Telcobright2(configFileName, null);
            tb.run();

        }
        private static string getLogFileName()
        {
            var binPath = System.IO.Path.GetDirectoryName(
                            System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            binPath = binPath.Substring(6);
            string logFileName = binPath + Path.DirectorySeparatorChar + "telcobright.log";
            return logFileName;
        }
        static string getLastGeneratedConfigFileName()
        {
            string execPath = FileAndPathHelper.GetCurrentExecPath();
            DirectoryInfo configDir = new DirectoryInfo(new DirectoryInfo(execPath).Parent.FullName +
                                                        Path.DirectorySeparatorChar + "Config");
            string configFileName = configDir.FullName + Path.DirectorySeparatorChar + "telcobright.conf";
            return configFileName;
        }
        private static TelcobrightConfig GetTelcobrightConfig(string configFileName)
        {
            bool disableParallelMediationForDebug =
                Convert.ToBoolean(ConfigurationManager.AppSettings["disableParallelMediationForDebug"]);
            
            TelcobrightConfig tbc = ConfigFactory.GetConfigFromFile(configFileName);
            if (Debugger.IsAttached)
            {
                tbc.CdrSetting.DisableParallelMediation = disableParallelMediationForDebug;
            }
            return tbc;
        }
        static IScheduler GetScheduler(SchedulerRunTimeType runTimeType,TelcobrightConfig tbc)// IApplicationContext springContext)
        {
            QuartzPropertyFactory quartzPropertyFactoryRuntime;
            QuartzPropertyFactory quartzPropertyFactoryDebug;
            NameValueCollection schedulerProperties = null;

            if (runTimeType == SchedulerRunTimeType.Runtime)
            {

                QuartzPropGenRemoteSchedulerAdoRuntime quartzPropGenRemoteSchedulerAdoRuntime =
                    new QuartzTelcobright.PropertyGen.QuartzPropGenRemoteSchedulerAdoRuntime(tbc.TcpPortNoForRemoteScheduler
                    , DbUtil.getDbConStrWithDatabase(tbc.DatabaseSetting));

                quartzPropertyFactoryRuntime =
                    new QuartzPropertyFactory(quartzPropGenRemoteSchedulerAdoRuntime);
                schedulerProperties = quartzPropertyFactoryRuntime.GetProperties();
                var factory = new QuartzSchedulerFactory(schedulerProperties);
                IScheduler scheduler = factory.CreateSchedulerInstance();
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
                                jobDetail.JobDataMap.Put("name", "apple");
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
            return choices.Split(',').Select(c=>c.Trim()).Select(keyWithArgs =>
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
