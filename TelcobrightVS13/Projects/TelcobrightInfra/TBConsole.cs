using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightInfra
{
    public class TBConsole
    {
        private string InstanceName { get; set; }
        //public string ConsolePrefix { get; set; }
        private Action<string> callbackFromUI;
        public TBConsole(string instanceName, Action<string> callbackFromUI, string consolePrefix= "%%%%%")
        {
            this.InstanceName = instanceName;
            //this.ConsolePrefix = consolePrefix;
            this.callbackFromUI = callbackFromUI;
        }

        public void WriteLine(string msgToPrintInConsole)
        {
            //Console.WriteLine(this.ConsolePrefix + this.InstanceName + this.ConsolePrefix + msgToPrintInConsole);
            Console.WriteLine(msgToPrintInConsole);
            callbackFromUI?.Invoke(msgToPrintInConsole);
        }
        public void WriteLine(Exception e)
        {
            //Console.WriteLine(this.ConsolePrefix + this.InstanceName + this.ConsolePrefix + msgToPrintInConsole);
            Console.WriteLine(e.Message);
            callbackFromUI?.Invoke(e.Message);
        }
    }
}
