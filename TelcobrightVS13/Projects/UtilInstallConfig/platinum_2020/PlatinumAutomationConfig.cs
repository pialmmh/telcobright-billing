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
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;
using QuartzTelcobright;
using TelcobrightMediation.Automation;
using TelcobrightMediation.Config;

namespace InstallConfig
{
    public partial class PlatinumConfigGenerator //quartz config part
    {
        public AutomationSetting GetAutomationSetting()
        {
            AutomationSetting automationSetting = new AutomationSetting();
            Dictionary<int, AutomatedNetworkElementCli> automatedNetworkElementClis =
                new Dictionary<int, AutomatedNetworkElementCli>()
                {
                    {
                        1, new AutomatedNetworkElementCli()
                        {
                            AutomationConfig = new SshAutomationConfig()
                            {
                                CliCommandSequence = new CliCommandSequence()
                                {
                                    commands = new List<SingleCliCommand>()
                                    {

                                    }
                                },
                                SessionInfo = new SessionInfo()
                                {

                                }
                            }
                        }
                    }
                };
            automationSetting.AutomatedNetworkElementClis = automatedNetworkElementClis;
            return automationSetting;
        }
    }
}
