using System;
using System.Diagnostics;
using System.Windows.Forms;
namespace Common_Tasks
{
    public partial class MainForm : Form
    {
        // Flag to indicate whether EventClearForm is open
        private bool eventClearFormOpen = false;
        // Event to notify when EventClearForm is closed
        public event Action EventClearFormClosed;
        public MainForm()
        {
            InitializeComponent();
        }
        //Methods
        private void MinuteBoard_ValueChanged(object sender, EventArgs e)
        {
            if (MinuteBoard.Value >= 60)
            {
                MinuteBoard.Value -= 60;
                HoursBoard.Value++;
            }
        }

        private void ShutdownBtn_Click(object sender, EventArgs e)
        {
            decimal minute = MinuteBoard.Value * 60;
            decimal hours = HoursBoard.Value * 3600;
            decimal time = minute + hours;
            string msg = time <= 0 ? "Windows will shutdown immediately"
                : $"Windows will shutdown in {HoursBoard.Value} hours and {MinuteBoard.Value} minutes.";
            if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            //System.Diagnostics.Process.Start("shutdown", "/s /t " + time);
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = $"/s /t {time}",
                    CreateNoWindow = true,  // Set to true to avoid Command Prompt window
                    UseShellExecute = false // Set to false to redirect output
                };

                // Start the process
                Process.Start(psi);
            }
            MinuteBoard.Value = 0;
            HoursBoard.Value = 0;
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start("shutdown", "/a");
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/a",  // /a argument cancels a shutdown
                CreateNoWindow = true,  // Set to true to hide the Command Prompt window
                UseShellExecute = false  // Set to false to redirect input, output, and error streams
            };

            // Create and start the process
            Process process = new Process
            {
                StartInfo = psi
            };
            process.Start();
            process.WaitForExit();
            process.Dispose();
            MinuteBoard.Value = 0;
            HoursBoard.Value = 0;
        }

        private void ClrEvntBtn_Click(object sender, EventArgs e)
        {
            if (!eventClearFormOpen)
            {
                ClrEvntBtn.Enabled = false;
                EventClearForm eventClearForm = new EventClearForm();
                // Subscribe to the FormClosed event of EventClearForm
                eventClearForm.FormClosed += (s, args) =>
                {
                    ClrEvntBtn.Enabled = true;
                    eventClearFormOpen = false;
                    // Notify that EventClearForm is closed
                    EventClearFormClosed?.Invoke();
                };
                eventClearFormOpen = true;
                eventClearForm.Show();
            }
        }
        private void EventClearForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ClrEvntBtn.Enabled = true;
        }
    }

}
