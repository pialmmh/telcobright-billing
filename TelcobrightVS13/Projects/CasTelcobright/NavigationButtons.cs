using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace CasTelcobright
{
    public class NavigationButtons
    {
        Dictionary<string, Button> buttons;
        Color defaultColor;
        Color selectedColor;

        public NavigationButtons(Dictionary<string, Button> buttons, Color defaultColor, Color selectedColor)
        {
            this.buttons = buttons;
            this.defaultColor = defaultColor;
            this.selectedColor = selectedColor;
            SetButtonColor();
        }

        private void SetButtonColor()
        {

            foreach (Button button in buttons.Values)
            {
                button.BackColor = defaultColor;
            }
        }

        public void Highlight(Button selectedButton)
        {
            foreach (Button button in buttons.Values)
            {
                if (button == selectedButton)
                {
                    selectedButton.BackColor = selectedColor;
                }
                else
                {
                    button.BackColor = defaultColor;
                }
            }
        }
    }
}
