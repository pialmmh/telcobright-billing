using System.Collections.Generic;

namespace TelcobrightMediation
{
    public class MySqlServer
    {
        public Server Server { get;}
        public MySqlVersion MySqlVersion { get; set; }
        public List<BindAddress> BindAddresses { get; }= new List<BindAddress>();
        public Dictionary<string, string> MysqlConfig { get;}
        public List<MySqlUser> Users { get;}
        public List<string> IgnoreDatabasesFromReplication { get; set; } = new List<string>() {"mysql"};
        public List<ReplicationHelper> SlaveInstances { get; set; }
        public MySqlServer(Server server,MySqlVersion mySqlVersion, Dictionary<string, string> mysqlConfig, List<MySqlUser> users)
        {
            this.Server = server;
            this.MySqlVersion = mySqlVersion;
            this.MysqlConfig = mysqlConfig;
            this.Users = users;
        }
    }
}