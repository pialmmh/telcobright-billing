using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_Telcobright_Topshelf
{
    public class TBConsole
    {
        private string InstanceName { get; set; }
        public string ConsolePrefix { get; set; }

        public TBConsole(string instanceName, string consolePrefix= "%%%%%")
        {
            this.InstanceName = instanceName;
            this.ConsolePrefix = consolePrefix;
        }

        public void WriteLine(string msgToPrintInConsole)
        {
            Console.WriteLine(this.ConsolePrefix + this.InstanceName + this.ConsolePrefix + msgToPrintInConsole);
        }
    }
}
