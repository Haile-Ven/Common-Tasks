using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common_Tasks
{
    public partial class MainForm : Form
    {
        private bool eventClearFormOpen = false;
        public event Action EventClearFormClosed;
        private bool isShutdownCancelled = false;
        public MainForm()
        {
            InitializeComponent();
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
                string[] fileContent = File.ReadAllText("log").Split(',');
                string shutDate = fileContent[1];
                var hr = Convert.ToInt32(fileContent[0].Split(':')[0]);
                while (hr >= 24) hr -= 24;
                var nwTime = hr + ":" + fileContent[0].Split(':')[1];
                var temp = Convert.ToDateTime(nwTime).ToShortTimeString();
                shtDwnTmLbl.Text = $"Windows will shutdown on {shutDate} {temp}";
                ShutdownBtn.Enabled = false;
                CancelBtn.Enabled = true;
                await CalculateTime();
            }
            else CancelBtn.Enabled = false;
        }

        private async Task CalculateTime()
        {
            if (isShutdownCancelled) 
            {
                File.Delete("log");
                return; 
            }

            string[] logTimeParts = File.ReadAllText("log").Split(',')[0].Split(':');
            int logHour = int.Parse(logTimeParts[0]);
            int logMinute = int.Parse(logTimeParts[1]);
            var remainingHours = logHour - DateTime.Now.Hour;
            var remainingMinutes = logMinute - DateTime.Now.Minute;
            if (remainingMinutes < 0) { remainingMinutes = 60 + remainingMinutes; remainingHours--; }
            if (remainingHours <= 0 && remainingMinutes == 2)
            {
                File.Delete("log");
                var remainingSeconds = 119;
                while (remainingSeconds != 0)
                {
                    if (isShutdownCancelled)
                    {
                        taskTrayIcon.Text = string.Empty;
                        return; 
                    }

                    if (remainingSeconds == 1)
                    {
                        remTmLbl.Text = "Shutdown time reached.";
                        return;
                    }
                    else
                    {
                        remTmLbl.Text = $"{remainingSeconds--} seconds remaining.";
                        taskTrayIcon.Text = remTmLbl.Text;
                        await Task.Delay(1000);
                    }
                }
            }
            string remainingTimeString = $"{remainingHours} hours and {remainingMinutes} minutes remaining.";
            remTmLbl.Text = remainingTimeString;
            taskTrayIcon.Text = remTmLbl.Text;
            await Task.Delay(1000); 
            await CalculateTime(); 
        }

        private void ShutdownBtn_Click(object sender, EventArgs e)
        {
            isShutdownCancelled = false; 

            decimal minute = MinuteBoard.Value * 60;
            decimal hours = HoursBoard.Value * 3600;
            decimal time = minute + hours;
            decimal tempTime = time / 3600;
            string msg = time <= 0 ? "Windows will shutdown immediately"
                : $"Windows will shutdown in {HoursBoard.Value} hours and {MinuteBoard.Value} minutes.";
            if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                DateTime start = DateTime.Now;
                decimal shutHr;
                decimal shutMn;
                DayOfWeek shutDate;
                int day = 0;
                shutHr = start.Hour + HoursBoard.Value;
                shutMn = start.Minute + MinuteBoard.Value;
                if (shutMn > 60)
                {
                    shutMn -= 60;
                    shutHr++;
                }
                var tmpHr = shutHr;
                while (tmpHr >= 24) { day++; tmpHr -= 24; }
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = $"/s /t {time}",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                _ = Process.Start(psi);
                shutDate = DateTime.Now.DayOfWeek + day;
                while (shutDate > DayOfWeek.Saturday)
                {
                    shutDate -= 6;
                }
                File.WriteAllText("log", shutHr + ":" + shutMn + "," + shutDate.ToString());
                CancelBtn.Enabled = true;
                _ = LoadTimer();
            }
            MinuteBoard.Value = 0;
            HoursBoard.Value = 0;
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            isShutdownCancelled = true;
            taskTrayIcon.Text = "Common Tasks";
            remTmLbl.Text = string.Empty;
            MinuteBoard.Value = 0;
            HoursBoard.Value = 0;
            File.Delete("log");
            remTmLbl.Text = String.Empty;
            shtDwnTmLbl.Text = String.Empty;
            ShutdownBtn.Enabled = true;
            CancelBtn.Enabled = false;
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/a",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            Process process = new Process
            {
                StartInfo = psi
            };
            process.Start();
            process.WaitForExit();
            process.Dispose();
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