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
    public partial class IcnlConfigGenerator //quartz config part
    {
        public void PrepareProductAndServiceSettings()
        {
            List<KeyValuePair<Regex, string>> serviceAliases = new List<KeyValuePair<Regex, string>>
            {
                new KeyValuePair<Regex, string>(new Regex(@".*/sg20/.*/sf1/.*"), "Local"),
                new KeyValuePair<Regex, string>(new Regex(@".*/sg21/.*/sf1/.*"), "International Out"),
                new KeyValuePair<Regex, string>(new Regex(@".*/sg22/.*/sf1/.*"), "International In"),
                new KeyValuePair<Regex, string>(new Regex(@".*/sg23/.*/sf5/.*"), "Toll-Free"),
                new KeyValuePair<Regex, string>(new Regex(@".*/sg24/.*/sf1/.*"), "Alphatech Premium"),
            };
            this.Tbc.ServiceAliasesRegex = serviceAliases;
        }
    }
}
