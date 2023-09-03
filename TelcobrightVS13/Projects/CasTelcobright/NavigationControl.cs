using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CasTelcobright.Forms;
namespace CasTelcobright
{
    public class NavigationControl
    {
        //List<UserControl> userControlList = new List<UserControl>();
        Dictionary<string, DisplayPanel> listUserControl1s = new Dictionary<string, DisplayPanel>();
        Panel panel;

        public NavigationControl(Dictionary<string, DisplayPanel> listUserControl1s, Panel panel)
        {
            this.listUserControl1s = listUserControl1s;
            this.panel = panel;
            AddUserControls();
        }

        private void AddUserControls()
        {
            foreach (DisplayPanel userControl1 in listUserControl1s.Values)
            {
                userControl1.Dock = DockStyle.Fill;
                panel.Controls.Add(userControl1);
            }
            //for (int i = 0; i < userControlList.Count(); i++)
            //{
            //    userControlList[i].Dock = DockStyle.Fill;
            //    panel.Controls.Add(userControlList[i]);
            //}
        }

        public void Display(String title)
        {
            if (listUserControl1s.ContainsKey(title))
            {
                DisplayPanel userConrControl1 = listUserControl1s[title];
                userConrControl1.BringToFront();
                //DisplayPanel userControl = (DisplayPanel)userConrControl1;
            }
        }
    }
}
