using System;
using System.Collections.Generic;
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
    public enum CrewKpiType
    {
        TodayOrders,
        PendingOrders,
        ReadyForPickup,
        PickedUp,
        Revenue,
        Loyalty
    }

    public class CrewKpiDetailDialog : Form
    {
        private readonly CrewKpiType _kpiType;
        private readonly CrewDashboardModel _dashboardData;
        private readonly OrderApiService _orderApi = new();

        private Panel _pnlHeader = null!;
        private Label _lblTitle = null!;
        private Label _lblSubtitle = null!;
        private Panel _pnlSummary = null!;
        private TextBox _txtSearch = null!;
        private ComboBox _cboStatus = null!;
        private DataGridView _dgv = null!;
        private Label _lblFooter = null!;
        private Button _btnClose = null!;

        private List<OrderModel> _allOrders = new();

        public CrewKpiDetailDialog(CrewKpiType kpiType, CrewDashboardModel dashboardData)
        {
            _kpiType = kpiType;
            _dashboardData = dashboardData;

            InitializeComponent();
            Load += async (s, e) => await LoadDataAsync();
        }

        private void InitializeComponent()
        {
            Text = GetDialogTitle();
            Size = new Size(960, 640);
            MinimumSize = new Size(800, 500);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Colors.Background;
            Font = Typography.Body;
            ShowIcon = false;
            ShowInTaskbar = false;

            // ============================================================
            // 1. HEADER
            // ============================================================
            _pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 12, 24, 12)
            };
            _pnlHeader.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border, 1f);
                e.Graphics.DrawLine(pen, 0, _pnlHeader.Height - 1, _pnlHeader.Width, _pnlHeader.Height - 1);
            };

            _lblTitle = new Label
            {
                Text = GetDialogTitle(),
                Font = Typography.H1,
                ForeColor = Colors.TextPrimary,
                Location = new Point(24, 10),
                AutoSize = true
            };
            _pnlHeader.Controls.Add(_lblTitle);

            _lblSubtitle = new Label
            {
                Text = GetDialogSubtitle(),
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                Location = new Point(24, 38),
                AutoSize = true
            };
            _pnlHeader.Controls.Add(_lblSubtitle);

            // ============================================================
            // 2. SUMMARY KPI CHIPS
            // ============================================================
            _pnlSummary = new Panel
            {
                Dock = DockStyle.Top,
                Height = 74,
                BackColor = Colors.Background,
                Padding = new Padding(24, 12, 24, 8)
            };

            BuildSummaryPills();

            // ============================================================
            // 3. TOOLBAR (Search & Filter)
            // ============================================================
            var pnlToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = Colors.Background,
                Padding = new Padding(24, 0, 24, 8)
            };

            _txtSearch = new TextBox
            {
                Width = 260, Height = 32,
                Location = new Point(24, 6),
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Search customer or order #..."
            };
            _txtSearch.TextChanged += (s, e) => ApplyFilter();
            pnlToolbar.Controls.Add(_txtSearch);

            _cboStatus = new ComboBox
            {
                Width = 160, Height = 32,
                Location = new Point(296, 6),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Typography.Body
            };
            _cboStatus.Items.AddRange(new object[] { "All Statuses", "Pending", "In Progress", "Ready", "Picked Up", "Completed" });
            _cboStatus.SelectedIndex = GetDefaultStatusFilterIndex();
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();
            pnlToolbar.Controls.Add(_cboStatus);

            var btnReload = new Button
            {
                Text = "⟳ Refresh",
                Width = 90, Height = 30,
                Location = new Point(468, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.SmallBold,
                Cursor = Cursors.Hand
            };
            btnReload.FlatAppearance.BorderSize = 1;
            btnReload.FlatAppearance.BorderColor = Colors.Border;
            btnReload.Click += async (s, e) => await LoadDataAsync();
            pnlToolbar.Controls.Add(btnReload);

            // ============================================================
            // 4. FOOTER
            // ============================================================
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 54,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 10, 24, 10)
            };
            pnlFooter.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border, 1f);
                e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
            };

            _lblFooter = new Label
            {
                Text = "💡 Showing full detailed breakdown. Double-click any row to view complete order details.",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                Location = new Point(24, 18),
                AutoSize = true
            };
            pnlFooter.Controls.Add(_lblFooter);

            _btnClose = new Button
            {
                Text = "Close",
                Width = 90, Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnClose.FlatAppearance.BorderSize = 1;
            _btnClose.FlatAppearance.BorderColor = Colors.Border;
            _btnClose.Click += (s, e) => Close();
            pnlFooter.Controls.Add(_btnClose);
            pnlFooter.Resize += (s, e) =>
            {
                _btnClose.Left = pnlFooter.ClientSize.Width - 24 - _btnClose.Width;
            };

            // ============================================================
            // 5. DATA GRID (Rounded Card Host)
            // ============================================================
            var gridContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Background,
                Padding = new Padding(24, 0, 24, 12)
            };

            var cardGrid = new DashboardCard
            {
                Dock = DockStyle.Fill,
                CornerRadius = 10,
                Padding = new Padding(1)
            };

            _dgv = new DataGridView
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
                ColumnHeadersHeight = 38,
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

            ConfigureGridColumns();

            _dgv.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && _dgv.CurrentRow?.DataBoundItem is OrderModel order)
                {
                    OpenOrderDetails(order.OrderId);
                }
            };

            cardGrid.Controls.Add(_dgv);
            gridContainer.Controls.Add(cardGrid);

            // Assembly
            Controls.Add(gridContainer);
            Controls.Add(pnlToolbar);
            Controls.Add(_pnlSummary);
            Controls.Add(_pnlHeader);
            Controls.Add(pnlFooter);
        }

        private string GetDialogTitle() => _kpiType switch
        {
            CrewKpiType.TodayOrders    => "📦  Today's Orders — Full Detailed View",
            CrewKpiType.PendingOrders  => "⏳  Pending Orders Queue — Full Detailed View",
            CrewKpiType.ReadyForPickup => "✅  Ready for Pickup — Full Detailed View",
            CrewKpiType.PickedUp       => "🚚  Picked Up Today — Full Detailed View",
            CrewKpiType.Revenue        => "💰  Today's Revenue Breakdown — Full Detailed View",
            CrewKpiType.Loyalty        => "⭐  Today's Loyalty Activity — Full Detailed View",
            _                          => "KPI Detailed View"
        };

        private string GetDialogSubtitle() => _kpiType switch
        {
            CrewKpiType.TodayOrders    => "All customer orders created, updated, or scheduled for today.",
            CrewKpiType.PendingOrders  => "Orders in queue waiting to be processed, washed, or dried.",
            CrewKpiType.ReadyForPickup => "Orders that have finished processing and are waiting for customer pickup.",
            CrewKpiType.PickedUp       => "Orders successfully claimed and picked up by customers today.",
            CrewKpiType.Revenue        => "Detailed payment breakdown, average ticket size, and payment transactions.",
            CrewKpiType.Loyalty        => "Points earned, points redeemed, and loyalty discounts applied today.",
            _                          => "Full operational breakdown."
        };

        private int GetDefaultStatusFilterIndex() => _kpiType switch
        {
            CrewKpiType.PendingOrders  => 1, // Pending
            CrewKpiType.ReadyForPickup => 3, // Ready
            CrewKpiType.PickedUp       => 4, // Picked Up
            _                          => 0  // All Statuses
        };

        private void BuildSummaryPills()
        {
            _pnlSummary.Controls.Clear();
            int x = 24;
            int gap = 12;
            int h = 48;

            switch (_kpiType)
            {
                case CrewKpiType.TodayOrders:
                    AddStatPill("TOTAL TODAY", _dashboardData.TodayOrderCount.ToString("N0"), Colors.Primary, ref x, gap, h);
                    AddStatPill("PENDING", _dashboardData.PendingOrderCount.ToString("N0"), Colors.Warning, ref x, gap, h);
                    AddStatPill("READY", _dashboardData.ReadyOrderCount.ToString("N0"), Colors.Success, ref x, gap, h);
                    AddStatPill("PICKED UP", _dashboardData.PickedUpCount.ToString("N0"), Color.FromArgb(108, 92, 231), ref x, gap, h);
                    AddStatPill("REVENUE", $"PHP {_dashboardData.TotalRevenue:N2}", Colors.Success, ref x, gap, h);
                    break;

                case CrewKpiType.PendingOrders:
                    AddStatPill("IN QUEUE", _dashboardData.PendingOrderCount.ToString("N0"), Colors.Warning, ref x, gap, h);
                    AddStatPill("STATUS", "Action Required", Colors.Warning, ref x, gap, h);
                    AddStatPill("TOTAL TODAY", _dashboardData.TodayOrderCount.ToString("N0"), Colors.TextSecondary, ref x, gap, h);
                    break;

                case CrewKpiType.ReadyForPickup:
                    AddStatPill("READY ORDERS", _dashboardData.ReadyOrderCount.ToString("N0"), Colors.Success, ref x, gap, h);
                    AddStatPill("PICKED UP TODAY", _dashboardData.PickedUpCount.ToString("N0"), Color.FromArgb(108, 92, 231), ref x, gap, h);
                    AddStatPill("STATUS", "Awaiting Pickup", Colors.Success, ref x, gap, h);
                    break;

                case CrewKpiType.PickedUp:
                    AddStatPill("PICKED UP TODAY", _dashboardData.PickedUpCount.ToString("N0"), Color.FromArgb(108, 92, 231), ref x, gap, h);
                    AddStatPill("TOTAL COMPLETED", _dashboardData.PickedUpCount.ToString("N0"), Colors.Success, ref x, gap, h);
                    break;

                case CrewKpiType.Revenue:
                    AddStatPill("TOTAL REVENUE", $"PHP {_dashboardData.TotalRevenue:N2}", Colors.Success, ref x, gap, h);
                    AddStatPill("PAYMENT COUNT", _dashboardData.PaymentCount.ToString("N0"), Colors.Primary, ref x, gap, h);
                    AddStatPill("AVG PAYMENT", $"PHP {_dashboardData.AveragePayment:N2}", Colors.Warning, ref x, gap, h);
                    break;

                case CrewKpiType.Loyalty:
                    AddStatPill("POINTS ISSUED", _dashboardData.PointsIssued.ToString("N0"), Colors.Primary, ref x, gap, h);
                    AddStatPill("POINTS REDEEMED", _dashboardData.PointsRedeemed.ToString("N0"), Colors.Success, ref x, gap, h);
                    AddStatPill("DISCOUNT GIVEN", $"PHP {_dashboardData.LoyaltyDiscountGiven:N2}", Colors.Warning, ref x, gap, h);
                    break;
            }
        }

        private void AddStatPill(string label, string value, Color accent, ref int x, int gap, int h)
        {
            int w = 155;
            var pill = new Panel
            {
                Left = x, Top = 12,
                Width = w, Height = h,
                BackColor = Colors.Surface
            };

            pill.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, pill.Width - 1, pill.Height - 1);
                using var path = DashboardCard.GetRoundedPath(r, 8);
                using var bg = new SolidBrush(Color.FromArgb(14, accent));
                g.FillPath(bg, path);

                var oldClip = g.Clip;
                g.SetClip(path);
                using var acc = new SolidBrush(accent);
                g.FillRectangle(acc, 0, 0, 3.5f, pill.Height);
                g.Clip = oldClip;

                using var pen = new Pen(Color.FromArgb(50, accent), 1f);
                g.DrawPath(pen, path);
            };

            pill.Controls.Add(new Label
            {
                Text = label,
                Font = Typography.TinyUpper,
                ForeColor = Colors.TextSecondary,
                Location = new Point(10, 6),
                AutoSize = true
            });

            pill.Controls.Add(new Label
            {
                Text = value,
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                Location = new Point(10, 22),
                AutoSize = true
            });

            _pnlSummary.Controls.Add(pill);
            x += w + gap;
        }

        private void ConfigureGridColumns()
        {
            _dgv.Columns.Clear();

            _dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Order #",
                DataPropertyName = "OrderNumber",
                FillWeight = 18
            });

            _dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Customer Name",
                DataPropertyName = "CustomerName",
                FillWeight = 28
            });

            _dgv.Columns.Add(new DataGridViewTextBoxColumn
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

            _dgv.Columns.Add(new DataGridViewTextBoxColumn
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

            _dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Time",
                DataPropertyName = "OrderDateText",
                FillWeight = 16,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    ForeColor = Colors.TextSecondary
                }
            });
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // First populate with available RecentOrders from dashboard
                if (_dashboardData.RecentOrders.Count > 0)
                {
                    _allOrders = _dashboardData.RecentOrders.Select(r => new OrderModel
                    {
                        OrderId = r.OrderId,
                        OrderNumber = r.OrderNumber,
                        CustomerName = r.CustomerName,
                        TotalAmount = r.TotalAmount,
                        StatusName = r.StatusName,
                        StatusCode = r.StatusCode,
                        OrderDate = r.OrderDate
                    }).ToList();
                    ApplyFilter();
                }

                // Now fetch fresh data from API
                DateTime fromUtc = DateTime.Today.ToUniversalTime();
                DateTime toUtc = DateTime.Today.AddDays(1).ToUniversalTime();

                string? statusFilter = _kpiType switch
                {
                    CrewKpiType.PendingOrders  => "Pending",
                    CrewKpiType.ReadyForPickup => "Ready",
                    _                          => null
                };

                var res = await _orderApi.GetOrdersAsync(page: 1, pageSize: 100, statusCode: statusFilter, dateFrom: DateTime.Today, dateTo: DateTime.Today.AddDays(1));
                if (res?.Data != null && res.Data.Count > 0)
                {
                    _allOrders = res.Data;
                    ApplyFilter();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CrewKpiDetailDialog] {ex}");
            }
        }

        private void ApplyFilter()
        {
            var search = (_txtSearch.Text ?? "").Trim().ToLowerInvariant();
            var statusChoice = _cboStatus.SelectedItem?.ToString() ?? "All Statuses";

            var filtered = _allOrders.AsEnumerable();

            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(o =>
                    (o.OrderNumber != null && o.OrderNumber.ToLowerInvariant().Contains(search)) ||
                    (o.CustomerName != null && o.CustomerName.ToLowerInvariant().Contains(search)));
            }

            if (statusChoice != "All Statuses")
            {
                filtered = filtered.Where(o =>
                    (o.StatusName != null && o.StatusName.Equals(statusChoice, StringComparison.OrdinalIgnoreCase)) ||
                    (o.StatusCode != null && o.StatusCode.Equals(statusChoice, StringComparison.OrdinalIgnoreCase)));
            }

            _dgv.DataSource = null;
            _dgv.DataSource = filtered.ToList();
        }

        private void OpenOrderDetails(int orderId)
        {
            try
            {
                // Open standard OrderDetailsView in a popup dialog
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
                detailsForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open order details:\n\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
