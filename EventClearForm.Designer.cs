﻿namespace Common_Tasks
{
    partial class EventClearForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EventClearForm));
            prgsBar = new System.Windows.Forms.ProgressBar();
            prgLbl = new System.Windows.Forms.Label();
            msgLbl = new System.Windows.Forms.Label();
            sucLbl = new System.Windows.Forms.Label();
            errLbl = new System.Windows.Forms.Label();
            imgPcBx = new System.Windows.Forms.PictureBox();
            totEvtLbl = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)imgPcBx).BeginInit();
            SuspendLayout();
            // 
            // prgsBar
            // 
            prgsBar.Location = new System.Drawing.Point(93, 15);
            prgsBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            prgsBar.Name = "prgsBar";
            prgsBar.Size = new System.Drawing.Size(646, 42);
            prgsBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            prgsBar.TabIndex = 5;
            prgsBar.UseWaitCursor = true;
            // 
            // prgLbl
            // 
            prgLbl.AutoSize = true;
            prgLbl.Location = new System.Drawing.Point(105, 61);
            prgLbl.Name = "prgLbl";
            prgLbl.Size = new System.Drawing.Size(0, 20);
            prgLbl.TabIndex = 4;
            // 
            // msgLbl
            // 
            msgLbl.AutoSize = true;
            msgLbl.Location = new System.Drawing.Point(13, 99);
            msgLbl.Name = "msgLbl";
            msgLbl.Size = new System.Drawing.Size(0, 20);
            msgLbl.TabIndex = 6;
            // 
            // sucLbl
            // 
            sucLbl.AutoSize = true;
            sucLbl.Location = new System.Drawing.Point(452, 61);
            sucLbl.Name = "sucLbl";
            sucLbl.Size = new System.Drawing.Size(96, 20);
            sucLbl.TabIndex = 7;
            sucLbl.Text = "0 Succeeded.";
            // 
            // errLbl
            // 
            errLbl.AutoSize = true;
            errLbl.Location = new System.Drawing.Point(572, 61);
            errLbl.Name = "errLbl";
            errLbl.Size = new System.Drawing.Size(63, 20);
            errLbl.TabIndex = 8;
            errLbl.Text = "0 Failed.";
            // 
            // imgPcBx
            // 
            imgPcBx.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            imgPcBx.Image = (System.Drawing.Image)resources.GetObject("imgPcBx.Image");
            imgPcBx.Location = new System.Drawing.Point(13, 15);
            imgPcBx.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            imgPcBx.Name = "imgPcBx";
            imgPcBx.Size = new System.Drawing.Size(74, 66);
            imgPcBx.TabIndex = 9;
            imgPcBx.TabStop = false;
            // 
            // totEvtLbl
            // 
            totEvtLbl.AutoSize = true;
            totEvtLbl.Location = new System.Drawing.Point(636, 61);
            totEvtLbl.Name = "totEvtLbl";
            totEvtLbl.Size = new System.Drawing.Size(0, 20);
            totEvtLbl.TabIndex = 10;
            // 
            // EventClearForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(751, 144);
            Controls.Add(totEvtLbl);
            Controls.Add(imgPcBx);
            Controls.Add(errLbl);
            Controls.Add(sucLbl);
            Controls.Add(msgLbl);
            Controls.Add(prgsBar);
            Controls.Add(prgLbl);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "EventClearForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Clear Events";
            ((System.ComponentModel.ISupportInitialize)imgPcBx).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ProgressBar prgsBar;
        private System.Windows.Forms.Label prgLbl;
        private System.Windows.Forms.Label msgLbl;
        private System.Windows.Forms.Label sucLbl;
        private System.Windows.Forms.Label errLbl;
        private System.Windows.Forms.PictureBox imgPcBx;
        private System.Windows.Forms.Label totEvtLbl;
    }
}