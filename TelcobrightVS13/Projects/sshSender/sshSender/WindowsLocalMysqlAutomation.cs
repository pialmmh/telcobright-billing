﻿using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace sshSender
{
    public class WindowsLocalMysqlAutomation
    {
        static Process process;
        public static void execute()
        {

            //ans = RunCommand("cd C:\\mysql && nul > filename.txt");
            //string ans = RunCommand("ipconfig");
            //ans = RunCommand($"cd C:\\mysql\\bin && mysql -uroot --skip-password -e 'show databases'");
            //string command = "C:\\mysql\\bin\\mysql --skip-password -e \"show databases/"";
            List<string> devServerIpAddresses = new List<string> { "59.221.153.128", "146.85.89.185" ,"238.140.111.244","28.159.14.156"};

            foreach (string ipAddress in devServerIpAddresses)
            {
                List<string> permissionCommands = buildPermissionForRoot(ipAddress, "root", "Takay1#$ane");
                foreach (string command in permissionCommands)
                {
                    string answer = RunCommand(command);
                }
            }
            string ans = RunCommand($"C:\\mysql\\bin\\mysql -uroot --skip-password -e \"show databases\"");
            //ans = RunCommand($"Show database;");
            //ans = RunCommand("cd D:\\ && nul > filename.txt");
            //RunCommand(process, "nul > filename.txt");

            //generateMySqlConfig("mysqlConfig.txt", "C:\\mysql");

            //generateMySqlConfig("mysqlConfig.txt");

            /* List<string> devServerIpAddresses = new List<string> { "192.168.0.230", "192.168.0.231" };

             foreach (string ipAddress in devServerIpAddresses)
             {
                 List<string> permissionCommands = buildPermissionForRoot(ipAddress, "root", "Takay1#$ane");
                 foreach (string command in permissionCommands)
                 {
                     string answer = RunCommand(command);
                 }
             }*/
        }
        private static string RunCommand(string cmd)
        {
            process = new Process();
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
        //private static void generateMySqlConfig(string sourceFile, string path)
        //{
        //    string cmd = File.ReadAllText(sourceFile);
        //    File.WriteAllText(path, cmd);
        //}
        /*private static string RunCommand(string command)
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
        }*/
        //private static string runBase64ShellCommand(string cmd)
        //{
        //    string base64Content = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(cmd));
        //    string command = $"echo '{base64Content}' | base64 --decode | bash";
        //    var promptRegex = new Regex(@"\][#$>]"); // regular expression for matching terminal prompt
        //    var modes = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
        //    using (var stream = sshclient.CreateShellStream("xterm", 255, 50, 800, 600, 1024, modes))
        //    {
        //        var writer = new StreamWriter(stream);
        //        var reader = new StreamReader(stream);
        //        writer.WriteLine(command);
        //        writer.Flush();
        //        return reader.ReadToEnd();
        //    }
        //}
        // public static void execute()
        //{
        //    process = new Process();
        //    process.StartInfo.FileName = "cmd.exe";
        //    process.StartInfo.RedirectStandardInput = true;
        //    process.StartInfo.RedirectStandardOutput = true;
        //    process.StartInfo.CreateNoWindow = false;
        //    process.StartInfo.UseShellExecute = false;
        //    process.Start();

        //    string command = "cd C:\\mysql && nul > filename.txt";
        //    RunCommand(process, command);
        //    //process.WaitForExit();
        //    Console.WriteLine(process.StandardOutput.ReadToEnd());

        //    // Execute another command
        //    string anotherCommand = "dir";
        //    RunCommand(process, anotherCommand);
        //    process.WaitForExit();
        //    Console.WriteLine(process.StandardOutput.ReadToEnd());

        //}

        //private static void RunCommand(Process process, string command)
        //{
        //    process.StandardInput.WriteLine(command);
        //    process.StandardInput.Flush();
        //}
    }
}