using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Reflection;

namespace Common_Tasks
{
    /// <summary>
    /// A custom toast notification control for displaying shutdown countdown with a circular progress indicator
    /// </summary>
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
        private int _progressSize = 40;
        private Point _progressLocation = new Point(260, 25);

        /// <summary>
        /// Creates a new shutdown toast notification control
        /// </summary>
        public ShutdownToastNotification()
        {
            InitializeComponent();
            this.BackColor = Color.Transparent;
            this.Visible = true;

            // Set double buffering to prevent flickering and enable transparency
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                          ControlStyles.AllPaintingInWmPaint | 
                          ControlStyles.UserPaint | 
                          ControlStyles.SupportsTransparentBackColor, true);
            this.UpdateStyles();

            // Create update timer with higher frequency for smoother animation
            _updateTimer = new Timer
            {
                Interval = 1000
            };
            _updateTimer.Tick += UpdateTimer_Tick;
        }

        private void InitializeComponent()
        {
            // Configure the control
            this.Size = new Size(315, 97); // Match timerPanel size
            this.BackColor = Color.Transparent;
            this.BorderStyle = BorderStyle.None;
            
            // Create title label
            _titleLabel = new Label
            {
                Text = "Shutdown Scheduled",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = _titleColor,
                AutoSize = true,
                Location = new Point(5, 5)
            };

            // Create time remaining label
            _timeRemainingLabel = new Label
            {
                Text = "Shutdown Scheduled",
                Font = new Font("Segoe UI", 8),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(5, 25)
            };

            // Create shutdown time label
            _shutdownTimeLabel = new Label
            {
                Text = "Shutdown time: ",
                Font = new Font("Segoe UI", 8),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(5, 45)
            };

            // No progress panel needed - we'll draw directly on the form

            // No close button as per user request

            // Add controls
            this.Controls.Add(_titleLabel);
            this.Controls.Add(_timeRemainingLabel);
            this.Controls.Add(_shutdownTimeLabel);

            // Set up event handlers
            this.Paint += ShutdownToastNotification_Paint;
            this.Resize += ShutdownToastNotification_Resize;
        }

        /// <summary>
        /// Attaches the notification to a parent form
        /// </summary>
        /// <param name="parentForm">The form that will display the notification</param>
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
                if (this.Visible)
                {
                    PositionNotification();
                }
            };
        }

        /// <summary>
        /// Positions the notification at the bottom right corner of the form
        /// </summary>
        private void PositionNotification()
        {
            if (_parentForm != null)
            {
                // Position in the bottom right corner with some padding
                this.Location = new Point(
                    _parentForm.ClientSize.Width - this.Width - 20, // 20px from right
                    _parentForm.ClientSize.Height - this.Height - 20); // 20px from bottom
            }
        }

        /// <summary>
        /// Shows the shutdown notification with countdown
        /// </summary>
        /// <param name="shutdownTime">The scheduled shutdown time</param>
        public void ShowShutdownCountdown(DateTime shutdownTime)
        {
            _shutdownTime = shutdownTime;
            _totalDuration = _shutdownTime - DateTime.Now;
            _progressPercentage = 1.0f;

            // Update the shutdown time label
            _shutdownTimeLabel.Text = $"Shutdown at: {_shutdownTime.ToString("dddd, dd MMMM yyyy hh:mm tt")}";
            _shutdownTimeLabel.MaximumSize = new Size(this.Width - 70, 0);
            _shutdownTimeLabel.AutoSize = true;

            // Make sure control is visible
            this.BringToFront();
            this.Visible = true;
            _isVisible = true;

            // Start update timer
            _updateTimer.Start();

            // Invalidate just the area where the progress circle is drawn
            this.Invalidate(new Rectangle(_progressLocation, new Size(_progressSize, _progressSize)));
        }

        /// <summary>
        /// Hides the shutdown notification and stops the timer
        /// </summary>
        public void HideShutdownCountdown()
        {
            this.Visible = false;
            _isVisible = false;
            _updateTimer.Stop();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (!_isVisible) return;

            TimeSpan remainingTime = _shutdownTime - DateTime.Now;

            // Check if shutdown time has been reached
            if (remainingTime <= TimeSpan.Zero)
            {
                _timeRemainingLabel.Text = "Shutdown time reached.";
                _progressPercentage = 0.0f;
                _updateTimer.Stop();
                this.Invalidate(new Rectangle(_progressLocation, new Size(_progressSize, _progressSize)));
                return;
            }

            // Calculate progress percentage with higher precision for smoother animation
            _progressPercentage = (float)(remainingTime.TotalMilliseconds / _totalDuration.TotalMilliseconds);
            if (_progressPercentage > 1.0f) _progressPercentage = 1.0f;
            if (_progressPercentage < 0.0f) _progressPercentage = 0.0f;

            // Only update the text display once per second to avoid flickering
            int tickCounter = 0;
            tickCounter = (tickCounter + 1) % 10;
            if (_updateTimer.Interval != 100 || tickCounter == 0)
            {
                // Format remaining time
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

                // Update the label
                _timeRemainingLabel.Text = $"Time remaining: {remainingTimeString}";
                _timeRemainingLabel.MaximumSize = new Size(this.Width - 70, 0);
                _timeRemainingLabel.AutoSize = true;
            }

            // Invalidate just the area where the progress circle is drawn
            this.Invalidate(new Rectangle(_progressLocation, new Size(_progressSize, _progressSize)));
        }

        private void ShutdownToastNotification_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            // Calculate center and radius for the progress indicator
            int centerX = _progressLocation.X + (_progressSize / 2);
            int centerY = _progressLocation.Y + (_progressSize / 2);
            int radius = (_progressSize / 2) - 5;

            // Draw background circle with anti-aliasing
            using (SolidBrush bgBrush = new SolidBrush(_backgroundColor))
            {
                g.FillEllipse(bgBrush, centerX - radius, centerY - radius, radius * 2, radius * 2);
            }

            // Draw progress arc with smoother edges
            if (_progressPercentage > 0)
            {
                // Calculate the sweep angle based on progress percentage
                float sweepAngle = 360 * _progressPercentage;

                // Use a slightly smaller radius for the progress to create a border effect
                int progressRadius = radius - 1;

                // Create a path for the arc with smoother edges
                using (GraphicsPath path = new GraphicsPath())
                {
                    // Add the arc to the path
                    path.AddArc(centerX - progressRadius, centerY - progressRadius, progressRadius * 2, progressRadius * 2, -90, sweepAngle);
                    // Add lines to create a pie slice
                    path.AddLine(centerX + (float)(progressRadius * Math.Cos((sweepAngle - 90) * Math.PI / 180)),
                                 centerY + (float)(progressRadius * Math.Sin((sweepAngle - 90) * Math.PI / 180)),
                                 centerX, centerY);
                    path.AddLine(centerX, centerY, centerX, centerY - progressRadius);

                    // Fill the path with anti-aliasing
                    using (SolidBrush progressBrush = new SolidBrush(_progressColor))
                    {
                        g.FillPath(progressBrush, path);
                    }
                }
            }
        }

        private void ShutdownToastNotification_Resize(object sender, EventArgs e)
        {
            // Update progress location to stay in the top right
            _progressLocation = new Point(this.Width - _progressSize - 10, 25);
        }

        // Close button functionality removed as per user request
    }
}
