using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Installer.App.Controls
{
    public class ModernProgressBar : Control
    {
        private int _value;
        private int _stripeOffset;
        private readonly System.Windows.Forms.Timer _timer;

        public ModernProgressBar()
        {
            DoubleBuffered = true;
            Size = new Size(300, 10);
            ForeColor = Color.White;

            ColorPrimary = Color.FromArgb(6, 182, 212);   // default cyan
            ColorTrack = Color.FromArgb(31, 41, 55);      // dark track
            TrackInactive = Color.FromArgb(230, 240, 255); // very light blue when value == 0

            ShowPercentage = true;
            CornerRadius = 8;
            Indeterminate = false;

            _timer = new System.Windows.Forms.Timer { Interval = 40 };
            _timer.Tick += (s, e) => { _stripeOffset = (_stripeOffset + 3) % 24; if (Indeterminate) Invalidate(); };
            _timer.Start();
        }

        [DefaultValue(false)]
        public bool Indeterminate { get; set; }

        [DefaultValue(0)]
        public int Value
        {
            get => _value;
            set { _value = Math.Max(0, Math.Min(100, value)); Invalidate(); }
        }

        [DefaultValue(typeof(Color), "6, 182, 212")]
        public Color ColorPrimary { get; set; }

        [DefaultValue(typeof(Color), "31, 41, 55")]
        public Color ColorTrack { get; set; }

        [DefaultValue(typeof(Color), "230, 240, 255")]
        public Color TrackInactive { get; set; }

        [DefaultValue(true)]
        public bool ShowPercentage { get; set; }

        [DefaultValue(8)]
        public int CornerRadius { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = ClientRectangle;
            rect.Inflate(-1, -1);

            using var path = Rounded(rect, CornerRadius);
            var trackClr = (!Indeterminate && _value <= 0) ? TrackInactive : ColorTrack;
            using (var track = new SolidBrush(trackClr))
            {
                g.FillPath(track, path);
            }

            if (Indeterminate)
            {
                using var gpInd = new GraphicsPath();
                gpInd.AddPath(Rounded(rect, CornerRadius), false);
                using var grad = new LinearGradientBrush(rect, ControlPaint.Light(ColorPrimary, .10f), ColorPrimary, LinearGradientMode.Vertical);
                g.FillPath(grad, gpInd);
            }
            else
            {
                var fillRect = rect;
                fillRect.Width = (int)(fillRect.Width * (_value / 100.0));
                if (fillRect.Width > 0)
                {
                    using var gp = new GraphicsPath();
                    gp.AddPath(Rounded(fillRect, CornerRadius), false);
                    using var grad = new LinearGradientBrush(fillRect, ControlPaint.Light(ColorPrimary, .10f), ColorPrimary, LinearGradientMode.Vertical);
                    g.FillPath(grad, gp);
                }
            }

            if (ShowPercentage && !Indeterminate)
            {
                string text = $"{_value}%";
                var size = g.MeasureString(text, Font);
                var pt = new PointF((Width - size.Width) / 2f, (Height - size.Height) / 2f);
                using var tbShadow = new SolidBrush(Color.FromArgb(120, 0, 0, 0));
                using var tb = new SolidBrush(ForeColor);
                g.DrawString(text, Font, tbShadow, new PointF(pt.X + 1, pt.Y + 1));
                g.DrawString(text, Font, tb, pt);
            }
        }

        private static GraphicsPath Rounded(Rectangle r, int radius)
        {
            int d = radius * 2;
            var p = new GraphicsPath();
            if (radius <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
