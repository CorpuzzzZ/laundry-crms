using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRM.UI;
using CRM.UI.Controls;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.WinForms.Forms
{
    public class CrewDashboardControl : UserControl
    {
        private readonly DashboardApiService _api = new(ApiClient.Instance);
        private CrewDashboardModel? _dashboardData;

        // Header
        private Panel _pnlHeader = null!;
        private Label _lblTitle = null!;
        private Label _lblRange = null!;
        private Panel _pnlDateRange = null!;
        private Button _btnRefresh = null!;

        // Layout hosts
        private Panel _scroller = null!;
        private Panel _stack = null!;

        // Row 1: Clickable Operational KPI cards
        private DashboardMetricCard _cardTodayOrders = null!;
        private DashboardMetricCard _cardPending = null!;
        private DashboardMetricCard _cardReady = null!;
        private DashboardMetricCard _cardPickedUp = null!;

        // Row 2: Revenue + Loyalty cards
        private DashboardCard _cardRevenue = null!;
        private Label _lblRevenue = null!;
        private Label _lblRevenueSub = null!;
        private Button _btnRevenueDetails = null!;

        private DashboardCard _cardLoyalty = null!;
        private DashboardMiniStat _statIssued = null!;
        private DashboardMiniStat _statRedeemed = null!;
        private DashboardMiniStat _statDiscount = null!;
        private Button _btnLoyaltyDetails = null!;

        // Row 3: Charts
        private DashboardCard _cardChartHourly = null!;
        private DashboardCard _cardChartStatus = null!;
        private ScottPlot.WinForms.FormsPlot _plotHourly = null!;
        private ScottPlot.WinForms.FormsPlot _plotStatus = null!;

        // Row 4: 7-Day Revenue Trend
        private DashboardCard _cardChartRev7d = null!;
        private ScottPlot.WinForms.FormsPlot _plotRevenue7d = null!;
        private Label _lblRev7dTotal = null!;

        // Row 5: Recent Orders
        private DashboardCard _cardRecent = null!;
        private DataGridView _dgvRecent = null!;

        public CrewDashboardControl()
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
            // 1. TOP HEADER (Padding: 24px left/right, 14px top)
            // ============================================================
            _pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 74,
                BackColor = Colors.Background,
                Padding = new Padding(24, 14, 24, 0)
            };

            _lblTitle = new Label
            {
                Text = "Crew Operations Dashboard",
                Left = 24, Top = 12,
                AutoSize = true,
                Font = Typography.H1,
                ForeColor = Colors.TextPrimary
            };
            _pnlHeader.Controls.Add(_lblTitle);

            // Date Range Pill Badge
            _pnlDateRange = new Panel
            {
                Left = 24, Top = 42,
                Height = 24,
                BackColor = Colors.Surface,
                Cursor = Cursors.Default
            };
            _pnlDateRange.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, _pnlDateRange.Width - 1, _pnlDateRange.Height - 1);
                using var path = DashboardCard.GetRoundedPath(r, 6);
                using var bg = new SolidBrush(Colors.Surface);
                g.FillPath(bg, path);
                using var pen = new Pen(Colors.Border, 1f);
                g.DrawPath(pen, path);
            };

            var lblCalIcon = new Label
            {
                Text = "📅",
                Font = new Font(Typography.Family, 8f),
                ForeColor = Colors.TextSecondary,
                Location = new Point(6, 3),
                AutoSize = true
            };
            _pnlDateRange.Controls.Add(lblCalIcon);

            _lblRange = new Label
            {
                Text = "Today · Live Operations Feed",
                Left = 24, Top = 3,
                AutoSize = true,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary
            };
            _pnlDateRange.Controls.Add(_lblRange);
            _pnlDateRange.Width = _lblRange.Right + 8;
            _pnlHeader.Controls.Add(_pnlDateRange);

            // Refresh Button
            _btnRefresh = new Button
            {
                Text = "⟳  Refresh",
                Width = 110, Height = 36,
                Top = 18,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnRefresh.FlatAppearance.BorderSize = 1;
            _btnRefresh.FlatAppearance.BorderColor = Colors.Border;
            _btnRefresh.MouseEnter += (s, e) =>
            {
                _btnRefresh.BackColor = Colors.PrimaryLight;
                _btnRefresh.ForeColor = Colors.Primary;
                _btnRefresh.FlatAppearance.BorderColor = Colors.Primary;
            };
            _btnRefresh.MouseLeave += (s, e) =>
            {
                _btnRefresh.BackColor = Colors.Surface;
                _btnRefresh.ForeColor = Colors.TextPrimary;
                _btnRefresh.FlatAppearance.BorderColor = Colors.Border;
            };
            _btnRefresh.Click += async (s, e) => await LoadAsync();
            _pnlHeader.Controls.Add(_btnRefresh);

            _pnlHeader.Resize += (s, e) =>
            {
                _btnRefresh.Left = _pnlHeader.ClientSize.Width - 24 - _btnRefresh.Width;
            };

            // ============================================================
            // 2. SCROLLABLE CONTAINER & STACK
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
                Height = 1450
            };
            _scroller.Controls.Add(_stack);

            // ============================================================
            // 3. ROW 1: 4 CLICKABLE OPERATIONAL KPI CARDS
            // ============================================================
            _cardTodayOrders = new DashboardMetricCard
            {
                Caption = "Today's Orders",
                Value = "0",
                DeltaText = "👆 Click for details ↗",
                DeltaIsPositive = null,
                IconText = "📦",
                IconBgColor = Colors.PrimaryLight,
                IconFgColor = Colors.Primary,
                AccentColor = Colors.Primary,
                CornerRadius = 12,
                Cursor = Cursors.Hand
            };
            _cardTodayOrders.Click += (s, e) => OpenKpiDetails(CrewKpiType.TodayOrders);
            _stack.Controls.Add(_cardTodayOrders);

            _cardPending = new DashboardMetricCard
            {
                Caption = "Pending Queue",
                Value = "0",
                DeltaText = "👆 In queue · Click ↗",
                DeltaIsPositive = false,
                IconText = "⏳",
                IconBgColor = Colors.WarningLight,
                IconFgColor = Colors.Warning,
                AccentColor = Colors.Warning,
                CornerRadius = 12,
                Cursor = Cursors.Hand
            };
            _cardPending.Click += (s, e) => OpenKpiDetails(CrewKpiType.PendingOrders);
            _stack.Controls.Add(_cardPending);

            _cardReady = new DashboardMetricCard
            {
                Caption = "Ready for Pickup",
                Value = "0",
                DeltaText = "👆 Waiting · Click ↗",
                DeltaIsPositive = true,
                IconText = "✅",
                IconBgColor = Colors.SuccessLight,
                IconFgColor = Colors.Success,
                AccentColor = Colors.Success,
                CornerRadius = 12,
                Cursor = Cursors.Hand
            };
            _cardReady.Click += (s, e) => OpenKpiDetails(CrewKpiType.ReadyForPickup);
            _stack.Controls.Add(_cardReady);

            _cardPickedUp = new DashboardMetricCard
            {
                Caption = "Picked Up Today",
                Value = "0",
                DeltaText = "👆 Completed · Click ↗",
                DeltaIsPositive = true,
                IconText = "🚚",
                IconBgColor = Color.FromArgb(238, 235, 255),
                IconFgColor = Color.FromArgb(108, 92, 231),
                AccentColor = Color.FromArgb(108, 92, 231),
                CornerRadius = 12,
                Cursor = Cursors.Hand
            };
            _cardPickedUp.Click += (s, e) => OpenKpiDetails(CrewKpiType.PickedUp);
            _stack.Controls.Add(_cardPickedUp);

            // ============================================================
            // 4. ROW 2: REVENUE CARD & LOYALTY CARD (Clickable Drilldowns)
            // ============================================================
            _cardRevenue = new DashboardCard
            {
                CornerRadius = 12,
                Padding = new Padding(20, 16, 20, 16),
                EnableHover = true,
                Cursor = Cursors.Hand
            };
            var hdrRev = MakeCardHeader("Today's Revenue", "Gross payments collected today", "Revenue Drilldown ↗", out _);
            _cardRevenue.Controls.Add(hdrRev);

            var revBody = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(0, 10, 0, 0)
            };

            _lblRevenue = new Label
            {
                Text = "PHP 0.00",
                Font = new Font(Typography.Family, 24f, FontStyle.Bold),
                ForeColor = Colors.Success,
                Location = new Point(0, 12),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            revBody.Controls.Add(_lblRevenue);

            _lblRevenueSub = new Label
            {
                Text = "0 payments · avg PHP 0.00",
                Font = Typography.Body,
                ForeColor = Colors.TextBody,
                Location = new Point(0, 56),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            revBody.Controls.Add(_lblRevenueSub);

            _btnRevenueDetails = new Button
            {
                Text = "📊  View Full Payment Breakdown ↗",
                Height = 32,
                Location = new Point(0, 88),
                Width = 260,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.PrimaryLight,
                ForeColor = Colors.Primary,
                Font = Typography.SmallBold,
                Cursor = Cursors.Hand
            };
            _btnRevenueDetails.FlatAppearance.BorderSize = 0;
            _btnRevenueDetails.Click += (s, e) => OpenKpiDetails(CrewKpiType.Revenue);
            revBody.Controls.Add(_btnRevenueDetails);

            _cardRevenue.Controls.Add(revBody);
            revBody.BringToFront();
            _cardRevenue.Click += (s, e) => OpenKpiDetails(CrewKpiType.Revenue);
            _stack.Controls.Add(_cardRevenue);

            // Loyalty Card
            _cardLoyalty = new DashboardCard
            {
                CornerRadius = 12,
                Padding = new Padding(20, 16, 20, 16),
                EnableHover = true,
                Cursor = Cursors.Hand
            };
            var hdrLoy = MakeCardHeader("Today's Loyalty Activity", "Points issued, redeemed & discounts", "Loyalty Drilldown ↗", out _);
            _cardLoyalty.Controls.Add(hdrLoy);

            var loyBody = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(0, 10, 0, 0)
            };

            _statIssued = new DashboardMiniStat
            {
                Caption = "Points Issued",
                Value = "0",
                Icon = "⭐",
                AccentColor = Colors.Primary,
                CornerRadius = 8
            };
            loyBody.Controls.Add(_statIssued);

            _statRedeemed = new DashboardMiniStat
            {
                Caption = "Points Redeemed",
                Value = "0",
                Icon = "🎁",
                AccentColor = Colors.Success,
                CornerRadius = 8
            };
            loyBody.Controls.Add(_statRedeemed);

            _statDiscount = new DashboardMiniStat
            {
                Caption = "Discount Given",
                Value = "PHP 0.00",
                Icon = "🏷️",
                AccentColor = Colors.Warning,
                CornerRadius = 8
            };
            loyBody.Controls.Add(_statDiscount);

            _btnLoyaltyDetails = new Button
            {
                Text = "⭐  View Full Loyalty Breakdown ↗",
                Height = 32,
                Location = new Point(0, 78),
                Width = 260,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(238, 235, 255),
                ForeColor = Color.FromArgb(108, 92, 231),
                Font = Typography.SmallBold,
                Cursor = Cursors.Hand
            };
            _btnLoyaltyDetails.FlatAppearance.BorderSize = 0;
            _btnLoyaltyDetails.Click += (s, e) => OpenKpiDetails(CrewKpiType.Loyalty);
            loyBody.Controls.Add(_btnLoyaltyDetails);

            _cardLoyalty.Controls.Add(loyBody);
            loyBody.BringToFront();
            _cardLoyalty.Click += (s, e) => OpenKpiDetails(CrewKpiType.Loyalty);
            _stack.Controls.Add(_cardLoyalty);

            // ============================================================
            // 5. ROW 3: CHARTS (Hourly traffic & Orders by status)
            // ============================================================
            _cardChartHourly = new DashboardCard
            {
                CornerRadius = 12,
                Padding = new Padding(20, 16, 20, 16)
            };
            var hdrHourly = MakeCardHeader("Hourly Order Traffic (Today)", "Orders created by hour today", "Traffic Chart", out _);
            _cardChartHourly.Controls.Add(hdrHourly);

            var hourlyPlotHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(0, 10, 0, 0)
            };
            _plotHourly = new ScottPlot.WinForms.FormsPlot
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface
            };
            hourlyPlotHost.Controls.Add(_plotHourly);
            _cardChartHourly.Controls.Add(hourlyPlotHost);
            hourlyPlotHost.BringToFront();
            _stack.Controls.Add(_cardChartHourly);

            _cardChartStatus = new DashboardCard
            {
                CornerRadius = 12,
                Padding = new Padding(20, 16, 20, 16)
            };
            var hdrStatus = MakeCardHeader("Orders by Status (Today)", "Operational pipeline status breakdown", "Status Chart", out _);
            _cardChartStatus.Controls.Add(hdrStatus);

            var statusPlotHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(0, 10, 0, 0)
            };
            _plotStatus = new ScottPlot.WinForms.FormsPlot
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface
            };
            statusPlotHost.Controls.Add(_plotStatus);
            _cardChartStatus.Controls.Add(statusPlotHost);
            statusPlotHost.BringToFront();
            _stack.Controls.Add(_cardChartStatus);

            // ============================================================
            // 6. ROW 4: 7-DAY REVENUE TREND
            // ============================================================
            _cardChartRev7d = new DashboardCard
            {
                CornerRadius = 12,
                Padding = new Padding(20, 16, 20, 16)
            };
            var hdrRev7d = MakeCardHeader("Revenue Trend (Last 7 Days)", "Daily revenue performance in PHP", "7-Day History", out _lblRev7dTotal);
            _cardChartRev7d.Controls.Add(hdrRev7d);

            var rev7dPlotHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(0, 10, 0, 0)
            };
            _plotRevenue7d = new ScottPlot.WinForms.FormsPlot
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface
            };
            rev7dPlotHost.Controls.Add(_plotRevenue7d);
            _cardChartRev7d.Controls.Add(rev7dPlotHost);
            rev7dPlotHost.BringToFront();
            _stack.Controls.Add(_cardChartRev7d);

            // ============================================================
            // 7. ROW 5: RECENT ORDERS TABLE
            // ============================================================
            _cardRecent = new DashboardCard
            {
                CornerRadius = 12,
                Padding = new Padding(20, 16, 20, 16)
            };
            var hdrRecent = MakeCardHeader("Recent Orders (Today)", "Latest incoming orders · Double-click to open order details", "Live Orders", out _);
            _cardRecent.Controls.Add(hdrRecent);

            var recentGridHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(0, 12, 0, 0)
            };

            _dgvRecent = new DataGridView
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
                },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(252, 253, 255),
                    ForeColor = Colors.TextBody,
                    SelectionBackColor = Colors.PrimaryLight,
                    SelectionForeColor = Colors.TextPrimary,
                    Font = Typography.Body,
                    Padding = new Padding(12, 0, 12, 0)
                }
            };

            _dgvRecent.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Order #",
                DataPropertyName = "OrderNumber",
                FillWeight = 18
            });
            _dgvRecent.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Customer Name",
                DataPropertyName = "CustomerName",
                FillWeight = 32
            });
            _dgvRecent.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Status",
                DataPropertyName = "StatusName",
                FillWeight = 18,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = Typography.SmallBold
                }
            });
            _dgvRecent.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Total Amount",
                DataPropertyName = "TotalAmountText",
                FillWeight = 20,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Font = Typography.BodyBold,
                    ForeColor = Colors.TextPrimary
                }
            });
            _dgvRecent.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Time",
                DataPropertyName = "OrderDateText",
                FillWeight = 12,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    ForeColor = Colors.TextSecondary
                }
            });

            _dgvRecent.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && _dgvRecent.CurrentRow?.DataBoundItem is RecentOrderModel r)
                {
                    OpenOrderDetails(r.OrderId);
                }
            };

            recentGridHost.Controls.Add(_dgvRecent);
            _cardRecent.Controls.Add(recentGridHost);
            recentGridHost.BringToFront();
            _stack.Controls.Add(_cardRecent);

            // Assembly
            Controls.Add(_scroller);
            Controls.Add(_pnlHeader);

            _scroller.Resize += (s, e) => ArrangeLayout();
        }

        private static Panel MakeCardHeader(string title, string subtitle, string badgeText, out Label badgeLabel)
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

            badgeLabel = badge;
            return pnl;
        }

        private void ArrangeLayout()
        {
            if (_scroller == null || _stack == null || _cardTodayOrders == null) return;

            int pad = 24;
            int gap = 16;
            int availableWidth = Math.Max(780, _scroller.ClientSize.Width - (pad * 2));
            _stack.Width = _scroller.ClientSize.Width;

            int y = 8;

            // Row 1: 4 Operational KPI Cards
            int cardW4 = (availableWidth - (3 * gap)) / 4;
            int cardH1 = 120;

            _cardTodayOrders.SetBounds(pad, y, cardW4, cardH1);
            _cardPending.SetBounds(pad + (cardW4 + gap), y, cardW4, cardH1);
            _cardReady.SetBounds(pad + (cardW4 + gap) * 2, y, cardW4, cardH1);
            _cardPickedUp.SetBounds(pad + (cardW4 + gap) * 3, y, availableWidth - (cardW4 + gap) * 3, cardH1);
            y += cardH1 + 20;

            // Row 2: Revenue Card & Loyalty Card (50/50)
            int cardW2 = (availableWidth - gap) / 2;
            int row2H = 190;
            _cardRevenue.SetBounds(pad, y, cardW2, row2H);
            _cardLoyalty.SetBounds(pad + cardW2 + gap, y, availableWidth - cardW2 - gap, row2H);

            // Loyalty mini-stats inside loyalty card
            if (_statIssued != null && _statRedeemed != null && _statDiscount != null && _btnLoyaltyDetails != null)
            {
                int statGap = 10;
                int innerW = _cardLoyalty.ClientSize.Width - 40;
                int statW = (innerW - (2 * statGap)) / 3;
                int statH = 54;
                int statY = 10;

                _statIssued.SetBounds(0, statY, statW, statH);
                _statRedeemed.SetBounds(statW + statGap, statY, statW, statH);
                _statDiscount.SetBounds((statW + statGap) * 2, statY, innerW - (statW + statGap) * 2, statH);
                _btnLoyaltyDetails.Top = statY + statH + 12;
            }

            y += row2H + 20;

            // Row 3: Hourly Traffic & Orders by Status Charts (50/50)
            int chartH = 260;
            _cardChartHourly.SetBounds(pad, y, cardW2, chartH);
            _cardChartStatus.SetBounds(pad + cardW2 + gap, y, availableWidth - cardW2 - gap, chartH);
            y += chartH + 20;

            // Row 4: 7-Day Revenue Trend (Full width)
            int rev7dH = 260;
            _cardChartRev7d.SetBounds(pad, y, availableWidth, rev7dH);
            y += rev7dH + 20;

            // Row 5: Recent Orders (Full width)
            int recentH = 320;
            _cardRecent.SetBounds(pad, y, availableWidth, recentH);
            y += recentH + 28;

            _stack.Height = y;
        }

        private async Task LoadAsync()
        {
            try
            {
                var localFrom = DateTime.Today;
                var localTo = localFrom.AddDays(1);
                var fromUtc = localFrom.ToUniversalTime();
                var toUtc = localTo.ToUniversalTime();

                _lblRange.Text = $"Today · {localFrom:dddd, MMMM dd, yyyy}";
                _pnlDateRange.Width = _lblRange.Right + 8;

                var d = await _api.GetCrewDashboardAsync(fromUtc, toUtc);
                if (d == null)
                {
                    MessageBox.Show("Failed to load dashboard data.",
                        "Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _dashboardData = d;

                // Operational KPIs
                _cardTodayOrders.Value = d.TodayOrderCount.ToString("N0");
                _cardPending.Value = d.PendingOrderCount.ToString("N0");
                _cardReady.Value = d.ReadyOrderCount.ToString("N0");
                _cardPickedUp.Value = d.PickedUpCount.ToString("N0");

                // Revenue
                _lblRevenue.Text = $"PHP {d.TotalRevenue:N2}";
                _lblRevenueSub.Text = $"{d.PaymentCount} payment(s) · avg PHP {d.AveragePayment:N2}";

                // Loyalty
                _statIssued.Value = d.PointsIssued.ToString("N0");
                _statRedeemed.Value = d.PointsRedeemed.ToString("N0");
                _statDiscount.Value = $"PHP {d.LoyaltyDiscountGiven:N2}";

                // Recent Orders
                _dgvRecent.DataSource = null;
                _dgvRecent.DataSource = d.RecentOrders;

                // Render Charts
                RenderCharts(d);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dashboard error:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RenderCharts(CrewDashboardModel d)
        {
            // ---- Chart 1: Orders per hour (today) ----
            try
            {
                var p1 = _plotHourly.Plot;
                p1.Clear();
                p1.FigureBackground.Color = new ScottPlot.Color(255, 255, 255);
                p1.DataBackground.Color = new ScottPlot.Color(255, 255, 255);
                p1.Grid.MajorLineColor = new ScottPlot.Color(242, 244, 247);
                p1.Axes.Color(new ScottPlot.Color(160, 174, 192));
                p1.Axes.Title.Label.Text = "";

                var hours = d.OrdersByHour.OrderBy(h => h.Hour).ToArray();
                if (hours.Length > 0)
                {
                    var positions = hours.Select(h => (double)h.Hour).ToArray();
                    var values = hours.Select(h => (double)h.Count).ToArray();
                    var bars = p1.Add.Bars(positions, values);
                    foreach (var bar in bars.Bars)
                    {
                        bar.FillColor = new ScottPlot.Color(52, 152, 219);
                        bar.LineWidth = 0;
                    }
                    p1.Axes.Bottom.Label.Text = "Hour of Day";
                    p1.Axes.Left.Label.Text = "Orders";
                }
                _plotHourly.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Chart.Hourly] {ex}");
            }

            // ---- Chart 2: Orders by status (today) ----
            try
            {
                var p2 = _plotStatus.Plot;
                p2.Clear();
                p2.FigureBackground.Color = new ScottPlot.Color(255, 255, 255);
                p2.DataBackground.Color = new ScottPlot.Color(255, 255, 255);
                p2.Grid.MajorLineColor = new ScottPlot.Color(242, 244, 247);
                p2.Axes.Color(new ScottPlot.Color(160, 174, 192));
                p2.Axes.Title.Label.Text = "";

                var statuses = d.OrdersByStatus.ToArray();
                if (statuses.Length > 0)
                {
                    var positions = Enumerable.Range(0, statuses.Length).Select(i => (double)i).ToArray();
                    var values = statuses.Select(s => (double)s.Count).ToArray();
                    var bars = p2.Add.Bars(positions, values);

                    var palette = new[]
                    {
                        new ScottPlot.Color(52, 152, 219),
                        new ScottPlot.Color(243, 156, 18),
                        new ScottPlot.Color(46, 204, 113),
                        new ScottPlot.Color(108, 92, 231),
                        new ScottPlot.Color(231, 76, 60)
                    };

                    for (int i = 0; i < bars.Bars.Count; i++)
                    {
                        bars.Bars[i].FillColor = palette[i % palette.Length];
                        bars.Bars[i].LineWidth = 0;
                    }

                    p2.Axes.Bottom.Label.Text = "Status";
                    p2.Axes.Left.Label.Text = "Orders";
                    p2.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                        positions,
                        statuses.Select(s => s.StatusName).ToArray());
                }
                _plotStatus.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Chart.Status] {ex}");
            }

            // ---- Chart 3: Revenue — last 7 days (bar) ----
            try
            {
                var p3 = _plotRevenue7d.Plot;
                p3.Clear();
                p3.FigureBackground.Color = new ScottPlot.Color(255, 255, 255);
                p3.DataBackground.Color = new ScottPlot.Color(255, 255, 255);
                p3.Grid.MajorLineColor = new ScottPlot.Color(242, 244, 247);
                p3.Axes.Color(new ScottPlot.Color(160, 174, 192));
                p3.Axes.Title.Label.Text = "";

                var days = d.RevenueByDay.OrderBy(x => x.Day).ToArray();
                decimal rev7dTotal = 0;
                if (days.Length > 0)
                {
                    rev7dTotal = days.Sum(x => x.Revenue);
                    var positions = Enumerable.Range(0, days.Length).Select(i => (double)i).ToArray();
                    var values = days.Select(x => (double)x.Revenue).ToArray();
                    var bars = p3.Add.Bars(positions, values);
                    foreach (var bar in bars.Bars)
                    {
                        bar.FillColor = new ScottPlot.Color(46, 204, 113);
                        bar.LineWidth = 0;
                    }

                    p3.Axes.Bottom.Label.Text = "Day";
                    p3.Axes.Left.Label.Text = "Revenue (PHP)";
                    p3.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                        positions,
                        days.Select(x => x.Day.ToLocalTime().ToString("MMM dd")).ToArray());
                }
                _lblRev7dTotal.Text = $"7-Day Total: PHP {rev7dTotal:N0}";
                _plotRevenue7d.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Chart.Revenue] {ex}");
            }
        }

        private void OpenKpiDetails(CrewKpiType kpiType)
        {
            if (_dashboardData == null) return;

            using var dlg = new CrewKpiDetailDialog(kpiType, _dashboardData);
            dlg.ShowDialog(FindForm());
        }

        private void OpenOrderDetails(int orderId)
        {
            try
            {
                var detailsForm = new Form
                {
                    Text = $"Order #{orderId} Details",
                    Size = new Size(920, 680),
                    StartPosition = FormStartPosition.CenterParent,
                    ShowIcon = false
                };
                var view = new OrderDetailsView(orderId) { Dock = DockStyle.Fill };
                view.Cancelled += (s, e) => detailsForm.Close();
                detailsForm.Controls.Add(view);
                detailsForm.ShowDialog(FindForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open order details:\n\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
