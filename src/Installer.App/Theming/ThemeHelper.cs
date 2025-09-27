using System.Drawing.Drawing2D;

namespace Installer.App.Theming
{
    public static class ThemeHelper
    {
        public static Color ParseHex(string hex, Color fallback)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hex)) return fallback;
                hex = hex.Trim().TrimStart('#');
                if (hex.Length == 6)
                    return Color.FromArgb(
                        Convert.ToInt32(hex.Substring(0,2),16),
                        Convert.ToInt32(hex.Substring(2,2),16),
                        Convert.ToInt32(hex.Substring(4,2),16));
                if (hex.Length == 8)
                    return Color.FromArgb(
                        Convert.ToInt32(hex.Substring(0,2),16),
                        Convert.ToInt32(hex.Substring(2,2),16),
                        Convert.ToInt32(hex.Substring(4,2),16),
                        Convert.ToInt32(hex.Substring(6,2),16));
                return fallback;
            }
            catch { return fallback; }
        }

        public static void MakeRound(Control c, int radius = 10)
        {
            c.Resize += (s,e)=>
            {
                var path = new GraphicsPath();
                int r = radius;
                var rect = new Rectangle(0, 0, c.Width, c.Height);
                path.AddArc(rect.X, rect.Y, r, r, 180, 90);
                path.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
                path.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
                path.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);
                path.CloseAllFigures();
                c.Region = new Region(path);
            };
        }
    }
}
