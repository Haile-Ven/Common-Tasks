using Microsoft.Win32;
using SelfSampleProRAD_DB.UserControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

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

            UpdateButtonStatesBasedOnDatabase();

            try
            {
                toastNotification = new ToastNotification();
                toastNotification.AttachToForm(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing toast notification: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1 && args.Contains("--delayed-start"))
            {
                Timer delayTimer = new Timer();
                delayTimer.Interval = 3000;
                delayTimer.Tick += (s, e) =>
                {
                    delayTimer.Stop();
                };
                delayTimer.Start();
            }
            else if (args.Length > 1 && args.Contains("--clear-events"))
            {
                Timer startupTimer = new Timer();
                startupTimer.Interval = 500;
                startupTimer.Tick += (s, e) =>
                {
                    startupTimer.Stop();
                    OpenEventClearForm();
                };
                startupTimer.Start();
            }
            else if (args.Length > 1 && args.Contains("--view-network"))
            {
                Timer startupTimer = new Timer();
                startupTimer.Interval = 500;
                startupTimer.Tick += (s, e) =>
                {
                    startupTimer.Stop();
                    ShowViewNetworkMenu();
                };
                startupTimer.Start();
            }
            else if (args.Length > 1 && args.Contains("--clear-network"))
            {
                Timer startupTimer = new Timer();
                startupTimer.Interval = 500;
                startupTimer.Tick += async (s, e) =>
                {
                    startupTimer.Stop();
                    ClearNetworkList();
                    await Task.Delay(2000);
                    if (IsRunningAsAdmin())
                    {
                        RestartWithNormalPrivileges();
                    }
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

            shutdownToastNotification = new ShutdownToastNotification
            {
                Size = timerPanel.Size
            };
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
                var schedule = DatabaseManager.GetActiveShutdownSchedule();
                if (schedule == null)
                {
                    return;
                }

                DateTime startTime = schedule.Item1;
                DateTime shutdownTime = schedule.Item2;

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

                var schedule = DatabaseManager.GetActiveShutdownSchedule();
                if (schedule == null)
                {
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

                DatabaseManager.SaveShutdownSchedule(DateTime.Now, shutdownTime);

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
                ShutdownBtn.BackColor = System.Drawing.Color.FromArgb(0, 99, 177);
                ShutdownBtn.ForeColor = System.Drawing.Color.White;
                ShutdownBtn.FlatAppearance.BorderColor = ShutdownBtn.BackColor;
            }
            else
            {
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
                CancelBtn.BackColor = System.Drawing.Color.FromArgb(0, 99, 177);
                CancelBtn.ForeColor = System.Drawing.Color.White;
                CancelBtn.FlatAppearance.BorderColor = CancelBtn.BackColor;
            }
            else
            {
                CancelBtn.BackColor = System.Drawing.Color.FromArgb(233, 233, 233);
                CancelBtn.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
                CancelBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(233, 233, 233);
            }
        }

        private void UpdateButtonStatesBasedOnDatabase()
        {
            var activeSchedule = DatabaseManager.GetActiveShutdownSchedule();

            if (activeSchedule == null)
            {
                UpdateShutdownButtonState(true);
                UpdateCancelButtonState(false);
            }
            else
            {
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
                    if (!IsRunningAsAdmin())
                    {
                        RestartAsAdmin("--clear-events");
                        return;
                    }

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
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    UseShellExecute = true,
                    Verb = "runas",
                    Arguments = arguments
                };

                Process.Start(processInfo);

                if (shutdownToastNotification != null)
                {
                    shutdownToastNotification.Dispose();
                }

                if (toastNotification != null)
                {
                    toastNotification.Dispose();
                }

                this.Hide();
                taskTrayIcon.Visible = false;

                GC.Collect();
                GC.WaitForPendingFinalizers();

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
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
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    UseShellExecute = false,
                    Arguments = "--delayed-start"
                };

                Process.Start(processInfo);

                if (shutdownToastNotification != null)
                {
                    shutdownToastNotification.Dispose();
                }

                if (toastNotification != null)
                {
                    toastNotification.Dispose();
                }

                this.Hide();
                taskTrayIcon.Visible = false;

                GC.Collect();
                GC.WaitForPendingFinalizers();

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

        protected override void WndProc(ref Message m)
        {
            if (MessageReceived != null)
            {
                MessageReceived.Invoke(this, m);
            }

            base.WndProc(ref m);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void clearNetworkListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!IsRunningAsAdmin())
            {
                RestartAsAdmin("--clear-network");
                return;
            }
            ClearNetworkList();
        }

        private void ClearNetworkList()
        {
            using (RegistryKey profilesKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkList\Profiles", true))
            {
                if (profilesKey != null)
                {
                    string[] subKeyNames = profilesKey.GetSubKeyNames();
                    foreach (string subKeyName in subKeyNames)
                    {
                        try
                        {
                            profilesKey.DeleteSubKeyTree(subKeyName, false);
                        }
                        catch (Exception ex)
                        {
                            toastNotification.Show($"Failed to delete {subKeyName}: {ex.Message}", "WARNING", false);
                        }
                    }
                    toastNotification.Show($"Network list cleared. Deleted {subKeyNames.Length} profiles.", "SUCCESS", true);
                }
                else
                {
                    toastNotification.Show("Network profiles registry key not found.", "WARNING", false);
                }
            }
        }

        private List<(string ProfileName, string Guid)> GetAllNetworkList()
        {
            var networks = new List<(string ProfileName, string Guid)>();

            using (RegistryKey profilesKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkList\Profiles", false))
            {
                if (profilesKey != null)
                {
                    string[] subKeyNames = profilesKey.GetSubKeyNames();
                    foreach (string subKeyName in subKeyNames)
                    {
                        try
                        {
                            using (RegistryKey profileKey = profilesKey.OpenSubKey(subKeyName, false))
                            {
                                if (profileKey != null)
                                {
                                    string profileName = profileKey.GetValue("ProfileName")?.ToString() ?? "Unknown Network";
                                    networks.Add((profileName, subKeyName));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            toastNotification.Show($"Failed to read profile {subKeyName}: {ex.Message}", "WARNING", false);
                        }
                    }
                }
            }

            return networks;
        }

        private void ShowViewNetworkMenu()
        {
            var networks = GetAllNetworkList();

            clearNetworkListToolStripMenuItem.DropDownItems.Clear();
            if (networks.Count == 0)
            {
                ToolStripMenuItem emptyItem = new ToolStripMenuItem("No networks found");
                emptyItem.Enabled = false;
                clearNetworkListToolStripMenuItem.DropDownItems.Add(emptyItem);
            }
            else
            {
                foreach (var network in networks)
                {
                    ToolStripMenuItem networkItem = new ToolStripMenuItem(network.ProfileName);
                    networkItem.ToolTipText = $"GUID: {network.Guid}";
                    clearNetworkListToolStripMenuItem.DropDownItems.Add(networkItem);
                }
            }

            clearNetworkListToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            ToolStripMenuItem returnItem = new ToolStripMenuItem("Return to Normal Mode");
            returnItem.Click += (s, e) =>
            {
                RestartWithNormalPrivileges();
            };
            clearNetworkListToolStripMenuItem.DropDownItems.Add(returnItem);

            this.BeginInvoke(new Action(() =>
            {
                clearNetworkListToolStripMenuItem.ShowDropDown();
            }));
        }

        private void clearNetworkListToolStripMenuItem_MouseHover(object sender, EventArgs e)
        {
            if (!IsRunningAsAdmin())
            {
                RestartAsAdmin("--view-network");
                return;
            }
            ShowViewNetworkMenu();
        }

        private void resetPowershellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\PowerShell\PSReadLine\ConsoleHost_history.txt");
            if (File.Exists(path))
            {
                List<string> wrCon = new List<string> { "winget upgrade\nwinget upgrade --all\nwinget upgrade --all --include-unknown\n" };
                File.WriteAllLines(path, wrCon);
                toastNotification.Show("PowerShell command history reset.", "SUCCESS", true);
            }
        }

        private void resetPowershellToolStripMenuItem_MouseHover(object sender, EventArgs e)
        {
            resetPowershellToolStripMenuItem.DropDownItems.Clear();
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\PowerShell\PSReadLine\ConsoleHost_history.txt");
            if (File.Exists(path))
            {
                var res = File.ReadAllLines(path);
                foreach (var line in res)
                {
                    resetPowershellToolStripMenuItem.DropDownItems.Add(line);
                }
            }
        }
    }
}