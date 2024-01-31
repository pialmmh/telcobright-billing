using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions.ConfigHelper;
using MySql.Data.MySqlClient;
using TelcobrightInfra;

namespace InstallConfig
{
    public class MySqlPermissionManager
    {
        public Dictionary<string, MySqlCluster> Clusters { get; }

        public MySqlPermissionManager(Dictionary<string, MySqlCluster> clusters)
        {
            Clusters = clusters;
        }

        public void setupMySqlUsersAndPermissions()
        {
            Dictionary<string, MySqlServer> mySqlServers = this.Clusters.Values.Select(cl =>
                {
                    var servers = new List<MySqlServer> { cl.Master };
                    servers.AddRange(cl.Slaves);
                    return servers;
                }).SelectMany(servers => servers).OrderBy(server => server.FriendlyName)
                .GroupBy(server => server.FriendlyName).ToDictionary(g => g.Key, g => g.First());

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
                    MySqlSession mySqlSession = new MySqlSession(con);
                    //mySqlSession.executeCommand();
                    MySqlCommandGenerator comamndGenerator = new MySqlCommandGenerator();
                    Dictionary<string, List<string>> userVsCreateScript = mySqlServer.Users
                        .Select(user => new
                        {
                            userName = user.Username,
                            script = comamndGenerator.createMySqlUserTelcobrightStyle(user)
                        }).ToDictionary(a => a.userName, a => a.script);
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
    }
}
