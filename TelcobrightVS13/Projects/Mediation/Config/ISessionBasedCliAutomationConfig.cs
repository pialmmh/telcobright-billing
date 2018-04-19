using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcobrightMediation.Automation;

namespace TelcobrightMediation.Config
{
    public interface ISessionBasedCliAutomationConfig : ICliAutomationConfig
    {
        SessionInfo SessionInfo { get; set; }
        CliCommandSequence CliCommandSequence { get; set; }
    }
}
