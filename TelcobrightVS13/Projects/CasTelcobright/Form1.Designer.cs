using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using CasTelcobright.Forms;
using System.IO;

namespace CasTelcobright
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            panel2 = new Panel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.Silver;
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.TabIndex = 0;

            foreach (var icxName in allIcx)
            {
                var displayPanel = new DisplayPanel(icxName);

                displayPanel.richTextBox1.Tag = icxName;
                displayPanels.Add(icxName, displayPanel);
            }

            for (int i = allIcx.Count - 1, j = 1; i >= 0; --i, ++j)
            {
                addIcxButton(allIcx[i], 23 * i, j);
                addPictureBox(allIcx[i], ++j, 23 * i);
            }


            // 
            // panel2
            // 
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(220, 0);
            panel2.Name = "panel2";
            panel2.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 550);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Name = "Form1";
            Text = "CAS Project Manager";
            panel1.ResumeLayout(false);
            ResumeLayout(false);


            
            this.ResumeLayout(false);

        }

        private void addPictureBox(string operatorName, int tabIndex, int yCoordinate)
        {
            PictureBox pictureBox = new PictureBox();
            //this.pictureBoxes.Add(operatorName, pictureBox);
            //pictureBox = new System.Windows.Forms.PictureBox();
            //((System.ComponentModel.ISupportInitialize)(pictureBox)).BeginInit();
            //this.SuspendLayout();
            // 
            // pictureBox1
            // 
            // pictureBox.Dock = DockStyle.Left;
            pictureBox.Location = new System.Drawing.Point(183, yCoordinate+8);
            pictureBox.Name = "pictureBox1";
            pictureBox.Size = new Size(10, 10);
            pictureBox.TabIndex = tabIndex;
            //pictureBox.TabStop = false;
            pictureBox.BackColor = Color.DarkGray;       // Default Color
            pictureBox.Tag = operatorName;


            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, pictureBox.Width, pictureBox.Height);

            // Apply the mask to the PictureBox
            pictureBox.Region = new Region(path);
            //try
            //{
            //    string imagePath =
            //        "temp2/Amber.PNG";
            //    // Attempt to load an image from a file
            //    Image img = Image.FromFile(imagePath);
            //    pictureBox.Size = img.Size;
            //    pictureBox.Image = img;
            //}
            //catch (FileNotFoundException ex)
            //{
            //    // Handle the file not found exception by displaying an error message
            //    MessageBox.Show("Error: File not found. Please check the file path.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            

            // 
            // Form1
            // 
            //this.ClientSize = new System.Drawing.Size(284, 261);
            //this.Controls.Add(pictureBox);
            panel1.Controls.Add(pictureBox);
            //this.Name = "Form1";  
            //((System.ComponentModel.ISupportInitialize)(pictureBox)).EndInit();
        }

        private void addIcxButton(string operatorName, int yCoordinate, int tabIndex)
        {
            Button btnIcx = new Button();
            this.buttons.Add(operatorName, btnIcx);

            //btnIcx.Dock = DockStyle.Top;
            btnIcx.FlatAppearance.BorderColor = Color.DimGray;
            btnIcx.FlatAppearance.BorderSize = 1;
            btnIcx.FlatStyle = FlatStyle.Flat;
            btnIcx.Font = new Font("Segoe UI", 7F, FontStyle.Bold, GraphicsUnit.Point);
            btnIcx.ForeColor = Color.Gainsboro;
            btnIcx.Location = new Point(0, yCoordinate);
            btnIcx.Name = "btn  " + operatorName;
            btnIcx.Size = new Size(180, 23);
            btnIcx.TabIndex = tabIndex;
            btnIcx.Text = operatorName;
            btnIcx.UseVisualStyleBackColor = true;
            btnIcx.Click += commonClick;
            btnIcx.Tag = operatorName;
            panel1.Controls.Add(btnIcx);
        }

        #endregion

        private Panel panel1;
        //private Button btn_banglatel;
        //private Button btn_bantel;
        //private Button btn_jibondhara;
        //private Button btn_summit;
        private Panel panel2;
    }
}

