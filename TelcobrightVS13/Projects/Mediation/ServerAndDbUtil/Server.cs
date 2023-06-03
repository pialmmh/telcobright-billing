using System.Web.Script.Serialization;
using System.Collections.Generic;
using LibraryExtensions;

namespace TelcobrightMediation
{
    public class Server
    {
        public int ServerId { get; set; }
        public ServerOs ServerOs { get; set; }
        public string AutomationType { get; set; }
        public List<IpAddress> IpAddresses { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public string AutomationUsername { get; set; }
        public string AutomationPassword { get; set; }
        public Server(int serverId, ServerOs serverOs, List<IpAddress> ipAddresses)
        {
            this.ServerId = serverId;
            this.ServerOs = serverOs;
            this.IpAddresses = ipAddresses;
        }
        public Server(int serverId, List<IpAddress> ipAddresses)
        {
            ServerId = serverId;
            this.IpAddresses = ipAddresses;
        }
    }
}
