using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CRM.UI.Controls
{
    /// <summary>
    /// A professional card container with rounded corners, subtle drop shadow,
    /// anti-aliased border, and optional header.
    /// </summary>
    public class DashboardCard : Panel
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
        public bool EnableHover { get; set; } = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool ShowShadow { get; set; } = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Color AccentColor { get; set; } = Color.Transparent;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int AccentHeight { get; set; } = 0;

        public DashboardCard()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            BackColor = Colors.Background;
            Padding = new Padding(20);

            MouseEnter += (s, e) => { if (EnableHover) { _hover = true; Invalidate(); } };
            MouseLeave += (s, e) => { if (EnableHover) { _hover = false; Invalidate(); } };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Clear parent background so rounded corners blend seamlessly
            Color parentBg = Parent?.BackColor ?? Colors.Background;
            using (var parentBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(parentBrush, ClientRectangle);
            }

            var cardRect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (cardRect.Width <= 0 || cardRect.Height <= 0) return;

            // Soft elevation shadow
            if (ShowShadow)
            {
                using var shadowBrush = new SolidBrush(Color.FromArgb(_hover ? 20 : 10, 0, 0, 0));
                using var shadowPath = GetRoundedPath(
                    new Rectangle(cardRect.X + 1, cardRect.Y + 2, cardRect.Width - 1, cardRect.Height - 1),
                    CornerRadius);
                g.FillPath(shadowBrush, shadowPath);
            }

            // Rounded card background
            using (var path = GetRoundedPath(cardRect, CornerRadius))
            {
                using var bgBrush = new SolidBrush(FillColor);
                g.FillPath(bgBrush, path);

                // Top accent bar if set
                if (AccentHeight > 0 && AccentColor != Color.Transparent)
                {
                    var oldClip = g.Clip;
                    g.SetClip(path);
                    using var accentBrush = new SolidBrush(AccentColor);
                    g.FillRectangle(accentBrush, cardRect.X, cardRect.Y, cardRect.Width, AccentHeight);
                    g.Clip = oldClip;
                }

                // Smooth border
                var borderCol = _hover ? HoverBorderColor : BorderColor;
                var borderWidth = _hover ? 1.5f : 1f;
                using var pen = new Pen(borderCol, borderWidth);
                g.DrawPath(pen, path);
            }
        }

        public static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
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

    /// <summary>
    /// An elevated metric KPI card with top accent bar, category caption,
    /// large metric value, trend delta pill badge, and top-right icon badge.
    /// </summary>
    public class DashboardMetricCard : Panel
    {
        private bool _hover;
        private string _caption = "";
        private string _value = "0";
        private string _deltaText = "";
        private bool? _deltaIsPositive = null;
        private string _iconText = "";
        private Color _iconBgColor = Colors.PrimaryLight;
        private Color _iconFgColor = Colors.Primary;
        private Color _accentColor = Colors.Primary;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int CornerRadius { get; set; } = 12;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string Caption
        {
            get => _caption;
            set { _caption = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string Value
        {
            get => _value;
            set { _value = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string DeltaText
        {
            get => _deltaText;
            set { _deltaText = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool? DeltaIsPositive
        {
            get => _deltaIsPositive;
            set { _deltaIsPositive = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string IconText
        {
            get => _iconText;
            set { _iconText = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Color IconBgColor
        {
            get => _iconBgColor;
            set { _iconBgColor = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Color IconFgColor
        {
            get => _iconFgColor;
            set { _iconFgColor = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        public DashboardMetricCard()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            BackColor = Colors.Background;
            Cursor = Cursors.Default;

            MouseEnter += (s, e) => { _hover = true; Invalidate(); };
            MouseLeave += (s, e) => { _hover = false; Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Fill parent background
            Color parentBg = Parent?.BackColor ?? Colors.Background;
            using (var parentBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(parentBrush, ClientRectangle);
            }

            var cardRect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (cardRect.Width <= 0 || cardRect.Height <= 0) return;

            // Subtle drop shadow
            using (var shadowBrush = new SolidBrush(Color.FromArgb(_hover ? 20 : 10, 0, 0, 0)))
            {
                using var shadowPath = DashboardCard.GetRoundedPath(
                    new Rectangle(cardRect.X + 1, cardRect.Y + 2, cardRect.Width - 1, cardRect.Height - 1),
                    CornerRadius);
                g.FillPath(shadowBrush, shadowPath);
            }

            // Card body
            using (var path = DashboardCard.GetRoundedPath(cardRect, CornerRadius))
            {
                using var bgBrush = new SolidBrush(Colors.Surface);
                g.FillPath(bgBrush, path);

                // Top accent line (3.5px)
                if (_accentColor != Color.Transparent)
                {
                    var oldClip = g.Clip;
                    g.SetClip(path);
                    using var accentBrush = new SolidBrush(_accentColor);
                    g.FillRectangle(accentBrush, cardRect.X, cardRect.Y, cardRect.Width, 3.5f);
                    g.Clip = oldClip;
                }

                // Border
                var borderCol = _hover ? _accentColor : Colors.Border;
                var borderWidth = _hover ? 1.5f : 1f;
                using var pen = new Pen(borderCol, borderWidth);
                g.DrawPath(pen, path);
            }

            // 1. Icon Badge at top right
            int padX = 18;
            int padY = 16;
            int iconSize = 34;
            var iconRect = new Rectangle(Width - padX - iconSize, padY, iconSize, iconSize);

            if (!string.IsNullOrEmpty(_iconText))
            {
                using var iconBgBrush = new SolidBrush(_iconBgColor);
                using var iconPath = DashboardCard.GetRoundedPath(iconRect, 8);
                g.FillPath(iconBgBrush, iconPath);

                using var iconFont = new Font(Typography.Family, 11.5f, FontStyle.Bold);
                TextRenderer.DrawText(g, _iconText, iconFont, iconRect, _iconFgColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            // 2. Caption (Title)
            int contentWidth = Width - (padX * 2) - iconSize - 8;
            var captionRect = new Rectangle(padX, padY + 2, contentWidth, 18);
            TextRenderer.DrawText(g, _caption.ToUpperInvariant(), Typography.SmallBold, captionRect, Colors.TextSecondary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            // 3. Metric Value
            var valueRect = new Rectangle(padX, padY + 24, Width - (padX * 2), 38);
            using (var valFont = new Font(Typography.Family, 20f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, _value, valFont, valueRect, Colors.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            // 4. Trend Delta Badge (Pill)
            if (!string.IsNullOrEmpty(_deltaText))
            {
                Color pillBg;
                Color pillFg;

                if (_deltaIsPositive == true)
                {
                    pillBg = Colors.SuccessLight;
                    pillFg = Colors.Success;
                }
                else if (_deltaIsPositive == false)
                {
                    pillBg = Colors.DangerLight;
                    pillFg = Colors.Danger;
                }
                else
                {
                    pillBg = Colors.BorderLight;
                    pillFg = Colors.TextSecondary;
                }

                int pillY = padY + 68;
                using var deltaFont = new Font(Typography.Family, 8.5f, FontStyle.Bold);
                Size textSize = TextRenderer.MeasureText(g, _deltaText, deltaFont);
                int pillWidth = textSize.Width + 14;
                int pillHeight = 22;

                var pillRect = new Rectangle(padX, pillY, pillWidth, pillHeight);
                using (var pillPath = DashboardCard.GetRoundedPath(pillRect, 6))
                {
                    using var bgBrush = new SolidBrush(pillBg);
                    g.FillPath(bgBrush, pillPath);
                }

                TextRenderer.DrawText(g, _deltaText, deltaFont, pillRect, pillFg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }

    /// <summary>
    /// A sleek mini stat pill/chip used in summary rows.
    /// </summary>
    public class DashboardMiniStat : Panel
    {
        private string _caption = "";
        private string _value = "0";
        private string _icon = "";
        private Color _accentColor = Colors.Primary;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int CornerRadius { get; set; } = 8;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string Caption
        {
            get => _caption;
            set { _caption = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string Value
        {
            get => _value;
            set { _value = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string Icon
        {
            get => _icon;
            set { _icon = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        public DashboardMiniStat()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            BackColor = Colors.Background;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color parentBg = Parent?.BackColor ?? Colors.Surface;
            using (var parentBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(parentBrush, ClientRectangle);
            }

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using (var path = DashboardCard.GetRoundedPath(rect, CornerRadius))
            {
                // Soft background tint based on accent
                Color softBg = Color.FromArgb(20, _accentColor);
                using (var bgBrush = new SolidBrush(softBg))
                {
                    g.FillPath(bgBrush, path);
                }

                // Left accent border strip
                var oldClip = g.Clip;
                g.SetClip(path);
                using var accentBrush = new SolidBrush(_accentColor);
                g.FillRectangle(accentBrush, rect.X, rect.Y, 3.5f, rect.Height);
                g.Clip = oldClip;

                // Subtle outline
                Color outline = Color.FromArgb(40, _accentColor);
                using (var pen = new Pen(outline, 1f))
                {
                    g.DrawPath(pen, path);
                }
            }

            int leftPad = 14;
            int rightPad = 12;
            int availW = Width - leftPad - rightPad;

            // Icon at right
            if (!string.IsNullOrEmpty(_icon))
            {
                int iconW = 28;
                var iconRect = new Rectangle(Width - rightPad - iconW, (Height - iconW) / 2, iconW, iconW);
                using var iconFont = new Font(Typography.Family, 11f);
                TextRenderer.DrawText(g, _icon, iconFont, iconRect, _accentColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                availW -= (iconW + 6);
            }

            // Caption
            var capRect = new Rectangle(leftPad, 8, availW, 16);
            TextRenderer.DrawText(g, _caption.ToUpperInvariant(), Typography.TinyUpper, capRect, Colors.TextSecondary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            // Value
            var valRect = new Rectangle(leftPad, 26, availW, 26);
            using (var valFont = new Font(Typography.Family, 14f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, _value, valFont, valRect, Colors.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }
    }
}
