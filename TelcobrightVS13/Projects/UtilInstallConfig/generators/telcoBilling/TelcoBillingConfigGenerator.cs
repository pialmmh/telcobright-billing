using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstallConfig._generator;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;
using MediationModel;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using TelcobrightInfra;
using TelcobrightMediation;
using TelcobrightMediation.Config;

namespace InstallConfig
{
    public class TelcoBillingConfigGenerator
    {
        public TelcobrightConfig Tbc { get; set; }
        public ConfigPathHelper ConfigPathHelper { get; set; }
        public ConsoleUtil ConsoleUtil { get; set; }
        public TelcoBillingConfigGenerator(TelcobrightConfig tbc, ConfigPathHelper configPathHelper,
            ConsoleUtil consoleUtil)
        {
            this.Tbc = tbc;
            this.ConsoleUtil = consoleUtil;
            this.ConfigPathHelper = configPathHelper;
        }

        public static List<AbstractConfigGenerator> getSelectedOperatorsConfig(Deploymentprofile deploymentprofile)
        {
            List<string> notSkippedInstanceNames =
                deploymentprofile.instances.Where(i => i.Skip == false)
                    .Select(i => i.Name).ToList();

            List<AbstractConfigGenerator> allConfigGenerators
                = new MefConfigImportComposer().Compose()
                    .Where(op => notSkippedInstanceNames.Contains(op.Tbc.Telcobrightpartner.databasename)).ToList();
            return allConfigGenerators;
        }

        public void writeConfig()
        {
            Console.WriteLine("Writing Configuration Files for " + Tbc.Telcobrightpartner.databasename);
            WriteConfig(Tbc, ConfigPathHelper);
            Console.WriteLine(
                "Config Files have been generated successfully for " + Tbc.Telcobrightpartner.databasename);
        }

        public void LoadSeedData()
        {
            /*if (ConsoleUtil.getConfirmationFromUser("Load seed data? (Y/N) for "+
                this.Tbc.Telcobrightpartner.databasename))*/
            Console.WriteLine("Loading seed data for " + this.Tbc.Telcobrightpartner.databasename);
            PartnerEntities context =
                new PartnerEntities(DbUtil.GetEntityConnectionString(Tbc.DatabaseSetting));
            if (context.Database.Connection.State != ConnectionState.Open)
                context.Database.Connection.Open();
            using (MySqlConnection con =
                new MySqlConnection(DbUtil.getDbConStrWithDatabase(this.Tbc.DatabaseSetting)))
            {
                DbWriterForConfig dbWriter = new DbWriterForConfig(this.Tbc, this.ConfigPathHelper, con);
                dbWriter.LoadSeedDataSqlForTelcoBilling(SqlOperationType.SeedData);
                if (ConsoleUtil.getConfirmationFromUser("Load ddl scripts? (Y/N) for " +
                                                        this.Tbc.Telcobrightpartner.databasename))
                {
                    dbWriter.LoadSeedDataSqlForTelcoBilling(SqlOperationType.DDL);
                }
                dbWriter.WriteTelcobrightPartnerAndNes();
            }
            Console.WriteLine();
            Console.WriteLine("Seed data loaded successfully for " + Tbc.Telcobrightpartner.databasename);
        }

        static void WriteConfig(TelcobrightConfig tbc, ConfigPathHelper configPathHelper)
        {
            //write web & app.config files
            DatabaseSetting dbSettings = tbc.DatabaseSetting;
            WriteBillingRules(dbSettings);

            NameValueCollection configFiles = (NameValueCollection) ConfigurationManager.GetSection("appSettings");
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
            string configRoot = tbc.DirectorySettings.ConfigRoot;
            string targetDir =
                configPathHelper.GetOperatorWiseConfigDirInUtil(operatorShortName, configRoot);
            FileAndPathHelper.DeleteFileContaining(targetDir, "*.conf");
            SerializeConfigAndWriteJsonFile(tbc, configPathHelper.GetOperatorWiseTargetFileNameInUtil(operatorShortName,configRoot));
            //write config for windows service
            targetDir = configPathHelper.GetTopShelfConfigDir();
             SerializeConfigAndWriteJsonFile(tbc, configPathHelper.GetTemplateConfigFileName("telcobright.conf"),
                eraseAllPrevFilesFromConfigDir: true);
            //write config for portal
            targetDir = configPathHelper.GetPortalBinPath();
            //SerializeConfigAndWriteJsonFile(tbc, configPathHelper.GetTargetFileNameForPortal(operatorShortName));
            var portalConfigFilename = configPathHelper.GetTargetFileNameForPortal("telcobright");
            SerializeConfigAndWriteJsonFile(tbc, portalConfigFilename);

            if (operatorShortName == "btrc_cas")
            {
                portalConfigFilename = portalConfigFilename.Replace("portal", "portalBTRC");
                SerializeConfigAndWriteJsonFile(tbc, portalConfigFilename);//
            }

            Console.WriteLine("Successfully written configuration template for " + operatorShortName);
        }

        static void SerializeConfigAndWriteJsonFile(TelcobrightConfig tbc, string targetConfigFile,
            bool eraseAllPrevFilesFromConfigDir=false)
        {
            if (eraseAllPrevFilesFromConfigDir == true)
            {
                var arr = targetConfigFile.Split(Path.DirectorySeparatorChar);
                string folderPath = string.Join(Path.DirectorySeparatorChar.ToString(), arr.Take(arr.Length - 1));
                DirectoryInfo configDir= new DirectoryInfo(folderPath);
                if (Directory.Exists(configDir.FullName))
                {
                    configDir.Delete(recursive: true);
                }
                Directory.CreateDirectory(configDir.FullName);
            }
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
                if (!String.IsNullOrEmpty(impersonateUserName) && !String.IsNullOrEmpty(impersonatePassword))
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
            return String.Join(Environment.NewLine, newFileLines);
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
            return String.Join("", newSegments);
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
