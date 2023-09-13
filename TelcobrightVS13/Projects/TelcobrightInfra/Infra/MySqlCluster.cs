using System.Collections.Generic;

namespace TelcobrightInfra
{
    public class MySqlCluster
    {
        public MySqlServer Master { get; set; }
        public List<MySqlServer> Slaves { get; set; }
    }
}