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
            List<string> commands = new List<string>
            {
                $"sudo mysql -uroot -e 'CREATE USER root@192.168.0.230 IDENTIFIED WITH mysql_native_password BY \"Takay1#$ane\";'",
                $"sudo mysql -uroot -e 'alter user root@192.168.0.230 identified by \"Takay1#$ane\";'",
                $"sudo mysql -uroot -e 'grant all on *.* to root@192.168.0.230;'",
                $"sudo mysql -uroot -e 'flush privileges;'"
            };
            foreach (string command in commands) {
                string answer = RunCommand(command);
            }
        }

        private static string RunCommand(string command)
        {
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
        private static string runCommand(string command)
        {
            SshCommand sc = sshclient.CreateCommand(command);
            sc.Execute();
            string answer = sc.Result;
            return answer;
        }
    }
}