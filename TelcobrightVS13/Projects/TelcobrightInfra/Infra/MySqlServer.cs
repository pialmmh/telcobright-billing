using System.Collections.Generic;

namespace TelcobrightInfra
{
    public class MySqlServer : ApplicationService
    {
        public string FriendlyName { get; set; }
        public Server Server { get; set; }
        public BindAddress BindAddressForAutomation { get; set; }
        public string RootUserForAutomation { get; set; }
        public string RootPasswordForAutomation { get; set; }
        public MySqlVersion MySqlVersion { get; set; }
        public Dictionary<string, string> MysqlConfig { get; set; }
        public List<MySqlUser> Users { get; set; }
        public List<string> IgnoreDatabasesFromReplication { get; set; } = new List<string>() {"mysql"};
        public List<ReplicationHelper> SlaveInstances { get; set; }

        public MySqlServer(string friendlyName)
        {
            this.FriendlyName = friendlyName;
        }

        public MySqlServer(string friendlyName, Server server, MySqlVersion mySqlVersion, Dictionary<string, string> mysqlConfig,
            List<MySqlUser> users)
        {
            this.Server = server;
            this.MySqlVersion = mySqlVersion;
            this.MysqlConfig = mysqlConfig;
            this.Users = users;
        }
    }
}