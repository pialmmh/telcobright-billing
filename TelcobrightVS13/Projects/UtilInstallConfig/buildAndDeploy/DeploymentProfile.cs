using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace InstallConfig._generator
{
    public class Deploymentprofile
    {
        public string profileName { get; set; }
        public DeploymentProfileType type { get; set; }
        public List<InstanceConfig> instances { get; set; }
       
    }
}
