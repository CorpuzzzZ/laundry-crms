using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRM.UI;
using CRM.UI.Controls;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.WinForms.Forms
{
    public class SuperAdminDashboardControl : UserControl
    {
        private readonly SuperAdminApiService _api = new(ApiClient.Instance);

        private Panel _pnlHeader = null!;
        private Label _lblTitle = null!;
        private Button _btnRefresh = null!;

        private Panel _scroller = null!;
        private Panel _stack = null!;

        // Row 1: KPI Metric Cards
        private DashboardMetricCard _cardCompanies = null!;
        private DashboardMetricCard _cardSubscriptions = null!;
        private DashboardMetricCard _cardMrr = null!;
        private DashboardMetricCard _cardUsers = null!;

        // Row 2: Plan Breakdown & Quick Actions
        private DashboardCard _cardPlans = null!;
        private DataGridView _dgvPlans = null!;
        private DashboardCard _cardActions = null!;

        // Row 3: Recent Companies
        private DashboardCard _cardRecent = null!;
        private DataGridView _dgvRecent = null!;

        public SuperAdminDashboardControl()
        {
            InitializeComponent();
            Load += async (s, e) =>
            {
                ArrangeLayout();
                await LoadAsync();
            };
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Background;
            Font = Typography.Body;

            // ============================================================
            // 1. HEADER
            // ============================================================
            _pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Colors.Background,
                Padding = new Padding(24, 16, 24, 0)
            };

            _lblTitle = new Label
            {
                Text = "Platform Overview — Super Admin",
                Left = 24, Top = 16,
                AutoSize = true,
                Font = Typography.H1,
                ForeColor = Colors.TextPrimary
            };
            _pnlHeader.Controls.Add(_lblTitle);

            _btnRefresh = new Button
            {
                Text = "⟳  Refresh Data",
                Width = 130, Height = 36,
                Top = 16,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnRefresh.FlatAppearance.BorderSize = 1;
            _btnRefresh.FlatAppearance.BorderColor = Colors.Border;
            _btnRefresh.Click += async (s, e) => await LoadAsync();
            _pnlHeader.Controls.Add(_btnRefresh);

            _pnlHeader.Resize += (s, e) =>
            {
                _btnRefresh.Left = _pnlHeader.ClientSize.Width - 24 - _btnRefresh.Width;
            };

            // ============================================================
            // 2. SCROLLER & STACK
            // ============================================================
            _scroller = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Colors.Background
            };

            _stack = new Panel
            {
                Dock = DockStyle.Top,
                BackColor = Colors.Background,
                Height = 1100
            };
            _scroller.Controls.Add(_stack);

            // ============================================================
            // 3. ROW 1: 4 KPI METRIC CARDS
            // ============================================================
            _cardCompanies = new DashboardMetricCard
            {
                Caption = "Total Companies",
                Value = "0",
                DeltaText = "Registered Tenants",
                IconText = "🏢",
                IconBgColor = Colors.PrimaryLight,
                IconFgColor = Colors.Primary,
                AccentColor = Colors.Primary,
                CornerRadius = 12
            };
            _stack.Controls.Add(_cardCompanies);

            _cardSubscriptions = new DashboardMetricCard
            {
                Caption = "Active Subscriptions",
                Value = "0",
                DeltaText = "Active Tenants",
                IconText = "💳",
                IconBgColor = Colors.SuccessLight,
                IconFgColor = Colors.Success,
                AccentColor = Colors.Success,
                CornerRadius = 12
            };
            _stack.Controls.Add(_cardSubscriptions);

            _cardMrr = new DashboardMetricCard
            {
                Caption = "Platform MRR",
                Value = "PHP 0",
                DeltaText = "Monthly Recurring",
                IconText = "₱",
                IconBgColor = Color.FromArgb(238, 235, 255),
                IconFgColor = Color.FromArgb(108, 92, 231),
                AccentColor = Color.FromArgb(108, 92, 231),
                CornerRadius = 12
            };
            _stack.Controls.Add(_cardMrr);

            _cardUsers = new DashboardMetricCard
            {
                Caption = "Total Platform Users",
                Value = "0",
                DeltaText = "Across all companies",
                IconText = "👥",
                IconBgColor = Colors.WarningLight,
                IconFgColor = Colors.Warning,
                AccentColor = Colors.Warning,
                CornerRadius = 12
            };
            _stack.Controls.Add(_cardUsers);

            // ============================================================
            // 4. ROW 2: PLAN BREAKDOWN & QUICK ACTIONS
            // ============================================================
            _cardPlans = new DashboardCard
            {
                CornerRadius = 12,
                Padding = new Padding(20, 16, 20, 16)
            };
            var hdrPlans = MakeCardHeader("Subscription Plans Distribution", "Active tenant subscriptions by tier");
            _cardPlans.Controls.Add(hdrPlans);

            var hostPlans = new Panel { Dock = DockStyle.Fill, BackColor = Colors.Surface, Padding = new Padding(0, 10, 0, 0) };
            _dgvPlans = MakeGrid();
            _dgvPlans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Plan Name", DataPropertyName = "PlanName", FillWeight = 40 });
            _dgvPlans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Monthly Fee", DataPropertyName = "PriceText", FillWeight = 25 });
            _dgvPlans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Active Tenants", DataPropertyName = "CompanyCount", FillWeight = 20 });
            _dgvPlans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total MRR", DataPropertyName = "RevenueText", FillWeight = 25 });
            hostPlans.Controls.Add(_dgvPlans);
            _cardPlans.Controls.Add(hostPlans);
            hostPlans.BringToFront();
            _stack.Controls.Add(_cardPlans);

            _cardActions = new DashboardCard
            {
                CornerRadius = 12,
                Padding = new Padding(20, 16, 20, 16)
            };
            var hdrAct = MakeCardHeader("Platform Quick Actions", "Immediate administrative shortcuts");
            _cardActions.Controls.Add(hdrAct);

            var hostAct = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Colors.Surface,
                Padding = new Padding(0, 12, 0, 0)
            };

            var btnAddComp = MakeActionButton("🏢  Register New Company & Subscription", "Provision a new tenant and assign a plan", Colors.Primary);
            btnAddComp.Click += async (s, e) =>
            {
                var plans = await _api.GetPlansAsync();
                var dlg = new AddCompanyDialog(plans);
                if (dlg.ShowDialog() == DialogResult.OK) await LoadAsync();
            };
            hostAct.Controls.Add(btnAddComp);

            var btnAddPlan = MakeActionButton("💳  Create New Subscription Plan", "Define a new tier, limits, and pricing", Color.FromArgb(108, 92, 231));
            btnAddPlan.Click += async (s, e) =>
            {
                var dlg = new CreatePlanDialog();
                if (dlg.ShowDialog() == DialogResult.OK) await LoadAsync();
            };
            hostAct.Controls.Add(btnAddPlan);

            var btnTerms = MakeActionButton("📜  Manage Terms and Conditions", "Review, publish, or modify legal terms", Colors.Success);
            btnTerms.Click += async (s, e) =>
            {
                var dlg = new TermsEditDialog();
                if (dlg.ShowDialog() == DialogResult.OK) await LoadAsync();
            };
            hostAct.Controls.Add(btnTerms);

            _cardActions.Controls.Add(hostAct);
            hostAct.BringToFront();
            _stack.Controls.Add(_cardActions);

            // ============================================================
            // 5. ROW 3: RECENT COMPANIES
            // ============================================================
            _cardRecent = new DashboardCard
            {
                CornerRadius = 12,
                Padding = new Padding(20, 16, 20, 16)
            };
            var hdrRecent = MakeCardHeader("Recent Companies & Onboarding", "Latest tenants registered in the platform");
            _cardRecent.Controls.Add(hdrRecent);

            var hostRecent = new Panel { Dock = DockStyle.Fill, BackColor = Colors.Surface, Padding = new Padding(0, 10, 0, 0) };
            _dgvRecent = MakeGrid();
            _dgvRecent.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Code", DataPropertyName = "CompanyCode", FillWeight = 15 });
            _dgvRecent.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Company Name", DataPropertyName = "CompanyName", FillWeight = 35 });
            _dgvRecent.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Subscription Plan", DataPropertyName = "PlanName", FillWeight = 25 });
            _dgvRecent.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "StatusText", FillWeight = 15 });
            _dgvRecent.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Registered Date", DataPropertyName = "JoinedDateText", FillWeight = 20 });
            hostRecent.Controls.Add(_dgvRecent);
            _cardRecent.Controls.Add(hostRecent);
            hostRecent.BringToFront();
            _stack.Controls.Add(_cardRecent);

            Controls.Add(_scroller);
            Controls.Add(_pnlHeader);

            _scroller.Resize += (s, e) => ArrangeLayout();
        }

        private static Panel MakeCardHeader(string title, string subtitle)
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

            pnl.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.BorderLight, 1f);
                e.Graphics.DrawLine(pen, 0, pnl.Height - 1, pnl.Width, pnl.Height - 1);
            };

            return pnl;
        }

        private static Button MakeActionButton(string title, string desc, Color accent)
        {
            var btn = new Button
            {
                Width = 380,
                Height = 56,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Colors.TextPrimary,
                Font = Typography.BodyBold,
                Text = $"{title}\n   {desc}",
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 10)
            };
            btn.FlatAppearance.BorderColor = Colors.Border;
            btn.FlatAppearance.BorderSize = 1;
            btn.MouseEnter += (s, e) => { btn.BackColor = Colors.Surface; btn.FlatAppearance.BorderColor = accent; };
            btn.MouseLeave += (s, e) => { btn.BackColor = Color.FromArgb(248, 250, 252); btn.FlatAppearance.BorderColor = Colors.Border; };
            return btn;
        }

        private static DataGridView MakeGrid() => new()
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
                Padding = new Padding(12, 0, 12, 0),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Colors.Surface,
                ForeColor = Colors.TextBody,
                SelectionBackColor = Colors.PrimaryLight,
                SelectionForeColor = Colors.TextPrimary,
                Font = Typography.Body,
                Padding = new Padding(12, 0, 12, 0),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            }
        };

        private void ArrangeLayout()
        {
            if (_scroller == null || _stack == null || _cardCompanies == null) return;

            int pad = 24;
            int gap = 16;
            int availableWidth = Math.Max(780, _scroller.ClientSize.Width - (pad * 2));
            _stack.Width = _scroller.ClientSize.Width;

            int y = 8;

            // Row 1: 4 Metric Cards
            int cardW4 = (availableWidth - (3 * gap)) / 4;
            int cardH1 = 120;
            _cardCompanies.SetBounds(pad, y, cardW4, cardH1);
            _cardSubscriptions.SetBounds(pad + (cardW4 + gap), y, cardW4, cardH1);
            _cardMrr.SetBounds(pad + (cardW4 + gap) * 2, y, cardW4, cardH1);
            _cardUsers.SetBounds(pad + (cardW4 + gap) * 3, y, availableWidth - (cardW4 + gap) * 3, cardH1);
            y += cardH1 + 20;

            // Row 2: 2 Cards (Plans Breakdown & Quick Actions)
            int cardW2 = (availableWidth - gap) / 2;
            int row2H = 260;
            _cardPlans.SetBounds(pad, y, cardW2, row2H);
            _cardActions.SetBounds(pad + cardW2 + gap, y, availableWidth - cardW2 - gap, row2H);
            y += row2H + 20;

            // Row 3: Recent Companies
            int row3H = 320;
            _cardRecent.SetBounds(pad, y, availableWidth, row3H);
            y += row3H + 30;

            _stack.Height = y;
        }

        private async Task LoadAsync()
        {
            try
            {
                var d = await _api.GetDashboardAsync();
                if (d == null) return;

                _cardCompanies.Value = d.TotalCompanies.ToString("N0");
                _cardCompanies.DeltaText = $"{d.ActiveCompanies} active companies";

                _cardSubscriptions.Value = d.ActiveSubscriptions.ToString("N0");
                _cardSubscriptions.DeltaText = $"{d.ActiveSubscriptions} active subscriptions";

                _cardMrr.Value = $"PHP {d.MonthlyRecurringRevenue:N0}";
                _cardMrr.DeltaText = "Estimated monthly";

                _cardUsers.Value = d.TotalUsers.ToString("N0");
                _cardUsers.DeltaText = "Registered platform users";

                // Plans grid
                var planRows = d.PlanDistribution.Select(p => new
                {
                    p.PlanName,
                    PriceText = $"PHP {p.PricePerMonth:N0}/mo",
                    CompanyCount = $"{p.CompanyCount} tenants",
                    RevenueText = $"PHP {p.TotalRevenue:N0}"
                }).ToList();
                _dgvPlans.DataSource = null;
                _dgvPlans.DataSource = planRows;

                // Recent companies grid
                var compRows = d.RecentCompanies.Select(c => new
                {
                    c.CompanyCode,
                    c.CompanyName,
                    c.PlanName,
                    c.StatusText,
                    JoinedDateText = c.CreatedAt.ToString("MMM dd, yyyy")
                }).ToList();
                _dgvRecent.DataSource = null;
                _dgvRecent.DataSource = compRows;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Super Admin Dashboard error:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
