using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace sshSender
{
    class Program
    {
        static void Main(string[] args)

        {
            //LinuxMysqlAutomation.execute();
            WindowsLocalMysqlAutomation.execute();
            //Process cmd = new Process();
            //cmd.StartInfo.FileName = "cmd.exe";
            //cmd.StartInfo.RedirectStandardInput = true;
            //cmd.StartInfo.RedirectStandardOutput = true;
            //cmd.StartInfo.CreateNoWindow = false;
            //cmd.StartInfo.UseShellExecute = false;
            //cmd.Start();

            //cmd.StandardInput.WriteLine("dir");
            //cmd.StandardInput.Flush();
            //cmd.StandardInput.Close();
            //cmd.WaitForExit();
            //Console.WriteLine(cmd.StandardOutput.ReadToEnd());
        }

        //private static string runCommand(string command)
        //{
        //    SshCommand sc = sshclient.CreateCommand(command);
        //    sc.Execute();
        //    string answer = sc.Result;
        //    return answer;
        //}
    }
}