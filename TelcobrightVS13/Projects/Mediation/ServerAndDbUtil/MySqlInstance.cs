using System.Collections.Generic;

namespace TelcobrightMediation
{
    public class MySqlInstance
    {
        private Server Server { get; set; }
        public string HostnameOrIp { get; set; }
        public int Port { get; set; }
        public string MasterUsername { get; set; }
        public string MasterPassword { get; set; }
        public Dictionary<string, string> ConfigParams { get; set; }
        public List<MySqlUser> Users { get; set; }
        public List<string> IgnoreDatabasesFromReplication { get; set; } = new List<string>() {"mysql"};
        public List<ReplicationHelper> SlaveInstances { get; set; }
        public MySqlInstance(Server server, Dictionary<string, string> configParams)
        {
            this.Server = server;
            ConfigParams = configParams;
        }
    }

    public class MySqlCluster
    {
        public MySqlInstance Master { get; set; }
        private List<MySqlInstance> Slaves { get; set; }
    }
}