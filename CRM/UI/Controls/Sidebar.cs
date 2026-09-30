using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CRM.UI.Controls
{
    /// <summary>
    /// The complete sidebar: logo, sections, nav items, bottom items.
    /// </summary>
    public class Sidebar : Panel
    {
        private readonly Panel _logoArea;
        private readonly Panel _navScroll;
        private readonly Panel _bottomArea;
        private readonly Label _userLabel;

        private readonly List<SidebarItem> _allItems = new();

        /// <summary>Fired when the user clicks a nav item. Arg is the PageKey.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler<string>? NavigationRequested;

        private string _userDisplayName = "User";
        private string _userRole = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string UserDisplayName
        {
            get => _userDisplayName;
            set
            {
                _userDisplayName = value;
                if (_userLabel != null)
                    _userLabel.Text = $"👤  {value}";
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string UserRole
        {
            get => _userRole;
            set => _userRole = value;
        }

        public Sidebar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            Width = 220;
            BackColor = Colors.SidebarBg;
            Dock = DockStyle.Left;

            // ── Logo area ─────────────────────────────────────
            _logoArea = new Panel
            {
                Dock = DockStyle.Top,
                Height = 76,
                BackColor = Colors.SidebarBg
            };

            _logoArea.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Subtle bottom divider
                using var divPen = new Pen(Colors.SidebarDivider, 1f);
                g.DrawLine(divPen, 0, _logoArea.Height - 1, _logoArea.Width, _logoArea.Height - 1);

                // Modern logo icon badge
                var iconRect = new Rectangle(Spacing.Xl, 16, 42, 42);
                using var path = DashboardCard.GetRoundedPath(iconRect, 10);
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    iconRect,
                    Color.FromArgb(93, 173, 226),
                    Color.FromArgb(52, 152, 219),
                    System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal);
                g.FillPath(brush, path);

                // Laundry emblem inside badge
                using var whitePen = new Pen(Color.White, 2f);
                g.DrawEllipse(whitePen, iconRect.X + 11, iconRect.Y + 11, 20, 20);
                g.DrawEllipse(whitePen, iconRect.X + 16, iconRect.Y + 16, 10, 10);
                using var dotBrush = new SolidBrush(Color.White);
                g.FillEllipse(dotBrush, iconRect.X + 13, iconRect.Y + 7, 4, 4);

                // Brand titles
                using var fontTitle = new Font(Typography.Family, 11f, FontStyle.Bold);
                using var fontSub = new Font(Typography.Family, 7.5f, FontStyle.Bold);
                using var titleBrush = new SolidBrush(Colors.SidebarTextActive);
                using var subBrush = new SolidBrush(Colors.SidebarTextMuted);

                g.DrawString("LAUNDRY CRM", fontTitle, titleBrush, Spacing.Xl + 50, 18);
                g.DrawString("ENTERPRISE CLOUD", fontSub, subBrush, Spacing.Xl + 50, 38);
            };

            // ── Bottom area ───────────────────────────────────
            _bottomArea = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                BackColor = Colors.SidebarBg
            };

            var bottomDivider = new SidebarDivider
            {
                Dock = DockStyle.Top,
                Height = 1
            };

            _userLabel = new Label
            {
                Text = $"👤  {_userDisplayName}",
                Font = Typography.Body,
                ForeColor = Colors.SidebarTextActive,
                AutoSize = false,
                Height = 46,
                Dock = DockStyle.Top,
                Padding = new Padding(Spacing.Xl, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var logoutItem = new SidebarItem
            {
                Icon = SidebarIcon.Logout,
                Label = "Logout",
                PageKey = "__logout__",
                Dock = DockStyle.Top
            };
            logoutItem.Click += OnItemClick;

            _bottomArea.Controls.Add(logoutItem);
            _bottomArea.Controls.Add(_userLabel);
            _bottomArea.Controls.Add(bottomDivider);

            // ── Nav area ──────────────────────────────────────
            _navScroll = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.SidebarBg,
                AutoScroll = true
            };

            // ── Assemble ──────────────────────────────────────
            Controls.Add(_navScroll);
            Controls.Add(_bottomArea);
            Controls.Add(_logoArea);
        }

        public void SetUser(string displayName, string role)
        {
            UserDisplayName = displayName;
            UserRole = role;
        }

        public void AddSection(string title)
        {
            var section = new SidebarSection
            {
                Text = title,
                Dock = DockStyle.Top
            };
            _navScroll.Controls.Add(section);
            section.BringToFront();
        }

        public SidebarItem AddItem(SidebarIcon icon, string label, string pageKey)
        {
            var item = new SidebarItem
            {
                Icon = icon,
                Label = label,
                PageKey = pageKey,
                Dock = DockStyle.Top
            };
            item.Click += OnItemClick;

            _navScroll.Controls.Add(item);
            item.BringToFront();

            _allItems.Add(item);
            return item;
        }

        public void AddDivider()
        {
            var d = new SidebarDivider { Dock = DockStyle.Top };
            _navScroll.Controls.Add(d);
            d.BringToFront();
        }

        public void SetActive(string pageKey)
        {
            foreach (var item in _allItems)
                item.IsActive = string.Equals(item.PageKey, pageKey, StringComparison.OrdinalIgnoreCase);
        }

        private void OnItemClick(object? sender, EventArgs e)
        {
            if (sender is not SidebarItem item) return;

            SetActive(item.PageKey);
            NavigationRequested?.Invoke(this, item.PageKey);
        }
    }
}