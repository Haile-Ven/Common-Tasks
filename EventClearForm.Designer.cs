namespace Common_Tasks
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
            this.prgsBar = new System.Windows.Forms.ProgressBar();
            this.prgLbl = new System.Windows.Forms.Label();
            this.msgLbl = new System.Windows.Forms.Label();
            this.sucLbl = new System.Windows.Forms.Label();
            this.errLbl = new System.Windows.Forms.Label();
            this.imgPcBx = new System.Windows.Forms.PictureBox();
            this.totEvtLbl = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.imgPcBx)).BeginInit();
            this.SuspendLayout();
            // 
            // prgsBar
            // 
            this.prgsBar.Location = new System.Drawing.Point(93, 12);
            this.prgsBar.Name = "prgsBar";
            this.prgsBar.Size = new System.Drawing.Size(646, 34);
            this.prgsBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prgsBar.TabIndex = 5;
            this.prgsBar.UseWaitCursor = true;
            // 
            // prgLbl
            // 
            this.prgLbl.AutoSize = true;
            this.prgLbl.Location = new System.Drawing.Point(105, 49);
            this.prgLbl.Name = "prgLbl";
            this.prgLbl.Size = new System.Drawing.Size(0, 16);
            this.prgLbl.TabIndex = 4;
            // 
            // msgLbl
            // 
            this.msgLbl.AutoSize = true;
            this.msgLbl.Location = new System.Drawing.Point(106, 85);
            this.msgLbl.Name = "msgLbl";
            this.msgLbl.Size = new System.Drawing.Size(0, 16);
            this.msgLbl.TabIndex = 6;
            // 
            // sucLbl
            // 
            this.sucLbl.AutoSize = true;
            this.sucLbl.Location = new System.Drawing.Point(452, 49);
            this.sucLbl.Name = "sucLbl";
            this.sucLbl.Size = new System.Drawing.Size(90, 16);
            this.sucLbl.TabIndex = 7;
            this.sucLbl.Text = "0 Succeeded.";
            // 
            // errLbl
            // 
            this.errLbl.AutoSize = true;
            this.errLbl.Location = new System.Drawing.Point(572, 49);
            this.errLbl.Name = "errLbl";
            this.errLbl.Size = new System.Drawing.Size(58, 16);
            this.errLbl.TabIndex = 8;
            this.errLbl.Text = "0 Failed.";
            // 
            // imgPcBx
            // 
            this.imgPcBx.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.imgPcBx.Image = ((System.Drawing.Image)(resources.GetObject("imgPcBx.Image")));
            this.imgPcBx.Location = new System.Drawing.Point(13, 12);
            this.imgPcBx.Name = "imgPcBx";
            this.imgPcBx.Size = new System.Drawing.Size(74, 78);
            this.imgPcBx.TabIndex = 9;
            this.imgPcBx.TabStop = false;
            // 
            // totEvtLbl
            // 
            this.totEvtLbl.AutoSize = true;
            this.totEvtLbl.Location = new System.Drawing.Point(636, 49);
            this.totEvtLbl.Name = "totEvtLbl";
            this.totEvtLbl.Size = new System.Drawing.Size(0, 16);
            this.totEvtLbl.TabIndex = 10;
            // 
            // EventClearForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(751, 115);
            this.Controls.Add(this.totEvtLbl);
            this.Controls.Add(this.imgPcBx);
            this.Controls.Add(this.errLbl);
            this.Controls.Add(this.sucLbl);
            this.Controls.Add(this.msgLbl);
            this.Controls.Add(this.prgsBar);
            this.Controls.Add(this.prgLbl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EventClearForm";
            this.Text = "Clear Events";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EventClearForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EventClearForm_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.imgPcBx)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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