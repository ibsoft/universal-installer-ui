using System.Drawing;
using System.Drawing.Drawing2D;

namespace Installer.App.Controls
{
    public class StepBadge : Label
    {
        public enum StepState { Pending, Active, Done, Error }

        private StepState _state;
        public StepState State
        {
            get => _state;
            set { _state = value; Invalidate(); }
        }

        // Themeable colors
        public Color ActiveBorderColor { get; set; } = Color.FromArgb(20, 184, 166);
        public Color ActiveFillColor { get; set; } = Color.FromArgb(20, 64, 70);
        public Color DoneBorderColor { get; set; } = Color.FromArgb(34, 197, 94);
        public Color DoneFillColor { get; set; } = Color.FromArgb(22, 101, 52);
        public Color ErrorBorderColor { get; set; } = Color.FromArgb(239, 68, 68);
        public Color ErrorFillColor { get; set; } = Color.FromArgb(127, 29, 29);
        public Color PendingBorderColor { get; set; } = Color.FromArgb(64, 64, 64);
        public Color PendingFillColor { get; set; } = Color.FromArgb(28, 28, 30);

        public StepBadge()
        {
            AutoSize = false;
            Height = 28;
            Width = 140;
            TextAlign = ContentAlignment.MiddleCenter;
            Margin = new Padding(6, 5, 6, 5);
            Padding = new Padding(8, 0, 8, 0);
            ForeColor = Color.White;
            BackColor = Color.Transparent;
            DoubleBuffered = true;
            State = StepState.Pending;
            Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = ClientRectangle;
            rect.Inflate(-1, -1);

            Color border;
            Color fill;
            Color text;

            switch (State)
            {
                case StepState.Active:
                    border = ActiveBorderColor;
                    fill = ActiveFillColor;
                    text = Color.White;
                    break;
                case StepState.Done:
                    border = DoneBorderColor;
                    fill = DoneFillColor;
                    text = Color.White;
                    break;
                case StepState.Error:
                    border = ErrorBorderColor;
                    fill = ErrorFillColor;
                    text = Color.White;
                    break;
                default:
                    border = PendingBorderColor;
                    fill = PendingFillColor;
                    text = ForeColor;
                    break;
            }

            using var path = new GraphicsPath();
            int r = 12;
            path.AddArc(rect.X, rect.Y, r, r, 180, 90);
            path.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
            path.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);
            path.CloseFigure();

            using var br = new SolidBrush(fill);
            using var pen = new Pen(border, 1.5f);
            g.FillPath(br, path);
            g.DrawPath(pen, path);

            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var tb = new SolidBrush(text);
            g.DrawString(Text, Font, tb, rect, sf);
        }
    }
}