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
    public partial class DisplayPanel : UserControl
    {
        public string instanceName;
        public DisplayPanel(string title)
        {
            InitializeComponent();
            operatorLabel.Text = title;
            this.instanceName = title;
        }

        public void StartStopClick(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Text == "Stop")
            {
                if (Form1.processes.ContainsKey(instanceName))
                {
                    Telcobright2 t2 = Form1.processes[instanceName];
                    Form1.processes.Remove(instanceName);
                    t2 = null;
                    btn.Text = "Start";
                }
            } else if (btn.Text == "Start")
            {
                btn.Text = "Stop";
                Form1 form1 = new Form1();
                form1.process(instanceName);
            }
            //btn.Text = (btn.Text == "Stop") ? "Start" : "Stop";
        }

    }
}
