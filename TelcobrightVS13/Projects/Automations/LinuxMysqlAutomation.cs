using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation;
using Renci.SshNet;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace CdrRules
{
    [Export("Automation", typeof(IAutomation))]
    public class LinuxMysqlAutomation: IAutomation
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "Linux mysql automation";
        SshClient sshclient;
        public void execute(Object automationData)

        {
            Dictionary<string, object> data= (Dictionary<string, object>)automationData;
            ServerInfo serverInfo = (ServerInfo)data["serverInfo"];
            string serverName = serverInfo.ServerNameOrIp;
            string userName = serverInfo.Username;
            string password = serverInfo.Password;
            sshclient = new SshClient(serverName, userName, password);
            sshclient.Connect();

            generateMySqlConfig("mysqlConfig.txt");

            List<string> devServerIpAddresses = new List<string> { "192.168.0.230", "192.168.0.231" };

            foreach (string ipAddress in devServerIpAddresses)
            {
                List<string> permissionCommands = buildPermissionForRoot(ipAddress, "root", "Takay1#$ane");
                foreach (string command in permissionCommands)
                {
                    string answer = RunCommand(command);
                }
            }
        }
        private static List<string> buildPermissionForRoot(string ipAddress, string userName, string password)
        {
            return new List<string>
            {
                $"sudo mysql -uroot -e 'CREATE USER {userName}@{ipAddress} IDENTIFIED WITH mysql_native_password BY \"{password}\";'",
                $"sudo mysql -uroot -e 'alter user {userName}@{ipAddress} identified by \"{password}\";'",
                $"sudo mysql -uroot -e 'grant all on *.* to {userName}@{ipAddress};'",
                $"sudo mysql -uroot -e 'flush privileges;'"
            };
        }
        private static List<string> buildPermissionDbReader(string ipAddress, string userName, string password, string partner)
        {
            return new List<string>
            {
                $"sudo mysql -uroot -e 'CREATE USER {userName}@{ipAddress} IDENTIFIED WITH mysql_native_password BY \"{password}\";'",
                $"sudo mysql -uroot -e 'alter user {userName}@{ipAddress} identified by \"{password}\";'",
                $"sudo mysql -uroot -e 'grant select, execute on {partner}.* to {userName}@{ipAddress};'",
                $"sudo mysql -uroot -e 'flush privileges;'"
            };
        }
        private void generateMySqlConfig(string sourceFile)
        {
            string cmd = File.ReadAllText(sourceFile);
            runBase64ShellCommand(cmd);
        }
        private string RunCommand(string command)
        {
            //var promptRegex = new Regex(@"\][#$>]"); // regular expression for matching terminal prompt
            var modes = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();

            using (var stream = sshclient.CreateShellStream("xterm", 255, 50, 800, 600, 1024, modes))
            {
                var writer = new StreamWriter(stream);
                var reader = new StreamReader(stream);
                writer.WriteLine(command);
                writer.Flush();
                return reader.ReadToEnd();
            }
        }
        private string runBase64ShellCommand(string cmd)
        {
            string base64Content = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(cmd));
            string command = $"echo '{base64Content}' | base64 --decode | bash";
            var promptRegex = new Regex(@"\][#$>]"); // regular expression for matching terminal prompt
            var modes = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
            using (var stream = sshclient.CreateShellStream("xterm", 255, 50, 800, 600, 1024, modes))
            {
                var writer = new StreamWriter(stream);
                var reader = new StreamReader(stream);
                writer.WriteLine(command);
                writer.Flush();
                return reader.ReadToEnd();
            }
        }
    }
}
