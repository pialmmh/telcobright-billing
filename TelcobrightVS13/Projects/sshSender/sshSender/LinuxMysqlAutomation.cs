using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace sshSender
{
    public class LinuxMysqlAutomation
    {
        static SshClient sshclient;
        public static void execute()

        {
            string userName = "telcobright";
            string password = "Takay1#$ane%%";
            sshclient = new SshClient("192.168.0.137", userName, password);
            sshclient.Connect();

            generateMySqlConfig("mysqlConfig.txt");

            List<string> devServerIpAddresses = new List<string> { "192.168.0.230", "192.168.0.231" };

            foreach (string ipAddress in devServerIpAddresses) {
                List<string> permissionCommands = buildPermissionForRoot(ipAddress,"root","Takay1#$ane");
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
        private static List<string> buildPermissionDbReader(string ipAddress, string userName, string password,string partner)
        {
            return new List<string>
            {
                $"sudo mysql -uroot -e 'CREATE USER {userName}@{ipAddress} IDENTIFIED WITH mysql_native_password BY \"{password}\";'",
                $"sudo mysql -uroot -e 'alter user {userName}@{ipAddress} identified by \"{password}\";'",
                $"sudo mysql -uroot -e 'grant select, execute on {partner}.* to {userName}@{ipAddress};'",
                $"sudo mysql -uroot -e 'flush privileges;'"
            };
        }
        private static string RunCommand(string command)
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
        private static string runCommand(string command)
        {
            SshCommand sc = sshclient.CreateCommand(command);
            sc.Execute();
            string answer = sc.Result;
            return answer;
        }
        private static void generateMySqlConfig(string sourceFile)
        {
            string cmd = File.ReadAllText(sourceFile);
            runBase64ShellCommand(cmd);
        }
        private static string runBase64ShellCommand(string cmd)
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