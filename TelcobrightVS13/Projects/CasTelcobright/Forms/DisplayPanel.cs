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
    public partial class DisplayPanel : UserControl
    {
        public string instanceName;
        public DisplayPanel(string title)
        {
            InitializeComponent();
            operatorLabel.Text = title;
            this.instanceName = title;
        }

        public void SetTitle(string title)
        {
            
        }

    }
}
