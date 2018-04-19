using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcobrightMediation.Automation;

namespace TelcobrightMediation.Config
{
    public class SshAutomationConfig : ISessionBasedCliAutomationConfig
    {
        public SessionInfo SessionInfo { get; set; }

        public CliCommandSequence CliCommandSequence { get; set; }
    }
}
