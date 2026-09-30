using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRM.UI;
using CRM.UI.Controls;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.WinForms.Forms
{
    /// <summary>
    /// Users list view with FAB for new user and card-based list.
    /// </summary>
    public class UsersView : UserControl
    {
        private readonly UserApiService _userService = new(ApiClient.Instance);
        private FlowLayoutPanel pnlCards = null!;
        private TextBox txtSearch = null!;
        private ComboBox cmbRole = null!;
        private ComboBox cmbActive = null!;
        private CheckBox chkShowArchived = null!;
        private Label lblHeader = null!;
        private FloatingActionButton fabNew = null!;

        public UsersView()
        {
            InitializeComponent();
            Load += async (s, e) => await LoadDataAsync();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Background;

            // Ã¢â€â‚¬Ã¢â€â‚¬ Filter Panel Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
            var pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Colors.Surface,
                Padding = new Padding(Spacing.Xl, Spacing.Lg, Spacing.Xl, Spacing.Lg)
            };
            pnlFilter.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawLine(pen, 0, pnlFilter.Height - 1, pnlFilter.Width, pnlFilter.Height - 1);
            };

            lblHeader = new Label
            {
                Text = "Users",
                Font = Typography.H2,
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(Spacing.Xl, 22)
            };
            pnlFilter.Controls.Add(lblHeader);

            var lblSearch = new Label
            {
                Text = "Search",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(180, 12)
            };
            pnlFilter.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Location = new Point(180, 32),
                Width = 220,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.KeyPress += async (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter) await LoadDataAsync();
            };
            pnlFilter.Controls.Add(txtSearch);

            var lblRole = new Label
            {
                Text = "Role",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(420, 12)
            };
            pnlFilter.Controls.Add(lblRole);

            cmbRole = new ComboBox
            {
                Location = new Point(420, 32),
                Width = 120,
                Font = Typography.Body,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRole.Items.AddRange(new object[] { "All", "Admin", "Manager", "Crew" });
            cmbRole.SelectedIndex = 0;
            cmbRole.SelectedIndexChanged += async (s, e) => await LoadDataAsync();
            pnlFilter.Controls.Add(cmbRole);

            var lblActive = new Label
            {
                Text = "Status",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(560, 12)
            };
            pnlFilter.Controls.Add(lblActive);

            cmbActive = new ComboBox
            {
                Location = new Point(560, 32),
                Width = 120,
                Font = Typography.Body,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbActive.Items.AddRange(new object[] { "All", "Active", "Inactive" });
            cmbActive.SelectedIndex = 0;
            cmbActive.SelectedIndexChanged += async (s, e) => await LoadDataAsync();
            pnlFilter.Controls.Add(cmbActive);

            chkShowArchived = new CheckBox
            {
                Text = "Show Archived",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                Location = new Point(700, 34),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            chkShowArchived.CheckedChanged += async (s, e) => await LoadDataAsync();
            pnlFilter.Controls.Add(chkShowArchived);

            var btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(830, 30),
                Width = 90,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += async (s, e) => await LoadDataAsync();
            pnlFilter.Controls.Add(btnSearch);

            var btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(930, 30),
                Width = 90,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.Body,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderColor = Colors.Border;
            btnRefresh.Click += async (s, e) => await LoadDataAsync();
            pnlFilter.Controls.Add(btnRefresh);

            // Ã¢â€â‚¬Ã¢â€â‚¬ Cards Panel Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
            pnlCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Colors.Background,
                Padding = new Padding(Spacing.Xl, Spacing.Lg, Spacing.Xl, 100)
            };

            // Ã¢â€â‚¬Ã¢â€â‚¬ FAB Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
            fabNew = new FloatingActionButton
            {
                NormalColor = Colors.Primary,
                HoverColor = Colors.PrimaryHover,
                PressedColor = Colors.PrimaryActive,
                GlyphColor = Color.White,
                Visible = false
            };
            fabNew.Click += (s, e) => OpenEditView("");

            Controls.Add(fabNew);   // FAB on UsersView, NOT pnlCards
            Controls.Add(pnlCards);
            Controls.Add(pnlFilter);

            fabNew.BringToFront();
            fabNew.PinToParentBottomRight();

            Load += (s, e) => ApplyPermissionGates();
        }

        private void ApplyPermissionGates()
        {
            var user = SessionManager.CurrentUser;
            bool canCreate = user?.CanAccessUsers == true;
            fabNew.Visible = canCreate;
            if (canCreate && fabNew.Parent != null)
                fabNew.BringToFront();
        }

        private async Task LoadDataAsync()
        {
            pnlCards.Controls.Clear();

            string? search = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text.Trim();
            string? role = cmbRole.SelectedIndex == 0 ? null : cmbRole.SelectedItem?.ToString();
            bool? isActive = cmbActive.SelectedIndex switch
            {
                1 => true,
                2 => false,
                _ => null
            };

            List<UserModel> users;
            try
            {
                users = await _userService.GetAllAsync(search, role, isActive, chkShowArchived.Checked);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load users: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            lblHeader.Text = $"Users ({users.Count})";

            if (users.Count == 0)
            {
                var empty = new Label
                {
                    Text = "No users found.\nClick the + button to add a user.",
                    Font = Typography.H2,
                    ForeColor = Colors.TextMuted,
                    AutoSize = false,
                    Width = 600,
                    Height = 120,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Margin = new Padding(0, 40, 0, 0)
                };
                pnlCards.Controls.Add(empty);
                return;
            }

            int cardWidth = pnlCards.ClientSize.Width
                            - pnlCards.Padding.Left - pnlCards.Padding.Right
                            - SystemInformation.VerticalScrollBarWidth - 4;
            if (cardWidth < 200) cardWidth = 700;

            foreach (var u in users)
                pnlCards.Controls.Add(BuildUserCard(u, cardWidth));
        }

        private Panel BuildUserCard(UserModel user, int width)
        {
            bool isArchived = user.IsArchived;

            var accentColor = isArchived
                ? Colors.TextMuted
                : (user.IsActive ? Colors.Success : Colors.TextMuted);
            var fillColor = isArchived ? Color.FromArgb(250, 250, 252) : Colors.Surface;
            var nameColor = isArchived ? Colors.TextMuted : Colors.TextPrimary;
            var hoverBorder = isArchived ? Colors.TextMuted : Colors.Primary;

            var card = new RoundedCard
            {
                Width = width,
                Height = 118,
                CornerRadius = 14,
                FillColor = fillColor,
                BorderColor = Colors.Border,
                HoverBorderColor = hoverBorder,
                AccentColor = accentColor,
                AccentWidth = 4,
                EnableHover = true,
                ShowShadow = true,
                Margin = new Padding(0, 0, 0, 14),
                Padding = new Padding(24, 16, 24, 16),
                Cursor = Cursors.Hand
            };

            card.Controls.Add(new Label
            {
                Text = user.FullName,
                Font = Typography.H3,
                ForeColor = nameColor,
                AutoSize = true,
                Location = new Point(24, 16),
                BackColor = Color.Transparent
            });

            card.Controls.Add(new Label
            {
                Text = $"{user.Email}  •  {user.Role}",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(24, 42),
                BackColor = Color.Transparent
            });

            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                card.Controls.Add(new Label
                {
                    Text = "📞 " + user.PhoneNumber,
                    Font = Typography.Small,
                    ForeColor = Colors.TextMuted,
                    AutoSize = true,
                    Location = new Point(24, 64),
                    BackColor = Color.Transparent
                });
            }

            card.Controls.Add(new Label
            {
                Text = "🏢 " + user.BranchSummary,
                Font = Typography.Small,
                ForeColor = user.BranchNames.Count == 0 ? Colors.TextMuted : Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(24, string.IsNullOrWhiteSpace(user.PhoneNumber) ? 64 : 86),
                BackColor = Color.Transparent
            });

            // Status badge (Archived / Active / Inactive)
            string badgeText = isArchived ? "Archived" : (user.IsActive ? "Active" : "Inactive");
            Color badgeFg = isArchived ? Colors.TextMuted : (user.IsActive ? Colors.Success : Colors.TextMuted);
            Color badgeBg = isArchived ? Color.FromArgb(241, 245, 249) : (user.IsActive ? Colors.SuccessLight : Color.FromArgb(241, 245, 249));

            var badge = new PillBadge
            {
                BadgeText = badgeText,
                FillColor = badgeBg,
                TextColor = badgeFg,
                Size = new Size(84, 24),
                CornerRadius = 12,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(card.Width - 108, 16)
            };
            card.Controls.Add(badge);

            // Role badge
            var (roleBg, roleFg) = user.Role switch
            {
                "Admin" => (Color.FromArgb(238, 242, 255), Color.FromArgb(79, 70, 229)),
                "Manager" => (Color.FromArgb(204, 251, 241), Color.FromArgb(13, 148, 136)),
                "Crew" => (Color.FromArgb(254, 243, 199), Color.FromArgb(217, 119, 6)),
                _ => (Color.FromArgb(241, 245, 249), Colors.TextSecondary)
            };
            var roleBadge = new PillBadge
            {
                BadgeText = user.Role,
                FillColor = roleBg,
                TextColor = roleFg,
                Size = new Size(84, 24),
                CornerRadius = 12,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(card.Width - 108, 46)
            };
            card.Controls.Add(roleBadge);

            EventHandler onClick = (s, e) => OpenEditView(user.Id);
            AttachClickRecursive(card, onClick);

            return card;
        }

        private static void AttachClickRecursive(Control parent, EventHandler handler)
        {
            parent.Click += handler;
            foreach (Control child in parent.Controls)
                AttachClickRecursive(child, handler);
        }

        private void OpenEditView(string userId)
        {
            var editPanel = new CRM.UI.Controls.UserEditPanel(userId);
            editPanel.Saved += (s, e) =>
            {
                editPanel.Dispose();
                _ = LoadDataAsync();
            };
            editPanel.Cancelled += (s, e) =>
            {
                editPanel.Dispose();
            };
            editPanel.Dock = DockStyle.Fill;
            Controls.Add(editPanel);
            editPanel.BringToFront();
        }
    }
}
