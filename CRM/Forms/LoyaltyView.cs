using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRM.UI;
using CRM.UI.Controls;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.WinForms.Forms
{
    public class LoyaltyView : UserControl
    {
        private readonly LoyaltyApiService _api = new(ApiClient.Instance);
        private bool _canManage;

        private TabControl _tabs = null!;

        // Settings tab
        private NumericUpDown _numPointsPerOrder = null!;
        private NumericUpDown _numPointsPerDollar = null!;
        private NumericUpDown _numRedeemRequired = null!;
        private NumericUpDown _numRedeemDiscount = null!;
        private NumericUpDown _numExpiryDays = null!;
        private CheckBox _chkExpiryEnabled = null!;
        private CheckBox _chkActive = null!;
        private Label _lblEarnPreview = null!;
        private Label _lblRedeemPreview = null!;
        private Button _btnSaveSettings = null!;

        // Active-rule banner (read-only display of what's actually saved)
        private Panel _bannerPanel = null!;
        private Label _bannerTitle = null!;
        private Label _bannerEarn = null!;
        private Label _bannerRedeem = null!;
        private Label _bannerStatus = null!;
        private LoyaltySettingModel? _savedSnapshot;

        // Tiers tab & PDF document card
        private DataGridView _dgvTiers = null!;
        private Button _btnNewTier = null!;
        private Button _btnEditTier = null!;
        private Button _btnDeleteTier = null!;
        private Button _btnRefreshTiers = null!;
        private List<LoyaltyTierModel> _tiers = new();
        private DashboardCard _cardPdf = null!;
        private Panel _pdfContentPanel = null!;

        // Customers tab
        private DataGridView _dgvCustomers = null!;
        private TextBox _txtCustomerSearch = null!;
        private Button _btnRefreshCustomers = null!;
        private List<LoyaltyCustomerModel> _customers = new();

        public LoyaltyView()
        {
            InitializeComponent();
            Load += async (s, e) => await LoadAllAsync();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Background;
            Font = Typography.Body;

            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(16, 6),
                Font = Typography.Body
            };

            var _user = CRM.WinForms.Services.SessionManager.CurrentUser;
            _canManage = _user?.IsAdmin == true || _user?.IsManager == true;

            if (_canManage)
            {
                _tabs.TabPages.Add(BuildSettingsTab());
                _tabs.TabPages.Add(BuildTiersTab());
            }
            _tabs.TabPages.Add(BuildCustomersTab());
            _tabs.SelectedIndexChanged += async (s, e) =>
            {
                if (!_canManage)
                {
                    // Crew only sees the Customers tab
                    await LoadCustomersAsync();
                    return;
                }

                if (_tabs.SelectedIndex == 0) await LoadSettingsAsync();
                else if (_tabs.SelectedIndex == 1) await LoadTiersAsync();
                else await LoadCustomersAsync();
            };

            Controls.Add(_tabs);


        }

        // ============================================================
        // SETTINGS TAB
        // ============================================================
        private TabPage BuildSettingsTab()
        {
            var page = new TabPage("Settings")
            {
                BackColor = Colors.Background,
                Padding = new Padding(0)
            };

            // Scrollable host: fills the tab, shows scrollbars when needed
            var scroller = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Colors.Background,
                Padding = new Padding(40, 24, 40, 24)
            };

            // Inner card with fixed height that content actually needs
            var card = new DashboardCard
            {
                Location = new Point(40, 24),
                Width = 900,
                Height = 580,
                CornerRadius = 14,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true,
                Padding = new Padding(28, 24, 28, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            int y = 24;

            // ---- Active rule banner (read-only) ----
            _bannerPanel = BuildActiveRuleBanner(y);
            card.Controls.Add(_bannerPanel);
            y += _bannerPanel.Height + 16;

            card.Controls.Add(HeaderLabel("Earning", y)); y += 32;
            card.Controls.Add(SubLabel("How customers accumulate loyalty points.", y)); y += 30;

            card.Controls.Add(FieldLabel("Points per order", y));
            _numPointsPerOrder = MakeNum(y + 22, 140, 0m, 10000m, 2);
            _numPointsPerOrder.ValueChanged += (s, e) => UpdatePreviews();
            card.Controls.Add(_numPointsPerOrder);
            card.Controls.Add(InlineHint("awarded for every completed order", 200, y + 26));
            y += 72;

            card.Controls.Add(FieldLabel("Points per peso spent", y));
            _numPointsPerDollar = MakeNum(y + 22, 140, 0m, 10000m, 2);
            _numPointsPerDollar.ValueChanged += (s, e) => UpdatePreviews();
            card.Controls.Add(_numPointsPerDollar);
            card.Controls.Add(InlineHint("multiplied by the order total", 200, y + 26));
            y += 88;

            card.Controls.Add(HeaderLabel("Redemption", y)); y += 32;
            card.Controls.Add(SubLabel("What a customer gets when they redeem points.", y)); y += 30;

            card.Controls.Add(FieldLabel("Points required to redeem", y));
            _numRedeemRequired = MakeNum(y + 22, 140, 1m, 1000000m, 0);
            _numRedeemRequired.ValueChanged += (s, e) => UpdatePreviews();
            card.Controls.Add(_numRedeemRequired);
            y += 72;

            card.Controls.Add(FieldLabel("Discount amount (PHP)", y));
            _numRedeemDiscount = MakeNum(y + 22, 140, 0m, 1000000m, 2);
            _numRedeemDiscount.ValueChanged += (s, e) => UpdatePreviews();
            card.Controls.Add(_numRedeemDiscount);
            y += 88;

            card.Controls.Add(HeaderLabel("Rules", y)); y += 32;

            _chkExpiryEnabled = new CheckBox
            {
                Text = "Points expire after",
                Left = 28, Top = y, Width = 160, Height = 26,
                Font = Typography.Body,
                ForeColor = Colors.TextBody
            };
            _chkExpiryEnabled.CheckedChanged += (s, e) =>
                _numExpiryDays.Enabled = _chkExpiryEnabled.Checked;
            card.Controls.Add(_chkExpiryEnabled);

            _numExpiryDays = MakeNum(y - 2, 100, 1m, 36500m, 0);
            _numExpiryDays.Left = 195;
            _numExpiryDays.Enabled = false;
            card.Controls.Add(_numExpiryDays);
            card.Controls.Add(InlineHint("days", 305, y + 4));
            y += 44;

            _chkActive = new CheckBox
            {
                Text = "Loyalty program is active",
                Left = 28, Top = y, Width = 260, Height = 26,
                Font = Typography.Body,
                ForeColor = Colors.TextBody
            };
            card.Controls.Add(_chkActive);
            y += 52;

            _lblEarnPreview = new Label
            {
                Left = 28, Top = y, Width = 700, Height = 26,
                Font = Typography.Body, ForeColor = Colors.TextSecondary
            };
            card.Controls.Add(_lblEarnPreview); y += 26;

            _lblRedeemPreview = new Label
            {
                Left = 28, Top = y, Width = 700, Height = 26,
                Font = Typography.Body, ForeColor = Colors.TextSecondary
            };
            card.Controls.Add(_lblRedeemPreview); y += 42;

            _btnSaveSettings = MakePrimaryButton("Save Settings");
            _btnSaveSettings.Left = 28;
            _btnSaveSettings.Top = y;
            _btnSaveSettings.Click += async (s, e) => await SaveSettingsAsync();
            card.Controls.Add(_btnSaveSettings);
            y += 56;

            // Set final card height so scrollbar triggers when window is small
            card.Height = y + 24;

            scroller.Controls.Add(card);

            // Keep card width in sync with the scroller's client width
            scroller.Resize += (s, e) =>
            {
                int availW = scroller.ClientSize.Width - scroller.Padding.Horizontal;
                if (availW < 400) availW = 400;   // minimum
                card.Width = availW;
            };

            page.Controls.Add(scroller);
            return page;
        }

        // ============================================================
        // TIERS TAB
        // ============================================================
        private TabPage BuildTiersTab()
        {
            var page = new TabPage("Tiers")
            {
                BackColor = Colors.Background,
                Padding = new Padding(24, 20, 24, 20)
            };

            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                BackColor = Colors.Background
            };

            _btnNewTier = MakePrimaryButton("+ New Tier");
            _btnNewTier.Left = 0; _btnNewTier.Top = 8;
            _btnNewTier.Click += (s, e) => ShowTierEditor(0);

            _btnEditTier = MakeSecondaryButton("Edit");
            _btnEditTier.Left = 150; _btnEditTier.Top = 8;
            _btnEditTier.Click += (s, e) =>
            {
                if (SelectedTier is { } t) ShowTierEditor(t.LoyaltyTierId);
            };

            _btnDeleteTier = MakeDangerButton("Delete");
            _btnDeleteTier.Left = 260; _btnDeleteTier.Top = 8;
            _btnDeleteTier.Click += async (s, e) => await DeleteSelectedTierAsync();

            _btnRefreshTiers = MakeSecondaryButton("Refresh");
            _btnRefreshTiers.Left = 370; _btnRefreshTiers.Top = 8;
            _btnRefreshTiers.Click += async (s, e) => await LoadTiersAsync();

            toolbar.Controls.AddRange(new Control[]
            { _btnNewTier, _btnEditTier, _btnDeleteTier, _btnRefreshTiers });

            // Main body containing Table Card (Left) and PDF Card (Right) beside it
            var mainLayout = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Background,
                Padding = new Padding(0, 8, 0, 0)
            };

            // ---- RIGHT: PDF Document Card (Beside the table) ----
            _cardPdf = new DashboardCard
            {
                Dock = DockStyle.Right,
                Width = 360,
                CornerRadius = 14,
                Padding = new Padding(20, 16, 20, 16)
            };

            var hdrPdf = MakeTiersCardHeader("Tier Documentation", "Official PDF reference for tier perks", "PDF Doc");
            _cardPdf.Controls.Add(hdrPdf);

            _pdfContentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(0, 12, 0, 0)
            };
            _cardPdf.Controls.Add(_pdfContentPanel);
            _pdfContentPanel.BringToFront();

            // Spacer between Table and PDF
            var spacer = new Panel
            {
                Dock = DockStyle.Right,
                Width = 16,
                BackColor = Colors.Background
            };

            // ---- LEFT: Tier Table Card ----
            var cardTable = new DashboardCard
            {
                Dock = DockStyle.Fill,
                CornerRadius = 14,
                Padding = new Padding(20, 16, 20, 16)
            };

            var hdrTable = MakeTiersCardHeader("Loyalty Tiers", "Configured customer tiers and point thresholds", "Tiers Table");
            cardTable.Controls.Add(hdrTable);

            var gridHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(0, 12, 0, 0)
            };

            _dgvTiers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Colors.Surface,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Colors.BorderLight,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeight = 36,
                EnableHeadersVisualStyles = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 36 },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 250, 252),
                    ForeColor = Colors.TextSecondary,
                    Font = Typography.SmallBold,
                    Padding = new Padding(10, 0, 10, 0),
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Colors.Surface,
                    ForeColor = Colors.TextBody,
                    SelectionBackColor = Colors.PrimaryLight,
                    SelectionForeColor = Colors.TextPrimary,
                    Font = Typography.Body,
                    Padding = new Padding(10, 0, 10, 0),
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(252, 253, 255),
                    ForeColor = Colors.TextBody,
                    SelectionBackColor = Colors.PrimaryLight,
                    SelectionForeColor = Colors.TextPrimary,
                    Font = Typography.Body,
                    Padding = new Padding(10, 0, 10, 0)
                }
            };

            _dgvTiers.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Name", DataPropertyName = "TierName", FillWeight = 26 });
            _dgvTiers.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Points Range", DataPropertyName = "RangeText", FillWeight = 24 });
            _dgvTiers.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Discount", DataPropertyName = "DiscountText", FillWeight = 16,
                  DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            _dgvTiers.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Multiplier", DataPropertyName = "MultiplierText", FillWeight = 16,
                  DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            _dgvTiers.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Sort", DataPropertyName = "SortOrder", FillWeight = 8,
                  DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            _dgvTiers.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Status", DataPropertyName = "StatusText", FillWeight = 14 });

            _dgvTiers.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && SelectedTier is { } t)
                    ShowTierEditor(t.LoyaltyTierId);
            };

            gridHost.Controls.Add(_dgvTiers);
            cardTable.Controls.Add(gridHost);
            gridHost.BringToFront();

            mainLayout.Controls.Add(cardTable);
            mainLayout.Controls.Add(spacer);
            mainLayout.Controls.Add(_cardPdf);

            page.Controls.Add(mainLayout);
            page.Controls.Add(toolbar);

            RenderPdfSection();

            return page;
        }

        // ============================================================
        // CUSTOMERS TAB
        // ============================================================
        private TabPage BuildCustomersTab()
        {
            var page = new TabPage("Customers")
            {
                BackColor = Colors.Background,
                Padding = new Padding(24, 20, 24, 20)
            };

            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                BackColor = Colors.Background
            };

            toolbar.Controls.Add(new Label
            {
                Text = "Search",
                Left = 0, Top = 4, AutoSize = true,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary
            });

            _txtCustomerSearch = new TextBox
            {
                Left = 0, Top = 22, Width = 300,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtCustomerSearch.TextChanged += (s, e) => ApplyCustomerFilter();
            toolbar.Controls.Add(_txtCustomerSearch);

            _btnRefreshCustomers = MakeSecondaryButton("Refresh");
            _btnRefreshCustomers.Left = 320;
            _btnRefreshCustomers.Top = 20;
            _btnRefreshCustomers.Click += async (s, e) => await LoadCustomersAsync();
            toolbar.Controls.Add(_btnRefreshCustomers);

            _dgvCustomers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Colors.Surface,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeight = 36,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Colors.Background,
                    ForeColor = Colors.TextSecondary,
                    Font = Typography.Small,
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Colors.TextBody,
                    SelectionBackColor = Colors.PrimaryLight,
                    SelectionForeColor = Colors.TextPrimary,
                    Padding = new Padding(8, 0, 8, 0)
                },
                RowTemplate = { Height = 34 }
            };

            _dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Customer", DataPropertyName = "CustomerName", Width = 220 });
            _dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Tier", DataPropertyName = "TierDisplay", Width = 110 });
            _dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Current Points", DataPropertyName = "CurrentPoints", Width = 110 });
            _dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Total Earned", DataPropertyName = "TotalPointsEarned", Width = 110 });
            _dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Total Redeemed", DataPropertyName = "TotalPointsRedeemed", Width = 120 });
            _dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Lifetime Spend", DataPropertyName = "LifetimeSpendText", Width = 130 });
            _dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Last Activity", DataPropertyName = "LastActivityText", Width = 150 });

            _dgvCustomers.CellDoubleClick += async (s, e) =>
            {
                if (e.RowIndex >= 0 &&
                    _dgvCustomers.CurrentRow?.DataBoundItem is LoyaltyCustomerModel c)
                {
                    await ShowCustomerDetailsAsync(c);
                }
            };

            var cardCustTable = new DashboardCard
            {
                Dock = DockStyle.Fill,
                CornerRadius = 14,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true,
                Padding = new Padding(16),
                Margin = new Padding(0, 12, 0, 0)
            };
            cardCustTable.Controls.Add(_dgvCustomers);

            page.Controls.Add(cardCustTable);
            page.Controls.Add(toolbar);
            return page;
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                _customers = await _api.GetCustomersAsync();
                ApplyCustomerFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load customers.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyCustomerFilter()
        {
            var q = (_txtCustomerSearch?.Text ?? string.Empty).Trim();
            System.Collections.Generic.IEnumerable<LoyaltyCustomerModel> rows = _customers;
            if (q.Length > 0)
            {
                var lower = q.ToLowerInvariant();
                rows = rows.Where(c =>
                    (c.CustomerName ?? "").ToLowerInvariant().Contains(lower) ||
                    (c.CustomerCode ?? "").ToLowerInvariant().Contains(lower) ||
                    (c.Phone ?? "").ToLowerInvariant().Contains(lower) ||
                    (c.Email ?? "").ToLowerInvariant().Contains(lower));
            }
            _dgvCustomers.DataSource = null;
            _dgvCustomers.DataSource = rows.ToList();
        }

        private async Task ShowCustomerDetailsAsync(LoyaltyCustomerModel c)
        {
            using var dlg = new LoyaltyCustomerDetailsDialog(_api, c);
            dlg.ShowDialog(FindForm());
        }

        private LoyaltyTierModel? SelectedTier =>
            _dgvTiers.CurrentRow?.DataBoundItem as LoyaltyTierModel;

        // ============================================================
        // HELPERS — labels, inputs, buttons
        // ============================================================
        private static Label HeaderLabel(string text, int top) => new()
        {
            Text = text,
            Left = 28, Top = top, AutoSize = true,
            Font = Typography.H3,
            ForeColor = Colors.TextPrimary
        };

        private static Label SubLabel(string text, int top) => new()
        {
            Text = text,
            Left = 28, Top = top, AutoSize = true,
            Font = Typography.Small,
            ForeColor = Colors.TextSecondary
        };

        private static Label FieldLabel(string text, int top) => new()
        {
            Text = text,
            Left = 28, Top = top, AutoSize = true,
            Font = Typography.Small,
            ForeColor = Colors.TextBody
        };

        private static Label InlineHint(string text, int left, int top) => new()
        {
            Text = text,
            Left = left, Top = top, AutoSize = true,
            Font = Typography.Small,
            ForeColor = Colors.TextMuted
        };

        private static NumericUpDown MakeNum(int top, int width, decimal min, decimal max, int decimals) => new()
        {
            Left = 28, Top = top, Width = width,
            Minimum = min, Maximum = max, DecimalPlaces = decimals,
            Font = Typography.Body,
            BorderStyle = BorderStyle.FixedSingle
        };

        private static Button MakePrimaryButton(string text) => new()
        {
            Text = text,
            Width = 140, Height = 36,
            FlatStyle = FlatStyle.Flat,
            BackColor = Colors.Primary,
            ForeColor = Color.White,
            Font = Typography.Body,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 0 }
        };

        private static Button MakeSecondaryButton(string text) => new()
        {
            Text = text,
            Width = 100, Height = 36,
            FlatStyle = FlatStyle.Flat,
            BackColor = Colors.Surface,
            ForeColor = Colors.TextPrimary,
            Font = Typography.Body,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 1, BorderColor = Colors.Border }
        };

        private static Button MakeDangerButton(string text) => new()
        {
            Text = text,
            Width = 100, Height = 36,
            FlatStyle = FlatStyle.Flat,
            BackColor = Colors.Danger,
            ForeColor = Color.White,
            Font = Typography.Body,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 0 }
        };

        // ============================================================
        // LOAD
        // ============================================================
        private async Task LoadAllAsync()
        {
            if (!_canManage)
            {
                // Crew only sees the Customers tab
                await LoadCustomersAsync();
                return;
            }

            await LoadSettingsAsync();
            await LoadTiersAsync();
        }

        private async Task LoadSettingsAsync()
        {
            if (!_canManage) return;   // Settings tab not built for Crew

            try
            {
                var s = await _api.GetSettingsAsync();
                if (s == null)
                {
                    // No settings yet — leave defaults
                    _numPointsPerOrder.Value = 10;
                    _numPointsPerDollar.Value = 1;
                    _numRedeemRequired.Value = 100;
                    _numRedeemDiscount.Value = 10;
                    _chkExpiryEnabled.Checked = false;
                    _chkActive.Checked = true;
                }
                else
                {
                    _numPointsPerOrder.Value = Clamp(s.PointsPerOrder, _numPointsPerOrder);
                    _numPointsPerDollar.Value = Clamp(s.PointsPerDollar, _numPointsPerDollar);
                    _numRedeemRequired.Value = Clamp(s.RedeemPointsRequired, _numRedeemRequired);
                    _numRedeemDiscount.Value = Clamp(s.RedeemDiscountAmount, _numRedeemDiscount);
                    _chkExpiryEnabled.Checked = s.PointsExpiryDays.HasValue;
                    _numExpiryDays.Enabled = _chkExpiryEnabled.Checked;
                    if (s.PointsExpiryDays.HasValue)
                        _numExpiryDays.Value = Clamp(s.PointsExpiryDays.Value, _numExpiryDays);
                    _chkActive.Checked = s.IsActive;
                }

                RenderActiveRuleBanner(s);
                UpdatePreviews();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load loyalty settings.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static decimal Clamp(decimal v, NumericUpDown n)
            => v < n.Minimum ? n.Minimum : (v > n.Maximum ? n.Maximum : v);

        private async Task LoadTiersAsync()
        {
            if (!_canManage) return;   // Tiers tab not built for Crew

            try
            {
                _tiers = await _api.GetTiersAsync();
                _dgvTiers.DataSource = null;
                _dgvTiers.DataSource = _tiers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load tiers.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdatePreviews()
        {
            _lblEarnPreview.Text =
                $"Earning: {_numPointsPerOrder.Value:0.##} pt per order + " +
                $"{_numPointsPerDollar.Value:0.##} pt per PHP spent.";
            _lblRedeemPreview.Text =
                $"Redemption: {_numRedeemRequired.Value:0.##} points = " +
                $"PHP {_numRedeemDiscount.Value:0.##} off.";

            UpdateBannerDirtyState();
        }

        // ============================================================
        // ACTIVE RULE BANNER
        // ============================================================
        private Panel BuildActiveRuleBanner(int top)
        {
            var panel = new Panel
            {
                Left = 28, Top = top, Width = 700, Height = 118,
                BackColor = Colors.PrimaryLight,
                Padding = new Padding(16, 12, 16, 12)
            };

            _bannerTitle = new Label
            {
                Text = "ACTIVE RULE",
                Left = 16, Top = 12, AutoSize = true,
                Font = Typography.Small,
                ForeColor = Colors.Primary
            };
            panel.Controls.Add(_bannerTitle);

            _bannerEarn = new Label
            {
                Left = 16, Top = 36, Width = 660, Height = 22,
                Font = Typography.Body,
                ForeColor = Colors.TextPrimary,
                Text = "No rule saved yet."
            };
            panel.Controls.Add(_bannerEarn);

            _bannerRedeem = new Label
            {
                Left = 16, Top = 60, Width = 660, Height = 22,
                Font = Typography.Body,
                ForeColor = Colors.TextPrimary
            };
            panel.Controls.Add(_bannerRedeem);

            _bannerStatus = new Label
            {
                Left = 16, Top = 84, Width = 660, Height = 22,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary
            };
            panel.Controls.Add(_bannerStatus);

            return panel;
        }

        private void RenderActiveRuleBanner(LoyaltySettingModel? saved)
        {
            _savedSnapshot = saved;

            if (saved == null)
            {
                _bannerPanel.BackColor = Colors.WarningLight;
                _bannerTitle.Text = "ACTIVE RULE — none yet";
                _bannerTitle.ForeColor = Colors.Warning;
                _bannerEarn.Text = "No loyalty rule has been saved for this tenant.";
                _bannerRedeem.Text = "Fill in the fields below and click Save Settings.";
                _bannerStatus.Text = "";
                return;
            }

            _bannerPanel.BackColor = Colors.PrimaryLight;
            _bannerTitle.Text = "ACTIVE RULE (saved)";
            _bannerTitle.ForeColor = Colors.Primary;

            _bannerEarn.Text =
                $"Earning:  {saved.PointsPerOrder:0.##} pt per order  +  " +
                $"{saved.PointsPerDollar:0.##} pt per PHP spent";

            _bannerRedeem.Text =
                $"Redeem:  {saved.RedeemPointsRequired:0.##} points  =  " +
                $"PHP {saved.RedeemDiscountAmount:0.##} off";

            string expiry = saved.PointsExpiryDays.HasValue
                ? $"expires after {saved.PointsExpiryDays} days"
                : "never expires";
            string status = saved.IsActive ? "Active" : "Inactive";
            _bannerStatus.Text =
                $"Expiry: {expiry}   |   Status: {status}   |   " +
                $"Last updated: {saved.UpdatedAt.ToLocalTime():yyyy-MM-dd HH:mm}";
        }

        private void UpdateBannerDirtyState()
        {
            if (_savedSnapshot == null) return;

            bool dirty =
                _numPointsPerOrder.Value != _savedSnapshot.PointsPerOrder ||
                _numPointsPerDollar.Value != _savedSnapshot.PointsPerDollar ||
                _numRedeemRequired.Value != _savedSnapshot.RedeemPointsRequired ||
                _numRedeemDiscount.Value != _savedSnapshot.RedeemDiscountAmount ||
                (_chkExpiryEnabled.Checked
                    ? (_numExpiryDays.Value != (_savedSnapshot.PointsExpiryDays ?? 0))
                    : _savedSnapshot.PointsExpiryDays.HasValue) ||
                _chkActive.Checked != _savedSnapshot.IsActive;

            if (dirty)
            {
                _bannerPanel.BackColor = Colors.WarningLight;
                _bannerTitle.Text = "ACTIVE RULE — unsaved changes below";
                _bannerTitle.ForeColor = Colors.Warning;
            }
            else
            {
                _bannerPanel.BackColor = Colors.PrimaryLight;
                _bannerTitle.Text = "ACTIVE RULE (saved)";
                _bannerTitle.ForeColor = Colors.Primary;
            }
        }

        // ============================================================
        // SAVE
        // ============================================================
        private async Task SaveSettingsAsync()
        {
            _btnSaveSettings.Enabled = false;
            try
            {
                var req = new UpdateLoyaltySettingRequest
                {
                    PointsPerOrder = _numPointsPerOrder.Value,
                    PointsPerDollar = _numPointsPerDollar.Value,
                    RedeemPointsRequired = _numRedeemRequired.Value,
                    RedeemDiscountAmount = _numRedeemDiscount.Value,
                    PointsExpiryDays = _chkExpiryEnabled.Checked
                        ? (int?)_numExpiryDays.Value : null,
                    IsActive = _chkActive.Checked
                };

                var updated = await _api.UpdateSettingsAsync(req);
                if (updated == null)
                {
                    MessageBox.Show("Save failed. Check the API log for details.",
                        "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Loyalty settings saved.",
                        "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadSettingsAsync();
                }
            }
            finally
            {
                _btnSaveSettings.Enabled = true;
            }
        }

        // ============================================================
        // TIER EDITOR
        // ============================================================
        private void ShowTierEditor(int tierId)
        {
            using var dlg = new LoyaltyTierDialog(_api, tierId);
            if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
            {
                _ = LoadTiersAsync();
            }
        }

        private async Task DeleteSelectedTierAsync()
        {
            if (SelectedTier is not { } t) return;

            if (MessageBox.Show(
                $"Delete tier \"{t.TierName}\"?",
                "Confirm Delete", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes) return;

            bool ok = await _api.DeleteTierAsync(t.LoyaltyTierId);
            if (!ok)
            {
                MessageBox.Show(
                    "This tier cannot be deleted — it is assigned to customers.",
                    "Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            await LoadTiersAsync();
        }

        private static Panel MakeTiersCardHeader(string title, string subtitle, string badgeText)
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.Transparent
            };

            var lblT = new Label
            {
                Text = title,
                Font = Typography.H3,
                ForeColor = Colors.TextPrimary,
                Location = new Point(0, 4),
                AutoSize = true
            };
            pnl.Controls.Add(lblT);

            var lblSub = new Label
            {
                Text = subtitle,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                Location = new Point(0, 26),
                AutoSize = true
            };
            pnl.Controls.Add(lblSub);

            var badge = new Label
            {
                Text = badgeText,
                Font = Typography.SmallBold,
                ForeColor = Colors.Primary,
                BackColor = Colors.PrimaryLight,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 24,
                AutoSize = true,
                Padding = new Padding(8, 3, 8, 3),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            badge.Top = 10;
            pnl.Controls.Add(badge);

            var localBadge = badge;
            pnl.Resize += (s, e) =>
            {
                localBadge.Left = Math.Max(0, pnl.ClientSize.Width - localBadge.Width);
            };

            pnl.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.BorderLight, 1f);
                e.Graphics.DrawLine(pen, 0, pnl.Height - 1, pnl.Width, pnl.Height - 1);
            };

            return pnl;
        }

        // ============================================================
        // PDF UPLOAD & REMOVE FEATURE (Beside Tier Table)
        // ============================================================
        private string GetLoyaltyDocDirectory()
        {
            var companyId = SessionManager.CurrentUser?.CompanyId ?? 0;
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CRM",
                $"Company_{companyId}",
                "Loyalty"
            );
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }

        private string GetPdfFilePath() => Path.Combine(GetLoyaltyDocDirectory(), "tier_benefits.pdf");
        private string GetPdfMetaPath() => Path.Combine(GetLoyaltyDocDirectory(), "tier_benefits_meta.json");
        private bool HasUploadedPdf() => File.Exists(GetPdfFilePath());

        private LoyaltyTierPdfMeta? LoadPdfMeta()
        {
            try
            {
                var metaPath = GetPdfMetaPath();
                if (File.Exists(metaPath))
                {
                    var json = File.ReadAllText(metaPath);
                    return JsonSerializer.Deserialize<LoyaltyTierPdfMeta>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadPdfMeta] {ex}");
            }

            if (HasUploadedPdf())
            {
                var fi = new FileInfo(GetPdfFilePath());
                return new LoyaltyTierPdfMeta
                {
                    OriginalFileName = "tier_benefits.pdf",
                    FileSizeBytes = fi.Length,
                    UploadedAt = fi.LastWriteTimeUtc,
                    UploadedBy = "System"
                };
            }

            return null;
        }

        private void RenderPdfSection()
        {
            if (_pdfContentPanel == null) return;
            _pdfContentPanel.Controls.Clear();

            if (HasUploadedPdf())
            {
                var meta = LoadPdfMeta();
                RenderUploadedPdfView(meta);
            }
            else
            {
                RenderEmptyUploadView();
            }
        }

        private void RenderUploadedPdfView(LoyaltyTierPdfMeta? meta)
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                AutoScroll = true
            };

            // Document Details Box
            var docBox = new Panel
            {
                Dock = DockStyle.Top,
                Height = 152,
                BackColor = Color.FromArgb(248, 250, 253),
                Padding = new Padding(14)
            };
            docBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, docBox.Width - 1, docBox.Height - 1);
                using var path = DashboardCard.GetRoundedPath(r, 10);
                using var pen = new Pen(Colors.Border, 1f);
                e.Graphics.DrawPath(pen, path);
            };

            // Red PDF Icon Badge
            var pdfBadge = new Panel
            {
                Location = new Point(14, 14),
                Size = new Size(40, 46),
                BackColor = Color.FromArgb(231, 76, 60),
                Cursor = Cursors.Hand
            };
            pdfBadge.Click += (s, e) => HandleViewPdf();
            pdfBadge.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, pdfBadge.Width - 1, pdfBadge.Height - 1);
                using var path = DashboardCard.GetRoundedPath(r, 6);
                using var fill = new SolidBrush(Color.FromArgb(231, 76, 60));
                e.Graphics.FillPath(fill, path);

                using var font = new Font(Typography.Family, 9f, FontStyle.Bold);
                TextRenderer.DrawText(e.Graphics, "PDF", font, r, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            docBox.Controls.Add(pdfBadge);

            // File Name
            var fileName = meta?.OriginalFileName ?? "tier_benefits.pdf";
            var lblFileName = new Label
            {
                Text = fileName,
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                Location = new Point(62, 14),
                Width = Math.Max(100, docBox.Width - 76),
                Height = 22,
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };
            lblFileName.Click += (s, e) => HandleViewPdf();
            docBox.Controls.Add(lblFileName);

            // File Size
            var sizeText = meta?.FormattedSize ?? "Unknown size";
            var lblSize = new Label
            {
                Text = $"Size: {sizeText}",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                Location = new Point(62, 38),
                AutoSize = true
            };
            docBox.Controls.Add(lblSize);

            // Upload Date
            var dateText = meta != null && meta.UploadedAt != default
                ? $"Uploaded: {meta.UploadedAt.ToLocalTime():MMM dd, yyyy HH:mm}"
                : "Uploaded: Recently";
            var lblDate = new Label
            {
                Text = dateText,
                Font = Typography.Tiny,
                ForeColor = Colors.TextMuted,
                Location = new Point(62, 58),
                AutoSize = true
            };
            docBox.Controls.Add(lblDate);

            // Uploaded by
            if (!string.IsNullOrEmpty(meta?.UploadedBy))
            {
                var lblUser = new Label
                {
                    Text = $"By: {meta.UploadedBy}",
                    Font = Typography.Tiny,
                    ForeColor = Colors.TextMuted,
                    Location = new Point(62, 76),
                    AutoSize = true
                };
                docBox.Controls.Add(lblUser);
            }

            // Status Pill
            var statusPill = new Label
            {
                Text = "✓ Document Attached",
                Font = Typography.TinyUpper,
                ForeColor = Colors.Success,
                BackColor = Colors.SuccessLight,
                Location = new Point(14, 114),
                Height = 22,
                AutoSize = true,
                Padding = new Padding(8, 3, 8, 3)
            };
            docBox.Controls.Add(statusPill);

            pnl.Controls.Add(docBox);

            // Action Buttons
            int btnY = 170;

            var btnView = MakePrimaryButton("👁️  View / Open PDF");
            btnView.Location = new Point(0, btnY);
            btnView.Width = Math.Max(120, _pdfContentPanel.ClientSize.Width);
            btnView.Height = 38;
            btnView.Click += (s, e) => HandleViewPdf();
            pnl.Controls.Add(btnView);
            btnY += 46;

            var btnDownload = MakeSecondaryButton("💾  Save Copy...");
            btnDownload.Location = new Point(0, btnY);
            btnDownload.Width = Math.Max(120, _pdfContentPanel.ClientSize.Width);
            btnDownload.Height = 36;
            btnDownload.Click += (s, e) => HandleDownloadPdf();
            pnl.Controls.Add(btnDownload);
            btnY += 44;

            var btnReplace = MakeSecondaryButton("🔄  Replace PDF...");
            btnReplace.Location = new Point(0, btnY);
            btnReplace.Width = Math.Max(120, _pdfContentPanel.ClientSize.Width);
            btnReplace.Height = 36;
            btnReplace.Click += (s, e) => HandleUploadPdf();
            pnl.Controls.Add(btnReplace);
            btnY += 44;

            var btnRemove = MakeDangerButton("🗑️  Remove PDF");
            btnRemove.Location = new Point(0, btnY);
            btnRemove.Width = Math.Max(120, _pdfContentPanel.ClientSize.Width);
            btnRemove.Height = 36;
            btnRemove.Click += (s, e) => HandleRemovePdf();
            pnl.Controls.Add(btnRemove);

            pnl.Resize += (s, e) =>
            {
                int w = pnl.ClientSize.Width;
                docBox.Width = w;
                lblFileName.Width = Math.Max(100, w - 76);
                btnView.Width = w;
                btnDownload.Width = w;
                btnReplace.Width = w;
                btnRemove.Width = w;
            };

            _pdfContentPanel.Controls.Add(pnl);
        }

        private void RenderEmptyUploadView()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 252, 255),
                Padding = new Padding(16),
                AllowDrop = true
            };
            pnl.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1);
                using var path = DashboardCard.GetRoundedPath(r, 10);
                using var pen = new Pen(Color.FromArgb(205, 220, 240), 1.5f)
                {
                    DashStyle = DashStyle.Dash
                };
                e.Graphics.DrawPath(pen, path);
            };

            pnl.DragEnter += (s, e) =>
            {
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                {
                    var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
                    if (files != null && files.Length > 0 && Path.GetExtension(files[0]).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        e.Effect = DragDropEffects.Copy;
                        return;
                    }
                }
                e.Effect = DragDropEffects.None;
            };

            pnl.DragDrop += (s, e) =>
            {
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                {
                    var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
                    if (files != null && files.Length > 0 && Path.GetExtension(files[0]).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        SavePdfFile(files[0]);
                    }
                }
            };

            var lblIcon = new Label
            {
                Text = "📄",
                Font = new Font(Typography.Family, 32f),
                ForeColor = Colors.Primary,
                Dock = DockStyle.Top,
                Height = 52,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblPrompt = new Label
            {
                Text = "Upload Tier Benefits PDF",
                Font = Typography.H3,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblDesc = new Label
            {
                Text = "Attach official guidelines, tier qualification perks, or rules document for reference.",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                Dock = DockStyle.Top,
                Height = 44,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var actionHost = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.Transparent
            };
            var btnUpload = MakePrimaryButton("+ Upload PDF");
            btnUpload.Width = 160;
            btnUpload.Height = 38;
            btnUpload.Click += (s, e) => HandleUploadPdf();
            actionHost.Controls.Add(btnUpload);
            actionHost.Resize += (s, e) =>
            {
                btnUpload.Left = (actionHost.ClientSize.Width - btnUpload.Width) / 2;
            };
            btnUpload.Left = Math.Max(0, (pnl.ClientSize.Width - btnUpload.Width) / 2);

            var lblHint = new Label
            {
                Text = "or drag and drop your .pdf here\nSupports files up to 50 MB",
                Font = Typography.Tiny,
                ForeColor = Colors.TextMuted,
                Dock = DockStyle.Top,
                Height = 36,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Order of controls so Dock.Top stacks top-to-bottom
            pnl.Controls.Add(lblHint);
            pnl.Controls.Add(actionHost);
            pnl.Controls.Add(lblDesc);
            pnl.Controls.Add(lblPrompt);
            pnl.Controls.Add(lblIcon);

            _pdfContentPanel.Controls.Add(pnl);
        }

        private void HandleUploadPdf()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select Loyalty Tier PDF Document",
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = false
            };

            if (ofd.ShowDialog(FindForm()) == DialogResult.OK)
            {
                SavePdfFile(ofd.FileName);
            }
        }

        private void SavePdfFile(string sourceFilePath)
        {
            try
            {
                if (!File.Exists(sourceFilePath)) return;
                var ext = Path.GetExtension(sourceFilePath);
                if (!ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Only .PDF files are supported.", "Invalid File Type",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var fi = new FileInfo(sourceFilePath);
                if (fi.Length > 50 * 1024 * 1024)
                {
                    MessageBox.Show("The selected PDF file is too large (maximum 50 MB).",
                        "File Too Large", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var targetPdf = GetPdfFilePath();
                File.Copy(sourceFilePath, targetPdf, true);

                var meta = new LoyaltyTierPdfMeta
                {
                    OriginalFileName = Path.GetFileName(sourceFilePath),
                    FileSizeBytes = fi.Length,
                    UploadedAt = DateTime.UtcNow,
                    UploadedBy = SessionManager.CurrentUser?.FullName ?? "Admin"
                };

                var json = JsonSerializer.Serialize(meta, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(GetPdfMetaPath(), json);

                MessageBox.Show("Loyalty Tier PDF uploaded successfully.", "Upload Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                RenderPdfSection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to upload PDF:\n\n{ex.Message}", "Upload Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleViewPdf()
        {
            try
            {
                var path = GetPdfFilePath();
                if (!File.Exists(path))
                {
                    MessageBox.Show("No PDF file found.", "File Not Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open PDF file:\n\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleDownloadPdf()
        {
            try
            {
                var path = GetPdfFilePath();
                if (!File.Exists(path)) return;

                var meta = LoadPdfMeta();
                var defaultName = meta?.OriginalFileName ?? "Loyalty_Tier_Benefits.pdf";

                using var sfd = new SaveFileDialog
                {
                    Title = "Save Tier PDF Copy",
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = defaultName
                };

                if (sfd.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    File.Copy(path, sfd.FileName, true);
                    MessageBox.Show("PDF copy saved successfully.", "Saved",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save copy:\n\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleRemovePdf()
        {
            var meta = LoadPdfMeta();
            var docName = meta?.OriginalFileName ?? "the tier PDF file";

            if (MessageBox.Show(
                $"Are you sure you want to remove \"{docName}\"?\n\nThis will permanently delete the uploaded PDF.",
                "Remove PDF Document",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                var pdfPath = GetPdfFilePath();
                if (File.Exists(pdfPath)) File.Delete(pdfPath);

                var metaPath = GetPdfMetaPath();
                if (File.Exists(metaPath)) File.Delete(metaPath);

                MessageBox.Show("Loyalty tier PDF removed successfully.", "Document Removed",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                RenderPdfSection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove PDF:\n\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}










