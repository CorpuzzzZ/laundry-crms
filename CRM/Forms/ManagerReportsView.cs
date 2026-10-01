using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRM.UI;
using CRM.UI.Controls;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.WinForms.Forms
{
    public class ManagerReportsView : UserControl
    {
        private readonly ReportApiService _api = new(ApiClient.Instance);

        // Header and branch badge elements
        private Label _lblBranchBadge = null!;
        private Label _lblSummaryMeta = null!;

        // Metric Cards
        private Label _lblMetricRevenue = null!;
        private Label _lblMetricOrders = null!;
        private Label _lblMetricAvg = null!;
        private Label _lblMetricPeak = null!;

        // Report type selection tabs
        private Button _btnTabDaily = null!;
        private Button _btnTabService = null!;
        private Button _btnTabPayment = null!;
        private Button _btnTabStatus = null!;
        private Button _btnTabPeak = null!;
        private string _selectedReportKey = "daily-sales";

        // Date selection
        private DateTimePicker _dtFrom = null!;
        private DateTimePicker _dtTo = null!;
        private Button _btnRefresh = null!;
        private Button _btnExport = null!;

        // Visual components
        private ScottPlot.WinForms.FormsPlot _plot = null!;
        private DataGridView _dgvData = null!;

        // Current cached report result
        private object? _currentResult;

        public ManagerReportsView()
        {
            InitializeComponent();
            Load += async (s, e) => await LoadCurrentReportAsync();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Background;
            Font = Typography.Body;
            Padding = new Padding(24);

            var user = SessionManager.CurrentUser;
            var branchName = !string.IsNullOrWhiteSpace(user?.AssignedBranchName)
                ? user.AssignedBranchName
                : (user?.AssignedBranchId.HasValue == true ? $"Branch #{user.AssignedBranchId.Value}" : "Assigned Branch");

            // ===== 1. TOP HEADER BANNER =====
            var pnlHeader = new DashboardCard
            {
                Dock = DockStyle.Top,
                Height = 84,
                CornerRadius = 14,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true,
                Padding = new Padding(20, 14, 20, 14)
            };

            var lblTitle = new Label
            {
                Text = "Branch Performance Reports",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Colors.TextPrimary,
                Location = new Point(18, 14),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "Financial metrics, orders distribution, and operational analytics scoped to your branch.",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                Location = new Point(20, 48),
                AutoSize = true
            };

            _lblBranchBadge = new Label
            {
                Text = $"📍 {branchName.ToUpperInvariant()} (ID: #{user?.AssignedBranchId ?? 1})",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 148, 136),
                BackColor = Color.FromArgb(240, 253, 250),
                Padding = new Padding(12, 6, 12, 6),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _lblBranchBadge.Location = new Point(pnlHeader.Width - 320, 24);
            _lblBranchBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(_lblBranchBadge);

            // ===== 2. METRIC CARDS ROW =====
            var pnlMetrics = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 100,
                ColumnCount = 4,
                RowCount = 1,
                Margin = new Padding(0, 16, 0, 16),
                Padding = new Padding(0, 12, 0, 12)
            };
            pnlMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pnlMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pnlMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pnlMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            var cardRevenue = CreateMetricCard("BRANCH TOTAL REVENUE", "PHP 0.00", Color.FromArgb(16, 185, 129), out _lblMetricRevenue);
            var cardOrders = CreateMetricCard("BRANCH TOTAL ORDERS", "0", Color.FromArgb(59, 130, 246), out _lblMetricOrders);
            var cardAvg = CreateMetricCard("AVG ORDER VALUE", "PHP 0.00", Color.FromArgb(139, 92, 246), out _lblMetricAvg);
            var cardPeak = CreateMetricCard("BUSIEST PEAK HOUR", "--:00", Color.FromArgb(245, 158, 11), out _lblMetricPeak);

            pnlMetrics.Controls.Add(cardRevenue, 0, 0);
            pnlMetrics.Controls.Add(cardOrders, 1, 0);
            pnlMetrics.Controls.Add(cardAvg, 2, 0);
            pnlMetrics.Controls.Add(cardPeak, 3, 0);

            // ===== 3. FILTER & TAB BAR =====
            var pnlControls = new DashboardCard
            {
                Dock = DockStyle.Top,
                Height = 62,
                CornerRadius = 14,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true,
                Padding = new Padding(16, 10, 16, 10)
            };

            // Segmented Pill Tabs
            int tabX = 16;
            _btnTabDaily = CreateReportTab("Daily Sales", "daily-sales", ref tabX, pnlControls);
            _btnTabService = CreateReportTab("Sales by Service", "sales-by-service", ref tabX, pnlControls);
            _btnTabPayment = CreateReportTab("Payment Methods", "sales-by-payment-method", ref tabX, pnlControls);
            _btnTabStatus = CreateReportTab("Order Statuses", "order-status", ref tabX, pnlControls);
            _btnTabPeak = CreateReportTab("Peak Hours", "peak-hours", ref tabX, pnlControls);

            // Date Pickers on Right
            var pnlDateControls = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 470,
                Height = 42,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 4, 0, 0)
            };

            var lblFrom = new Label { Text = "From:", AutoSize = true, Margin = new Padding(0, 6, 4, 0), Font = Typography.SmallBold, ForeColor = Colors.TextSecondary };
            _dtFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Width = 110,
                Value = DateTime.Today.AddDays(-29),
                Font = Typography.Small
            };

            var lblTo = new Label { Text = "To:", AutoSize = true, Margin = new Padding(10, 6, 4, 0), Font = Typography.SmallBold, ForeColor = Colors.TextSecondary };
            _dtTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Width = 110,
                Value = DateTime.Today,
                Font = Typography.Small
            };

            _btnRefresh = new Button
            {
                Text = "⚡ Run",
                Size = new Size(68, 28),
                Margin = new Padding(8, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.SmallBold,
                Cursor = Cursors.Hand
            };
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.Click += async (s, e) => await LoadCurrentReportAsync();

            _btnExport = new Button
            {
                Text = "📥 CSV",
                Size = new Size(68, 28),
                Margin = new Padding(6, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.SmallBold,
                Cursor = Cursors.Hand
            };
            _btnExport.FlatAppearance.BorderSize = 1;
            _btnExport.FlatAppearance.BorderColor = Colors.Border;
            _btnExport.Click += (s, e) => ExportCsv();

            pnlDateControls.Controls.Add(lblFrom);
            pnlDateControls.Controls.Add(_dtFrom);
            pnlDateControls.Controls.Add(lblTo);
            pnlDateControls.Controls.Add(_dtTo);
            pnlDateControls.Controls.Add(_btnRefresh);
            pnlDateControls.Controls.Add(_btnExport);

            pnlControls.Controls.Add(pnlDateControls);

            // ===== 4. CONTENT AREA (Split Chart + Data Grid) =====
            var pnlMainContent = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 16, 0, 0)
            };

            // Chart Container
            var cardChart = new DashboardCard
            {
                Dock = DockStyle.Top,
                Height = 250,
                CornerRadius = 14,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true,
                Padding = new Padding(16),
                Margin = new Padding(0, 0, 0, 16)
            };

            _plot = new ScottPlot.WinForms.FormsPlot
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface
            };
            cardChart.Controls.Add(_plot);

            // Grid Container
            var cardGrid = new DashboardCard
            {
                Dock = DockStyle.Fill,
                CornerRadius = 14,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true,
                Padding = new Padding(16)
            };

            _lblSummaryMeta = new Label
            {
                Text = "Fetching live branch metrics...",
                Dock = DockStyle.Top,
                Height = 28,
                Font = Typography.SmallBold,
                ForeColor = Colors.Primary,
                BackColor = Color.Transparent
            };

            _dgvData = new DataGridView
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
                    BackColor = Color.FromArgb(248, 250, 252),
                    ForeColor = Colors.TextSecondary,
                    Font = Typography.SmallBold,
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(8, 0, 8, 0)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Colors.TextBody,
                    SelectionBackColor = Colors.PrimaryLight,
                    SelectionForeColor = Colors.TextPrimary,
                    Padding = new Padding(8, 0, 8, 0)
                },
                RowTemplate = { Height = 32 }
            };

            cardGrid.Controls.Add(_dgvData);
            cardGrid.Controls.Add(_lblSummaryMeta);

            pnlMainContent.Controls.Add(cardGrid);
            pnlMainContent.Controls.Add(cardChart);

            // Add in reverse dock order
            Controls.Add(pnlMainContent);
            Controls.Add(pnlControls);
            Controls.Add(pnlMetrics);
            Controls.Add(pnlHeader);

            UpdateTabStyles();
        }

        private DashboardCard CreateMetricCard(string label, string initialValue, Color accentColor, out Label valueLabel)
        {
            var card = new DashboardCard
            {
                Dock = DockStyle.Fill,
                CornerRadius = 12,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true,
                Padding = new Padding(16, 12, 16, 12),
                Margin = new Padding(4)
            };

            var lblTitle = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Colors.TextSecondary,
                Dock = DockStyle.Top,
                Height = 18
            };

            valueLabel = new Label
            {
                Text = initialValue,
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = accentColor,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            card.Controls.Add(valueLabel);
            card.Controls.Add(lblTitle);
            return card;
        }

        private Button CreateReportTab(string text, string key, ref int xPos, Panel container)
        {
            var btn = new Button
            {
                Text = text,
                Tag = key,
                Location = new Point(xPos, 14),
                Height = 32,
                AutoSize = true,
                Padding = new Padding(12, 0, 12, 0),
                FlatStyle = FlatStyle.Flat,
                Font = Typography.SmallBold,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += async (s, e) =>
            {
                _selectedReportKey = key;
                UpdateTabStyles();
                await LoadCurrentReportAsync();
            };

            container.Controls.Add(btn);
            xPos += btn.PreferredSize.Width + 8;
            return btn;
        }

        private void UpdateTabStyles()
        {
            Button[] tabs = { _btnTabDaily, _btnTabService, _btnTabPayment, _btnTabStatus, _btnTabPeak };
            foreach (var tab in tabs)
            {
                bool isSelected = (string)tab.Tag! == _selectedReportKey;
                tab.BackColor = isSelected ? Colors.PrimaryLight : Color.Transparent;
                tab.ForeColor = isSelected ? Colors.Primary : Colors.TextSecondary;
            }
        }

        private async Task LoadCurrentReportAsync()
        {
            var user = SessionManager.CurrentUser;
            int? branchId = user?.AssignedBranchId;
            var fromUtc = _dtFrom.Value.Date.ToUniversalTime();
            var toUtc = _dtTo.Value.Date.AddDays(1).ToUniversalTime();

            _btnRefresh.Enabled = false;
            _lblSummaryMeta.Text = "Loading branch report data...";

            try
            {
                switch (_selectedReportKey)
                {
                    case "daily-sales":
                        await LoadDailySalesAsync(fromUtc, toUtc, branchId);
                        break;
                    case "sales-by-service":
                        await LoadSalesByServiceAsync(fromUtc, toUtc, branchId);
                        break;
                    case "sales-by-payment-method":
                        await LoadSalesByPaymentMethodAsync(fromUtc, toUtc, branchId);
                        break;
                    case "order-status":
                        await LoadOrderStatusAsync(fromUtc, toUtc, branchId);
                        break;
                    case "peak-hours":
                        await LoadPeakHoursAsync(fromUtc, toUtc, branchId);
                        break;
                }
            }
            catch (Exception ex)
            {
                _lblSummaryMeta.Text = $"Error loading report: {ex.Message}";
            }
            finally
            {
                _btnRefresh.Enabled = true;
            }
        }

        private void ResetGrid()
        {
            _dgvData.DataSource = null;
            _dgvData.Columns.Clear();
        }

        private void ResetPlot()
        {
            var p = _plot.Plot;
            p.Clear();
            _plot.Refresh();
        }

        // ============================================================
        // 1. DAILY SALES (BRANCH SCOPED)
        // ============================================================
        private async Task LoadDailySalesAsync(DateTime from, DateTime to, int? branchId)
        {
            var d = await _api.GetDailySalesAsync(from, to, branchId);
            if (d == null)
            {
                _lblSummaryMeta.Text = "No sales data found for this branch in the selected timeframe.";
                return;
            }

            _currentResult = d;
            _lblMetricRevenue.Text = $"PHP {d.TotalRevenue:N2}";
            _lblMetricOrders.Text = $"{d.TotalOrders:N0}";
            _lblMetricAvg.Text = $"PHP {d.AverageOrderValue:N2}";
            _lblSummaryMeta.Text = $"Branch Sales Summary  |  Total Revenue: PHP {d.TotalRevenue:N2}  •  Orders: {d.TotalOrders:N0}  •  Average Value: PHP {d.AverageOrderValue:N2}";

            ResetGrid();
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Date", DataPropertyName = "DayText", Width = 140 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Orders", DataPropertyName = "OrderCount", Width = 100 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Revenue", DataPropertyName = "RevenueText", Width = 150 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Avg Order", DataPropertyName = "AvgText", Width = 150 });
            _dgvData.DataSource = d.Rows;

            ResetPlot();
            var p = _plot.Plot;
            p.Title("Branch Daily Revenue Trend");
            var rows = d.Rows.ToArray();
            if (rows.Length > 0)
            {
                var pos = Enumerable.Range(0, rows.Length).Select(i => (double)i).ToArray();
                var vals = rows.Select(r => (double)r.Revenue).ToArray();
                p.Add.Bars(pos, vals);
                p.Axes.Left.Label.Text = "PHP";
                p.Axes.Bottom.Label.Text = "Day";
            }
            _plot.Refresh();
        }

        // ============================================================
        // 2. SALES BY SERVICE (BRANCH SCOPED)
        // ============================================================
        private async Task LoadSalesByServiceAsync(DateTime from, DateTime to, int? branchId)
        {
            var d = await _api.GetSalesByServiceAsync(from, to, branchId);
            if (d == null)
            {
                _lblSummaryMeta.Text = "No service transactions found for this branch.";
                return;
            }

            _currentResult = d;
            _lblMetricRevenue.Text = $"PHP {d.TotalRevenue:N2}";
            _lblMetricOrders.Text = $"{d.TotalItems:N0} items";
            _lblSummaryMeta.Text = $"Branch Service Breakdown  |  Total Revenue: PHP {d.TotalRevenue:N2}  •  Total Items Laundered: {d.TotalItems:N0}";

            ResetGrid();
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Service", DataPropertyName = "ServiceName", Width = 260 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Code", DataPropertyName = "ServiceCode", Width = 110 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Items Laundered", DataPropertyName = "ItemCount", Width = 130 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Revenue", DataPropertyName = "RevenueText", Width = 150 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "% of Total", DataPropertyName = "PctText", Width = 110 });
            _dgvData.DataSource = d.Rows;

            ResetPlot();
            var p = _plot.Plot;
            p.Title("Branch Revenue by Service");
            var rows = d.Rows.Take(8).ToArray();
            if (rows.Length > 0)
            {
                var pos = Enumerable.Range(0, rows.Length).Select(i => (double)i).ToArray();
                var vals = rows.Select(r => (double)r.Revenue).ToArray();
                p.Add.Bars(pos, vals);
                p.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                    pos, rows.Select(r => r.ServiceName).ToArray());
                p.Axes.Left.Label.Text = "PHP";
            }
            _plot.Refresh();
        }

        // ============================================================
        // 3. SALES BY PAYMENT METHOD (BRANCH SCOPED)
        // ============================================================
        private async Task LoadSalesByPaymentMethodAsync(DateTime from, DateTime to, int? branchId)
        {
            var d = await _api.GetSalesByPaymentMethodAsync(from, to, branchId);
            if (d == null)
            {
                _lblSummaryMeta.Text = "No payment transaction records found for this branch.";
                return;
            }

            _currentResult = d;
            _lblMetricRevenue.Text = $"PHP {d.TotalRevenue:N2}";
            _lblMetricOrders.Text = $"{d.TotalPayments:N0} pmts";
            _lblSummaryMeta.Text = $"Branch Payments Collection  |  Total Collected: PHP {d.TotalRevenue:N2}  •  Total Transactions: {d.TotalPayments:N0}";

            ResetGrid();
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Payment Method", DataPropertyName = "MethodName", Width = 220 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Code", DataPropertyName = "MethodCode", Width = 110 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Payments Count", DataPropertyName = "PaymentCount", Width = 130 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total Collected", DataPropertyName = "TotalAmountText", Width = 150 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "% of Total", DataPropertyName = "PctText", Width = 110 });
            _dgvData.DataSource = d.Rows;

            ResetPlot();
            var p = _plot.Plot;
            p.Title("Branch Payment Distribution");
            var rows = d.Rows.ToArray();
            if (rows.Length > 0)
            {
                var pos = Enumerable.Range(0, rows.Length).Select(i => (double)i).ToArray();
                var vals = rows.Select(r => (double)r.TotalAmount).ToArray();
                p.Add.Bars(pos, vals);
                p.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                    pos, rows.Select(r => r.MethodName).ToArray());
                p.Axes.Left.Label.Text = "PHP";
            }
            _plot.Refresh();
        }

        // ============================================================
        // 4. ORDER STATUS (BRANCH SCOPED)
        // ============================================================
        private async Task LoadOrderStatusAsync(DateTime from, DateTime to, int? branchId)
        {
            var d = await _api.GetOrderStatusAsync(from, to, branchId);
            if (d == null)
            {
                _lblSummaryMeta.Text = "No order records found for this branch.";
                return;
            }

            _currentResult = d;
            _lblMetricOrders.Text = $"{d.TotalOrders:N0}";
            _lblSummaryMeta.Text = $"Branch Pipeline Status  |  Total Active & Completed Orders: {d.TotalOrders:N0}";

            ResetGrid();
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "StatusName", Width = 240 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Code", DataPropertyName = "StatusCode", Width = 120 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Orders", DataPropertyName = "OrderCount", Width = 120 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "% Share", DataPropertyName = "PctText", Width = 120 });
            _dgvData.DataSource = d.Rows;

            ResetPlot();
            var p = _plot.Plot;
            p.Title("Branch Order Pipeline Breakdown");
            var rows = d.Rows.ToArray();
            if (rows.Length > 0)
            {
                var pos = Enumerable.Range(0, rows.Length).Select(i => (double)i).ToArray();
                var vals = rows.Select(r => (double)r.OrderCount).ToArray();
                p.Add.Bars(pos, vals);
                p.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                    pos, rows.Select(r => r.StatusName).ToArray());
                p.Axes.Left.Label.Text = "Orders";
            }
            _plot.Refresh();
        }

        // ============================================================
        // 5. PEAK HOURS (BRANCH SCOPED)
        // ============================================================
        private async Task LoadPeakHoursAsync(DateTime from, DateTime to, int? branchId)
        {
            var d = await _api.GetPeakHoursAsync(from, to, branchId);
            if (d == null)
            {
                _lblSummaryMeta.Text = "No hourly records found for this branch.";
                return;
            }

            _currentResult = d;
            _lblMetricOrders.Text = $"{d.TotalOrders:N0}";
            _lblMetricPeak.Text = $"{d.BusiestHour:D2}:00 ({d.BusiestHourCount} orders)";
            _lblSummaryMeta.Text = $"Branch Peak Operating Hours  |  Busiest Window: {d.BusiestHour:D2}:00 with {d.BusiestHourCount} order(s)";

            ResetGrid();
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Hour Window", DataPropertyName = "HourText", Width = 180 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Orders Processed", DataPropertyName = "OrderCount", Width = 160 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Revenue", DataPropertyName = "RevenueText", Width = 180 });
            _dgvData.DataSource = d.Rows;

            ResetPlot();
            var p = _plot.Plot;
            p.Title("Branch Hourly Customer Volume");
            var rows = d.Rows.ToArray();
            var pos = Enumerable.Range(0, 24).Select(i => (double)i).ToArray();
            var vals = rows.Select(r => (double)r.OrderCount).ToArray();
            p.Add.Bars(pos, vals);
            p.Axes.Left.Label.Text = "Orders";
            p.Axes.Bottom.Label.Text = "Hour (24h)";
            _plot.Refresh();
        }

        private void ExportCsv()
        {
            if (_currentResult == null)
            {
                MessageBox.Show("No report data is currently loaded to export.",
                    "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Title = "Export Branch Report to CSV",
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                FileName = $"Branch_Report_{_selectedReportKey}_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var sb = new StringBuilder();

                if (_currentResult is DailySalesReportModel daily)
                {
                    sb.AppendLine("Date,Orders,Revenue,AverageOrderValue");
                    foreach (var r in daily.Rows)
                        sb.AppendLine($"\"{r.DayText}\",{r.OrderCount},{r.Revenue},{r.AverageOrderValue}");
                }
                else if (_currentResult is SalesByServiceReportModel svc)
                {
                    sb.AppendLine("Service,Code,ItemCount,Revenue,PercentOfTotal");
                    foreach (var r in svc.Rows)
                        sb.AppendLine($"\"{r.ServiceName}\",\"{r.ServiceCode}\",{r.ItemCount},{r.Revenue},{r.RevenuePct}");
                }
                else if (_currentResult is SalesByPaymentMethodReportModel pmt)
                {
                    sb.AppendLine("PaymentMethod,Code,PaymentCount,TotalAmount,PercentOfTotal");
                    foreach (var r in pmt.Rows)
                        sb.AppendLine($"\"{r.MethodName}\",\"{r.MethodCode}\",{r.PaymentCount},{r.TotalAmount},{r.AmountPct}");
                }
                else if (_currentResult is OrderStatusReportModel st)
                {
                    sb.AppendLine("Status,Code,OrderCount,PercentOfTotal");
                    foreach (var r in st.Rows)
                        sb.AppendLine($"\"{r.StatusName}\",\"{r.StatusCode}\",{r.OrderCount},{r.PercentOfTotal}");
                }
                else if (_currentResult is PeakHoursReportModel pk)
                {
                    sb.AppendLine("Hour,OrderCount,Revenue");
                    foreach (var r in pk.Rows)
                        sb.AppendLine($"\"{r.HourText}\",{r.OrderCount},{r.Revenue}");
                }

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show($"Branch report exported successfully to:\n{sfd.FileName}",
                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export CSV: {ex.Message}",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
