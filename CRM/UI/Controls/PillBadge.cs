using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CRM.UI.Controls
{
    /// <summary>
    /// A sleek, anti-aliased pill-shaped badge for statuses, tags, and counts.
    /// </summary>
    public class PillBadge : Control
    {
        private string _text = "";
        private Color _fillColor = Color.FromArgb(239, 246, 255);
        private Color _textColor = Color.FromArgb(37, 99, 235);
        private Color _borderColor = Color.Transparent;
        private int _cornerRadius = 10;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string BadgeText
        {
            get => _text;
            set { _text = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Color FillColor
        {
            get => _fillColor;
            set { _fillColor = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Color TextColor
        {
            get => _textColor;
            set { _textColor = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = value; Invalidate(); }
        }

        public PillBadge()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            Size = new Size(90, 24);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            int w = Width - 1;
            int h = Height - 1;
            if (w <= 0 || h <= 0) return;

            int radius = _cornerRadius > 0 ? _cornerRadius : h / 2;
            var rect = new Rectangle(0, 0, w, h);

            using var path = GetRoundedPath(rect, radius);
            using var bgBrush = new SolidBrush(_fillColor);
            g.FillPath(bgBrush, path);

            if (_borderColor != Color.Transparent)
            {
                using var borderPen = new Pen(_borderColor, 1f);
                g.DrawPath(borderPen, path);
            }

            if (!string.IsNullOrEmpty(_text))
            {
                TextRenderer.DrawText(g, _text, Font, rect, _textColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
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
