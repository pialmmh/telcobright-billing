using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CasTelcobright.Forms;
using LibraryExtensions;
using WS_Telcobright_Topshelf;
using TelcobrightInfra;
namespace CasTelcobright
{
    public class ProcessWrapper
    {
        public string instanceName;
        private Telcobright2 telcobright;
        private Thread thread;
        
        public ProcessWrapper(string instanceName, Action<string> callbackFromUI)
        {
            this.instanceName = instanceName;
            this.telcobright = new Telcobright2(instanceName, callbackFromUI);
            this.thread = new Thread(() =>
            {
                this.telcobright.run();
            });
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
                Console.WriteLine(e.StackTrace);
            }
        }

        

    }
}
