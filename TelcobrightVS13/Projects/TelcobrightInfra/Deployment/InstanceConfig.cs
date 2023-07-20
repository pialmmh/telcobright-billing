using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightInfra
{
   
    public class InstanceConfig
    {
        public string Name { get; set; }
        public int SchedulerPortNo { get; set; }
        public  bool Skip { get; set; }
        public Dictionary<string, string> otherParams { get; set; }=
            new Dictionary<string, string>();
    }
}
