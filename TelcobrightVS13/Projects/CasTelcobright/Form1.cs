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
        private Dictionary<string, List<string>> displayPanelOutput = new Dictionary<string, List<string>>();
        Dictionary<string, DisplayPanel> displayPanels = new Dictionary<string, DisplayPanel>();
        public Dictionary<string, Telcobright2> processes = new Dictionary<string, Telcobright2>();

        List<string> allIcx = IcxFactory.getAllIcx();
        Dictionary<string, Button> buttons = new Dictionary<string, Button>();
        NavigationControl navigationControl;
        NavigationButtons navigationButtons;

        // Change the color of your buttons if you want
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
            //List<UserControl> userControls = new List<UserControl>() // Your UserControl list
            //{ displayPanel};
            

            navigationControl = new NavigationControl(displayPanels, panel2); // create an instance of NavigationControl class
        }

        private void InitializeNavigationButtons()
        {

            // create a NavigationButtons instance
            navigationButtons = new NavigationButtons(buttons, btnDefaultColor, btnSelectedtColor);
            // Make a default selected button
            Button btn = this.buttons.Values.ToList()[this.buttons.Values.Count - 1];

            navigationButtons.Highlight(btn);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void commonClick(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string title = (string)btn.Name;
            string tag = (string)btn.Tag;

            navigationControl.Display(tag);
            navigationButtons.Highlight(btn);



            

            if (!this.processes.ContainsKey(tag))
            {
                DisplayPanel displayPanel = displayPanels[tag];
                Telcobright2 t2 = new Telcobright2(tag);
                ProcessWrapper processWrapper = new ProcessWrapper(tag, t2, displayPanel);

                processWrapper.task.Start();
            }
        }

    }
}
