using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions.ConfigHelper;

namespace TelcobrightInfra
{
    public class Cloud
    {
        public string Name { get; set; } = "default";
        public List<Datacenter> Datacenters { get; set; }
    }

    public class Datacenter
    {
        public string Name { get; set; } = "default";
        public List<Server>
    }

}
