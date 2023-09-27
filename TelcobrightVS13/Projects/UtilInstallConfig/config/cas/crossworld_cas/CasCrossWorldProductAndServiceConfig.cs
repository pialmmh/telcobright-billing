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
using TelcobrightMediation.Config;

namespace InstallConfig
{
    public sealed partial class CasCrossWorldAbstractConfigGenerator //quartz config part
    {
        public void PrepareProductAndServiceConfiguration()
        {
            List<KeyValuePair<Regex, string>> serviceAliases = new List<KeyValuePair<Regex, string>>
            {
                new KeyValuePair<Regex, string>(new Regex(@".*/sg2/.*/sf7/.*"), "International Outgoing"),
                new KeyValuePair<Regex, string>(new Regex(@".*/sg1/.*/sf1/.*"), "Domestic"),
                new KeyValuePair<Regex, string>(new Regex(@".*/sg3/.*/sf1/.*"), "International Incoming"),
                new KeyValuePair<Regex, string>(new Regex(@".*/sg6/.*/sf5/.*"), "Local Toll-Free (LTFS)")
            };
            this.Tbc.ServiceAliasesRegex = serviceAliases;
        }
    }
}
