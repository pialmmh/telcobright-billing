using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace CasTelcobright
{
    public class ShellExecutor
    {
        List<string> commandWithArgs = new List<string>();
        //void DataReceivedEventHandler(object sender, DataReceivedEventArgs e)
        Action<object, DataReceivedEventArgs> callBackForConsole;
        public ShellExecutor(List<string> commandWithArgs,
            Action<object, DataReceivedEventArgs> callBackForConsole)
        {
            this.commandWithArgs = commandWithArgs;
            this.callBackForConsole = callBackForConsole;
        }

        public void execute()
        {
            var command = this.commandWithArgs[0];
            var shell = new Process();
            shell.StartInfo.FileName = command;
            shell.StartInfo.UseShellExecute = false;
            shell.StartInfo.RedirectStandardOutput = true;
            shell.StartInfo.RedirectStandardInput = true;
            shell.StartInfo.CreateNoWindow = true;

            List<string> args = this.commandWithArgs.Skip(1).ToList();
            shell.StartInfo.Arguments = "/c " + string.Join(" ", args);
            shell.OutputDataReceived += p_OutputDataReceived;

            shell.Start();

            shell.BeginOutputReadLine();
            //shell.StandardInput.WriteLine(string.Join(" ", args));



        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            //Console.WriteLine(">>> {0}", e.Data);
            this.callBackForConsole.Invoke(sender, e);
        }
    }
}
