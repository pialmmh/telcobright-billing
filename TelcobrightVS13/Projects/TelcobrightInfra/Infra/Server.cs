using System.Collections.Generic;
using LibraryExtensions;

namespace TelcobrightInfra
{
    public class Server
    {
        public int ServerId { get; }
        public string ServerName { get; }
        public ServerOs ServerOs { get; set; }
        public ServerAutomationType AutomationType { get; set; }
        public List<IpAddress> IpAddresses { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public string AutomationUsername { get; set; }
        public string AutomationPassword { get; set; }

        public Server(int serverId, string serverName)
        {
            ServerId = serverId;
            ServerName = serverName;
        }
    }
}
