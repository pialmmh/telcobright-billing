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
        public void execute(ServerCredential credential)
        {
            string serverName = credential.ServerNameOrIp;
            string userName = credential.Username;
            string password = credential.Password;
            sshclient = new SshClient(serverName, userName, password);
            sshclient.Connect();
            string command = "ls -al";
            string answer = runCommand(command);
        }

        private string runCommand(string command)
        {
            SshCommand sc = sshclient.CreateCommand(command);
            sc.Execute();
            string answer = sc.Result;
            return answer;
        }
    }
}
