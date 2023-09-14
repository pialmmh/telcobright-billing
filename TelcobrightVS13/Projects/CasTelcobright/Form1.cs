using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CasTelcobright.Forms;
using CasTelcobright;
using WS_Telcobright_Topshelf;
using LibraryExtensions;
using System.Threading.Tasks;
namespace CasTelcobright
{
   
    public partial class Form1 : Form
    {
        private string output;
        Dictionary<string, DisplayPanel> displayPanels = new Dictionary<string, DisplayPanel>();
        

        List<string> allIcx = IcxFactory.getAllIcx();
        Dictionary<string, Button> buttons = new Dictionary<string, Button>();
        public static Dictionary<string, PictureBox> pictureBoxes = new Dictionary<string, PictureBox>();
        NavigationControl navigationControl;
        NavigationButtons navigationButtons;
        
        Color btnDefaultColor = Color.FromKnownColor(KnownColor.GrayText);
        Color btnSelectedtColor = Color.FromKnownColor(KnownColor.SlateGray);

        public Form1()
        {
            InitializeComponent();
            InitializeNavigationControl();
            InitializeNavigationButtons();
        }

        private void InitializeNavigationControl()
        {
            navigationControl = new NavigationControl(displayPanels, panel2); 
        }

        private void InitializeNavigationButtons()
        {
            navigationButtons = new NavigationButtons(buttons, btnDefaultColor, btnSelectedtColor);
            Button btn = this.buttons.Values.ToList()[this.buttons.Values.Count - 1];
            navigationButtons.Highlight(btn);
        }

        public void commonClick(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string tag = (string)btn.Tag;
            navigationControl.Display(tag);
            navigationButtons.Highlight(btn);
        }
    }
}
