using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TelcobrightInfra
{
    public class Deploymentprofile
    {
        public string profileName { get; set; }
        public MySqlVersion MySqlVersion { get; set; }=MySqlVersion.MySql57;
        public DeploymentProfileType type { get; set; }
        public List<InstanceConfig> instances { get; set; }
        public string DebugOrReleaseBinaryPath { get; set; } = "debug";
        public List<MySqlUser> MySqlUsers { get; set; }= new List<MySqlUser>();
        public Dictionary<string, string> UserVsDbName = new Dictionary<string, string>();
    }
}
