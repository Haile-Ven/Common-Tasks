using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;

namespace Common_Tasks
{

    public partial class ShutdownToastNotification : UserControl
    {
        private Timer _updateTimer;
        private DateTime _shutdownTime;
        private TimeSpan _totalDuration;
        private float _progressPercentage = 1.0f;
        private bool _isVisible = false;
        private Form _parentForm;

        // Colors
        private Color _progressColor = Color.FromArgb(76, 175, 80); // Green
        private Color _backgroundColor = Color.FromArgb(240, 240, 240); // Light gray
        private Color _textColor = Color.FromArgb(60, 60, 60); // Dark gray
        private Color _titleColor = Color.FromArgb(76, 175, 80); // Green

        // UI elements
        private Label _titleLabel;
        private Label _timeRemainingLabel;
        private Label _shutdownTimeLabel;

        // Progress indicator properties
        private int _progressSize = 50;
        private Point _progressLocation = new Point(10, 25);

        public ShutdownToastNotification()
        {
            InitializeComponent();
            BackColor = Color.Transparent;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                          ControlStyles.AllPaintingInWmPaint | 
                          ControlStyles.UserPaint | 
                          ControlStyles.SupportsTransparentBackColor, true);
            UpdateStyles();

            // Create update timer with higher frequency for smoother animation
            _updateTimer = new Timer
            {
                Interval = 1000
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            
            // Check if there's a scheduled shutdown on load
            CheckForScheduledShutdown();
        }

        private void InitializeComponent()
        {
            _titleLabel = new Label();
            _timeRemainingLabel = new Label();
            _shutdownTimeLabel = new Label();
            SuspendLayout();
            // 
            // _titleLabel
            // 
            _titleLabel.AutoSize = true;
            _titleLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            _titleLabel.ForeColor = _titleColor;
            _titleLabel.Location = new Point(70, 5);
            _titleLabel.Name = "_titleLabel";
            _titleLabel.Size = new Size(58, 32);
            _titleLabel.TabIndex = 0;
            _titleLabel.Text = "Shutdown Scheduled";
            // 
            // _timeRemainingLabel
            // 
            _timeRemainingLabel.AutoSize = true;
            _timeRemainingLabel.Font = new Font("Segoe UI", 8.25F);
            _timeRemainingLabel.ForeColor = _textColor;
            _timeRemainingLabel.Location = new Point(70, 25);
            _timeRemainingLabel.Name = "_timeRemainingLabel";
            _timeRemainingLabel.Size = new Size(100, 23);
            _timeRemainingLabel.TabIndex = 1;
            _timeRemainingLabel.Text = "Time remaining: ";
            // 
            // _shutdownTimeLabel
            // 
            _shutdownTimeLabel.AutoSize = true;
            _shutdownTimeLabel.Font = new Font("Segoe UI", 8.25F);
            _shutdownTimeLabel.ForeColor = _textColor;
            _shutdownTimeLabel.Location = new Point(70, 65);
            _shutdownTimeLabel.Name = "_shutdownTimeLabel";
            _shutdownTimeLabel.Size = new Size(100, 23);
            _shutdownTimeLabel.TabIndex = 2;
            _shutdownTimeLabel.Text = "Shutdown at: ";
            // 
            // ShutdownToastNotification
            // 
            BackColor = Color.Transparent;
            Controls.Add(_titleLabel);
            Controls.Add(_timeRemainingLabel);
            Controls.Add(_shutdownTimeLabel);
            Name = "ShutdownToastNotification";
            Size = new Size(315, 97);
            Paint += ShutdownToastNotification_Paint;
            Resize += ShutdownToastNotification_Resize;
            ResumeLayout(false);
        }

        public void AttachToForm(Form parentForm)
        {
            _parentForm = parentForm;

            // Add control to form
            if (!_parentForm.Controls.Contains(this))
            {
                _parentForm.Controls.Add(this);
            }

            // Handle form resize to reposition the notification
            _parentForm.Resize += (s, e) =>
            {
                if (Visible)
                {
                    PositionNotification();
                }
            };
        }

        private void PositionNotification()
        {
            if (_parentForm != null)
            {
                // Position in the bottom right corner with some padding
                Location = new Point(
                    _parentForm.ClientSize.Width - Width - 20, // 20px from right
                    _parentForm.ClientSize.Height - Height - 20); // 20px from bottom
            }
        }


        public void ShowShutdownCountdown(DateTime shutdownTime)
        {
            _shutdownTime = shutdownTime;
            DateTime now = DateTime.Now;
            DateTime scheduledTime;
            
            // Try to get the original scheduled time from the database
            try
            {
                var schedule = DatabaseManager.GetActiveShutdownSchedule();
                if (schedule != null)
                {
                    // Get the start time from the database
                    scheduledTime = schedule.Item1;
                }
                else
                {
                    // Fallback: just use current time
                    scheduledTime = now;
                }
            }
            catch
            {
                // If there's any error, just use current time
                scheduledTime = now;
            }
            
            // Calculate the total duration from scheduled time until shutdown
            TimeSpan totalDuration = _shutdownTime - scheduledTime;
            
            // Store this as our reference for the progress indicator
            _totalDuration = totalDuration;
            
            // Calculate initial progress percentage
            TimeSpan elapsedTime = now - scheduledTime;
            _progressPercentage = 1.0f - (float)(elapsedTime.TotalSeconds / totalDuration.TotalSeconds);
            
            // Ensure percentage is within valid range
            if (_progressPercentage > 1.0f)
                _progressPercentage = 1.0f;
            if (_progressPercentage < 0.0f)
                _progressPercentage = 0.0f;

            // Update the UI
            _shutdownTimeLabel.Text = $"Shutdown at: {_shutdownTime.ToString("dddd, dd MMMM yyyy hh:mm tt")}";
            _shutdownTimeLabel.MaximumSize = new Size(this.Width - 80, 0);
            _shutdownTimeLabel.AutoSize = true;

            // Make sure the control is visible
            this.Visible = true;
            this.BringToFront();
            _isVisible = true;

            // Start the timer if it's not already running
            if (!_updateTimer.Enabled)
            {
                _updateTimer.Start();
            }

            // Force a complete redraw
            this.Invalidate();
        }


        public void HideShutdownCountdown()
        {
            Visible = false;
            _isVisible = false;
            if (_updateTimer.Enabled)
            {
                _updateTimer.Stop();

            }
        }
        

        private void CheckForScheduledShutdown()
        {
            try
            {

                if (!File.Exists(AppConfig.LogFilePath))
                {
                    return;
                }
                
                // Get the active shutdown schedule from the database
                var schedule = DatabaseManager.GetActiveShutdownSchedule();
                if (schedule == null)
                {
                    return;
                }
                
                DateTime shutdownTime = schedule.Item2;
                
                if (shutdownTime > DateTime.Now)
                {
                    Visible = true;
                    BringToFront();
                    ShowShutdownCountdown(shutdownTime);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {

            if (!_isVisible || !Visible)
            {

                return;
            }

            // Calculate remaining time
            TimeSpan remainingTime = _shutdownTime - DateTime.Now;



            if (remainingTime <= TimeSpan.Zero)
            {
                _timeRemainingLabel.Text = "Shutdown time reached.";
                _progressPercentage = 0.0f;
                _updateTimer.Stop();

                Invalidate(); // Force complete redraw
                return;
            }


            // Try to get the original scheduled time from the log file
            try
            {
                string logContent = File.ReadAllText(AppConfig.LogFilePath);
                string[] parts = logContent.Split('|');
                
                if (parts.Length > 1)
                {
                    // New format: shutdownTime|scheduledTime
                    DateTime scheduledTime = DateTime.Parse(parts[1]);
                    TimeSpan totalDuration = _shutdownTime - scheduledTime;
                    TimeSpan elapsedTime = DateTime.Now - scheduledTime;
                    
                    // Calculate progress percentage (1.0 - elapsed/total)
                    _progressPercentage = 1.0f - (float)(elapsedTime.TotalSeconds / totalDuration.TotalSeconds);
                }
                else
                {
                    // Old format or fallback: use remaining time / total duration
                    _progressPercentage = (float)(remainingTime.TotalSeconds / _totalDuration.TotalSeconds);
                }
            }
            catch
            {
                // If there's any error, use remaining time / total duration
                _progressPercentage = (float)(remainingTime.TotalSeconds / _totalDuration.TotalSeconds);
            }
            
            // Ensure percentage is within valid range
            if (_progressPercentage > 1.0f)
                _progressPercentage = 1.0f;
            if (_progressPercentage < 0.0f)
                _progressPercentage = 0.0f;
            



            int tickCounter = 0;
            tickCounter = (tickCounter + 1) % 10;
            if (_updateTimer.Interval != 100 || tickCounter == 0)
            {

                int remainingDays = remainingTime.Days;
                int remainingHours = remainingTime.Hours;
                int remainingMinutes = remainingTime.Minutes;
                int remainingSeconds = remainingTime.Seconds;

                string remainingTimeString;
                if (remainingDays > 0)
                {
                    remainingTimeString = $"{remainingDays} day{(remainingDays > 1 ? "s" : "")}, " +
                                          $"{remainingHours} hour{(remainingHours > 1 ? "s" : "")}, " +
                                          $"{remainingMinutes} minute{(remainingMinutes > 1 ? "s" : "")}, " +
                                          $"{remainingSeconds} second{(remainingSeconds > 1 ? "s" : "")}";
                }
                else if (remainingHours > 0)
                {
                    remainingTimeString = $"{remainingHours} hour{(remainingHours > 1 ? "s" : "")}, " +
                                          $"{remainingMinutes} minute{(remainingMinutes > 1 ? "s" : "")}, " +
                                          $"{remainingSeconds} second{(remainingSeconds > 1 ? "s" : "")}";
                }
                else if (remainingMinutes > 0)
                {
                    remainingTimeString = $"{remainingMinutes} minute{(remainingMinutes > 1 ? "s" : "")}, " +
                                          $"{remainingSeconds} second{(remainingSeconds > 1 ? "s" : "")}";
                }
                else
                {
                    remainingTimeString = $"{remainingSeconds} second{(remainingSeconds > 1 ? "s" : "")}";
                }


                _timeRemainingLabel.Text = $"Time remaining: {remainingTimeString}";
                _timeRemainingLabel.MaximumSize = new Size(Width - 80, 0);
                _timeRemainingLabel.AutoSize = true;
            }


            Invalidate();
        }

        private void ShutdownToastNotification_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;


            int centerX = _progressLocation.X + (_progressSize / 2);
            int centerY = _progressLocation.Y + (_progressSize / 2);
            int radius = (_progressSize / 2) - 5;


            using (SolidBrush bgBrush = new SolidBrush(_backgroundColor))
            {
                g.FillEllipse(bgBrush, centerX - radius, centerY - radius, radius * 2, radius * 2);
            }


            if (_progressPercentage > 0)
            {

                float sweepAngle = 360 * _progressPercentage;

                int progressRadius = radius - 1;

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddArc(centerX - progressRadius, centerY - progressRadius, progressRadius * 2, progressRadius * 2, -90, sweepAngle);

                    path.AddLine(centerX + (float)(progressRadius * Math.Cos((sweepAngle - 90) * Math.PI / 180)),
                                 centerY + (float)(progressRadius * Math.Sin((sweepAngle - 90) * Math.PI / 180)),
                                 centerX, centerY);
                    path.AddLine(centerX, centerY, centerX, centerY - progressRadius);

                    using (SolidBrush progressBrush = new SolidBrush(_progressColor))
                    {
                        g.FillPath(progressBrush, path);
                    }
                }
            }
        }

        private void ShutdownToastNotification_Resize(object sender, EventArgs e)
        {

            _progressLocation = new Point(10, 25);
            _timeRemainingLabel.MaximumSize = new Size(Width - 80, 0);
            _shutdownTimeLabel.MaximumSize = new Size(Width - 80, 0);
        }


    }
}
