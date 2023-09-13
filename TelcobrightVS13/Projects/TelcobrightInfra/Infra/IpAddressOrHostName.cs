using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightInfra
{
    public enum IPAddressCidrType
    {
        Any,
        Private,
        Public,
    }

    public class IpAddressOrHostName
    {
        public string Address { get; set; }
        public string SubnetMask { get; set; }
        private HostOrSubnetType HostOrSubnetType { get; set; }=HostOrSubnetType.Host;
        public IPAddressCidrType Type { get; set; } = IPAddressCidrType.Any;
    }
}
