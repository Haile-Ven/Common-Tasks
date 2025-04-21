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
            defaultFormSize = this.ClientSize;
            defaultFormLocation = this.Location;
            defaultPanelSize = timerPanel.Size;
            defaultLabelSize = remTmLbl.Size;
            taskTrayIcon.Text = "Common Tasks";
            DeleteFileIfExpired();
            _ = LoadTimer();
            
            // Initialize and attach toast notification
            toastNotification = new ToastNotification();
            toastNotification.AttachToForm(this);
            
            // Initialize shutdown toast notification and add it to timerPanel
            shutdownToastNotification = new ShutdownToastNotification();
            shutdownToastNotification.Size = timerPanel.Size;
            timerPanel.Controls.Clear(); // Remove existing controls from timerPanel
            timerPanel.Controls.Add(shutdownToastNotification);
            shutdownToastNotification.Dock = DockStyle.Fill;
            shutdownToastNotification.BringToFront();
            shutdownToastNotification.Visible = false; // Hide until shutdown button is clicked
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

        private async Task LoadTimer()
        {
            try
            {
                if (!File.Exists("log"))
                {
                    return;
                }

                string fileContent = File.ReadAllText("log");
                DateTime shutdownTime = DateTime.Parse(fileContent);

                // Convert to desired display format: "dddd, dd MMMM yyyy hh:mm tt"
                string formattedShutdownTime = shutdownTime.ToString("dddd, dd MMMM yyyy hh:mm tt");

                // Clear the existing labels since we're using ShutdownToastNotification
                shtDwnTmLbl.Text = string.Empty;
                remTmLbl.Text = string.Empty;
                ShutdownBtn.Enabled = false;
                CancelBtn.Enabled = true;

                // Show the shutdown toast notification with countdown
                shutdownToastNotification.ShowShutdownCountdown(shutdownTime);

                await CalculateTime();
            }
            catch (Exception ex)
            {
                toastNotification.Show($"Error: {ex.Message}", "ERROR", false);
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
                DateTime shutdownTime = DateTime.Parse(fileContent);

                while (true)
                {
                    if (isShutdownCancelled)
                    {
                        File.Delete("log");
                        shutdownToastNotification.HideShutdownCountdown();
                        return; // Exit the method when canceled
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

                    // Note: We don't need to update the shutdown toast notification here
                    // as it has its own timer that updates the UI

                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                toastNotification.Show($"Error: {ex.Message}", "ERROR", false);
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

                // Store the shutdown time in a standardized format
                File.WriteAllText("log", shutdownTime.ToString("yyyy-MM-dd HH:mm:ss"));
                
                //Restrict access

                CancelBtn.Enabled = true;
                _ = LoadTimer();
            }

            MinuteBoard.Value = 0;
            HoursBoard.Value = 0;
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            isShutdownCancelled = true;
            File.Delete("log"); // Delete log if it exists
            CancelBtn.Enabled = false;
            ShutdownBtn.Enabled = true;
            
            // Cancel the shutdown command
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/a",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            _ = Process.Start(psi);
            toastNotification.Show("Shutdown canceled.", "CANCELED", true);
            
            // Hide the shutdown toast notification
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

                // Read the shutdown time from the log file
                string dateLine = File.ReadLines("log").FirstOrDefault();

                if (DateTime.TryParse(dateLine, out DateTime shutdownTime))
                {
                    DateTime currentTime = DateTime.Now;

                    // Only delete the log file if the shutdown time has passed
                    // This properly handles day turnovers and future scheduled shutdowns
                    if (currentTime > shutdownTime)
                    {
                        File.Delete("log");
                    }
                    else
                    {
                        // Could not parse the shutdown time
                    }
                }
                else
                {
                    // Could not parse the shutdown time
                }
            }
            catch (Exception ex)
            {
                // Error in DeleteFileIfExpired
            }
        }

        private void AdjustLabelForWordWrap(Label label, Panel panel)
        {
            // Disable AutoSize to manually control the size of the label
            label.AutoSize = false;

            // Set the maximum width of the label to the width of the panel
            label.MaximumSize = new Size(panel.Width, 0);

            // Allow the label to adjust its height based on the wrapped text
            label.Size = new Size(panel.Width, 0);

            // Measure the required height for the current text with wrapping
            Size size = TextRenderer.MeasureText(label.Text, label.Font, label.MaximumSize, TextFormatFlags.WordBreak);

            // Update the label's size to fit the wrapped text
            label.Height = size.Height;
        }

        private void AdjustFormSizeIfPanelChanges(Form form, Panel panel)
        {
            // Add padding around the form
            int formPadding = 40;

            // Calculate the new height based on the panel's bottom position
            int newHeight = panel.Bottom + formPadding;

            // Only update the height of the form, keep the width unchanged
            form.ClientSize = new Size(
                form.ClientSize.Width, // Keep the current width
                Math.Max(form.ClientSize.Height, newHeight) // Adjust height based on panel
            );
        }

        private void RestoreFormSize()
        {
            // Restore the form size to its default value, but keep the current location
            Point currentLocation = this.Location;
            this.ClientSize = defaultFormSize;
            this.Location = currentLocation; // Keep the current position
        }

        private void AdjustPanelSizeIfLabelChanges(Panel panel, Label label)
        {
            // Adjust the label for word wrap within the panel
            AdjustLabelForWordWrap(label, panel);

            // Add padding around the panel
            int panelPadding = 20;

            // Calculate the new height based on the label's bottom position
            int newHeight = label.Bottom + panelPadding;

            // Only update the height of the panel, keep the width unchanged
            panel.Size = new Size(
                panel.Width, // Keep the current width
                Math.Max(panel.Height, newHeight) // Adjust height based on label
            );
        }

        private void RestorePanelSize()
        {
            // Restore the panel size to its default value
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
                toastNotification.Show("File Error", "ERROR", false);
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
                this.WindowState = FormWindowState.Minimized;
            }
        }
    }
}