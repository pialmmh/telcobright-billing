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
//using CrystalQuartzTest;
namespace InstallConfig
{
    internal enum RouteType
    {
        National=1,
        International=2
    }
    internal class ParnerRouteImportInfo
    {
        public string PartnerName { get; set; }
        public int IdPartner { get; set; }
        public int SwitchId{get; set; }
        public string DomesticTGs { get; set; }
        public string InternationalTGs { get; set; }
        //public string Description { get; set; }//can't use description as route naes are commaseparated in excel row
        public int Status { get; set; }
    }
    class ConfigGeneratorMain
    {
        private static AutomationContainer automationContainer = new AutomationContainer();
        static List<InstanceConfig> instanceConfigs = new List<InstanceConfig>();
        static void Main(string[] args)
        {
            automationContainer.Compose();
            List<string> deploymentProfiles = GetAllDeploymentInstanceNames();
            List<string> profiles= Menu.getChoices(deploymentProfiles,"Select a deployment profile to configure automation.");
            //try 
            {
            Start:
                string tbOperatorName = "summit";//todo: change
                Console.Clear();
                Console.WriteLine("Welcome to Telcobright Initial Configuration Utility");
                Console.WriteLine("Select Task:");
                Console.WriteLine("1=Setup mysql remote access.");
                Console.WriteLine("2=Append Prefix to Files");
                Console.WriteLine("3=[Not Set]");
                Console.WriteLine("4=Copy Portal to IIS Directory");
                Console.WriteLine("5=Not Set");
                Console.WriteLine("6=Generate Configuration & Reset Scheduler data");
                Console.WriteLine("7=Modify Partitions for tables");
                Console.WriteLine("q=Quit");

                ConsoleKeyInfo ki = new ConsoleKeyInfo();
                ki = Console.ReadKey(true);
                char cmdName = Convert.ToChar(ki.Key);
                ConfigPathHelper configPathHelper = new ConfigPathHelper("WS_Topshelf_Quartz", "portal", "UtilInstallConfig", "_dbscripts");
                DbUtil.configPathHelper = configPathHelper;

                List<TelcobrightConfig> selectedOperatorsConfig;
                List<string> menuItems = new List<string>();
                switch (cmdName)
                {
                    case '1':
                        Console.WriteLine("Setting up remote access for mysql...");
                        List<string> choices = Menu.getChoices(menuItems, "Select instances to create initial database:");
                        //choicesFromMenu = InstanceMenu.getInstancesFromMenu(keyValuesForMenu,"Select instances to create initial database:");
                        selectedOperatorsConfig = getSelectedOperatorsConfig(choices, configPathHelper);
                        foreach (var tbc in selectedOperatorsConfig)
                        {

                        }
                        //using(MySqlConnection con = new MySqlConnection())
                        break;
                    case '2':
                        Console.WriteLine("Enter Source Dir path & prefix without quotes, separated by comma...");
                        string str = Console.ReadLine();
                        string[] p = str.Split(',');
                        (new FileRename()).AppendPrefix(p[0], p[1]);
                        if (Convert.ToChar((Console.ReadKey(true)).Key) == 'q' || Convert.ToChar((Console.ReadKey(true)).Key) == 'Q') return;
                        break;
                    case '3':
                        if (Convert.ToChar((Console.ReadKey(true)).Key) == 'q' || Convert.ToChar((Console.ReadKey(true)).Key) == 'Q') return;
                        Console.WriteLine("Creating Database, none will be created if one exists.");
                        choices = Menu.getChoices(menuItems, "Select instances to create initial database:");
                        selectedOperatorsConfig = getSelectedOperatorsConfig(choices, configPathHelper);
                        foreach (var tbc in selectedOperatorsConfig)
                        {

                        }
                        break;
                    case '4':
                        Console.WriteLine("Copying Portal to c:/inetpub/wwwroot");
                        CopyPortal(tbOperatorName);
                        if (Convert.ToChar((Console.ReadKey(true)).Key) == 'q' || Convert.ToChar((Console.ReadKey(true)).Key) == 'Q') return;
                        break;
                    case '5':
                        if (Convert.ToChar((Console.ReadKey(true)).Key) == 'q' || Convert.ToChar((Console.ReadKey(true)).Key) == 'Q') return;
                        break;
                    case '6':
                        selectedOperatorsConfig = getSelectedOperatorsConfig(menuItems, configPathHelper);
                        if (!selectedOperatorsConfig.Any())
                        {
                            Console.WriteLine("No operator's config has been found. Press any key to start over.");
                            Console.ReadKey();
                            goto Start;
                        }
                        foreach (var tbc in selectedOperatorsConfig)
                        {
                            Console.WriteLine("Writing Configuration Files for " + tbc.Telcobrightpartner.CustomerName);
                            WriteConfig(tbc, configPathHelper);
                            Console.WriteLine("Config Files have been generated successfully for " + tbc.Telcobrightpartner.databasename);
                            Console.WriteLine("Create Telcobrightpartner and NE? (Y/N)");
                            ConsoleKeyInfo keyInfo1 = Console.ReadKey();
                            if (keyInfo1.KeyChar == 'Y' || keyInfo1.KeyChar == 'y')
                            {
                                PartnerEntities context = new PartnerEntities(ConnectionManager.GetEntityConnectionString(tbc.DatabaseSetting));
                                DbWriterForConfig.WriteTelcobrightPartnerAndNes(context, new List<telcobrightpartner>() { tbc.Telcobrightpartner },
                                    tbc.Nes);
                                Console.WriteLine();
                                Console.WriteLine("Telcobrightpartner and NE created successfully!");
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("Telcobrightpartner and NE was not created.");
                            }
                        }
                        //reset job store
                        Console.WriteLine("Reset QuartzJob Store (Y/N)? this will clear all job data.");
                        ConsoleKeyInfo keyInfo = Console.ReadKey();
                        if (keyInfo.KeyChar == 'Y' || keyInfo.KeyChar == 'y')
                        {
                            foreach (var tbc in selectedOperatorsConfig)
                            {
                                ConfigureQuartzJobStore(tbc); //configure job store for all opeartors
                            }
                            Console.WriteLine();
                            Console.WriteLine("Job store has been reset successfully.");
                        }
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("Job store was not reset.");
                        }
                        Console.WriteLine("Config generation is successful, press 'q' to quit");
                        var k = Convert.ToChar((Console.ReadKey(true)).Key);
                        if (k == 'q' || k == 'Q')
                        {
                            Environment.Exit(0);
                        }

                        break;
                    case '7':
                        choices = Menu.getChoices(menuItems, "Select instances to modify partitions:");
                        return;
                        //    schedulerType: "quartz",
                        //    databaseSetting: databaseSetting);
                        //PartitionUtil.ModifyPartitions(schedulerSetting.DatabaseSetting,operatorName);
                        Console.WriteLine("Partition modification is successful, press 'q' to quit");
                        k = Convert.ToChar((Console.ReadKey(true)).Key);
                        if (k == 'q' || k == 'Q')
                        {
                            Environment.Exit(0);
                        }
                        break;
                    case 'q':
                    case 'Q':
                        return;
                    default:
                        goto Start;
                }
                goto Start;
            }
            //catch (Exception e)
            //{
            //    Console.Write("Error: " + e.Message + Environment.NewLine + e.InnerException);
            //    Console.ReadLine();
            //}
        }

        private static Dictionary<string, string> GetDeploymentInstanceToMenuItems()
        {
            DirectoryInfo utilDir = (new DirectoryInfo(FileAndPathHelper.GetBinPath()).Parent).Parent;
            string deploymentProfile = ConfigurationManager.AppSettings.ToDictionary()
                                        .First(kv => kv.Key.Equals("deploymentProfile")).Value;
            string deployJson = utilDir.FullName + Path.DirectorySeparatorChar
                + "deployment" + Path.DirectorySeparatorChar + $"{deploymentProfile}.json";
            List<InstanceConfig> instanceConfigs =
                JsonConvert.DeserializeObject<List<InstanceConfig>>(File.ReadAllText(deployJson));
            Dictionary<string, string> keyValuesForMenu = new Dictionary<string, string>();
            for (int i = 1; i <= instanceConfigs.Count; i++)
            {
                keyValuesForMenu.Add(i.ToString(), instanceConfigs[i - 1].name);
            }

            return keyValuesForMenu;
        }

        private static List<string> GetAllDeploymentInstanceNames()
        {
            DirectoryInfo utilDir = (new DirectoryInfo(FileAndPathHelper.GetBinPath()).Parent).Parent;
            DirectoryInfo deploymentDir = new DirectoryInfo(utilDir.FullName +Path.DirectorySeparatorChar + "deployment"); //Assuming Test is your Folder
            FileInfo[] jsonFiles = deploymentDir.GetFiles("*.json"); //Getting Text files
            return jsonFiles
                .Select(j => JsonConvert.DeserializeObject<Deploymentprofile>(File.ReadAllText(j.FullName)))
                .Select(profile=>profile.profileName).ToList();
        }

        private static List<TelcobrightConfig> getSelectedOperatorsConfig(List<string> instances, ConfigPathHelper configPathHelper)
        {
            List<AbstractConfigConfigGenerator> operatorsToBeConfigured
                                        = new MefConfigImportComposer().Compose().Where(op => instances.Contains(op.Tbc.Telcobrightpartner.databasename)).ToList();
            List<TelcobrightConfig> operatorConfigs = new List<TelcobrightConfig>();
            DeletePrevConfigFilesForPortalAndWinService(configPathHelper);
            foreach (AbstractConfigConfigGenerator configGenerator in operatorsToBeConfigured)
            {
                //generate tbc & config file for each operator configure in app.config in installConfig
                TelcobrightConfig tbc = ConfigureSingleOperator(configGenerator, configPathHelper);
                tbc.SchedulerDaemonConfigs = configGenerator.GetSchedulerDaemonConfigs();
                operatorConfigs.Add(tbc);
            }
            return operatorConfigs;
        }

        static void ConfigureQuartzJobStore(TelcobrightConfig tbc)
        {
            DbUtil.CreateOrOverwriteQuartzTables(tbc.DatabaseSetting);   
            //read quartz config part for ALL configured operator (mef)
            int tcpPortNumber = tbc.TcpPortNoForRemoteScheduler;
            QuartzPropGenRemoteSchedulerAdoJobStore quartzPropGenRemoteSchedulerAdoJobStore =
                new QuartzPropGenRemoteSchedulerAdoJobStore(tcpPortNumber: tcpPortNumber);
            quartzPropGenRemoteSchedulerAdoJobStore.DatabaseSetting = tbc.DatabaseSetting;
            QuartzPropertyFactory quartzPropertyFactory =
                new QuartzPropertyFactory(quartzPropGenRemoteSchedulerAdoJobStore);
            NameValueCollection schedulerProperties = quartzPropertyFactory.GetProperties();
            IScheduler scheduler = QuartzSchedulerFactory.CreateSchedulerInstance(schedulerProperties);
            QuartzTelcobrightManager quartzManager = new QuartzTelcobrightManager(scheduler);
            quartzManager.ClearJobs(); //reset job store
            quartzManager.CreateJobs<QuartzTelcobrightProcessWrapper>(tbc.SchedulerDaemonConfigs);
        }

        static void ResetParnerRoutes(TelcobrightConfig tbc, 
            ConfigPathHelper configPathHelper)
        {
            string constr =
                "server=" + tbc.DatabaseSetting.ServerName + ";User Id=" + tbc.DatabaseSetting.AdminUserName +
                ";password=" + tbc.DatabaseSetting.AdminPassword + ";Persist Security Info=True;" +
                "database="+tbc.DatabaseSetting.DatabaseName+";";
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("", con))
                {
                    try
                    {
                        string routeImportFilename =
                            configPathHelper.GetOperatorWiseConfigDirInUtil(tbc.Telcobrightpartner.CustomerName)
                            + Path.DirectorySeparatorChar + tbc.Telcobrightpartner.CustomerName + "Routes.json";
                        List<ParnerRouteImportInfo> parnerRouteImportInfos =
                            JsonConvert.DeserializeObject<List<ParnerRouteImportInfo>>(
                                File.ReadAllText(routeImportFilename));
                        List<route> allRoutes = new List<route>();
                        Func<string, int, int, int,int,route> singleRouteCreator =
                            (routeName, switchId, idPartner, routeType,status) => new route()
                            {
                                RouteName = routeName,
                                SwitchId = switchId,
                                idPartner = idPartner,
                                NationalOrInternational = routeType,
                                Status = status
                            };
                        Func<ParnerRouteImportInfo, List<route>> multipleRoutesCreator = p =>
                        {
                            List<route> routes = new List<route>();
                            List<string> routeNames= p.DomesticTGs?.Split(',').Select(strRoute => strRoute
                                    .Trim()).ToList();
                            routeNames?.ForEach(tg => 
                                    routes.Add(singleRouteCreator(tg, p.SwitchId, p.IdPartner,
                                        (int)RouteType.National,p.Status)));
                            routeNames = p.InternationalTGs?.Split(',').Select(strRoute => strRoute
                                .Trim()).ToList();
                            routeNames?.ForEach(tg => 
                                    routes.Add(singleRouteCreator(tg, p.SwitchId, p.IdPartner,
                                        (int)RouteType.International,p.Status)));
                            return routes;
                        };
                        foreach (ParnerRouteImportInfo parnerRouteImportInfo in parnerRouteImportInfos)
                        {
                            allRoutes.AddRange(multipleRoutesCreator(parnerRouteImportInfo));
                        }
                        var duplicateRoutes = allRoutes.GroupBy(r =>
                                new StringBuilder(r.SwitchId.ToString()).Append("-").Append(r.RouteName).ToString())
                            .Where(g => g.Count() > 1)
                            .Select(g => g.Key).ToList();
                        if (duplicateRoutes.Any())
                        {
                            throw new NotSupportedException($@"Duplicate route names found for same switchId:
                                                                   {string.Join(",", duplicateRoutes)}");
                        }
                        cmd.CommandText = "set autocommit=0;";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "delete from route;";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = new StringBuilder(StaticExtInsertColumnHeaders.route)
                            .Append(string.Join(",", allRoutes.Select(c => c.GetExtInsertValues()).ToList()))
                            .ToString();
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "commit;";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        cmd.CommandText = "rollback;";
                        cmd.ExecuteNonQuery();
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine("Routes have been reset successfully for " + tbc.Telcobrightpartner.CustomerName + ".");
            
        }

        

        


        
        private static TelcobrightConfig ConfigureSingleOperator(AbstractConfigConfigGenerator abstractConfigConfigGenerator,
            ConfigPathHelper configPathHelper)
        {
            Console.WriteLine("Generating Configuration for " + abstractConfigConfigGenerator.Tbc.Telcobrightpartner.CustomerName);
            TelcobrightConfig tbc = abstractConfigConfigGenerator.GenerateConfig();
            return tbc;
        }

        static void DeletePrevConfigFilesForPortalAndWinService(ConfigPathHelper configPathHelper)
        {
            string targetDir = configPathHelper.GetTopShelfConfigDir();
            FileAndPathHelper.DeleteFileContaining(targetDir, "*.conf");
            targetDir = configPathHelper.GetPortalBinPath();
            FileAndPathHelper.DeleteFileContaining(targetDir, "*.conf");
        }
        static void WriteConfig(TelcobrightConfig tbc, ConfigPathHelper configPathHelper)
        {
            //write web & app.config files
            DatabaseSetting dbSettings = tbc.DatabaseSetting;
            WriteBillingRules(dbSettings);
            
            NameValueCollection configFiles = (NameValueCollection)ConfigurationManager.GetSection("appSettings");
            foreach (string key in configFiles)
            {
                if (key.StartsWith("conf"))
                {
                    WriteAppAndWebConfigFiles(configFiles[key].Replace("/", Path.DirectorySeparatorChar.ToString()),
                        dbSettings, tbc.PortalSettings);
                }
            }
            string operatorShortName = tbc.DatabaseSetting.DatabaseName;
            //write config to operator's folder in util directory
            string targetDir = configPathHelper.GetOperatorWiseConfigDirInUtil(operatorShortName);
            FileAndPathHelper.DeleteFileContaining(targetDir, "*.conf");
            SerializeConfig(tbc, configPathHelper.GetOperatorWiseTargetFileNameInUtil(operatorShortName));
            //write config for windows service
            targetDir = configPathHelper.GetTopShelfConfigDir();
            SerializeConfig(tbc, configPathHelper.GetOperatorWiseTargetFileNameInTopShelf(operatorShortName));
            //write config for portal
            targetDir = configPathHelper.GetPortalBinPath();
            SerializeConfig(tbc, configPathHelper.GetTargetFileNameForPortal(operatorShortName));
            Console.WriteLine("Successfully written configuration template for " + operatorShortName);
        }



        static void SerializeConfig(TelcobrightConfig tbc, string targetConfigFile)
        {
            if (File.Exists(targetConfigFile))
            {
                File.Delete(targetConfigFile); //debug directory
            }

            String jsonfile = targetConfigFile;
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            
            string tbcAsStr = JsonConvert.SerializeObject(tbc, Formatting.Indented, settings);
            File.WriteAllText(jsonfile, tbcAsStr);

        }

        static void CopyPortal(string operatorDatabaseName)
        {

            //remove existing
            string destinationPath = ConfigurationManager.AppSettings["PortalPath"].ToString().Replace("/", Path.DirectorySeparatorChar.ToString())
                + operatorDatabaseName;
            if (Directory.Exists(destinationPath))
            {
                Directory.Delete(destinationPath, true);
            }

            //copy
            //Now Create all of the directories
            string sourcePath = Directory.GetParent((Directory.GetParent(Directory.GetCurrentDirectory())).Parent.FullName).FullName +
                Path.DirectorySeparatorChar + "Portal";

            foreach (string dirPath in
                Directory.GetDirectories(sourcePath, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
            //set permission to temp folder
            string tempDir = destinationPath + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "temp";
            FileUtil.AddDirectorySecurity(tempDir, "IUSR", System.Security.AccessControl.FileSystemRights.FullControl, System.Security.AccessControl.AccessControlType.Allow);
            FileUtil.AddDirectorySecurity(tempDir, "IIS_IUSRS", System.Security.AccessControl.FileSystemRights.FullControl, System.Security.AccessControl.AccessControlType.Allow);

            Console.WriteLine("Portal Copied Successfully!");



        }


        static void WriteIisScriptsForWebAndFtp(TelcobrightConfig tbc, string configRootDir)
        {
            string scriptDir = configRootDir + Path.DirectorySeparatorChar + tbc.Telcobrightpartner.CustomerName
                    + Path.DirectorySeparatorChar + "scripts";
            if (Directory.Exists(scriptDir) == false)
            {
                Directory.CreateDirectory(scriptDir);
            }
            string importCommandFile = scriptDir + Path.DirectorySeparatorChar + "import_commands.txt";
            if (File.Exists(importCommandFile))
            {
                File.Delete(importCommandFile);
            }
            foreach (InternetSite site in tbc.PortalSettings.PortalSites)
            {
                //site xml part
                string script = ReplaceParametersInConfigFiles(site.GetDicProperties(), site.TemplateFileName);
                string scriptFileName = configRootDir + Path.DirectorySeparatorChar + tbc.Telcobrightpartner.CustomerName + Path.DirectorySeparatorChar
                    + "scripts" + Path.DirectorySeparatorChar +
                    site.SiteType + "_" + tbc.Telcobrightpartner.CustomerName + ".iisScript";
                File.WriteAllText(scriptFileName, script);
                //app pool part
                if (site.SiteType == "http")
                {
                    script = ReplaceParametersInConfigFiles(site.ApplicationPool.GetDicProperties(), site.ApplicationPool.TemplateFileName);
                    string appPoolScriptFileName = configRootDir + Path.DirectorySeparatorChar + tbc.Telcobrightpartner.CustomerName + Path.DirectorySeparatorChar
                        + "scripts" + Path.DirectorySeparatorChar +
                        "appPool" + "_" + tbc.Telcobrightpartner.CustomerName + ".iisScript";
                    File.WriteAllText(appPoolScriptFileName, script);

                    //write import_commands to help during installation
                    File.AppendAllText(importCommandFile, "%windir%/system32/inetsrv/appcmd add apppool /in < " + appPoolScriptFileName + Environment.NewLine);
                }
                //write import_commands to help during installation
                File.AppendAllText(importCommandFile, "%windir%/system32/inetsrv/appcmd add site /in < " + scriptFileName + Environment.NewLine);
            }
        }
        static string ReplaceParametersInConfigFiles(Dictionary<string, string> dicParams, string fileName)
        {
            string templateStr = File.ReadAllText(fileName);
            string[] splitString = templateStr.Split('`');
            for (int i = 0; i < splitString.Length; i++)
            {
                string param = splitString[i];
                if (param.StartsWith("%") && param.EndsWith("%"))
                {
                    param = param.Substring(1, param.Length - 2);
                    string paramVal = null;
                    dicParams.TryGetValue(param, out paramVal);
                    if (paramVal != null) splitString[i] = paramVal;
                }
            }
            return string.Join("", splitString);
        }
        static void WriteAppAndWebConfigFiles(string fileName, DatabaseSetting dbSettings, PortalSettings portalSettings)
        {
            List<string> fileLines = File.ReadAllLines(fileName).ToList();
            string fileNameOnly = Path.GetFileName(fileName);
            string appOrWebConfigFileName = fileNameOnly.ToLower().StartsWith("web") ? "web" : "app";
            List<string> newFileLines = new List<string>();
            bool insideConnectionStrings = false;

            InternetSite portalSite = portalSettings.PortalSites.Where(c => c.SiteType == "http").FirstOrDefault();
            string impersonateUserName = portalSite.ImpersonateUserName;
            string impersonatePassword = portalSite.ImpersonatePassword;
            foreach (string line in fileLines)
            {
                //identity impersonate in web.conf
                if (!string.IsNullOrEmpty(impersonateUserName) && !string.IsNullOrEmpty(impersonatePassword))
                {
                    if (fileName.ToLower().Contains("web.config"))
                    {
                        if (line.TrimStart().StartsWith("<identity impersonate"))
                        {
                            newFileLines.Add(("\t\t\t<identity impersonate=`true` userName=`" + impersonateUserName
                                + "` password=`" + impersonatePassword + "` />").Replace('`', '"'));
                            continue;
                        }
                    }
                }

                //detect <connectionStrings>
                if (line.Trim() == ("<connectionStrings>"))
                {
                    insideConnectionStrings = true;
                    continue;
                }
                else if (line.Trim() == ("</connectionStrings>"))
                {
                    insideConnectionStrings = false;
                    newFileLines.Add(GetConnectionStrinsgFromUtilConfig(appOrWebConfigFileName, dbSettings));
                    continue;
                }

                if (insideConnectionStrings == true)
                {
                    continue;
                }
                else//write old line as they were
                {
                    newFileLines.Add(line);
                }
            }
            //write new version of the config file
            //File.WriteAllLines(FileName, NewFileLines.ToArray());
            File.WriteAllLines(fileName, newFileLines.ToArray());
        }



        static string GetConnectionStrinsgFromUtilConfig(string appOrWebConfigFileName, DatabaseSetting dbSettings)
        {
            List<string> fileLines = File.ReadAllLines("..\\..\\app.config").ToList();
            List<string> newFileLines = new List<string>();
            bool insideConnectionStrings = false;

            foreach (string line in fileLines)
            {
                //detect <connectionStrings>
                if (line.Trim() == ("<connectionStrings>"))
                {
                    newFileLines.Add(line);
                    insideConnectionStrings = true;
                    continue;
                }
                else if (line.Trim() == ("</connectionStrings>"))
                {
                    newFileLines.Add(line);
                    break;
                }

                if (insideConnectionStrings == true)
                {
                    newFileLines.Add(ReplaceConfigVariableNamesWithValues(appOrWebConfigFileName, line, '{', '}', dbSettings));
                    continue;
                }
                else
                {
                    continue;
                }
            }
            return string.Join(Environment.NewLine, newFileLines);
        }

        static string ReplaceConfigVariableNamesWithValues(string appOrWebConfigFileName, string sourceString,
            char paramLeftEnclosure, char paramRightEnclosure, DatabaseSetting dbSettings)
        {
            List<string> newSegments = new List<string>();
            string[] strArr = sourceString.Split(paramLeftEnclosure);
            foreach (string str in strArr)
            {
                if (str.Contains(paramRightEnclosure))
                {
                    string[] twoParts = str.Split(paramRightEnclosure);
                    if (twoParts[0].ToLower() == "databasename")
                    {
                        if (appOrWebConfigFileName == "web") //web.config should contain the db name as 
                        {
                            //portal can only be configured for a single operator (the default in app.conf in 
                            //utilInstallConfig) at a time, 
                            newSegments.Add((string)dbSettings.GetType().GetProperty(twoParts[0])
                                .GetValue(dbSettings, null));
                        }
                        else
                        {
                            //multiple tb instance will be running for multiple operators for a single 
                            //service, app.config should only contain only server & authentication
                            //databasename should not be in it, it should be replaced dynamically 
                            //by proper databasename of each instance/operator
                            newSegments.Add("#DatabaseName#"); //
                        }
                    }
                    else
                    {
                        newSegments.Add((string)dbSettings.GetType().GetProperty(twoParts[0]).GetValue(dbSettings, null));
                    }
                    newSegments.Add(twoParts[1]);
                }
                else
                {
                    newSegments.Add(str);
                }
            }
            return string.Join("", newSegments);
        }

        private static void WriteBillingRules(DatabaseSetting databaseSetting)
        {
            var serverName = databaseSetting.ServerName;

            
            string constr =
                "server=" + serverName + ";User Id=" + databaseSetting.AdminUserName +
                ";password=" + databaseSetting.AdminPassword +
                ";Persist Security Info=True;database=" + databaseSetting.DatabaseName;

            using (MySqlConnection con = new MySqlConnection(constr))
            {
                try
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand("", con))
                    {
                        cmd.CommandText = "set foreign_key_checks=0; " +
                                          "set autocommit=0; delete from jsonbillingrule;";
                        cmd.ExecuteNonQuery();
                        foreach (var br in BIllingRulesDefiner.BillingRules)
                        {
                            cmd.CommandText = $@"insert into jsonbillingrule
                                           (id,rulename,isprepaid,description,jsonexpression) values 
                                            ({br.Id},{br.RuleName.EncloseWith("'")},
                                            {Convert.ToInt32(br.IsPrepaid)},{br.Description.EncloseWith("'")}
                                            ,{(JsonConvert.SerializeObject(br)).EncloseWith("'")});";
                            cmd.ExecuteNonQuery();
                        }
                        cmd.CommandText = "set foreign_key_checks=1;" +
                                          "commit;";
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    using (MySqlCommand cmd = new MySqlCommand("set foreign_key_checks=1;" +
                                                               "rollback;", con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }



}
