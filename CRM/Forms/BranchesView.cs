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
    /// Branches list view with FAB for new branch and card-based list.
    /// </summary>
    public class BranchesView : UserControl
    {
        private readonly BranchApiService _branchService = new(ApiClient.Instance);
        private FlowLayoutPanel pnlCards = null!;
        private TextBox txtSearch = null!;
        private ComboBox cmbActive = null!;
        private CheckBox chkShowArchived = null!;
        private Label lblHeader = null!;
        private FloatingActionButton fabNew = null!;

        public BranchesView()
        {
            InitializeComponent();
            Load += async (s, e) => await LoadDataAsync();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Background;

            // ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ Filter Panel ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬
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
                Text = "Branches",
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
                Width = 240,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.KeyPress += async (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter) await LoadDataAsync();
            };
            pnlFilter.Controls.Add(txtSearch);

            var lblActive = new Label
            {
                Text = "Status",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(440, 12)
            };
            pnlFilter.Controls.Add(lblActive);

            cmbActive = new ComboBox
            {
                Location = new Point(440, 32),
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
                Location = new Point(580, 34),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            chkShowArchived.CheckedChanged += async (s, e) => await LoadDataAsync();
            pnlFilter.Controls.Add(chkShowArchived);

            var btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(720, 30),
                Width = 100,
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
                Location = new Point(830, 30),
                Width = 100,
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

            // ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ Cards Panel ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬
            pnlCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Colors.Background,
                Padding = new Padding(Spacing.Xl, Spacing.Lg, Spacing.Xl, 100)
            };

            // ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ FAB ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬ÃƒÂ¢Ã¢â‚¬ÂÃ¢â€šÂ¬
            fabNew = new FloatingActionButton
            {
                NormalColor = Colors.Primary,
                HoverColor = Colors.PrimaryHover,
                PressedColor = Colors.PrimaryActive,
                GlyphColor = Color.White,
                Visible = false
            };
            fabNew.Click += (s, e) => OpenEditView(0);

            Controls.Add(fabNew);   // FAB added to BranchesView, NOT pnlCards
            Controls.Add(pnlCards);
            Controls.Add(pnlFilter);

            fabNew.BringToFront();
            fabNew.PinToParentBottomRight();

            Load += (s, e) => ApplyPermissionGates();
        }

        private void ApplyPermissionGates()
        {
            var user = SessionManager.CurrentUser;
            bool canCreate = user?.IsAdmin == true;
            fabNew.Visible = canCreate;
            if (canCreate && fabNew.Parent != null)
                fabNew.BringToFront();
        }

        private async Task LoadDataAsync()
        {
            pnlCards.Controls.Clear();

            string? search = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text.Trim();
            bool? isActive = cmbActive.SelectedIndex switch
            {
                1 => true,
                2 => false,
                _ => null
            };

            List<BranchModel> branches;
            try
            {
                branches = await _branchService.GetAllAsync(search, isActive, chkShowArchived.Checked);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load branches: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Update header with count
            lblHeader.Text = $"Branches ({branches.Count})";

            if (branches.Count == 0)
            {
                var empty = new Label
                {
                    Text = "No branches found.\nClick the + button to add a branch.",
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

            foreach (var branch in branches)
            {
                var card = BuildBranchCard(branch, cardWidth);
                pnlCards.Controls.Add(card);
            }

        }

        private Panel BuildBranchCard(BranchModel branch, int width)
        {
            bool isArchived = branch.IsArchived;

            var accentColor = isArchived
                ? Colors.TextMuted
                : (branch.IsActive ? Colors.Success : Colors.TextMuted);
            var fillColor = isArchived ? Color.FromArgb(250, 250, 252) : Colors.Surface;
            var nameColor = isArchived ? Colors.TextMuted : Colors.TextPrimary;
            var hoverBorder = isArchived ? Colors.TextMuted : Colors.Primary;

            var card = new RoundedCard
            {
                Width = width,
                Height = 136,
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
                Text = branch.BranchName,
                Font = Typography.H3,
                ForeColor = nameColor,
                AutoSize = true,
                Location = new Point(24, 16),
                BackColor = Color.Transparent
            });

            var codeParts = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrWhiteSpace(branch.BranchCode)) codeParts.Add(branch.BranchCode);
            if (!string.IsNullOrWhiteSpace(branch.City)) codeParts.Add(branch.City);
            card.Controls.Add(new Label
            {
                Text = string.Join("  •  ", codeParts),
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(24, 42),
                BackColor = Color.Transparent
            });

            var addrParts = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrWhiteSpace(branch.Street)) addrParts.Add(branch.Street);
            if (!string.IsNullOrWhiteSpace(branch.Village)) addrParts.Add(branch.Village);
            if (!string.IsNullOrWhiteSpace(branch.City)) addrParts.Add(branch.City);
            if (!string.IsNullOrWhiteSpace(branch.Province)) addrParts.Add(branch.Province);
            if (addrParts.Count > 0)
            {
                card.Controls.Add(new Label
                {
                    Text = "📍 " + string.Join(", ", addrParts),
                    Font = Typography.Small,
                    ForeColor = Colors.TextMuted,
                    AutoSize = true,
                    Location = new Point(24, 64),
                    BackColor = Color.Transparent
                });
            }

            var contact = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrWhiteSpace(branch.Phone)) contact.Add("📞 " + branch.Phone);
            if (!string.IsNullOrWhiteSpace(branch.Email)) contact.Add("✉ " + branch.Email);
            if (contact.Count > 0)
            {
                card.Controls.Add(new Label
                {
                    Text = string.Join("   ", contact),
                    Font = Typography.Small,
                    ForeColor = Colors.TextMuted,
                    AutoSize = true,
                    Location = new Point(24, 86),
                    BackColor = Color.Transparent
                });
            }

            card.Controls.Add(new Label
            {
                Text = string.IsNullOrWhiteSpace(branch.ManagerName)
                    ? "Manager: Unassigned"
                    : "Manager: " + branch.ManagerName,
                Font = Typography.Small,
                ForeColor = string.IsNullOrWhiteSpace(branch.ManagerName)
                    ? Colors.TextMuted
                    : Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(24, 108),
                BackColor = Color.Transparent
            });

            string badgeText = isArchived ? "Archived" : (branch.IsActive ? "Active" : "Inactive");
            Color badgeFg = isArchived ? Colors.TextMuted : (branch.IsActive ? Colors.Success : Colors.TextMuted);
            Color badgeBg = isArchived ? Color.FromArgb(241, 245, 249) : (branch.IsActive ? Colors.SuccessLight : Color.FromArgb(241, 245, 249));

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

            EventHandler onClick = (s, e) => OpenEditView(branch.BranchId);
            AttachClickRecursive(card, onClick);

            return card;
        }

        private static void AttachClickRecursive(Control parent, EventHandler handler)
        {
            parent.Click += handler;
            foreach (Control child in parent.Controls)
                AttachClickRecursive(child, handler);
        }

        private void OpenEditView(int branchId)
        {
            var editPanel = new CRM.UI.Controls.BranchEditPanel(branchId);
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
