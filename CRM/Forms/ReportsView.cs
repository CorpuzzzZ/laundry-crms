using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRM.UI;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.WinForms.Forms
{
    public class ReportsView : UserControl
    {
        private readonly ReportApiService _api = new(ApiClient.Instance);

        // Left sidebar
        private ListBox _lstReports = null!;

        // Right side
        private Label _lblTitle = null!;
        private DateTimePicker _dtFrom = null!;
        private DateTimePicker _dtTo = null!;
        private Button _btnRun = null!;
        private Button _btnExport = null!;

        private Label _lblSummary = null!;
        private DataGridView _dgvData = null!;
        private ScottPlot.WinForms.FormsPlot _plot = null!;

        private string _currentKey = "daily-sales";

        // Cached data for CSV export
        private object? _currentResult;

        public ReportsView()
        {
            InitializeComponent();
            Load += async (s, e) => await LoadAsync();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Background;
            Font = Typography.Body;
            Padding = new Padding(16);

            // ===== Left sidebar =====
            var sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Colors.Surface,
                Padding = new Padding(12)
            };

            var lblSidebarTitle = new Label
            {
                Text = "REPORTS",
                Dock = DockStyle.Top,
                Height = 28,
                Font = Typography.SmallBold,
                ForeColor = Colors.TextSecondary
            };
            sidebar.Controls.Add(lblSidebarTitle);

            _lstReports = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Font = Typography.Body,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextBody,
                IntegralHeight = false
            };
            _lstReports.Items.Add("Daily Sales");
            _lstReports.Items.Add("Sales by Service");
            _lstReports.Items.Add("Sales by Payment Method");
            _lstReports.Items.Add("Order Status Breakdown");
            _lstReports.Items.Add("Peak Hours");
            _lstReports.SelectedIndex = 0;
            _lstReports.SelectedIndexChanged += async (s, e) =>
            {
                var keys = new[] { "daily-sales", "sales-by-service", "sales-by-payment-method", "order-status", "peak-hours" };
                if (_lstReports.SelectedIndex >= 0 && _lstReports.SelectedIndex < keys.Length)
                {
                    _currentKey = keys[_lstReports.SelectedIndex];
                    await LoadAsync();
                }
            };
            sidebar.Controls.Add(_lstReports);
            _lstReports.BringToFront();

            // ===== Right canvas =====
            var right = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Background,
                Padding = new Padding(16, 0, 0, 0)
            };

            // Header / filter row
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Colors.Background
            };

            _lblTitle = new Label
            {
                Text = "Daily Sales",
                Left = 0, Top = 6,
                AutoSize = true,
                Font = Typography.H2,
                ForeColor = Colors.TextPrimary
            };
            header.Controls.Add(_lblTitle);

            header.Controls.Add(new Label
            {
                Text = "From",
                Left = 0, Top = 44, AutoSize = true,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary
            });

            _dtFrom = new DateTimePicker
            {
                Left = 40, Top = 40, Width = 130,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Font = Typography.Body,
                Value = DateTime.Today.AddDays(-29)
            };
            header.Controls.Add(_dtFrom);

            header.Controls.Add(new Label
            {
                Text = "To",
                Left = 184, Top = 44, AutoSize = true,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary
            });

            _dtTo = new DateTimePicker
            {
                Left = 210, Top = 40, Width = 130,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Font = Typography.Body,
                Value = DateTime.Today
            };
            header.Controls.Add(_dtTo);

            _btnRun = new Button
            {
                Text = "Run",
                Left = 356, Top = 40, Width = 80, Height = 26,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.Body,
                Cursor = Cursors.Hand
            };
            _btnRun.FlatAppearance.BorderSize = 0;
            _btnRun.Click += async (s, e) => await LoadAsync();
            header.Controls.Add(_btnRun);

            _btnExport = new Button
            {
                Text = "Export CSV",
                Left = 448, Top = 40, Width = 100, Height = 26,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.Body,
                Cursor = Cursors.Hand
            };
            _btnExport.FlatAppearance.BorderSize = 1;
            _btnExport.FlatAppearance.BorderColor = Colors.Border;
            _btnExport.Click += (s, e) => ExportCsv();
            header.Controls.Add(_btnExport);

            // Summary label
            var summaryHost = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = Colors.Surface,
                Padding = new Padding(14, 10, 14, 10)
            };
            summaryHost.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawRectangle(pen, 0, 0, summaryHost.Width - 1, summaryHost.Height - 1);
            };

            _lblSummary = new Label
            {
                Text = "Loading...",
                Dock = DockStyle.Fill,
                Font = Typography.Body,
                ForeColor = Colors.TextPrimary
            };
            summaryHost.Controls.Add(_lblSummary);

            // Chart panel
            var chartHost = new Panel
            {
                Dock = DockStyle.Top,
                Height = 220,
                BackColor = Colors.Background,
                Padding = new Padding(0, 12, 0, 0)
            };

            _plot = new ScottPlot.WinForms.FormsPlot
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface
            };
            chartHost.Controls.Add(_plot);

            // Grid
            var gridHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Background,
                Padding = new Padding(0, 12, 0, 0)
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
                ColumnHeadersHeight = 34,
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
                RowTemplate = { Height = 30 }
            };
            gridHost.Controls.Add(_dgvData);

            right.Controls.Add(gridHost);
            right.Controls.Add(chartHost);
            right.Controls.Add(summaryHost);
            right.Controls.Add(header);

            Controls.Add(right);
            Controls.Add(sidebar);
        }

        private async Task LoadAsync()
        {
            var fromUtc = _dtFrom.Value.Date.ToUniversalTime();
            var toUtc = _dtTo.Value.Date.AddDays(1).ToUniversalTime();

            try
            {
                switch (_currentKey)
                {
                    case "daily-sales": await LoadDailySalesAsync(fromUtc, toUtc); break;
                    case "sales-by-service": await LoadSalesByServiceAsync(fromUtc, toUtc); break;
                    case "sales-by-payment-method": await LoadSalesByPaymentMethodAsync(fromUtc, toUtc); break;
                    case "order-status": await LoadOrderStatusAsync(fromUtc, toUtc); break;
                    case "peak-hours": await LoadPeakHoursAsync(fromUtc, toUtc); break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to run report:\n\n{ex.Message}",
                    "Reports", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        // 1. DAILY SALES
        // ============================================================
        private async Task LoadDailySalesAsync(DateTime from, DateTime to)
        {
            _lblTitle.Text = "Daily Sales";
            var d = await _api.GetDailySalesAsync(from, to);
            if (d == null) { _lblSummary.Text = "No data."; return; }

            _currentResult = d;
            _lblSummary.Text = $"Total Revenue: PHP {d.TotalRevenue:N2}   |   Total Orders: {d.TotalOrders:N0}   |   Avg Order Value: PHP {d.AverageOrderValue:N2}";

            ResetGrid();
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Date", DataPropertyName = "DayText", Width = 140 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Orders", DataPropertyName = "OrderCount", Width = 100 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Revenue", DataPropertyName = "RevenueText", Width = 140 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Avg Order", DataPropertyName = "AvgText", Width = 140 });
            _dgvData.DataSource = d.Rows;

            ResetPlot();
            var p = _plot.Plot;
            p.Title("Daily Revenue");
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
        // 2. SALES BY SERVICE
        // ============================================================
        private async Task LoadSalesByServiceAsync(DateTime from, DateTime to)
        {
            _lblTitle.Text = "Sales by Service";
            var d = await _api.GetSalesByServiceAsync(from, to);
            if (d == null) { _lblSummary.Text = "No data."; return; }

            _currentResult = d;
            _lblSummary.Text = $"Total Revenue: PHP {d.TotalRevenue:N2}   |   Total Items: {d.TotalItems:N0}";

            ResetGrid();
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Service", DataPropertyName = "ServiceName", Width = 260 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Code", DataPropertyName = "ServiceCode", Width = 100 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Items", DataPropertyName = "ItemCount", Width = 100 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Revenue", DataPropertyName = "RevenueText", Width = 140 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "% of Total", DataPropertyName = "PctText", Width = 100 });
            _dgvData.DataSource = d.Rows;

            ResetPlot();
            var p = _plot.Plot;
            p.Title("Revenue by Service");
            var rows = d.Rows.Take(10).ToArray();
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
        // 3. SALES BY PAYMENT METHOD
        // ============================================================
        private async Task LoadSalesByPaymentMethodAsync(DateTime from, DateTime to)
        {
            _lblTitle.Text = "Sales by Payment Method";
            var d = await _api.GetSalesByPaymentMethodAsync(from, to);
            if (d == null) { _lblSummary.Text = "No data."; return; }

            _currentResult = d;
            _lblSummary.Text = $"Total Revenue: PHP {d.TotalRevenue:N2}   |   Total Payments: {d.TotalPayments:N0}";

            ResetGrid();
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Payment Method", DataPropertyName = "MethodName", Width = 200 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Code", DataPropertyName = "MethodCode", Width = 100 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Payments", DataPropertyName = "PaymentCount", Width = 100 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "TotalAmountText", Width = 140 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "% of Total", DataPropertyName = "PctText", Width = 100 });
            _dgvData.DataSource = d.Rows;

            ResetPlot();
            var p = _plot.Plot;
            p.Title("Revenue by Payment Method");
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
        // 4. ORDER STATUS BREAKDOWN
        // ============================================================
        private async Task LoadOrderStatusAsync(DateTime from, DateTime to)
        {
            _lblTitle.Text = "Order Status Breakdown";
            var d = await _api.GetOrderStatusAsync(from, to);
            if (d == null) { _lblSummary.Text = "No data."; return; }

            _currentResult = d;
            _lblSummary.Text = $"Total Orders: {d.TotalOrders:N0}";

            ResetGrid();
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "StatusName", Width = 200 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Code", DataPropertyName = "StatusCode", Width = 100 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Orders", DataPropertyName = "OrderCount", Width = 120 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "% of Total", DataPropertyName = "PctText", Width = 120 });
            _dgvData.DataSource = d.Rows;

            ResetPlot();
            var p = _plot.Plot;
            p.Title("Orders by Status");
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
        // 5. PEAK HOURS
        // ============================================================
        private async Task LoadPeakHoursAsync(DateTime from, DateTime to)
        {
            _lblTitle.Text = "Peak Hours";
            var d = await _api.GetPeakHoursAsync(from, to);
            if (d == null) { _lblSummary.Text = "No data."; return; }

            _currentResult = d;
            _lblSummary.Text = $"Total Orders: {d.TotalOrders:N0}   |   Busiest Hour: {d.BusiestHour:D2}:00 ({d.BusiestHourCount} orders)";

            ResetGrid();
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Hour", DataPropertyName = "HourText", Width = 120 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Orders", DataPropertyName = "OrderCount", Width = 120 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Revenue", DataPropertyName = "RevenueText", Width = 140 });
            _dgvData.DataSource = d.Rows;

            ResetPlot();
            var p = _plot.Plot;
            p.Title("Orders by Hour of Day");
            var rows = d.Rows.ToArray();
            if (rows.Length > 0)
            {
                var pos = rows.Select(r => (double)r.Hour).ToArray();
                var vals = rows.Select(r => (double)r.OrderCount).ToArray();
                p.Add.Bars(pos, vals);
                p.Axes.Left.Label.Text = "Orders";
                p.Axes.Bottom.Label.Text = "Hour of Day";
            }
            _plot.Refresh();
        }

        // ============================================================
        // CSV EXPORT
        // ============================================================
        private void ExportCsv()
        {
            if (_currentResult == null)
            {
                MessageBox.Show("Run a report first.",
                    "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"{_currentKey}_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };
            if (sfd.ShowDialog(FindForm()) != DialogResult.OK) return;

            try
            {
                var sb = new StringBuilder();
                var grid = _dgvData;
                // Header
                var headers = grid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText);
                sb.AppendLine(string.Join(",", headers));
                // Rows
                foreach (DataGridViewRow r in grid.Rows)
                {
                    var cells = grid.Columns.Cast<DataGridViewColumn>()
                        .Select(c => EscapeCsv(r.Cells[c.Index].Value?.ToString() ?? ""));
                    sb.AppendLine(string.Join(",", cells));
                }
                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show($"Exported to:\n{sfd.FileName}",
                    "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed:\n{ex.Message}",
                    "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string EscapeCsv(string value)
        {
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }
}
