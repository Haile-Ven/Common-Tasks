using System;
using System.Diagnostics;
using System.Drawing;
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
        public event EventHandler<Message> MessageReceived;
        public NotifyIcon TaskTrayIcon => taskTrayIcon;
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
            DatabaseManager.DeleteExpiredSchedules();
            _ = LoadTimer();

            // Set initial button states based on database state
            UpdateButtonStatesBasedOnDatabase();

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

            // Check for command line arguments
            string[] args = Environment.GetCommandLineArgs();

            // Handle delayed start to avoid instance conflicts
            if (args.Length > 1 && args.Contains("--delayed-start"))
            {
                // Wait longer to ensure previous instance is fully closed
                Timer delayTimer = new Timer();
                delayTimer.Interval = 3000; // 3 seconds should be enough
                delayTimer.Tick += (s, e) =>
                {
                    delayTimer.Stop();
                    // Continue with normal initialization
                };
                delayTimer.Start();
            }
            // Handle clear events command
            else if (args.Length > 1 && args.Contains("--clear-events"))
            {
                // Delay the opening of the form slightly to ensure the UI is fully loaded
                Timer startupTimer = new Timer();
                startupTimer.Interval = 500;
                startupTimer.Tick += (s, e) =>
                {
                    startupTimer.Stop();
                    OpenEventClearForm();
                };
                startupTimer.Start();
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
            // No special handling for minimize - let it behave normally
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
                // Get the active shutdown schedule from the database
                var schedule = DatabaseManager.GetActiveShutdownSchedule();
                if (schedule == null)
                {
                    return;
                }

                DateTime startTime = schedule.Item1;
                DateTime shutdownTime = schedule.Item2;

                // Check if the shutdown time has already passed
                if (shutdownTime <= DateTime.Now)
                {
                    DatabaseManager.DeleteAllActiveSchedules();
                    return;
                }

                shtDwnTmLbl.Text = string.Empty;
                remTmLbl.Text = string.Empty;

                UpdateShutdownButtonState(false);
                UpdateCancelButtonState(true);

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
                    DatabaseManager.DeleteAllActiveSchedules();
                    shutdownToastNotification.HideShutdownCountdown();
                    return;
                }

                // Get the active shutdown schedule from the database
                var schedule = DatabaseManager.GetActiveShutdownSchedule();
                if (schedule == null)
                {
                    // No active schedule found
                    return;
                }

                DateTime startTime = schedule.Item1;
                DateTime shutdownTime = schedule.Item2;

                while (true)
                {
                    if (isShutdownCancelled)
                    {
                        DatabaseManager.DeleteAllActiveSchedules();
                        shutdownToastNotification.HideShutdownCountdown();
                        return;
                    }

                    TimeSpan remainingTime = shutdownTime - DateTime.Now;

                    if (remainingTime <= TimeSpan.Zero)
                    {
                        taskTrayIcon.Text = "Shutdown time reached.";
                        DatabaseManager.DeleteAllActiveSchedules();
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

                // Save both the shutdown time and the original scheduling time to the database
                DatabaseManager.SaveShutdownSchedule(DateTime.Now, shutdownTime);

                // Update button states based on database
                UpdateButtonStatesBasedOnDatabase();
                _ = LoadTimer();
                toastNotification.Show("Shutdown scheduled.", "Scheduled", true);
            }

            MinuteBoard.Value = 0;
            HoursBoard.Value = 0;
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to cancel scheduled shutdown ?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                isShutdownCancelled = true;
                DatabaseManager.DeleteAllActiveSchedules();

                // Update button states based on database
                UpdateButtonStatesBasedOnDatabase();
                shtDwnTmLbl.Text = string.Empty;

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
                shtDwnTmLbl.Text = string.Empty;
                taskTrayIcon.Text = "Common Tasks";
            }
        }
        private void UpdateShutdownButtonState(bool enabled)
        {
            ShutdownBtn.Enabled = enabled;

            if (enabled)
            {
                // Restore original button appearance - Windows 11 accent blue
                ShutdownBtn.BackColor = System.Drawing.Color.FromArgb(0, 99, 177);
                ShutdownBtn.ForeColor = System.Drawing.Color.White;
                ShutdownBtn.FlatAppearance.BorderColor = ShutdownBtn.BackColor;
            }
            else
            {
                // Apply disabled style - Windows 11 disabled button
                ShutdownBtn.BackColor = System.Drawing.Color.FromArgb(233, 233, 233);
                ShutdownBtn.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
                ShutdownBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(233, 233, 233);
            }
        }

        private void UpdateCancelButtonState(bool enabled)
        {
            CancelBtn.Enabled = enabled;

            if (enabled)
            {
                // Restore original button appearance - Windows 11 accent blue
                CancelBtn.BackColor = System.Drawing.Color.FromArgb(0, 99, 177);
                CancelBtn.ForeColor = System.Drawing.Color.White;
                CancelBtn.FlatAppearance.BorderColor = CancelBtn.BackColor;
            }
            else
            {
                // Apply disabled style - Windows 11 disabled button
                CancelBtn.BackColor = System.Drawing.Color.FromArgb(233, 233, 233);
                CancelBtn.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
                CancelBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(233, 233, 233);
            }
        }

        /// <summary>
        /// Updates button states based on whether there are active schedules in the database
        /// </summary>
        private void UpdateButtonStatesBasedOnDatabase()
        {
            // Check if there's an active schedule in the database
            var activeSchedule = DatabaseManager.GetActiveShutdownSchedule();

            if (activeSchedule == null)
            {
                // No active schedule - enable Shutdown button, disable Cancel button
                UpdateShutdownButtonState(true);
                UpdateCancelButtonState(false);
            }
            else
            {
                // Active schedule exists - disable Shutdown button, enable Cancel button
                UpdateShutdownButtonState(false);
                UpdateCancelButtonState(true);
            }
        }

        public void RestoreFormSize()
        {
            Point currentLocation = Location;
            ClientSize = defaultFormSize;
            Location = currentLocation;
        }

        public void RestorePanelSize()
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
                    // Check if the current process has admin privileges
                    if (!IsRunningAsAdmin())
                    {
                        // Restart the application with admin privileges and pass command line argument
                        RestartAsAdmin("--clear-events");
                        return;
                    }

                    // If we're already running as admin, show the form
                    OpenEventClearForm();
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
                ClrEvntBtn.Enabled = true;
            }
        }

        public void OpenEventClearForm()
        {
            ClrEvntBtn.Enabled = false;
            EventClearForm eventClearForm = new EventClearForm();
            eventClearForm.FormClosed += EventClearForm_FormClosed;
            eventClearFormOpen = true;
            eventClearForm.ShowDialog();
        }

        private bool IsRunningAsAdmin()
        {
            // Check if the current process is running with administrative privileges
            using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
            {
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
        }

        private void RestartAsAdmin(string arguments = "")
        {
            try
            {
                // Create a new process start info
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    UseShellExecute = true,
                    Verb = "runas", // This is what triggers the UAC prompt
                    Arguments = arguments
                };

                // Start the new process
                Process.Start(processInfo);

                // Clean up resources before exiting
                if (shutdownToastNotification != null)
                {
                    shutdownToastNotification.Dispose();
                }

                if (toastNotification != null)
                {
                    toastNotification.Dispose();
                }

                // Hide the form and tray icon
                this.Hide();
                taskTrayIcon.Visible = false;

                // Force garbage collection to release resources
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Exit the application completely
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                // The user likely canceled the UAC prompt
                try
                {
                    toastNotification.Show("Administrator privileges required.", "Access Denied", false);
                }
                catch
                {
                    MessageBox.Show("Administrator privileges required to clear events.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void RestartWithNormalPrivileges()
        {
            try
            {
                // Create a new process start info without the runas verb
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    UseShellExecute = true,
                    // Add a delay to ensure the current instance has time to exit
                    Arguments = "--delayed-start"
                };

                // Start the new process
                Process.Start(processInfo);

                // Clean up resources before exiting
                if (shutdownToastNotification != null)
                {
                    shutdownToastNotification.Dispose();
                }

                if (toastNotification != null)
                {
                    toastNotification.Dispose();
                }

                // Hide the form and tray icon
                this.Hide();
                taskTrayIcon.Visible = false;

                // Force garbage collection to release resources
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Exit the application
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                try
                {
                    toastNotification.Show($"Error restarting application: {ex.Message}", "ERROR", false);
                }
                catch
                {
                    MessageBox.Show($"Error restarting application: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void EventClearForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ClrEvntBtn.Enabled = true;
            eventClearFormOpen = false;
            EventClearFormClosed?.Invoke();

            // If we're running as admin, restart in normal mode
            if (IsRunningAsAdmin())
            {
                RestartWithNormalPrivileges();
            }
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
                Hide();
                ShowInTaskbar = false;
                taskTrayIcon.Visible = true;
            }
        }

        /// <summary>
        /// Override the window procedure to handle custom messages
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            // Allow subscribers to handle the message first
            if (MessageReceived != null)
            {
                MessageReceived.Invoke(this, m);
            }

            // Pass the message to the base class
            base.WndProc(ref m);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}