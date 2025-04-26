using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SelfSampleProRAD_DB.UserControls;

namespace Common_Tasks
{
    public partial class MainForm : Form
    {
        private bool eventClearFormOpen = false;
        public event Action EventClearFormClosed;
        private bool isShutdownCancelled = false;
        private Size defaultFormSize;
        private ToastNotification toastNotification;
        private ShutdownToastNotification shutdownToastNotification;
        private Point defaultFormLocation;
        private Size defaultPanelSize;
        private Size defaultLabelSize;
        public MainForm()
        {
            InitializeComponent();
            defaultFormSize = ClientSize;
            defaultFormLocation = Location;
            defaultPanelSize = timerPanel.Size;
            defaultLabelSize = remTmLbl.Size;
            taskTrayIcon.Text = "Common Tasks";
            
            InitializeShutdownNotification();
            DeleteFileIfExpired();
            _ = LoadTimer();
            
            try
            {
                toastNotification = new ToastNotification();
                toastNotification.AttachToForm(this);
            }
            catch (Exception ex)
            {
                // Handle the exception without using toast notification
                MessageBox.Show($"Error initializing toast notification: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExitItem_Click(object sender, EventArgs e)
        {
            taskTrayIcon.Visible = false;
            Application.Exit();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                ShowInTaskbar = false;
                taskTrayIcon.Visible = true;
            }
        }

        private void MinuteBoard_ValueChanged(object sender, EventArgs e)
        {
            if (MinuteBoard.Value >= 60)
            {
                MinuteBoard.Value -= 60;
                HoursBoard.Value++;
            }
        }

        private void InitializeShutdownNotification()
        {

            shutdownToastNotification = new ShutdownToastNotification();
            shutdownToastNotification.Size = timerPanel.Size;
            timerPanel.Controls.Clear();
            timerPanel.Controls.Add(shutdownToastNotification);
            shutdownToastNotification.Dock = DockStyle.Fill;
            shutdownToastNotification.BringToFront();
            shutdownToastNotification.Visible = false; 
        }

        private async Task LoadTimer()
        {
            try
            {
                if (!File.Exists("log"))
                {

                    return;
                }

                string fileContent = File.ReadAllText("log");
                if (string.IsNullOrEmpty(fileContent))
                {
                    File.Delete("log");
                    return;
                }

                // Parse the log file content
                string[] parts = fileContent.Split('|');
                DateTime shutdownTime;
                
                if (parts.Length > 0)
                {
                    // Get the shutdown time (first part)
                    shutdownTime = DateTime.Parse(parts[0]);
                }
                else
                {
                    // Invalid format, delete the log file
                    File.Delete("log");
                    return;
                }


                if (shutdownTime <= DateTime.Now)
                {

                    File.Delete("log");
                    return;
                }

                shtDwnTmLbl.Text = string.Empty;
                remTmLbl.Text = string.Empty;
                
                ShutdownBtn.Enabled = false;
                CancelBtn.Enabled = true;
             
                shutdownToastNotification.Visible = true;
                shutdownToastNotification.BringToFront();

                shutdownToastNotification.ShowShutdownCountdown(shutdownTime);

                await CalculateTime();
            }
            catch (Exception ex)
            {
                try
                {
                    toastNotification.Show($"Error: {ex.Message}", "ERROR", false);
                }
                catch
                {
                    // Fallback to MessageBox if toast notification fails
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task CalculateTime()
        {
            try
            {
                if (isShutdownCancelled)
                {
                    File.Delete("log");
                    shutdownToastNotification.HideShutdownCountdown();
                    return;
                }

                string fileContent = File.ReadAllText("log");
                
                // Parse the log file content
                string[] parts = fileContent.Split('|');
                DateTime shutdownTime;
                
                if (parts.Length > 0)
                {
                    // Get the shutdown time (first part)
                    shutdownTime = DateTime.Parse(parts[0]);
                }
                else
                {
                    // Invalid format, delete the log file
                    File.Delete("log");
                    return;
                }

                while (true)
                {
                    if (isShutdownCancelled)
                    {
                        File.Delete("log");
                        shutdownToastNotification.HideShutdownCountdown();
                        return; 
                    }

                    TimeSpan remainingTime = shutdownTime - DateTime.Now;

                    if (remainingTime <= TimeSpan.Zero)
                    {
                        taskTrayIcon.Text = "Shutdown time reached.";
                        File.Delete("log");
                        return;
                    }

                    int remainingDays = remainingTime.Days;
                    int remainingHours = remainingTime.Hours;
                    int remainingMinutes = remainingTime.Minutes;
                    int remainingSeconds = remainingTime.Seconds;

                    string remainingTimeString;
                    if (remainingDays > 0)
                    {
                        remainingTimeString = $"{remainingDays} day{(remainingDays > 1 ? "s" : "")}, " +
                                              $"{remainingHours} hour{(remainingHours > 1 ? "s" : "")}, " +
                                              $"{remainingMinutes} minute{(remainingMinutes > 1 ? "s" : "")}, and " +
                                              $"{remainingSeconds} second{(remainingSeconds > 1 ? "s" : "")} remaining.";
                    }
                    else
                    {
                        remainingTimeString = $"{remainingHours} hour{(remainingHours > 1 ? "s" : "")}, " +
                                              $"{remainingMinutes} minute{(remainingMinutes > 1 ? "s" : "")}, and " +
                                              $"{remainingSeconds} second{(remainingSeconds > 1 ? "s" : "")} remaining.";
                    }

                    taskTrayIcon.Text = remainingTimeString;

                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    toastNotification.Show($"Error: {ex.Message}", "ERROR", false);
                }
                catch
                {
                    // Fallback to MessageBox if toast notification fails
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShutdownBtn_Click(object sender, EventArgs e)
        {
            isShutdownCancelled = false;
            decimal minutes = MinuteBoard.Value * 60;
            decimal hours = HoursBoard.Value * 3600;
            decimal totalSeconds = minutes + hours;

            string msg = totalSeconds <= 0 ? "Windows will shutdown immediately"
                : $"Windows will shutdown in {HoursBoard.Value} hours and {MinuteBoard.Value} minutes.";

            if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                DateTime shutdownTime = DateTime.Now.AddSeconds((double)totalSeconds);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = $"/s /t {(int)totalSeconds}",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                _ = Process.Start(psi);

                // Save both the shutdown time and the original scheduling time
                string shutdownTimeStr = shutdownTime.ToString("yyyy-MM-dd HH:mm:ss");
                string currentTimeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                File.WriteAllText("log", shutdownTimeStr + "|" + currentTimeStr);
                
                CancelBtn.Enabled = true;
                _ = LoadTimer();
                toastNotification.Show("Shutdown scheduled.", "Scheduled", true);
            }

            MinuteBoard.Value = 0;
            HoursBoard.Value = 0;
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            isShutdownCancelled = true;
            File.Delete("log"); 
            CancelBtn.Enabled = false;
            ShutdownBtn.Enabled = true;
            
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/a",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            _ = Process.Start(psi);
            try
            {
                toastNotification.Show("Shutdown canceled.", "Canceled", true);
            }
            catch
            {
                // Fallback to MessageBox if toast notification fails
                MessageBox.Show("Shutdown canceled.", "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            shutdownToastNotification.HideShutdownCountdown();
            
            remTmLbl.Text = string.Empty;
            shtDwnTmLbl.Text =  string.Empty;
            taskTrayIcon.Text = "Common Tasks";
        }
        static void DeleteFileIfExpired()
        {
            try
            {
                if (!File.Exists("log"))
                {
                    return;
                }

                string dateLine = File.ReadLines("log").FirstOrDefault();

                if (string.IsNullOrEmpty(dateLine))
                {
                    File.Delete("log");
                    return;
                }

                // Parse the log file content
                string[] parts = dateLine.Split('|');
                DateTime shutdownTime;

                if (parts.Length > 0 && DateTime.TryParse(parts[0], out shutdownTime))
                {
                    DateTime currentTime = DateTime.Now;

                    if (currentTime > shutdownTime)
                    {
                        File.Delete("log");
                    }
                }
                else
                {
                    File.Delete("log");
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void RestoreFormSize()
        {
            Point currentLocation = Location;
            ClientSize = defaultFormSize;
            Location = currentLocation; 
        }

        private void RestorePanelSize()
        {

            timerPanel.Size = defaultPanelSize;
            remTmLbl.Size = defaultLabelSize;
        }

        private void ClrEvntBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (!eventClearFormOpen)
                {
                    ClrEvntBtn.Enabled = false;
                    EventClearForm eventClearForm = new EventClearForm();
                    eventClearForm.FormClosed += (s, args) =>
                    {
                        ClrEvntBtn.Enabled = true;
                        eventClearFormOpen = false;
                        EventClearFormClosed?.Invoke();
                    };
                    eventClearFormOpen = true;
                    eventClearForm.ShowDialog();
                }
            }
            catch
            {
                try
                {
                    toastNotification.Show("File Error", "ERROR", false);
                }
                catch
                {
                    // Fallback to MessageBox if toast notification fails
                    MessageBox.Show("File Error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                ClrEvntBtn.Enabled = true;
            }
        }

        private void EventClearForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ClrEvntBtn.Enabled = true;
        }

        private void taskTrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            taskTrayIcon.Visible = false;
            RestoreFormSize();
            RestorePanelSize();
        }

        private void taskTrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                trayMenu.Visible = true;
            }
            else trayMenu.Visible = false;
        }

        private void ExitTrayMenu_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
            }
        }
    }
}