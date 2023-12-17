using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using CasTelcobright.Forms;
using System.IO;
using System.Collections.Generic;
namespace CasTelcobright
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        List<FileInfo> AllFileInfos { get; set; } = new List<FileInfo>();

        protected override void Dispose(bool disposing)
        {
            foreach (var display in displayPanels)
            {
                if (display.Value.ProcessState == ProcessState.On)
                {
                    int processId = display.Value.processWrapper.processId;
                    System.Diagnostics.Process procs = System.Diagnostics.Process.GetProcessById(processId, ".");
                    procs.Kill();
                }
            }

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            panel1 = new Panel();
            panel2 = new Panel();
            panel1.SuspendLayout();
            SuspendLayout();

            panel1.BackColor = Color.Silver;
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.TabIndex = 0;

            int lastYCoordinate = 0;
            for (int i = allIcx.Count - 1, j = 1; i >= 0; --i, ++j)
            {
                lastYCoordinate = 23 * i;
                addIcxButton(allIcx[i], lastYCoordinate, j);
                addPictureBox(allIcx[i], ++j, lastYCoordinate);
            }
            foreach (var icxName in allIcx)
            {
                var displayPanel = new DisplayPanel(icxName);
                displayPanel.richTextBox1.Tag = icxName;
                displayPanels.Add(icxName, displayPanel);
            }

            //for (int i = 23; i < 33; i += 2)
            //{
            //    Label pLabel = new Label();
            //    this.labels.Add(pLabel);
            //    pLabel.Text = "D:\nT: 500G, F: 2G, Fls: 1065";
            //    pLabel.Location = new Point(0, i * 24);
            //    pLabel.AutoSize = true;
            //    pLabel.Height = 60;
            //    pLabel.Width = 183;
            //    pLabel.Font = new Font("Calibri", 10);
            //    pLabel.ForeColor = Color.Green;
            //    panel1.Controls.Add(pLabel);
            //}

            string[] driveLetters = Environment.GetLogicalDrives();

            int labelIndex = 24;
            for (int i = 0; i < driveLetters.Length && labelIndex < 35; i++)
            {
                Label pLabel = new Label();
                this.labels.Add(pLabel);

                string driveLetter = driveLetters[i];
                DriveInfo driveInfo = new DriveInfo(driveLetter);

                string driveletter = driveLetters[i];
                getFilesInfoRecursively(driveLetter+"telcobright");

                string labelText = $"{driveLetter}:\nT: {FormatSize(driveInfo.TotalSize)}, F: {FormatSize(driveInfo.AvailableFreeSpace)}, Files: {AllFileInfos.Count}";
                AllFileInfos.Clear();
                pLabel.Text = labelText;

                pLabel.Location = new Point(0, labelIndex * 24);
                pLabel.AutoSize = false;
                pLabel.Height = 40;
                pLabel.Width = 183;
                pLabel.Font = new Font("Calibri", 8);
                pLabel.BackColor = Color.DimGray;
                pLabel.ForeColor = Color.White;
                panel1.Controls.Add(pLabel);

                labelIndex += 2;
            }


            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(220, 0);
            panel2.Name = "panel2";
            panel2.TabIndex = 1;

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

        private string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes /= 1024;
            }

            return $"{bytes} {sizes[order]}";
        }
        private void getFilesInfoRecursively(string parentDir)
        {
            string[] filePahts = Directory.GetFiles(parentDir);

            foreach (string filePath in filePahts)
            {
                FileInfo fileInfo = new FileInfo(filePath);
                AllFileInfos.Add(fileInfo);
            }

            string[] subDirs = Directory.GetDirectories(parentDir);
            foreach (string subDir in subDirs)
            {
               
                getFilesInfoRecursively(subDir);
            }
        }
        private void addPictureBox(string operatorName, int tabIndex, int yCoordinate)
        {
            PictureBox pictureBox = new PictureBox();
            pictureBoxes.Add(operatorName, pictureBox);

            pictureBox.Location = new System.Drawing.Point(183, yCoordinate + 8);
            pictureBox.Name = "pictureBox1";
            pictureBox.Size = new Size(10, 10);
            pictureBox.TabIndex = tabIndex;
            pictureBox.BackColor = Color.DarkGray;
            pictureBox.Tag = operatorName;

            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, pictureBox.Width, pictureBox.Height);
            pictureBox.Region = new Region(path);
            panel1.Controls.Add(pictureBox);
        }

        private void addIcxButton(string operatorName, int yCoordinate, int tabIndex)
        {
            Button btnIcx = new Button();
            this.buttons.Add(operatorName, btnIcx);

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
        private Button btn_new;
        private Panel panel2;
    }
}
