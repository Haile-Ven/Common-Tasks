using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common_Tasks
{
    public partial class MainForm : Form
    {
        private bool eventClearFormOpen = false;
        public event Action EventClearFormClosed;
        private bool isShutdownCancelled = false;
        private Size defaultFormSize;
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
            if (File.Exists("log"))
            {
                // Read the shutdown time from the log file
                string fileContent = File.ReadAllText("log");
                DateTime shutdownTime = DateTime.Parse(fileContent);

                // Convert to desired display format: "dddd, dd MMMM yyyy hh:mm tt"
                string formattedShutdownTime = shutdownTime.ToString("dddd, dd MMMM yyyy hh:mm tt");

                // Display the formatted shutdown time
                shtDwnTmLbl.Text = $"Windows will shutdown on\n{formattedShutdownTime}";
                ShutdownBtn.Enabled = false;
                CancelBtn.Enabled = true;

                await CalculateTime();
            }
            else
            {
                CancelBtn.Enabled = false;
            }
        }

        private async Task CalculateTime()
        {
            try
            {
                if (isShutdownCancelled)
                {
                    File.Delete("log");
                    return;
                }

                string fileContent = File.ReadAllText("log");
                DateTime shutdownTime = DateTime.Parse(fileContent);

                while (true)
                {
                    if (isShutdownCancelled)
                    {
                        File.Delete("log");
                        return; // Exit the method when canceled
                    }

                    TimeSpan remainingTime = shutdownTime - DateTime.Now;

                    if (remainingTime <= TimeSpan.Zero)
                    {
                        remTmLbl.Text = "Shutdown time reached.";
                        taskTrayIcon.Text = remTmLbl.Text;
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

                    remTmLbl.Text = remainingTimeString;
                    taskTrayIcon.Text = remainingTimeString;

                    // Adjust the label, panel, and form sizes
                    AdjustPanelSizeIfLabelChanges(timerPanel, remTmLbl);
                    AdjustFormSizeIfPanelChanges(this, timerPanel);

                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CalculateTime: {ex.Message}");
                remTmLbl.Text = "Error calculating remaining time.";
                AdjustPanelSizeIfLabelChanges(timerPanel, remTmLbl);
                AdjustFormSizeIfPanelChanges(this, timerPanel);
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
            RestoreFormSize();
            RestorePanelSize();
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/a",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            _ = Process.Start(psi);
            MessageBox.Show("Shutdown canceled.","Canceled",MessageBoxButtons.OK,MessageBoxIcon.Information);
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
                    Console.WriteLine("File does not exist.");
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
                        Console.WriteLine("Shutdown time has passed. Deleting log file.");
                        File.Delete("log");
                    }
                    else
                    {
                        Console.WriteLine("Shutdown is still scheduled for the future.");
                    }
                }
                else
                {
                    Console.WriteLine("Could not parse the shutdown time from the log file.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteFileIfExpired: {ex.Message}");
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
            // Restore the form size to its default value
            this.ClientSize = defaultFormSize;
            this.Location = defaultFormLocation;
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
                MessageBox.Show("ERROR!", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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