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
        private ProcessWrapper processWrapper;
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
                btn.Text = "Start";
                this.processWrapper.stop();
            }
            else if (btn.Text == "Start")
            {
                btn.Text = "Stop";
                this.processWrapper = new ProcessWrapper(instanceName, this);
                this.processWrapper.start();
            }
        }
    }
}
