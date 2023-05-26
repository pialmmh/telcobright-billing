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

            Console.WriteLine("Hello !!");

            //SshCommandLineRunner ssh = new SshCommandLineRunner("192.168.0.71", userName, password, "22");
            //ssh.Connect();

            sshclient = new SshClient("103.134.90.125", userName, password);
            sshclient.Connect();

            string command = "ls -al";
            string answer = RunCommand(command);

             //command = "sudo su";
             //answer = RunCommand(command);

            //command = "Bash";
            //answer = RunCommand(command);

            command = "sudo mysql -uroot -e 'show databases;'";
            answer = RunCommand(command);

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
                var output = "";
                var promptFound = false;
                var timeout = TimeSpan.FromSeconds(10); // Set a timeout of 10 seconds
                var startTime = DateTime.Now;

                while (!promptFound && DateTime.Now - startTime < timeout)
                {
                    var line = reader.ReadLine();
                    if (line != null)
                    {
                        output += line + "\n";
                        promptFound = promptRegex.IsMatch(line);
                    }
                }
                return output;
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