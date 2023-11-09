using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;
using MediationModel;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Quartz;
using QuartzTelcobright;
using QuartzTelcobright.PropertyGen;
using TelcobrightFileOperations;
using TelcobrightInfra;
using TelcobrightMediation;
using TelcobrightMediation.Config;

namespace InstallConfig
{
    public class TelcoBillingMenu
    {
        public ConsoleUtil ConsoleUtil { get; set; }
        //public List<TelcobrightConfig> Tbcs { get; set; }
        List<AbstractConfigGenerator> configGenerators= new List<AbstractConfigGenerator>();
        public ConfigPathHelper ConfigPathHelper { get; set; }
        public List<TelcobrightConfig> TbcWithoutGeneratedConfig { get; set; }
        public Deploymentprofile Deploymentprofile { get; set; }
        private Dictionary<string, IScript> DdlScripts { get; set; }= new Dictionary<string, IScript>();
        private Dictionary<string, IScript> SeedDataScripts { get; set; }= new Dictionary<string, IScript>();
        public ConfigPathHelper configPathHelper { get; set; }

        public TelcoBillingMenu(Deploymentprofile deploymentprofile, ConsoleUtil consoleUtil, 
            ConfigPathHelper configPathHelper,IEnumerable<IScript> sqlScripts)
        {
            this.Deploymentprofile = deploymentprofile;
            this.ConsoleUtil = consoleUtil;
            initConfig();
            this.configPathHelper = configPathHelper;

            List<IScript> ddlScripts = sqlScripts.Where(s => s.ScriptType == ScriptType.SqlDDL).ToList();
            foreach (IScript script in ddlScripts)
            {
                script.ScriptDir = configPathHelper.getTelcoBillingDdlHome();
            }
            this.DdlScripts= ddlScripts.ToDictionary(s => s.RuleName);
            List<IScript> seedDataScripts = sqlScripts.Where(s => s.ScriptType == ScriptType.SqlSeedData).ToList();
            this.SeedDataScripts= seedDataScripts.ToDictionary(s => s.RuleName);
        }

        private void initConfig()
        {
            
            DbUtil.configPathHelper = configPathHelper;
            this.configGenerators =
                TelcoBillingConfigGenerator.getSelectedOperatorsConfig(this.Deploymentprofile);
            this.TbcWithoutGeneratedConfig = this.configGenerators.Select(c => c.Tbc).ToList();
            this.ConfigPathHelper = configPathHelper;
        }

        public void showMenu()
        {
            {
                Start:
                this.initConfig();
                Console.Clear();
                Console.WriteLine("Welcome to Telcobright Initial Configuration Utility");
                Console.WriteLine("Select Task:");
                Console.WriteLine("1=Setup mysql remote access.");
                Console.WriteLine("2=Append Prefix to Files");
                Console.WriteLine("3=[Not Set]");
                Console.WriteLine("4=Copy Portal to IIS Directory");
                Console.WriteLine("5=Not Set");
                Console.WriteLine("6=Generate Configuration & Reset Scheduler data");
                Console.WriteLine("7=Init Databases");
                Console.WriteLine("8= Setup MySql user and permissions");
                Console.WriteLine("9=Modify Partitions for tables");
                Console.WriteLine("q=Quit");

                ConsoleKeyInfo ki = new ConsoleKeyInfo();
                ki = Console.ReadKey(true);
                char cmdName = Convert.ToChar(ki.Key);
                DbUtil.configPathHelper = this.ConfigPathHelper;
                switch (cmdName)
                {
                    case '1':
                    {
                        Console.WriteLine("Setting up remote access for mysql...");
                        /*List<string> choices =
                            Menu.getChoices(this.Tbcs.Select(config => config.Telcobrightpartner.databasename),
                                "Select instances to create initial database:");
                        List<TelcobrightConfig> tbcs = this.Tbcs
                            .Where(config => choices.Contains(config.Telcobrightpartner.databasename)).ToList();
                        foreach (var tbc in this.Tbcs)
                        {

                        }*/
                    }
                        break;
                    case '2':
                        Console.WriteLine("Enter Source Dir path & prefix without quotes, separated by comma...");
                        string str = Console.ReadLine();
                        string[] p = str.Split(',');
                        (new FileRename()).AppendPrefix(p[0], p[1]);
                        if (Convert.ToChar((Console.ReadKey(true)).Key) == 'q' ||
                            Convert.ToChar((Console.ReadKey(true)).Key) == 'Q') return;
                        break;
                    case '3':
                        if (Convert.ToChar((Console.ReadKey(true)).Key) == 'q' ||
                            Convert.ToChar((Console.ReadKey(true)).Key) == 'Q') return;
                        Console.WriteLine("Creating Database, none will be created if one exists.");
                        //choices = Menu.getChoices(menuItems, "Select instances to create initial database:");
                        //this.Tbcs = getthis.Tbcs(choices, configPathHelper);
                        /*foreach (var tbc in this.Tbcs)
                        {

                        }*/
                        break;
                    case '4':
                        Console.WriteLine("Copying Portal to c:/inetpub/wwwroot");
                        CopyPortal("portal");
                        if (Convert.ToChar((Console.ReadKey(true)).Key) == 'q' ||
                            Convert.ToChar((Console.ReadKey(true)).Key) == 'Q') return;
                        break;
                    case '5':
                        if (Convert.ToChar((Console.ReadKey(true)).Key) == 'q' ||
                            Convert.ToChar((Console.ReadKey(true)).Key) == 'Q') return;
                        break;
                    case '6': //generate config
                        if (!this.TbcWithoutGeneratedConfig.Any())
                        {
                            Console.WriteLine("No operator's config has been found. Press any key to start over.");
                            Console.ReadKey();
                            goto Start;
                        }
                        generateConfig();
                        goto Start;
                        break;
                    case '7':
                        //if (!this.TbcWithoutGeneratedConfig.Any())
                        //{
                        //    Console.WriteLine("No operator's config has been found. Press any key to start over.");
                        //    Console.ReadKey();
                        //    goto Start;
                        //}
                        //List<TelcobrightConfig> selectedTbcs = getSelectedTbcs();
                        //cleanDeploymentDir();
                        //foreach (var tbWithoutFullConfig in selectedTbcs)
                        //{
                        //    var dbOrInstanceName = tbWithoutFullConfig.Telcobrightpartner.databasename;
                        //    AbstractConfigGenerator configGenerator = this.configGenerators
                        //        .First(c => c.Tbc.Telcobrightpartner.databasename == dbOrInstanceName);
                        //    InstanceConfig ic = this.Deploymentprofile.instances.First(i => i.Name == dbOrInstanceName);
                        //    TelcobrightConfig tbc = configGenerator.GenerateFullConfig(ic, 1);
                        //    tbc.DeploymentProfile = this.Deploymentprofile;
                        //    int schedulerPortNo = ic.SchedulerPortNo;
                        //    tbWithoutFullConfig.TcpPortNoForRemoteScheduler = schedulerPortNo;
                        //    tbWithoutFullConfig.SchedulerDaemonConfigs = configGenerator.GetSchedulerDaemonConfigs();
                        //    TelcoBillingConfigGenerator cw =
                        //        new TelcoBillingConfigGenerator(tbc, this.ConfigPathHelper, this.ConsoleUtil);
                        //    cw.writeConfig();
                        //    cw.LoadSeedData();
                        //    configureQuarzJobStore(tbc);
                        //    deployBinariesForProduction(tbc);
                        //    Console.WriteLine("Successfully generated config for "
                        //                      + string.Join(",",
                        //                          selectedTbcs.Select(t => t.Telcobrightpartner.databasename)));
                        //}
                        goto Start;
                        break;
                    case '8':
                        setupMySqlUsersAndPermissions();
                        Console.WriteLine("Mysql users and permissions setup completed successfully.");
                        Console.WriteLine("Press any key to return.");
                        Console.Read();
                        break;
                    case 'q':
                    case 'Q':
                        return;
                    default:
                        return;
                }
                return;
            }
        }

        void setupMySqlUsersAndPermissions()
        {
            Dictionary<string, MySqlCluster> clusters = this.Deploymentprofile.MySqlClusters;
            Dictionary<string, MySqlServer> mySqlServers = clusters.Values.Select(cl =>
            {
                var servers = new List<MySqlServer> {cl.Master};
                servers.AddRange(cl.Slaves);
                return servers;
            }).SelectMany(servers=>servers).OrderBy(server=>server.FriendlyName)
            .GroupBy(server=>server.FriendlyName).ToDictionary(g=>g.Key,g=>g.First());

            Menu menu = new Menu(mySqlServers.Keys, "Select a mysql instance to configure:", "a");
            List<string> choices = menu.getChoices();
            foreach (var choice in choices)
            {
                MySqlServer mySqlServer = mySqlServers[choice];
                DatabaseSetting dbSettingForAutomation = new DatabaseSetting
                {
                    ServerName = mySqlServer.BindAddressForAutomation.IpAddressOrHostName.Address,
                    WriteUserNameForApplication = mySqlServer.RootUserForAutomation,
                    WritePasswordForApplication = mySqlServer.RootPasswordForAutomation,
                    DatabaseName = "mysql"
                };
                string constr = DbUtil.getDbConStrWithDatabase(dbSettingForAutomation);
                using (MySqlConnection con = new MySqlConnection(constr))
                {
                    MySqlSession mySqlSession= new MySqlSession(con);
                    //mySqlSession.executeCommand();
                    MySqlCommandGenerator comamndGenerator= new MySqlCommandGenerator();
                    Dictionary<string, List<string>> userVsCreateScript = mySqlServer.Users
                        .Select(user => new
                        {
                            userName=user.Username,
                            script= comamndGenerator.createMySqlUserTelcobrightStyle(user)
                        }).ToDictionary(a=>a.userName, a=>a.script);
                    foreach (var kv in userVsCreateScript)
                    {
                        string username = kv.Key;
                        Console.WriteLine("Creating mysql user for:" + username);
                        List<string> commands = kv.Value;
                        foreach (var command in commands)
                        {
                            mySqlSession.executeCommand(command);
                        }
                    }
                }
            }
        }
        void generateConfig()
        {
            List<TelcobrightConfig> selectedTbcs = getSelectedTbcs();
            //cleanDeploymentDir();
            foreach (var tbWithoutFullConfig in selectedTbcs)
            {
                var dbOrInstanceName = tbWithoutFullConfig.Telcobrightpartner.databasename;
                AbstractConfigGenerator configGenerator = this.configGenerators
                    .First(c => c.Tbc.Telcobrightpartner.databasename == dbOrInstanceName);
                InstanceConfig ic = this.Deploymentprofile.instances.First(i => i.Name == dbOrInstanceName);
                TelcobrightConfig tbc = configGenerator.GenerateFullConfig(ic, 1);
                tbc.DeploymentProfile = this.Deploymentprofile;
                int schedulerPortNo = ic.SchedulerPortNo;
                tbc.TcpPortNoForRemoteScheduler = schedulerPortNo;
                tbc.SchedulerDaemonConfigs = configGenerator.GetSchedulerDaemonConfigs();
                TelcoBillingConfigGenerator generator = new TelcoBillingConfigGenerator(tbc, this.ConfigPathHelper, this.ConsoleUtil, this.DdlScripts,
                    this.SeedDataScripts);
                generator.writeConfig();
                generator.LoadDdlScripts();
                generator.LoadSeedData();
                generator.LoadAdditionalSeedData();
                configureQuarzJobStore(tbc);
                deployBinariesForProduction(tbc);
                Console.WriteLine("Successfully generated config for "
                                  + string.Join(",", selectedTbcs.Select(t => t.Telcobrightpartner.databasename)));
            }
        }
        

        void initDatabase()
        {
            List<TelcobrightConfig> selectedTbcs = getSelectedTbcs();
            cleanDeploymentDir();
            foreach (var tbWithoutFullConfig in selectedTbcs)
            {
                var dbOrInstanceName = tbWithoutFullConfig.Telcobrightpartner.databasename;
                AbstractConfigGenerator configGenerator = this.configGenerators
                    .First(c => c.Tbc.Telcobrightpartner.databasename == dbOrInstanceName);
                InstanceConfig ic = this.Deploymentprofile.instances.First(i => i.Name == dbOrInstanceName);
                TelcobrightConfig tbc = configGenerator.GenerateFullConfig(ic, 1);
                tbc.DeploymentProfile = this.Deploymentprofile;

                Console.WriteLine("Successfully generated config for "
                                  + string.Join(",", selectedTbcs.Select(t => t.Telcobrightpartner.databasename)));
            }
        }

        private List<TelcobrightConfig> getSelectedTbcs()
        {
            Menu menu = new Menu(this.TbcWithoutGeneratedConfig.Select(config => config.Telcobrightpartner.databasename).ToList(),
            "Select one or multiple operators to configure.", "a");
            List<string> opNames = menu.getChoices();
            List<TelcobrightConfig> selectedTbcs =
                this.TbcWithoutGeneratedConfig.Where(config => opNames.Contains(config.Telcobrightpartner.databasename))
                    .ToList();
            return selectedTbcs;
        }

        private void cleanDeploymentDir()
        {
            //clean deployed instances
            string deployedInstancsPath = this.ConfigPathHelper.GetTopShelfDir() + Path.DirectorySeparatorChar + "deployedInstances";
            if (Directory.Exists(deployedInstancsPath))
            {
                DirectoryInfo targetDir = new DirectoryInfo(deployedInstancsPath);
                targetDir.DeleteContentRecusively();
            }
            else
            {
                Directory.CreateDirectory(deployedInstancsPath);
            }
        }

        private static void configureQuarzJobStore(TelcobrightConfig tbc)
        {
            //reset job store
            TelcoBillingQuartzWriter telcoBillingQuartzWriter = new TelcoBillingQuartzWriter(tbc);
            Console.WriteLine($"Reset QuartzJob Store for {tbc.Telcobrightpartner.databasename} (Y/N)? this will clear all job data.");
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            if (keyInfo.KeyChar == 'Y' || keyInfo.KeyChar == 'y')
            {
                telcoBillingQuartzWriter.configureQuartzJobStore();
                Console.WriteLine();
                Console.WriteLine("Job store has been reset successfully for " +
                                  getOperatorShortName(tbc));
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Job store was not reset for " +
                                  tbc.Telcobrightpartner.databasename);
            }
        }

        private static void deployBinariesForProduction(TelcobrightConfig tbc)
        {
            Console.WriteLine("Deploying binaries for " + tbc.Telcobrightpartner.databasename);
            string currentbinPath = FileAndPathHelperReadOnly.getBinPath();
            string solutionDir = new DirectoryInfo(currentbinPath).Parent.Parent.FullName;
            DeploymentHelper deploymentHelper =
                new DeploymentHelper(tbc, solutionDir,DeploymentPlatform.Win32);
            deploymentHelper.deploy();
            Console.WriteLine("Binaries deployed successfully for " + getOperatorShortName(tbc));
        }

        private static string getOperatorShortName(TelcobrightConfig tbc)
        {
            return tbc.Telcobrightpartner.databasename;
        }





        static void DeletePrevConfigFilesForPortalAndWinService(ConfigPathHelper configPathHelper)
        {
            string targetDir = configPathHelper.GetTopShelfConfigDir();
            FileAndPathHelperMutable pathHelper= new FileAndPathHelperMutable();
            pathHelper.DeleteFileContaining(targetDir, "*.conf");
            targetDir = configPathHelper.GetPortalBinPath();
            pathHelper.DeleteFileContaining(targetDir, "*.conf");
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
        


        

        

        



        


    }
}
