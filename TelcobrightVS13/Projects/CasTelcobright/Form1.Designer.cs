using System.Windows.Forms;
using System.Drawing;
using CasTelcobright.Forms;

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

            for (int i = allIcx.Count - 1, j = 1; i >= 0; i--, j++)
            {
                addIcxButton(allIcx[i], 40 * i, j);
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
        }

        private void addIcxButton(string operatorName, int yCoordinate, int tabIndex)
        {
            Button btnIcx = new Button();
            this.buttons.Add(operatorName, btnIcx);

            btnIcx.Dock = DockStyle.Top;
            btnIcx.FlatAppearance.BorderColor = Color.DimGray;
            btnIcx.FlatAppearance.BorderSize = 1;
            btnIcx.FlatStyle = FlatStyle.Flat;
            btnIcx.Font = new Font("Segoe UI", 7F, FontStyle.Bold, GraphicsUnit.Point);
            btnIcx.ForeColor = Color.Gainsboro;
            btnIcx.Location = new Point(0, yCoordinate);
            btnIcx.Name = "btn  " + operatorName;
            btnIcx.Size = new Size(220, 23);
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

