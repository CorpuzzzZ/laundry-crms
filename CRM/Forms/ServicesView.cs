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
    /// Services list view with FAB for new service, filters, and archived support.
    /// </summary>
    public class ServicesView : UserControl
    {
        private readonly ServiceApiService _serviceApi = new(ApiClient.Instance);
        private FlowLayoutPanel pnlCards = null!;
        private TextBox txtSearch = null!;
        private ComboBox cmbCategory = null!;
        private ComboBox cmbActive = null!;
        private CheckBox chkExpress = null!;
        private CheckBox chkShowArchived = null!;
        private ComboBox cmbServiceType = null!;
        private Label lblHeader = null!;
        private FloatingActionButton fabNew = null!;

        private List<ServiceCategoryModel> _categories = new();

        public ServicesView()
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
                Text = "Services",
                Font = Typography.H2,
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(Spacing.Xl, 22)
            };
            pnlFilter.Controls.Add(lblHeader);

            // Search
            pnlFilter.Controls.Add(new Label
            {
                Text = "Search",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(180, 12)
            });

            txtSearch = new TextBox
            {
                Location = new Point(180, 32),
                Width = 200,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.KeyPress += async (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter) await LoadDataAsync();
            };
            pnlFilter.Controls.Add(txtSearch);

            // Category
            pnlFilter.Controls.Add(new Label
            {
                Text = "Category",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(400, 12)
            });

            cmbCategory = new ComboBox
            {
                Location = new Point(400, 32),
                Width = 130,
                Font = Typography.Body,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCategory.Items.Add("All");
            cmbCategory.SelectedIndex = 0;
            cmbCategory.SelectedIndexChanged += async (s, e) => await LoadDataAsync();
            pnlFilter.Controls.Add(cmbCategory);

            // Status
            pnlFilter.Controls.Add(new Label
            {
                Text = "Status",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(550, 12)
            });

            cmbActive = new ComboBox
            {
                Location = new Point(550, 32),
                Width = 100,
                Font = Typography.Body,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbActive.Items.AddRange(new object[] { "All", "Active", "Inactive" });
            cmbActive.SelectedIndex = 0;
            cmbActive.SelectedIndexChanged += async (s, e) => await LoadDataAsync();
            pnlFilter.Controls.Add(cmbActive);

            // Express
            chkExpress = new CheckBox
            {
                Text = "Express only",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                Location = new Point(670, 34),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            chkExpress.CheckedChanged += async (s, e) => await LoadDataAsync();
            pnlFilter.Controls.Add(chkExpress);


            // Service Type
            pnlFilter.Controls.Add(new Label
            {
                Text = "Type",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(670, 12)
            });

            cmbServiceType = new ComboBox
            {
                Location = new Point(670, 32),
                Width = 120,
                Font = Typography.Body,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbServiceType.Items.AddRange(new object[] { "All", "Base", "Chemical", "Machine" });
            cmbServiceType.SelectedIndex = 0;
            cmbServiceType.SelectedIndexChanged += async (s, e) => await LoadDataAsync();
            pnlFilter.Controls.Add(cmbServiceType);

            chkShowArchived = new CheckBox
            {
                Text = "Show Archived",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                Location = new Point(790, 34),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            chkShowArchived.CheckedChanged += async (s, e) => await LoadDataAsync();
            pnlFilter.Controls.Add(chkShowArchived);

            // Search btn
            var btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(920, 30),
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

            // Refresh btn
            var btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(1020, 30),
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

            Controls.Add(fabNew);
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

            // Load categories once
            if (_categories.Count == 0)
            {
                try
                {
                    _categories = await _serviceApi.GetCategoriesAsync();
                    var keepIndex = cmbCategory.SelectedIndex;
                    cmbCategory.Items.Clear();
                    cmbCategory.Items.Add("All");
                    foreach (var c in _categories)
                        cmbCategory.Items.Add(c.CategoryName);
                    cmbCategory.SelectedIndex = Math.Min(keepIndex, cmbCategory.Items.Count - 1);
                }
                catch { /* ignore, list still works */ }
            }

            string? search = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text.Trim();

            int? categoryId = null;
            if (cmbCategory.SelectedIndex > 0 && cmbCategory.SelectedIndex - 1 < _categories.Count)
                categoryId = _categories[cmbCategory.SelectedIndex - 1].ServiceCategoryId;

            bool? isActive = cmbActive.SelectedIndex switch
            {
                1 => true,
                2 => false,
                _ => null
            };

            bool? isExpress = chkExpress.Checked ? true : (bool?)null;

            int? serviceType = cmbServiceType.SelectedIndex switch
            {
                1 => 1,   // Base
                2 => 2,   // Chemical
                3 => 3,   // Machine
                _ => null
            };

            List<ServiceModel> services;
            try
            {
                services = await _serviceApi.GetAllAsync(
                    serviceType: serviceType,
                    categoryId: categoryId,
                    isActive: isActive,
                    isExpress: isExpress,
                    search: search,
                    includeArchived: chkShowArchived.Checked);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load services: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            lblHeader.Text = $"Services ({services.Count})";

            if (services.Count == 0)
            {
                pnlCards.Controls.Add(new Label
                {
                    Text = "No services found.\nClick the + button to add a service.",
                    Font = Typography.H2,
                    ForeColor = Colors.TextMuted,
                    AutoSize = false,
                    Width = 600,
                    Height = 120,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Margin = new Padding(0, 40, 0, 0)
                });
                return;
            }

            int cardWidth = pnlCards.ClientSize.Width
                            - pnlCards.Padding.Left - pnlCards.Padding.Right
                            - SystemInformation.VerticalScrollBarWidth - 4;
            if (cardWidth < 200) cardWidth = 700;

            foreach (var s in services)
                pnlCards.Controls.Add(BuildServiceCard(s, cardWidth));
        }

        private Panel BuildServiceCard(ServiceModel service, int width)
        {
            bool isArchived = service.IsArchived;

            var accentColor = isArchived
                ? Colors.TextMuted
                : (service.IsActive ? Colors.Success : Colors.TextMuted);
            var fillColor = isArchived ? Color.FromArgb(250, 250, 252) : Colors.Surface;
            var nameColor = isArchived ? Colors.TextMuted : Colors.TextPrimary;
            var hoverBorder = isArchived ? Colors.TextMuted : Colors.Primary;

            var card = new RoundedCard
            {
                Width = width,
                Height = 108,
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

            // Name (row 1)
            card.Controls.Add(new Label
            {
                Text = service.ServiceName,
                Font = Typography.H3,
                ForeColor = nameColor,
                AutoSize = true,
                Location = new Point(24, 16),
                BackColor = Color.Transparent
            });

            // Code (row 2)
            card.Controls.Add(new Label
            {
                Text = service.ServiceCode,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(24, 44),
                BackColor = Color.Transparent
            });

            // Price (row 3)
            card.Controls.Add(new Label
            {
                Text = service.PriceDisplay,
                Font = Typography.H3,
                ForeColor = Colors.Primary,
                AutoSize = true,
                Location = new Point(24, 70),
                BackColor = Color.Transparent
            });

            // Status badge (top-right)
            string badgeText = isArchived ? "Archived" : (service.IsActive ? "Active" : "Inactive");
            Color badgeFg = isArchived ? Colors.TextMuted : (service.IsActive ? Colors.Success : Colors.TextMuted);
            Color badgeBg = isArchived ? Color.FromArgb(241, 245, 249) : (service.IsActive ? Colors.SuccessLight : Color.FromArgb(241, 245, 249));

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

            // Type badge for Chemical / Machine
            if (service.ServiceType != 1)
            {
                var typeFg = service.ServiceType == 2 ? Colors.Warning : Colors.Primary;
                var typeBg = service.ServiceType == 2 ? Colors.WarningLight : Colors.PrimaryLight;
                var typeBadge = new PillBadge
                {
                    BadgeText = service.TypeBadge,
                    FillColor = typeBg,
                    TextColor = typeFg,
                    Size = new Size(84, 24),
                    CornerRadius = 12,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Location = new Point(card.Width - 108, 46)
                };
                card.Controls.Add(typeBadge);
            }

            // Express badge if applicable
            if (service.IsExpressService && !isArchived)
            {
                var expressBadge = new PillBadge
                {
                    BadgeText = "Express",
                    FillColor = Colors.WarningLight,
                    TextColor = Colors.Warning,
                    Size = new Size(84, 24),
                    CornerRadius = 12,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Location = new Point(card.Width - 108, 46)
                };
                card.Controls.Add(expressBadge);
            }

            // Click ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬â„¢ edit
            EventHandler onClick = (s, e) => OpenEditView(service.ServiceId);
            AttachClickRecursive(card, onClick);

            return card;
        }

        private static void AttachClickRecursive(Control parent, EventHandler handler)
        {
            parent.Click += handler;
            foreach (Control child in parent.Controls)
                AttachClickRecursive(child, handler);
        }

        private void OpenEditView(int serviceId)
        {
            var editPanel = new CRM.UI.Controls.ServiceEditPanel(serviceId);
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
