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
    public class ProcessWrapper:IObservable
    {
        public DisplayPanel displayPanel;
        public string instanceName;
        private Telcobright2 telcobright;
        private ConsoleRedirector consoleRedirector;
        private Thread thread;
        private ThreadStart threadStart;
        public DateTime lastExceptionTime = new DateTime(2800, 1 , 1);
        public Action<string> updateTextbox = null;
        private IObserver Observer;
        private Boolean _exceptionOccur;
        private ExceptionHandler exceptionHandler;
        public Boolean ExceptionOccur
        {
            get { return _exceptionOccur; }
            set { _exceptionOccur = value; Notify(); }
        }

        public ProcessWrapper(string instanceName, DisplayPanel displayPanel)
        {
            this.displayPanel = displayPanel;
            this.instanceName = instanceName;
            this.telcobright = new Telcobright2(instanceName);
            this.exceptionHandler = new ExceptionHandler();
            this.subscribe(exceptionHandler);

            this.updateTextbox = (outputFromConsole) =>
            {
                string output = outputFromConsole;
                if (output.ToLower().Contains("exception"))
                {
                    lastExceptionTime = DateTime.Now;
                    ExceptionOccur = true;
                }
                else
                {
                    TimeSpan timeDifference = DateTime.Now - lastExceptionTime;                   
                    int minutesDifference = timeDifference.Minutes;

                    if (minutesDifference > 5)
                    {
                        ExceptionOccur = false;
                    }
                }
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

        public void subscribe(IObserver Observer)
        {
            this.Observer = Observer;
        }

        public void Notify()
        {
            this.Observer.Update(this);
        }
    }

    class ExceptionHandler : IObserver
    {
        public void Update(IObservable subject)
        {
            ProcessWrapper pw = subject as ProcessWrapper;
            if (pw != null)
            {
                if(pw.ExceptionOccur)
                    Form1.pictureBoxes[pw.instanceName].BackColor = StatusColors.StatusException;
                else
                    Form1.pictureBoxes[pw.instanceName].BackColor = StatusColors.StatusOn;
            }
        }
    }
}
