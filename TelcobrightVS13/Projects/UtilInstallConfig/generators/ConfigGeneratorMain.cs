using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using TelcobrightFileOperations;
using System.Reflection;
using System.Text;
using DocumentFormat.OpenXml.Drawing;
using InstallConfig._generator;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;
using MediationModel;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Quartz;
using QuartzTelcobright;
using QuartzTelcobright.PropertyGen;
using TelcobrightMediation.Config;
using TelcobrightMediation.Scheduler.Quartz;
using Path = System.IO.Path;
using MediationModel;
using LibraryExtensions.ConfigHelper;
using TelcobrightInfra;

//using CrystalQuartzTest;
namespace InstallConfig
{
    class ConfigGeneratorMain
    {
        private static AutomationContainer automationContainer = new AutomationContainer();
        static List<InstanceConfig> instanceConfigs = new List<InstanceConfig>();
        static void Main(string[] args)
        {
            automationContainer.Compose();
            IAutomation winAutomation = automationContainer.Automations["WinLocalShellAutomation"];
            List<MySqlUser> mysqlUsers = new List<MySqlUser>()
            {
                new MySqlUser("root","123456",
                                new List<string>() {"localhost", "10.0.0.29"},
                                new List<MySqlPermission>()
                                {
                                    new MySqlPermission(
                                        new List<MySqlPermissionType>
                                        {
                                            MySqlPermissionType.all,
                                            MySqlPermissionType.execute
                                        }, "summit")
                                })
            };
            List<string> commandSequence= MySqlAutomationHelper.createOrAlterUser(mysqlUsers,runFromShell: true);

            /*List<string> commandSequence= new List<string>()
            {
                @"cd c:\mysql\bin",
                @"dir"
            };*/
            Dictionary<string, object> executionData = new Dictionary<string, object>()
            {
                {"commandSequence", commandSequence},
                { "workingDirectory", @"c:\mysql\bin"}
            };
            //winAutomation.execute(executionData);;
            
            ConsoleUtil consoleUtil= new ConsoleUtil(new List<char>() {'y', 'Y'});
            List<Deploymentprofile> deploymentProfiles = AllDeploymenProfiles.getDeploymentprofiles();
                
            string selectedProfileName= Menu.getSingleChoice(deploymentProfiles.Select(dp=>dp.profileName).ToList(),
                "Select a deployment profile to configure automation.");
            Deploymentprofile deploymentprofile = deploymentProfiles.First(p => p.profileName == selectedProfileName);
            List<string> instanceNames = deploymentprofile.instances
                .Where(i=>i.Skip==false)
                .Select(i => i.Name).ToList();
            switch (deploymentprofile.type)
            {
                case DeploymentProfileType.TelcoBilling:
                    List<TelcobrightConfig> tbcs =
                        TelcoBillingConfigGenerator.getSelectedOperatorsConfig(deploymentprofile);
                    foreach (var tbc in tbcs)
                    {
                        tbc.Deploymentprofile = deploymentprofile;
                    }
                    TelcoBillingMenu telcoBillingMenu =
                        new TelcoBillingMenu(tbcs, consoleUtil);
                    telcoBillingMenu.showMenu();
                    break;
                default:
                    break;
            }

        }

        

        
        
        private static Dictionary<string, string> GetDeploymentInstanceToMenuItems()
        {
            DirectoryInfo utilDir = (new DirectoryInfo(FileAndPathHelper.GetCurrentExecPath()).Parent).Parent;
            string deploymentProfile = ConfigurationManager.AppSettings.ToDictionary()
                                        .First(kv => kv.Key.Equals("deploymentProfile")).Value;
            string deployJson = utilDir.FullName + Path.DirectorySeparatorChar
                + "deployment" + Path.DirectorySeparatorChar + $"{deploymentProfile}.json";
            List<InstanceConfig> instanceConfigs =
                JsonConvert.DeserializeObject<List<InstanceConfig>>(File.ReadAllText(deployJson));
            Dictionary<string, string> keyValuesForMenu = new Dictionary<string, string>();
            for (int i = 1; i <= instanceConfigs.Count; i++)
            {
                keyValuesForMenu.Add(i.ToString(), instanceConfigs[i - 1].Name);
            }

            return keyValuesForMenu;
        }
    }
}
