using System.Collections.Generic;
using LibraryExtensions;

namespace TelcobrightInfra
{
    public class Server
    {
        public string Name { get; }
        public Datacenter Datacenter { get; set; }
        public int ServerId { get; set; }
        public ServerOs ServerOs { get; set; }
        public Dictionary<string, Server> Vms { get; set; }
        public ServerAutomationType AutomationType { get; set; }
        public List<IpAddress> IpAddresses { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public string AutomationUsername { get; set; }
        public string AutomationPassword { get; set; }

        public Server(string name)
        {
            Name = name;
        }
    }
}
