using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PharmaCare
{
    public class NotificationForm : Form
    {
        private readonly string _patientName;
        private readonly string _medicine;
        private readonly string _mealTime;
        private readonly Action<int> _snoozeCallback;

        private static readonly Color BG_DARK = Color.FromArgb(13, 27, 42);
        private static readonly Color ACCENT = Color.FromArgb(0, 229, 160);
        private static readonly Color ACCENT2 = Color.FromArgb(0, 180, 216);
        private static readonly Color ACCENT_YLW = Color.FromArgb(249, 199, 79);
        private static readonly Color TEXT_MAIN = Color.FromArgb(232, 244, 240);
        private static readonly Color TEXT_DIM = Color.FromArgb(107, 143, 168);

        public NotificationForm(string patientName, string medicine,
                                string mealTime, Action<int> snoozeCallback)
        {
            _patientName = patientName;
            _medicine = medicine;
            _mealTime = mealTime;
            _snoozeCallback = snoozeCallback;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Medicine Reminder";
            this.Size = new Size(420, 420);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = BG_DARK;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.Font = new Font("Courier New", 10f);

            this.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                    e.Cancel = true;
            };

            // Logo
            var logoPanel = new PictureBox
            {
                Size = new Size(70, 70),
                Location = new Point((420 - 70) / 2, 16),
                BackColor = BG_DARK
            };
            logoPanel.Paint += (s, e) => DrawLogo(e.Graphics, 35, 35, 24);
            this.Controls.Add(logoPanel);

            int y = 100;

            var lblTitle = new Label
            {
                Text = "Reminder for : " + _patientName,
                Font = new Font("Courier New", 14f, FontStyle.Bold),
                ForeColor = ACCENT,
                BackColor = BG_DARK,
                AutoSize = true,
                Location = new Point(10, y)
            };
            this.Controls.Add(lblTitle);
            y += 40;

            var lblMed = new Label
            {
                Text = "It's time to take: " + _medicine,
                Font = new Font("Courier New", 13f),
                ForeColor = TEXT_MAIN,
                BackColor = BG_DARK,
                AutoSize = true,
                Location = new Point(10, y)
            };
            this.Controls.Add(lblMed);
            y += 34;

            var lblMeal = new Label
            {
                Text = "(" + _mealTime + ")",
                Font = new Font("Courier New", 10f),
                ForeColor = TEXT_DIM,
                BackColor = BG_DARK,
                AutoSize = true,
                Location = new Point(10, y)
            };
            this.Controls.Add(lblMeal);
            y += 28;

            var lblTime = new Label
            {
                Text = "Time now is:  " + DateTime.Now.ToString("hh:mm tt"),
                Font = new Font("Courier New", 9f),
                ForeColor = TEXT_DIM,
                BackColor = BG_DARK,
                AutoSize = true,
                Location = new Point(10, y)
            };
            this.Controls.Add(lblTime);
            y += 36;

            var btnDone = new Button
            {
                Text = "Done",
                Font = new Font("Courier New", 11f, FontStyle.Bold),
                BackColor = ACCENT,
                ForeColor = BG_DARK,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(360, 44),
                Location = new Point(30, y),
                Cursor = Cursors.Hand
            };
            btnDone.FlatAppearance.BorderSize = 0;
            btnDone.Click += (s, e) => OnDone();
            this.Controls.Add(btnDone);
            y += 56;

            var btnSnooze = new Button
            {
                Text = "Remind me after 5 minutes",
                Font = new Font("Courier New", 11f, FontStyle.Bold),
                BackColor = ACCENT_YLW,
                ForeColor = BG_DARK,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(360, 44),
                Location = new Point(30, y),
                Cursor = Cursors.Hand
            };
            btnSnooze.FlatAppearance.BorderSize = 0;
            btnSnooze.Click += (s, e) => OnSnooze();
            this.Controls.Add(btnSnooze);

            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Return) OnDone();
                if (e.KeyCode == Keys.Escape) OnSnooze();
            };

            // Center labels on load
            this.Load += (s, e) =>
            {
                foreach (Control c in this.Controls)
                {
                    Label lbl = c as Label;
                    if (lbl != null)
                        lbl.Left = (this.ClientSize.Width - lbl.Width) / 2;
                }
            };
        }

        private void DrawLogo(Graphics g, int cx, int cy, int r)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(ACCENT, 1.5f))
                g.DrawEllipse(pen, cx - r - 6, cy - r - 6, (r + 6) * 2, (r + 6) * 2);
            using (var brush = new SolidBrush(ACCENT2))
                g.FillRectangle(brush, cx - r, cy - 8, r * 2, 16);
            using (var brush = new SolidBrush(TEXT_MAIN))
            {
                g.FillRectangle(brush, cx - 5, cy - r, 10, r * 2);
                g.FillRectangle(brush, cx - r, cy - 5, r * 2, 10);
            }
            using (var brush = new SolidBrush(ACCENT))
                g.FillEllipse(brush, cx - 5, cy - 5, 10, 10);
        }

        private void OnDone()
        {
            AlertManager.StopAlert();
            this.Dispose();
        }

        private void OnSnooze()
        {
            AlertManager.StopAlert();
            _snoozeCallback?.Invoke(300_000);
            this.Dispose();
        }
    }
}