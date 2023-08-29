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

namespace CasTelcobright.Forms
{
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        public void SetTitle(string title)
        {
            operatorLabel.Text = title; // Assuming 'titleLabel' is the name of a label control on your user control.
        }


        public void executeShell(string operatorName)
        {
            ShellExecutor shellExecutor =
                new ShellExecutor(new List<string> { "C:\\Windows\\System32\\cmd.exe", "echo", "srtelecom" },
                this.p_OutputDataReceived);
            richTextBox1.Text = "";
            shellExecutor.execute();
        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            richTextBox1.Invoke(new Action(() =>
            {
                richTextBox1.AppendText(e.Data + Environment.NewLine);
            }));
        }

    }
}
