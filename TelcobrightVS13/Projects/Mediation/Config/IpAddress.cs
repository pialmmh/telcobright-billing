using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation
{
    public enum HostOrSubnet
    {
        Host,
        Subnet
    }

    public class IpAddress
    {
        public string Address { get; set; }
        public string SubnetMask { get; set; }
        private HostOrSubnet HostOrSubnet { get; set; }=HostOrSubnet.Host;
        public string PrivateOrPublic { get; set; }
    }
}
