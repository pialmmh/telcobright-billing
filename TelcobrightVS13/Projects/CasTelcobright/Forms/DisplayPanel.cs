using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using WS_Telcobright_Topshelf;

namespace CasTelcobright.Forms
{
    public partial class DisplayPanel : UserControl, IObservable
    {
        public string instanceName;
        private ProcessWrapper processWrapper;
        private ProcessState _processState;
        public ProcessState ProcessState
        {
            get { return _processState; }
            set
            {
                _processState = value; Notify();
            }
        }
        public DateTime lastExceptionTime = new DateTime(2800, 1, 1);
        private ExceptionHandler exceptionHandler;
        private IObserver Observer;
        private int maxLines = 500;
        public DisplayPanel(string title)
        {
            this.exceptionHandler = new ExceptionHandler();
            this.subscribe(exceptionHandler);
            InitializeComponent();
            operatorLabel.Text = title;
            this.instanceName = title;
        }

        public void StartStopClick(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Text == "Stop")
            {
                btn.Text = "Start";
                this.ProcessState = ProcessState.Off;
                this.processWrapper.stop();
            }
            else if (btn.Text == "Start")
            {
                btn.Text = "Stop";
                this.ProcessState = ProcessState.On;
                this.processWrapper = new ProcessWrapper(instanceName, this.updateOutputDisplay);
                this.processWrapper.start();
            }
        }

        public void updateOutputDisplay(String outputFromConsole)
        {
            if (outputFromConsole.ToLower().Contains("exception"))
            {
                lastExceptionTime = DateTime.Now;
                ProcessState = ProcessState.Alarm;
            }
            else if (outputFromConsole.ToLower().Contains("errorCount"))
            {
                string[] errorArray = outputFromConsole.Split('=');
                if (int.Parse(errorArray[1]) > 0)
                {
                    ProcessState = ProcessState.Alarm;
                }
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
            this.richTextBox1.Invoke(new Action(() =>
            {
                int lineCount = richTextBox1.Lines.Length;
                if (lineCount > maxLines)
                {
                    richTextBox1.Clear();
                }
                this.richTextBox1.AppendText(outputFromConsole + Environment.NewLine);
                this.richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            }));
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
            DisplayPanel displayPanel = subject as DisplayPanel;
            if (displayPanel != null)
            {
                if (displayPanel.ProcessState == ProcessState.Alarm)
                    Form1.pictureBoxes[displayPanel.instanceName].BackColor = Color.DarkOrange;
                else if (displayPanel.ProcessState == ProcessState.On)
                    Form1.pictureBoxes[displayPanel.instanceName].BackColor = Color.LimeGreen;
                else if (displayPanel.ProcessState == ProcessState.Off)
                    Form1.pictureBoxes[displayPanel.instanceName].BackColor = Color.DarkGray;
            }
        }
    }
}
