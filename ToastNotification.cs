using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SelfSampleProRAD_DB.UserControls
{
    /// <summary>
    /// A custom toast notification control for Windows Forms applications
    /// </summary>
    public partial class ToastNotification : UserControl
    {
        private Form _parentForm;
        private System.Windows.Forms.Timer _notificationTimer;
        private bool _isErrorStyle = false;

        /// <summary>
        /// Creates a new toast notification control
        /// </summary>
        public ToastNotification()
        {
            InitializeComponent();
            this.Visible = false;
            
            // Create timer for auto-hiding
            _notificationTimer = new System.Windows.Forms.Timer
            {
                Interval = 5000 // 5 seconds
            };
            _notificationTimer.Tick += (s, e) =>
            {
                this.Visible = false;
                _notificationTimer.Stop();
            };
        }

        /// <summary>
        /// Attaches the notification to a parent form
        /// </summary>
        /// <param name="parentForm">The form that will display the notifications</param>
        public void AttachToForm(Form parentForm)
        {
            _parentForm = parentForm;
            
            // Add control to form
            if (!_parentForm.Controls.Contains(this))
            {
                _parentForm.Controls.Add(this);
            }

            // Handle form resize to reposition the notification
            _parentForm.Resize += (s, e) => {
                if (this.Visible)
                {
                    PositionNotification();
                }
            };
        }

        /// <summary>
        /// Positions the notification at the top of the form
        /// </summary>
        private void PositionNotification()
        {
            if (_parentForm != null)
            {
                // Adjust width to be reasonable relative to the form width
                int width = Math.Min(350, _parentForm.ClientSize.Width - 40); // Max 350px or form width - 40px
                this.Width = width;
                
                // Position at the very top center of the form
                this.Location = new Point(
                    (_parentForm.ClientSize.Width - this.Width) / 2, // Center horizontally
                    0); // At the very top
            }
        }

        public void Show(string message, string title = "SUCCESS", bool isSuccess = true)
        {
            if (_parentForm == null)
            {
                throw new InvalidOperationException("Toast notification must be attached to a form before showing. Call AttachToForm first.");
            }

            // Update notification text
            titleLabel.Text = title;
            messageLabel.Text = message;
            
            // Keep the size as defined in the Designer
            messageLabel.MaximumSize = new Size(190, 0);
            messageLabel.AutoSize = true;

            // Set colors based on notification type
            _isErrorStyle = !isSuccess;
            UpdateStyle();

            // Position the notification at the top of the form
            PositionNotification();

            // Show notification
            this.BringToFront();
            this.Visible = true;

            // Start auto-hide timer
            _notificationTimer.Start();
        }

        private void UpdateStyle()
        {
            if (_isErrorStyle)
            {
                // Error style (red)
                titleLabel.ForeColor = Color.FromArgb(255, 99, 71); // Tomato red for errors
                iconPictureBox.Image = CreateErrorImage();
            }
            else
            {
                // Success style (green)
                titleLabel.ForeColor = Color.FromArgb(76, 175, 80); // Green for success
                iconPictureBox.Image = CreateCheckmarkImage();
            }
            this.Invalidate(); // Force repaint
        }

        /// <summary>
        /// Creates a checkmark image for success notifications
        /// </summary>
        private Image CreateCheckmarkImage()
        {
            // Create a bitmap for the checkmark
            Bitmap bmp = new Bitmap(24, 24);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(Color.FromArgb(76, 175, 80), 3))
                {
                    // Draw checkmark
                    g.DrawLines(pen, new Point[] {
                        new Point(5, 12),
                        new Point(10, 17),
                        new Point(19, 7)
                    });
                }
            }
            return bmp;
        }

        /// <summary>
        /// Creates an X image for error notifications
        /// </summary>
        private Image CreateErrorImage()
        {
            // Create a bitmap for the X
            Bitmap bmp = new Bitmap(24, 24);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(Color.FromArgb(255, 99, 71), 3))
                {
                    // Draw X
                    g.DrawLine(pen, 6, 6, 18, 18);
                    g.DrawLine(pen, 6, 18, 18, 6);
                }
            }
            return bmp;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Draw left border with appropriate color
            Color borderColor = _isErrorStyle ? 
                Color.FromArgb(255, 99, 71) : // Tomato red for errors
                Color.FromArgb(76, 175, 80);  // Green for success

            using (SolidBrush brush = new SolidBrush(borderColor))
            {
                e.Graphics.FillRectangle(brush, 0, 0, 5, this.Height);
            }

            // Draw subtle border around the rest
            using (Pen pen = new Pen(Color.FromArgb(230, 230, 230)))
            {
                e.Graphics.DrawLines(pen, new Point[] {
                    new Point(5, 0),
                    new Point(this.Width - 1, 0),
                    new Point(this.Width - 1, this.Height - 1),
                    new Point(5, this.Height - 1)
                });
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            _notificationTimer.Stop();
        }
    }
}
