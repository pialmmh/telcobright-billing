using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightInfra
{
    public class IpAddress
    {
        public string Address { get; set; }
        public string SubnetMask { get; set; }
        private HostOrSubnetType HostOrSubnetType { get; set; }=HostOrSubnetType.Host;
        public string PrivateOrPublic { get; set; }
    }
}
