using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CRM.UI.Controls
{
    /// <summary>
    /// A single customer rendered as a rounded card.
    /// Hover shows a blue border + drop shadow. Click opens the edit view.
    /// </summary>
    public class CustomerCard : Panel
    {
        private bool _hover;

        // Data properties
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int CustomerId { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string CustomerName { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string Email { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string Phone { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string CustomerType { get; set; } = "Individual";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string CustomerCode { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool IsActive { get; set; } = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool IsArchived { get; set; } = false;

        // Layout constants
        private const int CornerRadius = 8;
        private const int CardPadding = 16;
        private const int AvatarSize = 44;

        public CustomerCard()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            Height = 96;
            BackColor = Colors.Background;
            Cursor = Cursors.Hand;
            Margin = new Padding(0, 0, 0, 12);
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

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Deliberately empty - OnPaint handles the full render
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Fill parent color so rounded corners blend
            Color parentBg = Parent?.BackColor ?? Colors.Background;
            g.Clear(parentBg);

            var cardRect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Hover shadow
            if (_hover)
            {
                using var shadow = new SolidBrush(Color.FromArgb(18, 0, 0, 0));
                using var shadowPath = GetRoundedPath(
                    new Rectangle(cardRect.X + 1, cardRect.Y + 3, cardRect.Width, cardRect.Height),
                    CornerRadius);
                g.FillPath(shadow, shadowPath);
            }

            // Card background
            using (var path = GetRoundedPath(cardRect, CornerRadius))
            {
                using var bg = new SolidBrush(IsArchived ? Color.FromArgb(250, 250, 252) : Colors.Surface);
                g.FillPath(bg, path);

                using var border = new Pen(
                    _hover ? (IsArchived ? Colors.TextMuted : Colors.Primary) : Colors.Border,
                    _hover ? 1.5f : 1f);
                g.DrawPath(border, path);
            }

            // Avatar circle with initials
            var avatarRect = new Rectangle(CardPadding, (Height - AvatarSize) / 2, AvatarSize, AvatarSize);
            using (var avatarBg = new SolidBrush(IsArchived ? Color.FromArgb(0xEC, 0xF0, 0xF1) : Colors.PrimaryLight))
                g.FillEllipse(avatarBg, avatarRect);

            string initials = GetInitials(CustomerName);
            using (var initialsFont = new Font(Typography.Family, 13f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, initials, initialsFont, avatarRect,
                    IsArchived ? Colors.TextMuted : Colors.Primary,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            // Text block
            int textLeft = avatarRect.Right + 16;
            int textTop = CardPadding;
            int textWidth = Width - textLeft - 140;

            // Name
            using (var nameFont = new Font(Typography.Family, 11.5f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, CustomerName, nameFont,
                    new Rectangle(textLeft, textTop, textWidth, 22),
                    IsArchived ? Colors.TextMuted : Colors.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            // Email
            if (!string.IsNullOrEmpty(Email))
            {
                TextRenderer.DrawText(g, Email, Typography.Small,
                    new Rectangle(textLeft, textTop + 24, textWidth, 18),
                    Colors.TextSecondary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            // Phone / Type / Code
            var footer = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrEmpty(Phone)) footer.Add(Phone);
            if (!string.IsNullOrEmpty(CustomerType)) footer.Add(CustomerType);
            if (!string.IsNullOrEmpty(CustomerCode)) footer.Add(CustomerCode);

            if (footer.Count > 0)
            {
                TextRenderer.DrawText(g, string.Join("  ·  ", footer), Typography.Small,
                    new Rectangle(textLeft, textTop + 46, textWidth, 18),
                    Colors.TextMuted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            // Status badge
            DrawStatusBadge(g);
        }

        private void DrawStatusBadge(Graphics g)
        {
            string text;
            Color fg;
            Color bg;

            if (IsArchived)
            {
                text = "Archived";
                fg = Colors.TextMuted;
                bg = Color.FromArgb(0xEC, 0xF0, 0xF1);
            }
            else if (IsActive)
            {
                text = "Active";
                fg = Colors.Success;
                bg = Colors.SuccessLight;
            }
            else
            {
                text = "Inactive";
                fg = Colors.TextMuted;
                bg = Color.FromArgb(0xEC, 0xF0, 0xF1);
            }

            using var badgeFont = new Font(Typography.Family, 8.5f, FontStyle.Bold);
            var textSize = TextRenderer.MeasureText(text, badgeFont);
            int badgeW = textSize.Width + 20;
            int badgeH = 22;
            int badgeX = Width - badgeW - CardPadding - 4;
            int badgeY = CardPadding + 2;

            var badgeRect = new Rectangle(badgeX, badgeY, badgeW, badgeH);
            using (var path = GetRoundedPath(badgeRect, badgeH / 2))
            {
                using var bgBrush = new SolidBrush(bg);
                g.FillPath(bgBrush, path);
            }

            TextRenderer.DrawText(g, text, badgeFont, badgeRect, fg,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpperInvariant();
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