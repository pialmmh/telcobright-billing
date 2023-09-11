using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CasTelcobright.Forms;
using LibraryExtensions;
using WS_Telcobright_Topshelf;

namespace CasTelcobright
{
    public class ProcessWrapper
    {
        public DisplayPanel displayPanel;
        public string instanceName;
        private Telcobright2 telcobright;
        private ConsoleRedirector consoleRedirector;
        private Thread thread;
        public Task task;
        private ThreadStart threadStart;

        public Action<string> updateTextbox = null;

        public ProcessWrapper(string instanceName, DisplayPanel displayPanel)
        {
            this.displayPanel = displayPanel;
            this.instanceName = instanceName;
            this.telcobright = new Telcobright2(instanceName);

            this.updateTextbox = (outputFromConsole) =>
            {
                string output = outputFromConsole;
                //string output = string.Join("", outputFromConsole.Split('#').Skip(1));
                this.displayPanel.richTextBox1.Invoke(new Action(() =>
                {
                    this.displayPanel.richTextBox1.AppendText(output + Environment.NewLine);
                }));
            };
            this.consoleRedirector = new ConsoleRedirector(this.instanceName, this.updateTextbox);
            //task = new Task(() => this.telcobright.run(consoleRedirector));
            this.threadStart = new ThreadStart(() =>
            {
                this.telcobright.run(consoleRedirector);
            });

            this.thread = new Thread(this.threadStart);
        }

        public void start()
        {
            this.thread.Start();
        }

        public void stop()
        {
            try
            {
                if (this.thread.IsAlive)
                {
                    this.thread.Abort();
                }
            }
            catch (ThreadAbortException e)
            { 
                Thread.ResetAbort();
            }
        }
    }
}
