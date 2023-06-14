using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallConfig._generator
{
   
    public class InstanceConfig
    {
        public string name { get; set; }
        public Dictionary<string, string> otherParams { get; set; }
    }
}
