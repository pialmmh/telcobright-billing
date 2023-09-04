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
        public CancellationTokenSource cancellationTokenSource;
        private Telcobright2 telcobright;
       // private Thread thread;
        public Task task;

        public Action<string> updateTextbox = null;

        public ProcessWrapper(string instanceName, DisplayPanel displayPanel)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
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
            ConsoleRedirector consoleRedirector = new ConsoleRedirector(this.instanceName, this.updateTextbox);
            task = new Task(() => this.telcobright.run(consoleRedirector, cancellationToken), cancellationToken);

        }
    }
}
