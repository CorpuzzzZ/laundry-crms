using System;
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
    public class AdminDashboardControl : UserControl
    {
        private readonly DashboardApiService _api = new(ApiClient.Instance);

        // Header
        private Panel _pnlHeader = null!;
        private Label _lblTitle = null!;
        private Label _lblRange = null!;
        private Panel _pnlDateRange = null!;
        private Button _btnRefresh = null!;

        // Layout hosts
        private Panel _scroller = null!;
        private Panel _stack = null!;

        // Row 1: KPI Metric Cards
        private DashboardMetricCard _cardRevenue = null!;
        private DashboardMetricCard _cardOrders = null!;
        private DashboardMetricCard _cardCustomers = null!;
        private DashboardMetricCard _cardAov = null!;

        // Row 2: Charts
        private DashboardCard _cardChartRev = null!;
        private DashboardCard _cardChartOrd = null!;
        private Label _lblRevTotal = null!;
        private Label _lblOrdTotal = null!;
        private ScottPlot.WinForms.FormsPlot _plotRevenue = null!;
        private ScottPlot.WinForms.FormsPlot _plotOrders = null!;

        // Row 3: Top Performers
        private DashboardCard _cardTopCust = null!;
        private DashboardCard _cardTopSvc = null!;
        private DataGridView _dgvTopCustomers = null!;
        private DataGridView _dgvTopServices = null!;

        // Row 4: Loyalty Program
        private DashboardCard _cardLoyalty = null!;
        private DashboardMiniStat _statIssued = null!;
        private DashboardMiniStat _statRedeemed = null!;
        private DashboardMiniStat _statRate = null!;
        private ScottPlot.WinForms.FormsPlot _plotTiers = null!;

        public AdminDashboardControl()
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
            // 1. TOP HEADER (Padding: 24px left/right, 16px top)
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
                Text = "Admin Dashboard",
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
                Text = "Last 30 days vs previous 30 days",
                Left = 24, Top = 3,
                AutoSize = true,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary
            };
            _pnlDateRange.Controls.Add(_lblRange);
            _pnlDateRange.Width = _lblRange.Right + 8;
            _pnlHeader.Controls.Add(_pnlDateRange);

            // Refresh Button (Modern rounded button)
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
                Height = 1200
            };
            _scroller.Controls.Add(_stack);

            // ============================================================
            // 3. ROW 1: 4 KPI METRIC CARDS
            // ============================================================
            _cardRevenue = new DashboardMetricCard
            {
                Caption = "Revenue (30d)",
                Value = "PHP 0",
                DeltaText = "Calculating...",
                DeltaIsPositive = null,
                IconText = "₱",
                IconBgColor = Colors.SuccessLight,
                IconFgColor = Colors.Success,
                AccentColor = Colors.Success,
                CornerRadius = 12
            };
            _stack.Controls.Add(_cardRevenue);

            _cardOrders = new DashboardMetricCard
            {
                Caption = "Orders (30d)",
                Value = "0",
                DeltaText = "Calculating...",
                DeltaIsPositive = null,
                IconText = "📦",
                IconBgColor = Colors.PrimaryLight,
                IconFgColor = Colors.Primary,
                AccentColor = Colors.Primary,
                CornerRadius = 12
            };
            _stack.Controls.Add(_cardOrders);

            _cardCustomers = new DashboardMetricCard
            {
                Caption = "New Customers",
                Value = "0",
                DeltaText = "Calculating...",
                DeltaIsPositive = null,
                IconText = "👥",
                IconBgColor = Color.FromArgb(238, 235, 255),
                IconFgColor = Color.FromArgb(108, 92, 231),
                AccentColor = Color.FromArgb(108, 92, 231),
                CornerRadius = 12
            };
            _stack.Controls.Add(_cardCustomers);

            _cardAov = new DashboardMetricCard
            {
                Caption = "Avg Order Value",
                Value = "PHP 0.00",
                DeltaText = "Calculating...",
                DeltaIsPositive = null,
                IconText = "🏷️",
                IconBgColor = Colors.WarningLight,
                IconFgColor = Colors.Warning,
                AccentColor = Colors.Warning,
                CornerRadius = 12
            };
            _stack.Controls.Add(_cardAov);

            // ============================================================
            // 4. ROW 2: 2 CHARTS (Revenue Trend & Orders Volume)
            // ============================================================
            _cardChartRev = new DashboardCard
            {
                CornerRadius = 12,
                Padding = new Padding(20, 16, 20, 16)
            };
            var hdrRev = MakeCardHeader("Revenue Trend (30 Days)", "Daily gross revenue performance", "30-Day Trend", out _lblRevTotal);
            _cardChartRev.Controls.Add(hdrRev);

            var revPlotHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(0, 10, 0, 0)
            };
            _plotRevenue = new ScottPlot.WinForms.FormsPlot
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface
            };
            revPlotHost.Controls.Add(_plotRevenue);
            _cardChartRev.Controls.Add(revPlotHost);
            revPlotHost.BringToFront();
            _stack.Controls.Add(_cardChartRev);

            _cardChartOrd = new DashboardCard
            {
                CornerRadius = 12,
                Padding = new Padding(20, 16, 20, 16)
            };
            var hdrOrd = MakeCardHeader("Order Volume (30 Days)", "Daily completed and active orders", "30-Day Volume", out _lblOrdTotal);
            _cardChartOrd.Controls.Add(hdrOrd);

            var ordPlotHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(0, 10, 0, 0)
            };
            _plotOrders = new ScottPlot.WinForms.FormsPlot
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface
            };
            ordPlotHost.Controls.Add(_plotOrders);
            _cardChartOrd.Controls.Add(ordPlotHost);
            ordPlotHost.BringToFront();
            _stack.Controls.Add(_cardChartOrd);

            // ============================================================
            // 5. ROW 3: TOP PERFORMERS (Customers & Services)
            // ============================================================
            _cardTopCust = new DashboardCard
            {
                CornerRadius = 12,
                Padding = new Padding(20, 16, 20, 16)
            };
            var hdrCust = MakeCardHeader("Top Customers", "Highest spending customers in this period", "Top 5 Spenders", out _);
            _cardTopCust.Controls.Add(hdrCust);

            var gridCustHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(0, 12, 0, 0)
            };
            _dgvTopCustomers = MakeGrid();
            _dgvTopCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Customer Name",
                DataPropertyName = "CustomerName",
                FillWeight = 52
            });
            _dgvTopCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Orders",
                DataPropertyName = "OrderCount",
                FillWeight = 20,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });
            _dgvTopCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Revenue",
                DataPropertyName = "TotalRevenueText",
                FillWeight = 28,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Font = Typography.BodyBold,
                    ForeColor = Colors.TextPrimary
                }
            });
            gridCustHost.Controls.Add(_dgvTopCustomers);
            _cardTopCust.Controls.Add(gridCustHost);
            gridCustHost.BringToFront();
            _stack.Controls.Add(_cardTopCust);

            _cardTopSvc = new DashboardCard
            {
                CornerRadius = 12,
                Padding = new Padding(20, 16, 20, 16)
            };
            var hdrSvc = MakeCardHeader("Top Services", "Most requested laundry services by volume", "Top 5 Services", out _);
            _cardTopSvc.Controls.Add(hdrSvc);

            var gridSvcHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(0, 12, 0, 0)
            };
            _dgvTopServices = MakeGrid();
            _dgvTopServices.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Service Name",
                DataPropertyName = "ServiceName",
                FillWeight = 52
            });
            _dgvTopServices.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Items",
                DataPropertyName = "OrderItemCount",
                FillWeight = 20,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });
            _dgvTopServices.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Revenue",
                DataPropertyName = "RevenueText",
                FillWeight = 28,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Font = Typography.BodyBold,
                    ForeColor = Colors.TextPrimary
                }
            });
            gridSvcHost.Controls.Add(_dgvTopServices);
            _cardTopSvc.Controls.Add(gridSvcHost);
            gridSvcHost.BringToFront();
            _stack.Controls.Add(_cardTopSvc);

            // ============================================================
            // 6. ROW 4: LOYALTY PROGRAM HEALTH
            // ============================================================
            _cardLoyalty = new DashboardCard
            {
                CornerRadius = 12,
                Padding = new Padding(20, 16, 20, 16)
            };
            var hdrLoy = MakeCardHeader("Loyalty Program Health", "Points velocity, redemption rate, and tier distribution", "Loyalty Analytics", out _);
            _cardLoyalty.Controls.Add(hdrLoy);

            _statIssued = new DashboardMiniStat
            {
                Caption = "Points Issued",
                Value = "0",
                Icon = "⭐",
                AccentColor = Colors.Primary,
                CornerRadius = 8
            };
            _cardLoyalty.Controls.Add(_statIssued);

            _statRedeemed = new DashboardMiniStat
            {
                Caption = "Points Redeemed",
                Value = "0",
                Icon = "🎁",
                AccentColor = Colors.Success,
                CornerRadius = 8
            };
            _cardLoyalty.Controls.Add(_statRedeemed);

            _statRate = new DashboardMiniStat
            {
                Caption = "Redemption Rate",
                Value = "0.0%",
                Icon = "🔄",
                AccentColor = Colors.Warning,
                CornerRadius = 8
            };
            _cardLoyalty.Controls.Add(_statRate);

            _plotTiers = new ScottPlot.WinForms.FormsPlot
            {
                BackColor = Colors.Surface
            };
            _cardLoyalty.Controls.Add(_plotTiers);
            _stack.Controls.Add(_cardLoyalty);

            // Controls hierarchy
            Controls.Add(_scroller);
            Controls.Add(_pnlHeader);

            // Resize event to trigger fluid layout calculation
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
            badgeLabel = badge;

            pnl.Resize += (s, e) =>
            {
                badge.Left = Math.Max(0, pnl.ClientSize.Width - badge.Width);
            };

            pnl.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.BorderLight, 1f);
                e.Graphics.DrawLine(pen, 0, pnl.Height - 1, pnl.Width, pnl.Height - 1);
            };

            return pnl;
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

        private void ArrangeLayout()
        {
            if (_scroller == null || _stack == null || _cardRevenue == null) return;

            int pad = 24;
            int gap = 16;
            int availableWidth = Math.Max(780, _scroller.ClientSize.Width - (pad * 2));
            _stack.Width = _scroller.ClientSize.Width;

            int y = 8;

            // Row 1: 4 Metric Cards
            int cardW4 = (availableWidth - (3 * gap)) / 4;
            int cardH1 = 120;

            _cardRevenue.SetBounds(pad, y, cardW4, cardH1);
            _cardOrders.SetBounds(pad + (cardW4 + gap), y, cardW4, cardH1);
            _cardCustomers.SetBounds(pad + (cardW4 + gap) * 2, y, cardW4, cardH1);
            _cardAov.SetBounds(pad + (cardW4 + gap) * 3, y, availableWidth - (cardW4 + gap) * 3, cardH1);
            y += cardH1 + 20;

            // Row 2: 2 Chart Cards
            int cardW2 = (availableWidth - gap) / 2;
            int chartH = 300;
            _cardChartRev.SetBounds(pad, y, cardW2, chartH);
            _cardChartOrd.SetBounds(pad + cardW2 + gap, y, availableWidth - cardW2 - gap, chartH);
            y += chartH + 20;

            // Row 3: 2 Table Cards
            int tableH = 290;
            _cardTopCust.SetBounds(pad, y, cardW2, tableH);
            _cardTopSvc.SetBounds(pad + cardW2 + gap, y, availableWidth - cardW2 - gap, tableH);
            y += tableH + 20;

            // Row 4: Loyalty Health Card
            int loyH = 360;
            _cardLoyalty.SetBounds(pad, y, availableWidth, loyH);

            // Mini stats inside loyalty card
            if (_statIssued != null && _statRedeemed != null && _statRate != null && _plotTiers != null)
            {
                int statPad = 20;
                int statGap = 12;
                int statAvailW = _cardLoyalty.Width - (statPad * 2);
                int statW = (statAvailW - (2 * statGap)) / 3;
                int statH = 58;
                int statY = 66;

                _statIssued.SetBounds(statPad, statY, statW, statH);
                _statRedeemed.SetBounds(statPad + statW + statGap, statY, statW, statH);
                _statRate.SetBounds(statPad + (statW + statGap) * 2, statY, statAvailW - (statW + statGap) * 2, statH);

                int chartY = statY + statH + 14;
                _plotTiers.SetBounds(statPad, chartY, statAvailW, loyH - chartY - 16);
            }

            y += loyH + 28;
            _stack.Height = y;
        }

        private async Task LoadAsync()
        {
            try
            {
                var localTo = DateTime.Today.AddDays(1);
                var localFrom = DateTime.Today.AddDays(-29);
                var fromUtc = localFrom.ToUniversalTime();
                var toUtc = localTo.ToUniversalTime();

                _lblRange.Text = $"{localFrom:MMM dd} – {localTo.AddDays(-1):MMM dd} (30 days, local time)";
                _pnlDateRange.Width = _lblRange.Right + 8;

                var d = await _api.GetAdminDashboardAsync(fromUtc, toUtc);
                if (d == null)
                {
                    MessageBox.Show("Failed to load admin dashboard data.",
                        "Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Metric Card 1: Revenue
                _cardRevenue.Value = $"PHP {d.RevenueThisMonth:N0}";
                _cardRevenue.DeltaText = $"{d.RevenueChangeText} vs prev 30d";
                _cardRevenue.DeltaIsPositive = d.RevenueChangePct >= 0;

                // Metric Card 2: Orders
                _cardOrders.Value = d.OrdersThisMonth.ToString("N0");
                _cardOrders.DeltaText = $"{d.OrdersChangeText} vs prev 30d";
                _cardOrders.DeltaIsPositive = d.OrdersChangePct >= 0;

                // Metric Card 3: Customers
                _cardCustomers.Value = d.NewCustomersThisMonth.ToString("N0");
                _cardCustomers.DeltaText = $"{d.CustomersChangeText} vs prev 30d";
                _cardCustomers.DeltaIsPositive = d.CustomersChangePct >= 0;

                // Metric Card 4: Average Order Value
                _cardAov.Value = $"PHP {d.AverageOrderValue:N2}";
                _cardAov.DeltaText = $"{d.AvgOrderChangeText} vs prev 30d";
                _cardAov.DeltaIsPositive = d.AvgOrderChangePct >= 0;

                // Loyalty Summary Mini Stats
                _statIssued.Value = d.PointsIssuedThisMonth.ToString("N0");
                _statRedeemed.Value = d.PointsRedeemedThisMonth.ToString("N0");
                _statRate.Value = $"{d.RedemptionRatePct:N1}%";

                // Grids
                _dgvTopCustomers.DataSource = null;
                _dgvTopCustomers.DataSource = d.TopCustomers;

                _dgvTopServices.DataSource = null;
                _dgvTopServices.DataSource = d.TopServices;

                // Charts
                RenderCharts(d);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dashboard error:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RenderCharts(AdminDashboardModel d)
        {
            // Revenue 30 days
            try
            {
                var p = _plotRevenue.Plot;
                p.Clear();
                p.FigureBackground.Color = new ScottPlot.Color(255, 255, 255);
                p.DataBackground.Color = new ScottPlot.Color(255, 255, 255);
                p.Grid.MajorLineColor = new ScottPlot.Color(242, 244, 247);
                p.Axes.Color(new ScottPlot.Color(160, 174, 192));

                var days = d.RevenueByDay.OrderBy(x => x.Day).ToArray();
                decimal totalRev = 0;
                if (days.Length > 0)
                {
                    totalRev = days.Sum(x => x.Revenue);
                    var pos = Enumerable.Range(0, days.Length).Select(i => (double)i).ToArray();
                    var vals = days.Select(x => (double)x.Revenue).ToArray();
                    var bars = p.Add.Bars(pos, vals);
                    foreach (var bar in bars.Bars)
                    {
                        bar.FillColor = new ScottPlot.Color(52, 152, 219);
                        bar.LineWidth = 0;
                    }
                    p.Axes.Bottom.Label.Text = "Day";
                    p.Axes.Left.Label.Text = "PHP";

                    // Select readable day ticks (every few days if many)
                    p.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                        pos, days.Select(x => x.Day.ToString("dd")).ToArray());
                }
                _lblRevTotal.Text = $"Total: PHP {totalRev:N0}";
                _plotRevenue.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminChart.Rev] {ex}");
            }

            // Orders 30 days
            try
            {
                var p = _plotOrders.Plot;
                p.Clear();
                p.FigureBackground.Color = new ScottPlot.Color(255, 255, 255);
                p.DataBackground.Color = new ScottPlot.Color(255, 255, 255);
                p.Grid.MajorLineColor = new ScottPlot.Color(242, 244, 247);
                p.Axes.Color(new ScottPlot.Color(160, 174, 192));

                var days = d.OrdersByDay.OrderBy(x => x.Day).ToArray();
                int totalOrd = 0;
                if (days.Length > 0)
                {
                    totalOrd = days.Sum(x => x.Count);
                    var pos = Enumerable.Range(0, days.Length).Select(i => (double)i).ToArray();
                    var vals = days.Select(x => (double)x.Count).ToArray();
                    var bars = p.Add.Bars(pos, vals);
                    foreach (var bar in bars.Bars)
                    {
                        bar.FillColor = new ScottPlot.Color(46, 204, 113);
                        bar.LineWidth = 0;
                    }
                    p.Axes.Bottom.Label.Text = "Day";
                    p.Axes.Left.Label.Text = "Orders";

                    p.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                        pos, days.Select(x => x.Day.ToString("dd")).ToArray());
                }
                _lblOrdTotal.Text = $"Total: {totalOrd:N0} orders";
                _plotOrders.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminChart.Ord] {ex}");
            }

            // Tier distribution
            try
            {
                var p = _plotTiers.Plot;
                p.Clear();
                p.FigureBackground.Color = new ScottPlot.Color(255, 255, 255);
                p.DataBackground.Color = new ScottPlot.Color(255, 255, 255);
                p.Grid.MajorLineColor = new ScottPlot.Color(242, 244, 247);
                p.Axes.Color(new ScottPlot.Color(160, 174, 192));

                var tiers = d.TierDistribution.ToArray();
                if (tiers.Length > 0)
                {
                    var pos = Enumerable.Range(0, tiers.Length).Select(i => (double)i).ToArray();
                    var vals = tiers.Select(t => (double)t.CustomerCount).ToArray();
                    var bars = p.Add.Bars(pos, vals);

                    // Refined modern tier palette
                    var tierColors = new[]
                    {
                        new ScottPlot.Color(180, 130, 80),   // Bronze
                        new ScottPlot.Color(148, 163, 184),  // Silver
                        new ScottPlot.Color(245, 158, 11),   // Gold
                        new ScottPlot.Color(99, 102, 241),   // Platinum
                        new ScottPlot.Color(52, 152, 219)    // Diamond / Other
                    };

                    for (int i = 0; i < bars.Bars.Count; i++)
                    {
                        bars.Bars[i].FillColor = tierColors[i % tierColors.Length];
                        bars.Bars[i].LineWidth = 0;
                    }

                    p.Axes.Left.Label.Text = "Customers";
                    p.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                        pos, tiers.Select(t => t.TierName).ToArray());
                }
                _plotTiers.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminChart.Tier] {ex}");
            }
        }
    }
}
