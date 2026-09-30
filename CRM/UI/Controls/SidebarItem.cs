using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CRM.UI.Controls
{
    /// <summary>Built-in icon shapes for sidebar items.</summary>
    public enum SidebarIcon
    {
        None,
        Dashboard,
        Orders,
        Customers,
        Services,
        Loyalty,
        Branches,
        Users,
        Reports,
        Subscription,
        Terms,
        Logout
    }

    /// <summary>
    /// A single navigation item in the sidebar.
    /// Icons are drawn as vector shapes - no font/glyph dependency.
    /// </summary>
    public class SidebarItem : Panel
    {
        private bool _isActive;
        private bool _hover;
        private string _label = "";
        private string _pageKey = "";
        private SidebarIcon _icon = SidebarIcon.None;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string Label
        {
            get => _label;
            set { _label = value ?? ""; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string PageKey
        {
            get => _pageKey;
            set { _pageKey = value ?? ""; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public SidebarIcon Icon
        {
            get => _icon;
            set { _icon = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; Invalidate(); }
        }

        public SidebarItem()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            Height = 46;
            Cursor = Cursors.Hand;
            BackColor = Colors.SidebarBg;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Clear background
            using (var bgBrush = new SolidBrush(Colors.SidebarBg))
                g.FillRectangle(bgBrush, ClientRectangle);

            // Floating rounded pill for hover/active states
            if (IsActive || _hover)
            {
                var pillRect = new Rectangle(10, 3, Width - 20, Height - 6);
                using var pillPath = DashboardCard.GetRoundedPath(pillRect, 8);
                Color pillColor = IsActive ? Colors.SidebarActive : Colors.SidebarHover;
                using var pillBrush = new SolidBrush(pillColor);
                g.FillPath(pillBrush, pillPath);

                if (IsActive)
                {
                    // Subtle left indicator bar inside pill
                    using var accentBrush = new SolidBrush(Colors.SidebarAccent);
                    using var indPath = DashboardCard.GetRoundedPath(new Rectangle(12, 11, 3, Height - 22), 2);
                    g.FillPath(accentBrush, indPath);
                }
            }

            var fg = IsActive ? Colors.SidebarTextActive : (_hover ? Color.White : Colors.SidebarText);
            var font = IsActive ? Typography.BodyBold : Typography.Body;

            // Icon area (left, 24px wide)
            var iconRect = new Rectangle(Spacing.Xl, 0, 20, Height);
            DrawIcon(g, _icon, iconRect, fg);

            // Label
            var labelRect = new Rectangle(Spacing.Xl + 28, 0, Width - Spacing.Xl - 36, Height);
            TextRenderer.DrawText(g, _label, font, labelRect, fg,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private static void DrawIcon(Graphics g, SidebarIcon icon, Rectangle r, Color color)
        {
            int cx = r.X + r.Width / 2;
            int cy = r.Y + r.Height / 2;
            int s = 14;   // icon square size

            using var pen = new Pen(color, 1.6f);
            using var brush = new SolidBrush(color);

            switch (icon)
            {
                case SidebarIcon.Dashboard:
                    // Four small squares (grid)
                    int half = (s - 2) / 2;
                    g.FillRectangle(brush, cx - s / 2, cy - s / 2, half, half);
                    g.FillRectangle(brush, cx + 1, cy - s / 2, half, half);
                    g.FillRectangle(brush, cx - s / 2, cy + 1, half, half);
                    g.FillRectangle(brush, cx + 1, cy + 1, half, half);
                    break;

                case SidebarIcon.Orders:
                    // Box outline
                    var boxRect = new Rectangle(cx - s / 2, cy - s / 2 + 1, s, s - 2);
                    g.DrawRectangle(pen, boxRect);
                    g.DrawLine(pen, cx - s / 2, cy - s / 2 + 5, cx + s / 2, cy - s / 2 + 5);
                    break;

                case SidebarIcon.Customers:
                    // Two circles (heads) + line (bodies)
                    g.DrawEllipse(pen, cx - 6, cy - 6, 6, 6);
                    g.DrawEllipse(pen, cx + 1, cy - 5, 5, 5);
                    g.DrawLine(pen, cx - 7, cy + 2, cx - 1, cy + 2);
                    g.DrawLine(pen, cx + 1, cy + 3, cx + 6, cy + 3);
                    break;

                case SidebarIcon.Services:
                    // Sparkle / star
                    var pts = new[]
                    {
                        new PointF(cx, cy - 7),
                        new PointF(cx + 2, cy - 2),
                        new PointF(cx + 7, cy),
                        new PointF(cx + 2, cy + 2),
                        new PointF(cx, cy + 7),
                        new PointF(cx - 2, cy + 2),
                        new PointF(cx - 7, cy),
                        new PointF(cx - 2, cy - 2),
                    };
                    g.FillPolygon(brush, pts);
                    break;

                case SidebarIcon.Loyalty:
                    // Star
                    var star = BuildStar(cx, cy, 7, 3, 5);
                    g.FillPolygon(brush, star);
                    break;

                case SidebarIcon.Branches:
                    // House/building
                    g.DrawRectangle(pen, cx - 6, cy - 2, 12, 9);
                    g.DrawLine(pen, cx - 7, cy - 2, cx, cy - 8);
                    g.DrawLine(pen, cx + 7, cy - 2, cx, cy - 8);
                    g.FillRectangle(brush, cx - 1, cy + 2, 3, 5);
                    break;

                case SidebarIcon.Users:
                    // Single person outline
                    g.DrawEllipse(pen, cx - 3, cy - 7, 6, 6);
                    g.DrawArc(pen, cx - 6, cy - 1, 12, 10, 180, 180);
                    break;

                case SidebarIcon.Reports:
                    // Bar chart
                    g.FillRectangle(brush, cx - 6, cy + 2, 2, 5);
                    g.FillRectangle(brush, cx - 2, cy - 2, 2, 9);
                    g.FillRectangle(brush, cx + 2, cy - 5, 2, 12);
                    break;

                case SidebarIcon.Subscription:
                    // Card / rectangle with stripe
                    var cardRect = new Rectangle(cx - 7, cy - 5, 14, 10);
                    g.DrawRectangle(pen, cardRect);
                    g.DrawLine(pen, cx - 7, cy - 2, cx + 7, cy - 2);
                    break;

                case SidebarIcon.Terms:
                    // Document with fold
                    g.DrawRectangle(pen, cx - 5, cy - 7, 10, 14);
                    g.DrawLine(pen, cx - 3, cy - 3, cx + 3, cy - 3);
                    g.DrawLine(pen, cx - 3, cy, cx + 3, cy);
                    g.DrawLine(pen, cx - 3, cy + 3, cx + 3, cy + 3);
                    break;

                case SidebarIcon.Logout:
                    // Door + arrow
                    g.DrawRectangle(pen, cx - 6, cy - 6, 8, 12);
                    g.DrawLine(pen, cx + 2, cy, cx + 7, cy);
                    g.DrawLine(pen, cx + 4, cy - 3, cx + 7, cy);
                    g.DrawLine(pen, cx + 4, cy + 3, cx + 7, cy);
                    break;

                case SidebarIcon.None:
                default:
                    // Small dot
                    g.FillEllipse(brush, cx - 2, cy - 2, 4, 4);
                    break;
            }
        }

        private static PointF[] BuildStar(float cx, float cy, float outerR, float innerR, int points)
        {
            var result = new PointF[points * 2];
            double angle = -Math.PI / 2;
            double step = Math.PI / points;
            for (int i = 0; i < points * 2; i++)
            {
                float r = (i % 2 == 0) ? outerR : innerR;
                result[i] = new PointF(
                    cx + (float)(r * Math.Cos(angle)),
                    cy + (float)(r * Math.Sin(angle)));
                angle += step;
            }
            return result;
        }
    }
}
