using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
namespace WatchDog
{
    class WatchDog
    {
        static void Main(string[] args)
        {
            var binPath = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            binPath = binPath.Substring(6);

            string exe = "C:/sftp_root/TelcobrightProject/TelcobrightVS13/Projects/WsTest/bin/Debug/WsTest.exe";

            var command = $"{exe} mustafa1 arg2";
            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd", "/k " + command);
            processStartInfo.UseShellExecute = true;
            processStartInfo.RedirectStandardOutput = false;
            processStartInfo.CreateNoWindow = false;
            Process process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();


            command = $"{exe} mustafa2 arg2";
            ProcessStartInfo processStartInfo2 = new ProcessStartInfo("cmd", "/k " + command);
            processStartInfo2.UseShellExecute = true;
            processStartInfo2.RedirectStandardOutput = false;
            processStartInfo2.CreateNoWindow = false;
            Process process2 = new Process();
            process2.StartInfo = processStartInfo2;
            process2.Start();
            Console.Read();

            Console.Read();

        }
    }
}
