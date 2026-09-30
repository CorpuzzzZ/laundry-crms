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
    public class SuperAdminReportsView : UserControl
    {
        private readonly SuperAdminApiService _api = new(ApiClient.Instance);

        private ListBox _lstReports = null!;
        private Label _lblTitle = null!;
        private DateTimePicker _dtFrom = null!;
        private DateTimePicker _dtTo = null!;
        private Button _btnRun = null!;
        private Button _btnExport = null!;

        private Label _lblSummary = null!;
        private DataGridView _dgvData = null!;

        private string _currentKey = "subscriptions";
        private SuperAdminReportModel? _currentReport;

        public SuperAdminReportsView()
        {
            InitializeComponent();
            Load += async (s, e) => await LoadReportAsync();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Background;
            Font = Typography.Body;
            Padding = new Padding(16);

            // Left sidebar of reports
            var sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = Colors.Surface,
                Padding = new Padding(12)
            };

            var lblSidebarTitle = new Label
            {
                Text = "PLATFORM REPORTS",
                Font = Typography.SmallBold,
                ForeColor = Colors.TextSecondary,
                Dock = DockStyle.Top,
                Height = 28
            };
            sidebar.Controls.Add(lblSidebarTitle);

            _lstReports = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Colors.Surface,
                Font = Typography.Body,
                ItemHeight = 36,
                DrawMode = DrawMode.OwnerDrawFixed
            };
            _lstReports.Items.Add(new ReportMenuItem("Company Subscriptions", "subscriptions"));
            _lstReports.Items.Add(new ReportMenuItem("Plan Summaries & MRR", "plans"));
            _lstReports.SelectedIndex = 0;
            _lstReports.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                var g = e.Graphics;
                var r = e.Bounds;

                using var bg = new SolidBrush(isSelected ? Colors.PrimaryLight : Colors.Surface);
                g.FillRectangle(bg, r);

                var item = (ReportMenuItem)_lstReports.Items[e.Index];
                using var fg = new SolidBrush(isSelected ? Colors.Primary : Colors.TextPrimary);
                var font = isSelected ? Typography.BodyBold : Typography.Body;
                g.DrawString(item.Title, font, fg, r.Left + 12, r.Top + 8);
            };
            _lstReports.SelectedIndexChanged += async (s, e) =>
            {
                if (_lstReports.SelectedItem is ReportMenuItem item)
                {
                    _currentKey = item.Key;
                    _lblTitle.Text = item.Title;
                    await LoadReportAsync();
                }
            };
            sidebar.Controls.Add(_lstReports);

            // Main content
            var main = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Background,
                Padding = new Padding(16, 0, 0, 0)
            };

            // Top bar
            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Colors.Surface,
                Padding = new Padding(16, 12, 16, 12)
            };

            _lblTitle = new Label
            {
                Text = "Company Subscriptions & Billing",
                Font = Typography.H2,
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(16, 18)
            };
            topBar.Controls.Add(_lblTitle);

            var pnlControls = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 4, 0, 0)
            };

            var lblFrom = new Label { Text = "From:", AutoSize = true, Margin = new Padding(0, 8, 4, 0), Font = Typography.Small };
            _dtFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddMonths(-6),
                Width = 100,
                Margin = new Padding(0, 4, 12, 0)
            };

            var lblTo = new Label { Text = "To:", AutoSize = true, Margin = new Padding(0, 8, 4, 0), Font = Typography.Small };
            _dtTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(1),
                Width = 100,
                Margin = new Padding(0, 4, 12, 0)
            };

            _btnRun = new Button
            {
                Text = "⟳ Run Report",
                Width = 110,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 8, 0)
            };
            _btnRun.FlatAppearance.BorderSize = 0;
            _btnRun.Click += async (s, e) => await LoadReportAsync();

            _btnExport = new Button
            {
                Text = "⬇ Export CSV",
                Width = 110,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.Body,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 0, 0)
            };
            _btnExport.FlatAppearance.BorderColor = Colors.Border;
            _btnExport.Click += (s, e) => ExportCsv();

            pnlControls.Controls.AddRange(new Control[] { lblFrom, _dtFrom, lblTo, _dtTo, _btnRun, _btnExport });
            topBar.Controls.Add(pnlControls);

            // Summary banner
            _lblSummary = new Label
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.BodyBold,
                Padding = new Padding(16, 12, 16, 0),
                Text = "Loading summary..."
            };

            // Grid host
            var gridHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(16)
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
                }
            };
            gridHost.Controls.Add(_dgvData);

            main.Controls.Add(gridHost);
            main.Controls.Add(_lblSummary);
            main.Controls.Add(topBar);

            Controls.Add(main);
            Controls.Add(sidebar);
        }

        private async Task LoadReportAsync()
        {
            _btnRun.Enabled = false;
            _btnRun.Text = "Loading...";

            try
            {
                _currentReport = await _api.GetSubscriptionReportAsync(_dtFrom.Value.Date, _dtTo.Value.Date.AddDays(1));
                if (_currentReport == null)
                {
                    _lblSummary.Text = "Failed to load report data.";
                    return;
                }

                _lblSummary.Text = $"Platform Revenue: PHP {_currentReport.TotalRevenue:N2}   |   Active Subscriptions: {_currentReport.TotalActiveSubscriptions}";

                if (_currentKey == "subscriptions")
                {
                    SetupSubscriptionColumns();
                    _dgvData.DataSource = _currentReport.Subscriptions.Select(s => new
                    {
                        s.CompanyCode,
                        s.CompanyName,
                        s.PlanName,
                        PriceText = $"PHP {s.PricePerMonth:N0}/mo",
                        StartDateText = s.StartDate.ToString("MMM dd, yyyy"),
                        EndDateText = s.EndDate?.ToString("MMM dd, yyyy") ?? "Ongoing",
                        s.PaymentStatus,
                        AmountPaidText = $"PHP {(s.AmountPaid ?? s.PricePerMonth):N2}",
                        StatusText = s.IsActive ? "Active" : "Expired"
                    }).ToList();
                }
                else
                {
                    SetupPlanColumns();
                    _dgvData.DataSource = _currentReport.PlanSummaries.Select(p => new
                    {
                        p.PlanName,
                        p.PlanCode,
                        PriceText = $"PHP {p.MonthlyPrice:N0}/mo",
                        p.TenantCount,
                        EstRevenueText = $"PHP {p.EstimatedMonthlyRevenue:N2}",
                        p.MaxUsers,
                        p.MaxBranches
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load report:\n{ex.Message}", "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnRun.Enabled = true;
                _btnRun.Text = "⟳ Run Report";
            }
        }

        private void SetupSubscriptionColumns()
        {
            _dgvData.Columns.Clear();
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Company Code", DataPropertyName = "CompanyCode", FillWeight = 12 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Company Name", DataPropertyName = "CompanyName", FillWeight = 25 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Plan", DataPropertyName = "PlanName", FillWeight = 18 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fee", DataPropertyName = "PriceText", FillWeight = 14 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Start Date", DataPropertyName = "StartDateText", FillWeight = 13 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "End Date", DataPropertyName = "EndDateText", FillWeight = 13 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Payment", DataPropertyName = "PaymentStatus", FillWeight = 12 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total Paid", DataPropertyName = "AmountPaidText", FillWeight = 15 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "StatusText", FillWeight = 10 });
        }

        private void SetupPlanColumns()
        {
            _dgvData.Columns.Clear();
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Plan Name", DataPropertyName = "PlanName", FillWeight = 25 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Plan Code", DataPropertyName = "PlanCode", FillWeight = 15 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Monthly Price", DataPropertyName = "PriceText", FillWeight = 15 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Active Tenants", DataPropertyName = "TenantCount", FillWeight = 15 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Monthly Revenue", DataPropertyName = "EstRevenueText", FillWeight = 20 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Max Users", DataPropertyName = "MaxUsers", FillWeight = 10 });
            _dgvData.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Max Branches", DataPropertyName = "MaxBranches", FillWeight = 10 });
        }

        private void ExportCsv()
        {
            if (_dgvData.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"SuperAdmin_{_currentKey}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var sb = new StringBuilder();
                var headers = _dgvData.Columns.Cast<DataGridViewColumn>().Select(c => $"\"{c.HeaderText}\"");
                sb.AppendLine(string.Join(",", headers));

                foreach (DataGridViewRow row in _dgvData.Rows)
                {
                    var cells = row.Cells.Cast<DataGridViewCell>().Select(c => $"\"{c.Value?.ToString()?.Replace("\"", "\"\"")}\"");
                    sb.AppendLine(string.Join(",", cells));
                }

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Report exported successfully!", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class ReportMenuItem
        {
            public string Title { get; }
            public string Key { get; }
            public ReportMenuItem(string title, string key) { Title = title; Key = key; }
        }
    }
}
