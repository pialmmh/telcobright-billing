using System.Collections.Generic;

namespace TelcobrightInfra
{
    public class MySqlCluster
    {
        public MySqlServer Master { get; set; }
        private List<MySqlServer> Slaves { get; set; }

        public MySqlCluster(MySqlServer master, List<MySqlServer> slaves)
        {
            Master = master;
            Slaves = slaves;
        }
    }
}