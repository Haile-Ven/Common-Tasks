using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SelfSampleProRAD_DB.UserControls
{

    public partial class ToastNotification : UserControl
    {
        private Form _parentForm;
        private System.Windows.Forms.Timer _notificationTimer;
        private bool _isErrorStyle = false;


        public ToastNotification()
        {
            InitializeComponent();
            Visible = false;
            
            _notificationTimer = new System.Windows.Forms.Timer
            {
                Interval = 5000
            };
            _notificationTimer.Tick += (s, e) =>
            {
                Visible = false;
                _notificationTimer.Stop();
            };
        }

        public void AttachToForm(Form parentForm)
        {
            _parentForm = parentForm;
            

            if (!_parentForm.Controls.Contains(this))
            {
                _parentForm.Controls.Add(this);
            }

            _parentForm.Resize += (s, e) => {
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
                int width = Math.Min(300, _parentForm.ClientSize.Width - 40);
                Width = width;
                
                // Position at bottom left with a small margin
                Location = new Point(
                    20,
                    _parentForm.ClientSize.Height - Height - 20);
            }
        }

        public void Show(string message, string title = "SUCCESS", bool isSuccess = true)
        {
            if (_parentForm == null)
            {
                throw new InvalidOperationException("Toast notification must be attached to a form before showing. Call AttachToForm first.");
            }


            titleLabel.Text = title;
            messageLabel.Text = message;
            messageLabel.MaximumSize = new Size(190, 0);
            messageLabel.AutoSize = true;

            _isErrorStyle = !isSuccess;
            UpdateStyle();
            PositionNotification();
            BringToFront();
            Visible = true;
            _notificationTimer.Start();
        }

        private void UpdateStyle()
        {
            if (_isErrorStyle)
            {
                titleLabel.ForeColor = Color.FromArgb(255, 99, 71);
                iconPictureBox.Image = CreateErrorImage();
            }
            else
            {
                titleLabel.ForeColor = Color.FromArgb(30, 144, 255); // Match blue color with icon
                iconPictureBox.Image = CreateInfoImage();
            }
            Invalidate();
        }


        private Image CreateInfoImage()
        {
            Bitmap bmp = new Bitmap(24, 24);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Color blueColor = Color.FromArgb(30, 144, 255); // Dodger Blue
                Color whiteColor = Color.White;
                
                // Draw filled blue circle
                using (SolidBrush brush = new SolidBrush(blueColor))
                {
                    g.FillEllipse(brush, 2, 2, 20, 20);
                }
                
                // Draw white 'i' stem
                using (Pen pen = new Pen(whiteColor, 2))
                {
                    g.DrawLine(pen, 12, 10, 12, 18);
                }
                
                // Draw white 'i' dot
                using (SolidBrush brush = new SolidBrush(whiteColor))
                {
                    g.FillEllipse(brush, 11, 6, 2, 2);
                }
            }
            return bmp;
        }


        private Image CreateErrorImage()
        {

            Bitmap bmp = new Bitmap(24, 24);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(Color.FromArgb(255, 99, 71), 3))
                {

                    g.DrawLine(pen, 6, 6, 18, 18);
                    g.DrawLine(pen, 6, 18, 18, 6);
                }
            }
            return bmp;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            Color borderColor = _isErrorStyle ? 
                Color.FromArgb(255, 99, 71) : 
                Color.FromArgb(30, 144, 255); // Match blue color with icon

            using (SolidBrush brush = new SolidBrush(borderColor))
            {
                e.Graphics.FillRectangle(brush, 0, 0, 5, Height);
            }


            using (Pen pen = new Pen(Color.FromArgb(230, 230, 230)))
            {
                e.Graphics.DrawLines(pen, new Point[] {
                    new Point(5, 0),
                    new Point(Width - 1, 0),
                    new Point(Width - 1, Height - 1),
                    new Point(5, Height - 1)
                });
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Visible = false;
            _notificationTimer.Stop();
        }
    }
}
