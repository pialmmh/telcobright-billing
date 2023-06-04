using System.Collections.Generic;

namespace TelcobrightMediation
{
    public class MySqlServer
    {
        private Server Server { get; set; }
        public string HostnameOrIp { get; set; }
        public int Port { get; set; }
        public string MasterUsername { get; set; }
        public string MasterPassword { get; set; }
        public Dictionary<string, string> SectionWiseConfig { get; set; }
        public List<MySqlUser> Users { get; set; }
        public List<string> IgnoreDatabasesFromReplication { get; set; } = new List<string>() {"mysql"};
        public List<ReplicationHelper> SlaveInstances { get; set; }
        public MySqlServer(Server server, Dictionary<string, string> sectionWiseConfig)
        {
            this.Server = server;
            SectionWiseConfig = sectionWiseConfig;
        }
    }

    public class MySqlCluster
    {
        public MySqlServer Master { get; set; }
        private List<MySqlServer> Slaves { get; set; }
    }
}