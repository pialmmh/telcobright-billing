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
        private Dictionary<string, ProcessWrapper> processesStatusDictionary = new Dictionary<string, ProcessWrapper>();
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
<<<<<<< HEAD
                Form1.pictureBoxes[instanceName].BackColor = StatusPictureBox.StatusOff;
=======
                Form1.pictureBoxes[instanceName].BackColor = StatusColors.StatusOff;
>>>>>>> a341a24c1fdc2a61116133492f415135cb69eafc
            }
            else if (btn.Text == "Start")
            {
                btn.Text = "Stop";
                this.processWrapper = new ProcessWrapper(instanceName, this);
                this.processWrapper.start();
<<<<<<< HEAD
                Form1.pictureBoxes[instanceName].BackColor = StatusPictureBox.StatusOn;
=======
                Form1.pictureBoxes[instanceName].BackColor = StatusColors.StatusOn;
>>>>>>> a341a24c1fdc2a61116133492f415135cb69eafc
            }
        }

        
    }
}
