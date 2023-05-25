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
    class WindowsMysqlAutomation
    {
        static SshClient sshclient;
        public static void execute()

        {
            string userName = "telcobright";
            string password = "Btl@bright2$";

            Console.WriteLine("Hello !!");

            //SshCommandLineRunner ssh = new SshCommandLineRunner("192.168.0.71", userName, password, "22");
            //ssh.Connect();

            sshclient = new SshClient("103.134.90.125", userName, password);
            sshclient.Connect();

            string command = "ls -al";
            string answer = runCommand(command);

            //command = "mysql -uroot -p'Takay1#$ane'";
            //answer = runCommand(command);

            //Thread.Sleep(3000);

            //command = "show databases;";
            //answer = runCommand(command);

            File.WriteAllText("sshOutput.txt", answer);

            var promptRegex = new Regex(@"\][#$>]"); // regular expression for matching terminal prompt
            var modes = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
            using (var stream = sshclient.CreateShellStream("xterm", 255, 50, 800, 600, 1024, modes))
            {
                stream.Write("sudo iptables -L -n\n");
                stream.Expect("password");
                stream.Write(password);
                var output = stream.Expect(promptRegex);
                Console.Read();
                Console.WriteLine(output);
            }

            Console.Read();
            Console.WriteLine();


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