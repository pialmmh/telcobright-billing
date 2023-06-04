using System.Web.Script.Serialization;
using System.Collections.Generic;
using LibraryExtensions;

namespace TelcobrightMediation
{
    public class Server
    {
        public int ServerId { get; set; }
        public ServerOs ServerOs { get; set; }
        public ServerAutomationType AutomationType { get; set; }
        public List<IpAddress> IpAddresses { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public string AutomationUsername { get; set; }
        public string AutomationPassword { get; set; }
    }
}
