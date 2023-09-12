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
        private ThreadStart threadStart;
        public DateTime lastExceptionTime = new DateTime(1800, 1 , 1);
        public Action<string> updateTextbox = null;

        public ProcessWrapper(string instanceName, DisplayPanel displayPanel)
        {
            this.displayPanel = displayPanel;
            this.instanceName = instanceName;
            this.telcobright = new Telcobright2(instanceName);

            this.updateTextbox = (outputFromConsole) =>
            {
                this.displayPanel.richTextBox1.Invoke(new Action(() =>
                {
                    this.displayPanel.richTextBox1.AppendText(outputFromConsole + Environment.NewLine);
                }));
            };
            this.consoleRedirector = new ConsoleRedirector(this.instanceName, this.updateTextbox);
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
