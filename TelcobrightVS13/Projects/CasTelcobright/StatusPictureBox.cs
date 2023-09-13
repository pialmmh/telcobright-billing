using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CasTelcobright
{
    class StatusPictureBox
    {
        Dictionary<string, PictureBox> pictureBoxes;
        Color statusOn = Color.LimeGreen;
        Color statusOff = Color.DarkGray;
        Color statusException = Color.DarkOrange;

        public StatusPictureBox(Dictionary<string, PictureBox> pictureBoxes, Color statusOn, Color statusOff, Color statusException)
        {
            this.pictureBoxes = pictureBoxes;
            this.statusOn = statusOn;
            this.statusOff = statusOff;
            this.statusException = statusException;
            SetPictureBoxBackColor();
        }

        private void SetPictureBoxBackColor()
        {
            foreach (PictureBox pictureBox in pictureBoxes.Values)
            {
                pictureBox.BackColor = statusOff;
            }
        }

        public void SetStatusOn(PictureBox pictureBox)
        {
            pictureBox.BackColor = statusOn;
        }

        public void SetStatusException(PictureBox pictureBox)
        {
            pictureBox.BackColor = statusException;
        }
    }
}
