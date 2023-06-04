using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation;
using Renci.SshNet;
using System.Diagnostics;
namespace CdrRules
{
    [Export("Automation", typeof(IAutomation))]
    public class WinLocalShellAutomation : IAutomation
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "Percona docker automation";
        SshClient sshclient;
        public void connect(object automationData)
        {
            throw new NotImplementedException();
        }

        public void execute(object executionData)
        {
            List<string> devServerIpAddresses = new List<string> { "59.221.153.128", "146.85.89.185", "238.140.111.244", "28.159.14.156" };
            foreach (string ipAddress in devServerIpAddresses)
            {
                List<string> permissionCommands = buildPermissionForRoot(ipAddress, "root", "Takay1#$ane");
                foreach (string command in permissionCommands)
                {
                    string answer = RunCommand(command);
                }
            }
            string ans = RunCommand($"C:\\mysql\\bin\\mysql -uroot --skip-password -e \"show databases\"");
        }
        private static string RunCommand(string cmd)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            //process.StartInfo.Arguments = "/user:Administrator \"cmd /K " + cmd + "\"";
            process.StartInfo.Arguments = "/k" + cmd;
            process.StartInfo.WorkingDirectory = "C:\\mysql\\bin";
            process.StandardInput.WriteLine(cmd);
            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();
            return process.StandardOutput.ReadToEnd();
            //Close the process
            //process.Close();
            //process.Dispose();
        }
        private static List<string> buildPermissionForRoot(string ipAddress, string userName, string password)
        {
            return new List<string>
            {
                $"C:\\mysql\\bin\\mysql -uroot --skip-password -e \"CREATE USER {userName}@{ipAddress} IDENTIFIED WITH mysql_native_password BY '{password}';\"",
                $"C:\\mysql\\bin\\mysql -uroot --skip-password -e \"alter user {userName}@{ipAddress} identified by '{password}';\"",
                $"C:\\mysql\\bin\\mysql -uroot --skip-password -e \"grant all on *.* to {userName}@{ipAddress};\"",
                $"C:\\mysql\\bin\\mysql -uroot --skip-password -e \"flush privileges;\""
            };
        }
        private static List<string> buildPermissionDbReader(string ipAddress, string userName, string password, string partner)
        {
            return new List<string>
            {
                $"C:\\mysql\\bin\\mysql -uroot --skip-password -e \"CREATE USER {userName}@{ipAddress} IDENTIFIED WITH mysql_native_password BY '{password}';\"",
                $"C:\\mysql\\bin\\mysql -uroot --skip-password -e \"alter user {userName}@{ipAddress} identified by '{password}';\"",
                $"C:\\mysql\\bin\\mysql -uroot --skip-password -e \"grant select, execute on {partner}.* to {userName}@{ipAddress};\"",
                $"C:\\mysql\\bin\\mysql -uroot --skip-password -e \"flush privileges;\""
            };
        }
        
    }
}
