using System.Drawing;
using System.Windows.Forms;

namespace CasTelcobright.Forms
{
    partial class UserControl1
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutPanel1 = new TableLayoutPanel();
            btn_start = new Button();
            btn_stop = new Button();
            richTextBox1 = new RichTextBox();
            operatorLabel = new Label();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(btn_start, 1, 0);
            tableLayoutPanel1.Controls.Add(btn_stop, 0, 0);
            tableLayoutPanel1.Location = new Point(17, 326);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(237, 28);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // btn_start
            // 
            btn_start.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btn_start.Location = new Point(121, 3);
            btn_start.Name = "btn_start";
            btn_start.Size = new Size(113, 22);
            btn_start.TabIndex = 0;
            btn_start.Text = "Start";
            btn_start.UseVisualStyleBackColor = true;
            // 
            // btn_stop
            // 
            btn_stop.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btn_stop.Location = new Point(3, 3);
            btn_stop.Name = "btn_stop";
            btn_stop.Size = new Size(112, 22);
            btn_stop.TabIndex = 1;
            btn_stop.Text = "Stop";
            btn_stop.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox1.Location = new Point(17, 49);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.ReadOnly = true;
            richTextBox1.Size = new Size(528, 262);
            richTextBox1.TabIndex = 2;
            richTextBox1.Text = "";
            // 
            // operatorLabel
            // 
            operatorLabel.AutoSize = true;
            operatorLabel.BackColor = Color.Transparent;
            operatorLabel.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point);
            operatorLabel.ForeColor = SystemColors.ControlDarkDark;
            operatorLabel.Location = new Point(17, 9);
            operatorLabel.Name = "operatorLabel";
            operatorLabel.Size = new Size(134, 37);
            operatorLabel.TabIndex = 3;
            operatorLabel.Text = "Operator";
            // 
            // UserControl1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(operatorLabel);
            Controls.Add(richTextBox1);
            Controls.Add(tableLayoutPanel1);
            Name = "UserControl1";
            Size = new Size(564, 380);
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TableLayoutPanel tableLayoutPanel1;
        private Button btn_stop;
        private Button btn_start;
        private RichTextBox richTextBox1;
        private Label operatorLabel;
    }
}
