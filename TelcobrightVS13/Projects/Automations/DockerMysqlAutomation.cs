using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation;
using Renci.SshNet;

namespace CdrRules
{
    [Export("Automation", typeof(IAutomation))]
    public class DockerMysqlAutomation: IAutomation
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "Percona docker automation";
        SshClient sshclient;
        public void execute(Object automationData)
        {
            
        }

    }
}
