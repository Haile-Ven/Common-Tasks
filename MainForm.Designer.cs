
namespace Common_Tasks
{
    partial class MainForm
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            groupBox2 = new System.Windows.Forms.GroupBox();
            ClrEvntBtn = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label2 = new System.Windows.Forms.Label();
            HoursBoard = new System.Windows.Forms.NumericUpDown();
            MinuteBoard = new System.Windows.Forms.NumericUpDown();
            label1 = new System.Windows.Forms.Label();
            CancelBtn = new System.Windows.Forms.Button();
            ShutdownBtn = new System.Windows.Forms.Button();
            remTmLbl = new System.Windows.Forms.Label();
            shtDwnTmLbl = new System.Windows.Forms.Label();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            timerPanel = new System.Windows.Forms.Panel();
            taskTrayIcon = new System.Windows.Forms.NotifyIcon(components);
            trayMenu = new System.Windows.Forms.ContextMenuStrip(components);
            ExitTrayMenu = new System.Windows.Forms.ToolStripMenuItem();
            OptionmenuStrip = new System.Windows.Forms.MenuStrip();
            optionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)HoursBoard).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MinuteBoard).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            timerPanel.SuspendLayout();
            trayMenu.SuspendLayout();
            OptionmenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox2
            // 
            groupBox2.BackColor = System.Drawing.Color.FromArgb(251, 251, 251);
            groupBox2.Controls.Add(ClrEvntBtn);
            groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            groupBox2.ForeColor = System.Drawing.Color.FromArgb(32, 32, 32);
            groupBox2.Location = new System.Drawing.Point(160, 21);
            groupBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(12, 16, 12, 12);
            groupBox2.Size = new System.Drawing.Size(157, 144);
            groupBox2.TabIndex = 16;
            groupBox2.TabStop = false;
            groupBox2.Text = "Event Viewer";
            // 
            // ClrEvntBtn
            // 
            ClrEvntBtn.BackColor = System.Drawing.Color.FromArgb(0, 99, 177);
            ClrEvntBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 99, 177);
            ClrEvntBtn.FlatAppearance.BorderSize = 0;
            ClrEvntBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            ClrEvntBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            ClrEvntBtn.ForeColor = System.Drawing.Color.White;
            ClrEvntBtn.Image = (System.Drawing.Image)resources.GetObject("ClrEvntBtn.Image");
            ClrEvntBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            ClrEvntBtn.Location = new System.Drawing.Point(14, 55);
            ClrEvntBtn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ClrEvntBtn.Name = "ClrEvntBtn";
            ClrEvntBtn.Size = new System.Drawing.Size(126, 47);
            ClrEvntBtn.TabIndex = 8;
            ClrEvntBtn.Text = "Clear Events";
            ClrEvntBtn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            ClrEvntBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            ClrEvntBtn.UseVisualStyleBackColor = false;
            ClrEvntBtn.Click += ClrEvntBtn_Click;
            // 
            // groupBox1
            // 
            groupBox1.BackColor = System.Drawing.Color.FromArgb(251, 251, 251);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(HoursBoard);
            groupBox1.Controls.Add(MinuteBoard);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(CancelBtn);
            groupBox1.Controls.Add(ShutdownBtn);
            groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            groupBox1.ForeColor = System.Drawing.Color.FromArgb(32, 32, 32);
            groupBox1.Location = new System.Drawing.Point(12, 195);
            groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(12, 16, 12, 12);
            groupBox1.Size = new System.Drawing.Size(288, 146);
            groupBox1.TabIndex = 18;
            groupBox1.TabStop = false;
            groupBox1.Text = "Schedule Power Management";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label2.ForeColor = System.Drawing.Color.FromArgb(32, 32, 32);
            label2.Location = new System.Drawing.Point(81, 46);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(43, 16);
            label2.TabIndex = 12;
            label2.Text = "Hours";
            // 
            // HoursBoard
            // 
            HoursBoard.BackColor = System.Drawing.Color.FromArgb(251, 251, 251);
            HoursBoard.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            HoursBoard.ForeColor = System.Drawing.Color.FromArgb(32, 32, 32);
            HoursBoard.Location = new System.Drawing.Point(26, 38);
            HoursBoard.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            HoursBoard.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            HoursBoard.Name = "HoursBoard";
            HoursBoard.Size = new System.Drawing.Size(49, 22);
            HoursBoard.TabIndex = 11;
            // 
            // MinuteBoard
            // 
            MinuteBoard.BackColor = System.Drawing.Color.FromArgb(251, 251, 251);
            MinuteBoard.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            MinuteBoard.ForeColor = System.Drawing.Color.FromArgb(32, 32, 32);
            MinuteBoard.Location = new System.Drawing.Point(130, 38);
            MinuteBoard.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            MinuteBoard.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            MinuteBoard.Name = "MinuteBoard";
            MinuteBoard.Size = new System.Drawing.Size(49, 22);
            MinuteBoard.TabIndex = 10;
            MinuteBoard.ValueChanged += MinuteBoard_ValueChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label1.ForeColor = System.Drawing.Color.FromArgb(32, 32, 32);
            label1.Location = new System.Drawing.Point(185, 46);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(53, 16);
            label1.TabIndex = 9;
            label1.Text = "Minutes";
            // 
            // CancelBtn
            // 
            CancelBtn.BackColor = System.Drawing.Color.FromArgb(0, 99, 177);
            CancelBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 99, 177);
            CancelBtn.FlatAppearance.BorderSize = 0;
            CancelBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            CancelBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            CancelBtn.ForeColor = System.Drawing.Color.White;
            CancelBtn.Location = new System.Drawing.Point(132, 83);
            CancelBtn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            CancelBtn.Name = "CancelBtn";
            CancelBtn.Size = new System.Drawing.Size(100, 47);
            CancelBtn.TabIndex = 8;
            CancelBtn.Text = "Cancel";
            CancelBtn.UseVisualStyleBackColor = false;
            CancelBtn.Click += CancelBtn_Click;
            // 
            // ShutdownBtn
            // 
            ShutdownBtn.BackColor = System.Drawing.Color.FromArgb(0, 99, 177);
            ShutdownBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 99, 177);
            ShutdownBtn.FlatAppearance.BorderSize = 0;
            ShutdownBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            ShutdownBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            ShutdownBtn.ForeColor = System.Drawing.Color.White;
            ShutdownBtn.Location = new System.Drawing.Point(26, 83);
            ShutdownBtn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ShutdownBtn.Name = "ShutdownBtn";
            ShutdownBtn.Size = new System.Drawing.Size(100, 47);
            ShutdownBtn.TabIndex = 7;
            ShutdownBtn.Text = "Shutdown";
            ShutdownBtn.UseVisualStyleBackColor = false;
            ShutdownBtn.Click += ShutdownBtn_Click;
            // 
            // remTmLbl
            // 
            remTmLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            remTmLbl.ForeColor = System.Drawing.Color.FromArgb(32, 32, 32);
            remTmLbl.Location = new System.Drawing.Point(14, 66);
            remTmLbl.MaximumSize = new System.Drawing.Size(295, 0);
            remTmLbl.Name = "remTmLbl";
            remTmLbl.Size = new System.Drawing.Size(290, 0);
            remTmLbl.TabIndex = 14;
            // 
            // shtDwnTmLbl
            // 
            shtDwnTmLbl.AutoSize = true;
            shtDwnTmLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            shtDwnTmLbl.ForeColor = System.Drawing.Color.FromArgb(32, 32, 32);
            shtDwnTmLbl.Location = new System.Drawing.Point(14, 9);
            shtDwnTmLbl.Name = "shtDwnTmLbl";
            shtDwnTmLbl.Size = new System.Drawing.Size(0, 18);
            shtDwnTmLbl.TabIndex = 13;
            // 
            // pictureBox1
            // 
            pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            pictureBox1.Image = (System.Drawing.Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.InitialImage = null;
            pictureBox1.Location = new System.Drawing.Point(12, 15);
            pictureBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(142, 163);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 17;
            pictureBox1.TabStop = false;
            // 
            // timerPanel
            // 
            timerPanel.BackColor = System.Drawing.Color.FromArgb(251, 251, 251);
            timerPanel.Controls.Add(remTmLbl);
            timerPanel.Controls.Add(shtDwnTmLbl);
            timerPanel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            timerPanel.Location = new System.Drawing.Point(2, 349);
            timerPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            timerPanel.Name = "timerPanel";
            timerPanel.Size = new System.Drawing.Size(315, 135);
            timerPanel.TabIndex = 20;
            // 
            // taskTrayIcon
            // 
            taskTrayIcon.ContextMenuStrip = trayMenu;
            taskTrayIcon.Icon = (System.Drawing.Icon)resources.GetObject("taskTrayIcon.Icon");
            taskTrayIcon.Text = "Common Tasks";
            taskTrayIcon.MouseClick += taskTrayIcon_MouseClick;
            taskTrayIcon.MouseDoubleClick += taskTrayIcon_MouseDoubleClick;
            // 
            // trayMenu
            // 
            trayMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { ExitTrayMenu });
            trayMenu.Name = "trayMenu";
            trayMenu.Size = new System.Drawing.Size(103, 28);
            // 
            // ExitTrayMenu
            // 
            ExitTrayMenu.Name = "ExitTrayMenu";
            ExitTrayMenu.Size = new System.Drawing.Size(102, 24);
            ExitTrayMenu.Text = "Exit";
            ExitTrayMenu.Click += ExitTrayMenu_Click;
            // 
            // OptionmenuStrip
            // 
            OptionmenuStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            OptionmenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            OptionmenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { optionToolStripMenuItem });
            OptionmenuStrip.Location = new System.Drawing.Point(0, 476);
            OptionmenuStrip.Name = "OptionmenuStrip";
            OptionmenuStrip.Size = new System.Drawing.Size(328, 28);
            OptionmenuStrip.TabIndex = 21;
            OptionmenuStrip.Text = "OptionmenuStrip";
            // 
            // optionToolStripMenuItem
            // 
            optionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exitToolStripMenuItem });
            optionToolStripMenuItem.Name = "optionToolStripMenuItem";
            optionToolStripMenuItem.Size = new System.Drawing.Size(69, 24);
            optionToolStripMenuItem.Text = "Option";
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(116, 26);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(243, 243, 243);
            ClientSize = new System.Drawing.Size(328, 504);
            Controls.Add(OptionmenuStrip);
            Controls.Add(timerPanel);
            Controls.Add(pictureBox1);
            Controls.Add(groupBox1);
            Controls.Add(groupBox2);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = OptionmenuStrip;
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            MaximizeBox = false;
            Name = "MainForm";
            Text = "Common Tasks";
            FormClosing += MainForm_FormClosing;
            groupBox2.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)HoursBoard).EndInit();
            ((System.ComponentModel.ISupportInitialize)MinuteBoard).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            timerPanel.ResumeLayout(false);
            timerPanel.PerformLayout();
            trayMenu.ResumeLayout(false);
            OptionmenuStrip.ResumeLayout(false);
            OptionmenuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button ClrEvntBtn;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown HoursBoard;
        private System.Windows.Forms.NumericUpDown MinuteBoard;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Button ShutdownBtn;
        private System.Windows.Forms.Label remTmLbl;
        private System.Windows.Forms.Label shtDwnTmLbl;
        private System.Windows.Forms.Panel timerPanel;
        private System.Windows.Forms.NotifyIcon taskTrayIcon;
        private System.Windows.Forms.ToolStripMenuItem ExitTrayMenu;
        private System.Windows.Forms.ContextMenuStrip trayMenu;
        private System.Windows.Forms.MenuStrip OptionmenuStrip;
        private System.Windows.Forms.ToolStripMenuItem optionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    }
}

