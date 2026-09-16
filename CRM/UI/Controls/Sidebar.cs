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
                Height = 72,
                BackColor = Colors.SidebarBg
            };

            var logoIcon = new Label
            {
                Text = "🧺",
                Font = new Font(Typography.Family, 18f),
                ForeColor = Colors.SidebarTextActive,
                AutoSize = true,
                Location = new Point(Spacing.Xl, 18)
            };

            var logoText = new Label
            {
                Text = "LAUNDRY CRM",
                Font = Typography.H3,
                ForeColor = Colors.SidebarTextActive,
                AutoSize = true,
                Location = new Point(Spacing.Xl + 38, 24)
            };

            _logoArea.Controls.Add(logoIcon);
            _logoArea.Controls.Add(logoText);

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