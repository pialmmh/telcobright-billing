using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using TelcobrightMediation;
using TelcobrightMediation.Scheduler.Quartz;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;
using QuartzTelcobright;
using TelcobrightMediation.Automation;
using TelcobrightMediation.Config;

namespace InstallConfig
{
    public partial class BtelAbstractConfigConfigGenerator //quartz config part
    {
        public void PrepareProductAndServiceSettings()
        {
            List<KeyValuePair<Regex, string>> serviceAliases = new List<KeyValuePair<Regex, string>>
            {
                new KeyValuePair<Regex, string>(new Regex(@".*/sg5/.*/sf4/.*"), "International Outgoing"),
                new KeyValuePair<Regex, string>(new Regex(@".*/sg4/.*/sf1/.*"), "AZ Voice")
            };
            this.Tbc.ServiceAliasesRegex = serviceAliases;
        }
    }
}
