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
    class Program
    {
        static SshClient sshclient;
        static void Main(string[] args)

        {
            LinuxMysqlAutomation.execute();
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