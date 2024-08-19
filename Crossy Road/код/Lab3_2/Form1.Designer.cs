using System.Drawing;
using System.Windows.Forms;

namespace Lab3_2
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
            this.trainWay = new System.Windows.Forms.Panel();
            this.PlayerField = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // trainWay
            // 
            this.trainWay.Location = new System.Drawing.Point(300, 100);
            this.trainWay.Name = "trainWay";
            this.trainWay.Size = new System.Drawing.Size(700, 500);
            this.trainWay.TabIndex = 0;
            // 
            // PlayerField
            // 
            this.PlayerField.Location = new System.Drawing.Point(0, 0);
            this.PlayerField.Name = "PlayerField";
            this.PlayerField.Size = new System.Drawing.Size(1366, 749);
            this.PlayerField.TabIndex = 0;
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(1366, 749);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "LAB3: Game";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private Panel trainWay;
        private Panel PlayerField;
    }
}

