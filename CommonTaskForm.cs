using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using SelfSampleProRAD_DB.UserControls;

namespace Common_Tasks
{
    public partial class common_taskForm : Form
    {
        private ToastNotification toastNotification;

        public common_taskForm()
        {
            InitializeComponent();

            // Initialize and attach toast notification
            toastNotification = new ToastNotification();
            toastNotification.AttachToForm(this);
        }

        private void shutdownBtn_Click(object sender, EventArgs e)
        {
            decimal minute = this.minuteBoard.Value * 60;
            decimal hours = this.hoursBoard.Value * 3600;
            decimal time = minute + hours;
            System.Diagnostics.Process.Start("shutdown", "/s /t " + time);
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("shutdown", "/a"); 
        }

        private void clrEvntBtn_Click(object sender, EventArgs e)
        {
             try{
                Process pr = new Process();
                pr.StartInfo.UseShellExecute = false;
                pr.StartInfo.FileName = "CAEL.bat";
                pr.StartInfo.RedirectStandardOutput = true;
                pr.Start();
                // Do not wait for the child process to exit before
                // reading to the end of its redirected stream.
                // p.WaitForExit();
                // Read the output stream first and then wait.
                string str = pr.StandardOutput.ReadToEnd();
               

                pr.WaitForExit();
                //label3.Text = System.Diagnostics.Process.Start("CAEL.bat").StandardOutput.ReadLine();
                //promptLbl.Text = "Event viewer clear !";
                clrEvntBtn.Enabled = false;
            }
            catch(System.ComponentModel.Win32Exception) { toastNotification.Show("Run program as an Administrator!!", "WARNING", false); };
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clrEvntBtn.Enabled = true;
            promptLbl.Text = "";
        }

        private void minuteBoard_ValueChanged(object sender, EventArgs e)
        {
            if(minuteBoard.Value>=60)
            {
                this.minuteBoard.Value -= 60;
                hoursBoard.Value++;
            }
            else if(minuteBoard.Value<0) minuteBoard.Value=0;
        }

        private void hoursBoard_ValueChanged(object sender, EventArgs e)
        {
            if (hoursBoard.Value < 0) hoursBoard.Value = 0;
        }
    }
}
