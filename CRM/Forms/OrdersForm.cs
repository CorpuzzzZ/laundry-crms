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
using CRM.WinForms.UI;

namespace CRM.WinForms.Forms
{
    public class OrdersForm : Form
    {
        public event EventHandler? NewOrderRequested;
        public event EventHandler<int>? EditOrderRequested;

        /// <summary>Raised when user clicks Pay on an order card.</summary>
        public event EventHandler<int>? PayOrderRequested;

        /// <summary>Raised when a Picked Up / Delivered order card is clicked (view-only).</summary>
        public event EventHandler<int>? ViewOrderDetailsRequested;

        private readonly OrderApiService _orderService = new();

        // Filter bar
        private TextBox txtSearch = null!;
        private ComboBox cmbStatus = null!;
        private DateTimePicker dtpFrom = null!;
        private DateTimePicker dtpTo = null!;
        private Button btnSearch = null!;

        // Toolbar
        private Button btnRefresh = null!;
        private Label lblCount = null!;

        // FAB
        private Button btnFab = null!;

        // Tab state
        private enum OrdersTab { Pending, Ready, PickedUp, History }
        private OrdersTab _currentTab = OrdersTab.Pending;
        private Button btnTabPending = null!;
        private Button btnTabReady = null!;
        private Button btnTabPickedUp = null!;
        private Button btnTabHistory = null!;

        // Order list
        private FlowLayoutPanel pnlOrderList = null!;

        private readonly List<OrderModel> _orders = new();

        public OrdersForm()
        {
            InitializeComponent();
            Load += async (s, e) => await LoadDataAsync();
        }

        private void InitializeComponent()
        {
            Text = "Order Management";
            Size = new Size(1300, 750);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Theme.BackgroundColor;
            Font = Theme.BodyFont;

            // ============ TABS ============
            var pnlTabs = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 10, 24, 10)
            };
            pnlTabs.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border, 1f);
                e.Graphics.DrawLine(pen, 0, pnlTabs.Height - 1, pnlTabs.Width, pnlTabs.Height - 1);
            };

            btnTabPending = CreateTabButton("Pending Orders", 24, 150, OrdersTab.Pending);
            pnlTabs.Controls.Add(btnTabPending);

            btnTabReady = CreateTabButton("Ready", 182, 110, OrdersTab.Ready);
            pnlTabs.Controls.Add(btnTabReady);

            btnTabPickedUp = CreateTabButton("Picked Up", 300, 120, OrdersTab.PickedUp);
            pnlTabs.Controls.Add(btnTabPickedUp);

            btnTabHistory = CreateTabButton("Transaction History", 428, 170, OrdersTab.History);
            pnlTabs.Controls.Add(btnTabHistory);

            // ============ TOP: FILTERS ============
            var pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 74,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 12, 24, 12)
            };
            pnlFilter.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border, 1f);
                e.Graphics.DrawLine(pen, 0, pnlFilter.Height - 1, pnlFilter.Width, pnlFilter.Height - 1);
            };

            var lblSearch = new Label { Text = "Search", Font = Typography.Small, ForeColor = Colors.TextSecondary, Location = new Point(24, 8), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(24, 28), Width = 230, Font = Typography.Body, BorderStyle = BorderStyle.FixedSingle };
            txtSearch.KeyPress += async (s, e) => { if (e.KeyChar == (char)Keys.Enter) await SearchAsync(); };

            var lblStatus = new Label { Text = "Status", Font = Typography.Small, ForeColor = Colors.TextSecondary, Location = new Point(270, 8), AutoSize = true };
            cmbStatus = new ComboBox { Location = new Point(270, 28), Width = 140, Font = Typography.Body, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.Add("All Statuses");
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += async (s, e) => await SearchAsync();

            var lblFrom = new Label { Text = "From", Font = Typography.Small, ForeColor = Colors.TextSecondary, Location = new Point(426, 8), AutoSize = true };
            dtpFrom = new DateTimePicker { Location = new Point(426, 28), Width = 120, Font = Typography.Body, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30) };

            var lblTo = new Label { Text = "To", Font = Typography.Small, ForeColor = Colors.TextSecondary, Location = new Point(560, 8), AutoSize = true };
            dtpTo = new DateTimePicker { Location = new Point(560, 28), Width = 120, Font = Typography.Body, Format = DateTimePickerFormat.Short, Value = DateTime.Today };

            btnSearch = new Button { Text = "Search", Location = new Point(696, 26), Width = 90, Height = 32 };
            Theme.StylePrimaryButton(btnSearch);
            btnSearch.Click += async (s, e) => await SearchAsync();

            btnRefresh = new Button { Text = "Refresh", Location = new Point(796, 26), Width = 90, Height = 32 };
            Theme.StyleSecondaryButton(btnRefresh);
            btnRefresh.Click += async (s, e) => await LoadDataAsync();

            lblCount = new Label
            {
                Text = "0 orders",
                Font = Typography.BodyBold,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlFilter.Width - 110, 32)
            };
            pnlFilter.Resize += (s, e) =>
            {
                lblCount.Left = pnlFilter.ClientSize.Width - lblCount.Width - 24;
            };

            pnlFilter.Controls.AddRange(new Control[]
            {
                lblSearch, txtSearch, lblStatus, cmbStatus,
                lblFrom, dtpFrom, lblTo, dtpTo, btnSearch, btnRefresh, lblCount
            });

            // ============ ORDER LIST (cards) ============
            pnlOrderList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Colors.Background,
                Padding = new Padding(24, 20, 24, 100)  // 100px bottom padding for FAB clearance
            };
            pnlOrderList.Resize += (s, e) => ResizeCards();

            // ============ FLOATING ACTION BUTTON ============
            btnFab = new Button
            {
                Size = new Size(58, 58),
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnFab.FlatAppearance.BorderSize = 0;
            btnFab.FlatAppearance.MouseOverBackColor = Colors.PrimaryHover;
            btnFab.Click += (s, e) => CreateOrder();

            // Draw a centered white "+"
            btnFab.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using var pen = new Pen(Color.White, 3f);
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                int cx = btnFab.Width / 2;
                int cy = btnFab.Height / 2;
                int arm = 14;

                g.DrawLine(pen, cx - arm, cy, cx + arm, cy);   // horizontal
                g.DrawLine(pen, cx, cy - arm, cx, cy + arm);   // vertical
            };

            // Clip to circle — apply immediately + on resize
            Action applyCircle = () =>
            {
                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddEllipse(0, 0, btnFab.Width, btnFab.Height);
                var oldRegion = btnFab.Region;
                btnFab.Region = new Region(path);
                oldRegion?.Dispose();
            };
            btnFab.SizeChanged += (s, e) => applyCircle();
            applyCircle();

            Controls.Add(pnlOrderList);
            Controls.Add(pnlFilter);
            Controls.Add(pnlTabs);
            Controls.Add(btnFab);
            btnFab.BringToFront();

            // Position FAB on form resize
            Action positionFab = () =>
            {
                btnFab.Location = new Point(
                    ClientSize.Width - btnFab.Width - 30,
                    ClientSize.Height - btnFab.Height - 30);
            };
            Resize += (s, e) => positionFab();
            positionFab();

            // Apply initial tab styles
            ApplyTabStyles();
        }

        private async Task LoadDataAsync()
        {
            await LoadStatusesAsync();

            var response = await _orderService.GetOrdersAsync(
                page: 1,
                pageSize: 500,
                search: string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text,
                statusCode: cmbStatus.SelectedIndex > 0 ? cmbStatus.SelectedItem?.ToString() : null,
                dateFrom: dtpFrom.Value.Date,
                dateTo: dtpTo.Value.Date);

            _orders.Clear();
            _orders.AddRange(response.Data);

            BuildOrderCards();

            int visibleCount = GetVisibleOrders().Count();
            lblCount.Text = $"{visibleCount} order{(visibleCount != 1 ? "s" : "")}";
        }

        private void BuildOrderCards()
        {
            pnlOrderList.SuspendLayout();
            pnlOrderList.Controls.Clear();

            foreach (var order in GetVisibleOrders())
            {
                var card = BuildOrderCard(order);
                pnlOrderList.Controls.Add(card);
            }

            pnlOrderList.ResumeLayout();
        }

        private static decimal GetPaidAmount(OrderModel o)
        {
            return o.Payments?.Sum(p => p.Amount) ?? 0m;
        }

        private bool IsFullyPaid(OrderModel o)
        {
            var paid = GetPaidAmount(o);
            return paid >= o.TotalAmount && o.TotalAmount > 0;
        }

        private IEnumerable<OrderModel> GetVisibleOrders()
        {
            return _currentTab switch
            {
                OrdersTab.Pending => _orders.Where(o =>
                    (o.StatusCode == "PE" || o.StatusCode == "PR") || IsPartiallyPaid(o)),

                OrdersTab.Ready => _orders.Where(o =>
                    o.StatusCode == "RD"),

                OrdersTab.PickedUp => _orders.Where(o =>
                    o.StatusCode == "PU" || o.StatusCode == "DE"),

                OrdersTab.History => _orders.Where(o =>
                    o.Payments != null && o.Payments.Count > 0),

                _ => _orders
            };
        }

        private bool IsPartiallyPaid(OrderModel o)
        {
            var paid = GetPaidAmount(o);
            return paid > 0 && paid < o.TotalAmount;
        }

        private void SwitchTab(OrdersTab tab)
        {
            _currentTab = tab;
            ApplyTabStyles();
            BuildOrderCards();

            int count = GetVisibleOrders().Count();
            lblCount.Text = $"{count} order{(count != 1 ? "s" : "")}";
        }

        private Button CreateTabButton(string text, int x, int width, OrdersTab tab)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, 10),
                Width = width,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => SwitchTab(tab);
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, 16, 16, 180, 90);
            path.AddArc(width - 16, 0, 16, 16, 270, 90);
            path.AddArc(width - 16, 36 - 16, 16, 16, 0, 90);
            path.AddArc(0, 36 - 16, 16, 16, 90, 90);
            path.CloseFigure();
            btn.Region = new Region(path);
            return btn;
        }

        private void ApplyTabStyles()
        {
            StyleTab(btnTabPending, _currentTab == OrdersTab.Pending);
            StyleTab(btnTabReady, _currentTab == OrdersTab.Ready);
            StyleTab(btnTabPickedUp, _currentTab == OrdersTab.PickedUp);
            StyleTab(btnTabHistory, _currentTab == OrdersTab.History);
        }

        private static void StyleTab(Button btn, bool isActive)
        {
            btn.BackColor = isActive ? Colors.Primary : Color.Transparent;
            btn.ForeColor = isActive ? Color.White : Colors.TextSecondary;
        }

        private void ResizeCards()
        {
            int targetWidth = pnlOrderList.ClientSize.Width - 48;
            if (targetWidth < 400) targetWidth = 400;

            foreach (Control c in pnlOrderList.Controls)
            {
                c.Width = targetWidth;
            }
        }

        private RoundedCard BuildOrderCard(OrderModel order)
        {
            int cardWidth = pnlOrderList.ClientSize.Width - 48;
            if (cardWidth < 400) cardWidth = 400;

            var card = new RoundedCard
            {
                Width = cardWidth,
                Height = 126,
                Margin = new Padding(0, 0, 0, 14),
                Padding = new Padding(24, 18, 24, 18),
                CornerRadius = 14,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                HoverBorderColor = Colors.Primary,
                EnableHover = true,
                ShowShadow = true,
                Cursor = Cursors.Hand,
                Tag = order
            };

            var lblOrder = new Label
            {
                Text = order.OrderNumber,
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                ForeColor = Colors.TextPrimary,
                Location = new Point(24, 18),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var (statusBg, statusFg) = GetStatusPillColors(order.StatusName);
            var badgeStatus = new PillBadge
            {
                BadgeText = order.StatusName ?? "Pending",
                FillColor = statusBg,
                TextColor = statusFg,
                Size = new Size(116, 26),
                CornerRadius = 13,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };

            var lblCustomer = new Label
            {
                Text = $"{order.CustomerName}   •   {order.CustomerPhone}",
                Font = Typography.Body,
                ForeColor = Colors.TextSecondary,
                Location = new Point(24, 52),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var lblDate = new Label
            {
                Text = _currentTab == OrdersTab.History && order.Payments != null && order.Payments.Count > 0
                    ? $"Paid: {order.Payments.Max(p => p.PaymentDate):yyyy-MM-dd HH:mm}"
                    : $"Ordered: {order.OrderDate:yyyy-MM-dd HH:mm}",
                Font = Typography.Small,
                ForeColor = Colors.TextMuted,
                Location = new Point(24, 86),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var lblTotal = new Label
            {
                Text = $"PHP {order.TotalAmount:N2}",
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                ForeColor = Colors.Primary,
                Size = new Size(160, 26),
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent
            };

            card.Controls.Add(lblOrder);
            card.Controls.Add(badgeStatus);
            card.Controls.Add(lblCustomer);
            card.Controls.Add(lblDate);
            card.Controls.Add(lblTotal);

            // Pay button — hidden if fully paid
            var totalPaid = order.Payments?.Sum(p => p.Amount) ?? 0m;
            bool isFullyPaid = totalPaid >= order.TotalAmount && order.TotalAmount > 0;

            var btnPay = new Button
            {
                Text = isFullyPaid ? "✓ Paid" : "Pay",
                Size = new Size(84, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = isFullyPaid ? Color.FromArgb(241, 245, 249) : Colors.Primary,
                ForeColor = isFullyPaid ? Colors.TextSecondary : Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = isFullyPaid ? Cursors.Default : Cursors.Hand,
                Enabled = !isFullyPaid
            };
            btnPay.FlatAppearance.BorderSize = 0;
            // Round pay button corners
            using (var bPath = new System.Drawing.Drawing2D.GraphicsPath())
            {
                bPath.AddArc(0, 0, 10, 10, 180, 90);
                bPath.AddArc(84 - 10, 0, 10, 10, 270, 90);
                bPath.AddArc(84 - 10, 32 - 10, 10, 10, 0, 90);
                bPath.AddArc(0, 32 - 10, 10, 10, 90, 90);
                bPath.CloseFigure();
                btnPay.Region = new Region(bPath);
            }
            btnPay.Click += (s, e) => PayOrderRequested?.Invoke(this, order.OrderId);
            card.Controls.Add(btnPay);

            Action layoutRight = () =>
            {
                badgeStatus.Location = new Point(card.Width - badgeStatus.Width - 24, 18);
                lblTotal.Location    = new Point(card.Width - lblTotal.Width - 24, 86);
                btnPay.Location      = new Point(card.Width - btnPay.Width - 24, 48);
            };
            card.Resize += (s, e) => layoutRight();
            layoutRight();

            // Card click → open the order directly
            EventHandler clickHandler = (s, e) =>
            {
                if (order.StatusCode == "PU" || order.StatusCode == "DE")
                {
                    ViewOrderDetailsRequested?.Invoke(this, order.OrderId);
                    return;
                }

                EditOrderRequested?.Invoke(this, order.OrderId);
            };
            card.Click += clickHandler;
            foreach (Control child in card.Controls)
                if (child is not Button) child.Click += clickHandler;

            return card;
        }

        private static (Color bg, Color fg) GetStatusPillColors(string? statusName)
        {
            return statusName?.Trim().ToLowerInvariant() switch
            {
                "pending" => (Color.FromArgb(254, 243, 199), Color.FromArgb(217, 119, 6)),
                "in progress" => (Color.FromArgb(239, 246, 255), Color.FromArgb(37, 99, 235)),
                "processing" => (Color.FromArgb(239, 246, 255), Color.FromArgb(37, 99, 235)),
                "ready" => (Color.FromArgb(209, 250, 229), Color.FromArgb(5, 150, 105)),
                "picked up" => (Color.FromArgb(236, 253, 245), Color.FromArgb(4, 120, 87)),
                "completed" => (Color.FromArgb(236, 253, 245), Color.FromArgb(4, 120, 87)),
                "delivered" => (Color.FromArgb(236, 253, 245), Color.FromArgb(4, 120, 87)),
                "cancelled" => (Color.FromArgb(255, 228, 230), Color.FromArgb(225, 29, 72)),
                _ => (Color.FromArgb(241, 245, 249), Color.FromArgb(100, 116, 139))
            };
        }

        private async Task LoadStatusesAsync()
        {
            if (cmbStatus.Items.Count > 1) return;

            var statuses = await _orderService.GetStatusesAsync();
            foreach (var s in statuses)
                cmbStatus.Items.Add(s.StatusName);
        }

        private async Task SearchAsync()
        {
            await LoadDataAsync();
        }

        private void CreateOrder()
        {
            NewOrderRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
