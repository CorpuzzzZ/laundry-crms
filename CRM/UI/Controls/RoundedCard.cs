using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CRM.UI.Controls
{
    /// <summary>
    /// A panel with rounded corners, optional border, optional drop shadow,
    /// and a customizable left accent bar.
    /// </summary>
    public class RoundedCard : Panel
    {
        private bool _hover;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int CornerRadius { get; set; } = 12;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Color FillColor { get; set; } = Colors.Surface;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Color BorderColor { get; set; } = Colors.Border;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Color HoverBorderColor { get; set; } = Colors.Primary;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Color AccentColor { get; set; } = Color.Transparent;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int AccentWidth { get; set; } = 0;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool EnableHover { get; set; } = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool ShowShadow { get; set; } = true;

        public RoundedCard()
        {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            BackColor = Colors.Background;
            Padding = new Padding(16);

            MouseEnter += (s, e) => { if (EnableHover) { _hover = true; Invalidate(); } };
            MouseLeave += (s, e) => { if (EnableHover) { _hover = false; Invalidate(); } };
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            using var path = GetRoundedPath(new Rectangle(0, 0, Width, Height), CornerRadius);
            var oldRegion = Region;
            Region = new Region(path);
            oldRegion?.Dispose();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var cardRect = new Rectangle(0, 0, Width - 1, Height - 1);


            // Rounded background fill — covers the shadow except for a soft edge below
            using (var path = GetRoundedPath(cardRect, CornerRadius))
            {
                using var bg = new SolidBrush(FillColor);
                g.FillPath(bg, path);
            }

            // Accent bar on the left edge, clipped to the rounded shape
            if (AccentWidth > 0 && AccentColor != Color.Transparent)
            {
                using var clipPath = GetRoundedPath(cardRect, CornerRadius);
                var oldClip = g.Clip;
                g.SetClip(clipPath);
                using (var accent = new SolidBrush(AccentColor))
                    g.FillRectangle(accent, cardRect.X, cardRect.Y, AccentWidth, cardRect.Height);
                g.Clip = oldClip;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var cardRect = new Rectangle(0, 0, Width - 1, Height - 1);


            // Border only — thin outline, does not obscure children
            using (var path = GetRoundedPath(cardRect, CornerRadius))
            {
                var borderWidth = _hover ? 1.5f : 1f;
                var border = _hover ? HoverBorderColor : BorderColor;
                using var pen = new Pen(border, borderWidth);
                g.DrawPath(pen, path);
            }
        }

        private static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;

            if (d <= 0 || d > rect.Width || d > rect.Height)
            {
                path.AddRectangle(rect);
                return path;
            }

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
