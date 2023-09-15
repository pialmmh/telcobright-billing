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

namespace CasTelcobright
{
    public class ProcessWrapper:IObservable
    {
         
        private ProcessState _processState;
        public ProcessState ProcessState
        {
            get { return _processState; }
            set {
                _processState = value; Notify();
            }
        }
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
                    ProcessState = ProcessState.Alarm;
                }
                else
                {
                    TimeSpan timeDifference = DateTime.Now - lastExceptionTime;                   
                    int minutesDifference = timeDifference.Seconds;

                    if (minutesDifference > 5)
                    {
                        ProcessState = ProcessState.On;
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
            this.ProcessState = ProcessState.On;
        }

        public void stop()
        {
            try
            {
                this.ProcessState = ProcessState.Off;

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
                if (pw.ProcessState == ProcessState.Alarm)
                    Form1.pictureBoxes[pw.instanceName].BackColor = Color.DarkOrange;
                else if(pw.ProcessState == ProcessState.On)
                    Form1.pictureBoxes[pw.instanceName].BackColor = Color.LimeGreen;
                else if (pw.ProcessState == ProcessState.Off)
                    Form1.pictureBoxes[pw.instanceName].BackColor = Color.DarkGray;
            }
        }
    }
}
