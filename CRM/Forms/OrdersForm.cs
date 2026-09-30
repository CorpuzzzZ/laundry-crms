using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
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
                Height = 52,
                BackColor = Color.White,
                Padding = new Padding(20, 8, 20, 8)
            };

            btnTabPending = new Button
            {
                Text = "Pending Orders",
                Location = new Point(20, 10),
                Width = 160,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTabPending.FlatAppearance.BorderSize = 0;
            btnTabPending.Click += (s, e) => SwitchTab(OrdersTab.Pending);
            pnlTabs.Controls.Add(btnTabPending);

            btnTabReady = new Button
            {
                Text = "Ready",
                Location = new Point(184, 10),
                Width = 120,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTabReady.FlatAppearance.BorderSize = 0;
            btnTabReady.Click += (s, e) => SwitchTab(OrdersTab.Ready);
            pnlTabs.Controls.Add(btnTabReady);

            btnTabPickedUp = new Button
            {
                Text = "Picked Up",
                Location = new Point(308, 10),
                Width = 130,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTabPickedUp.FlatAppearance.BorderSize = 0;
            btnTabPickedUp.Click += (s, e) => SwitchTab(OrdersTab.PickedUp);
            pnlTabs.Controls.Add(btnTabPickedUp);

            btnTabHistory = new Button
            {
                Text = "Transaction History",
                Location = new Point(442, 10),
                Width = 180,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTabHistory.FlatAppearance.BorderSize = 0;
            btnTabHistory.Click += (s, e) => SwitchTab(OrdersTab.History);
            pnlTabs.Controls.Add(btnTabHistory);

            // ============ TOP: FILTERS ============
            var pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(20, 15, 20, 15)
            };

            var lblSearch = new Label { Text = "Search", Font = Theme.SmallFont, ForeColor = Theme.TextLightColor, Location = new Point(20, 5), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(20, 25), Width = 250, Font = Theme.BodyFont, BorderStyle = BorderStyle.FixedSingle };
            txtSearch.KeyPress += async (s, e) => { if (e.KeyChar == (char)Keys.Enter) await SearchAsync(); };

            var lblStatus = new Label { Text = "Status", Font = Theme.SmallFont, ForeColor = Theme.TextLightColor, Location = new Point(290, 5), AutoSize = true };
            cmbStatus = new ComboBox { Location = new Point(290, 25), Width = 140, Font = Theme.BodyFont, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.Add("All Statuses");
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += async (s, e) => await SearchAsync();

            var lblFrom = new Label { Text = "From", Font = Theme.SmallFont, ForeColor = Theme.TextLightColor, Location = new Point(450, 5), AutoSize = true };
            dtpFrom = new DateTimePicker { Location = new Point(450, 25), Width = 130, Font = Theme.BodyFont, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30) };

            var lblTo = new Label { Text = "To", Font = Theme.SmallFont, ForeColor = Theme.TextLightColor, Location = new Point(600, 5), AutoSize = true };
            dtpTo = new DateTimePicker { Location = new Point(600, 25), Width = 130, Font = Theme.BodyFont, Format = DateTimePickerFormat.Short, Value = DateTime.Today };

            btnSearch = new Button { Text = "Search", Location = new Point(750, 23), Width = 100, Height = 32 };
            Theme.StylePrimaryButton(btnSearch);
            btnSearch.Click += async (s, e) => await SearchAsync();

            btnRefresh = new Button { Text = "Refresh", Location = new Point(860, 23), Width = 100, Height = 32 };
            Theme.StyleSecondaryButton(btnRefresh);
            btnRefresh.Click += async (s, e) => await LoadDataAsync();

            pnlFilter.Controls.AddRange(new Control[]
            {
                lblSearch, txtSearch, lblStatus, cmbStatus,
                lblFrom, dtpFrom, lblTo, dtpTo, btnSearch, btnRefresh
            });

            // ============ TOOLBAR ============
            var pnlToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };

            lblCount = new Label
            {
                Text = "0 orders",
                Font = Theme.BodyFont,
                ForeColor = Theme.TextLightColor,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            pnlToolbar.Controls.Add(lblCount);
            pnlToolbar.Resize += (s, e) =>
            {
                lblCount.Location = new Point(pnlToolbar.Width - lblCount.Width - 20, 22);
            };


            // ============ ORDER LIST (cards) ============
            pnlOrderList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Theme.BackgroundColor,
                Padding = new Padding(20, 20, 20, 100)  // 100px bottom padding for FAB clearance
            };
            pnlOrderList.Resize += (s, e) => ResizeCards();

            // ============ FLOATING ACTION BUTTON ============
            btnFab = new Button
            {
                Size = new Size(60, 60),
                FlatStyle = FlatStyle.Flat,
                BackColor = Theme.AccentColor,
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnFab.FlatAppearance.BorderSize = 0;
            btnFab.FlatAppearance.MouseOverBackColor = Theme.PrimaryColor;
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
            Controls.Add(pnlToolbar);
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

        private void ApplyTabStyles()
        {
            StyleTab(btnTabPending, _currentTab == OrdersTab.Pending);
            StyleTab(btnTabReady, _currentTab == OrdersTab.Ready);
            StyleTab(btnTabPickedUp, _currentTab == OrdersTab.PickedUp);
            StyleTab(btnTabHistory, _currentTab == OrdersTab.History);
        }

        private static void StyleTab(Button btn, bool isActive)
        {
            btn.BackColor = isActive ? Theme.AccentColor : Color.White;
            btn.ForeColor = isActive ? Color.White : Theme.TextLightColor;
        }

        private void ResizeCards()
        {
            int targetWidth = pnlOrderList.ClientSize.Width - 40;
            if (targetWidth < 400) targetWidth = 400;

            foreach (Control c in pnlOrderList.Controls)
            {
                c.Width = targetWidth;
            }
        }

        private RoundedCard BuildOrderCard(OrderModel order)
        {
            int cardWidth = pnlOrderList.ClientSize.Width - 40;
            if (cardWidth < 400) cardWidth = 400;

            var card = new RoundedCard
            {
                Width = cardWidth,
                Height = 120,
                Margin = new Padding(0, 0, 0, 14),
                Padding = new Padding(24, 16, 24, 16),
                CornerRadius = 10,
                FillColor = Color.White,
                BorderColor = Theme.BorderColor,
                HoverBorderColor = Theme.AccentColor,
                EnableHover = true,
                ShowShadow = false,
                Cursor = Cursors.Hand,
                Tag = order
            };

            var lblOrder = new Label
            {
                Text = order.OrderNumber,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Theme.TextDarkColor,
                Location = new Point(24, 18),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var lblStatus = new Label
            {
                Text = order.StatusName,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = GetStatusColor(order.StatusName),
                Size = new Size(110, 24),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblCustomer = new Label
            {
                Text = $"{order.CustomerName}   {order.CustomerPhone}",
                Font = Theme.BodyFont,
                ForeColor = Theme.TextLightColor,
                Location = new Point(24, 52),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var lblDate = new Label
            {
                Text = _currentTab == OrdersTab.History && order.Payments != null && order.Payments.Count > 0
                    ? $"Paid: {order.Payments.Max(p => p.PaymentDate):yyyy-MM-dd HH:mm}"
                    : order.OrderDate.ToString("yyyy-MM-dd HH:mm"),
                Font = Theme.SmallFont,
                ForeColor = Theme.TextLightColor,
                Location = new Point(24, 84),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var lblTotal = new Label
            {
                Text = $"PHP {order.TotalAmount:N2}",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Theme.AccentColor,
                Size = new Size(160, 26),
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent
            };

            card.Controls.Add(lblOrder);
            card.Controls.Add(lblStatus);
            card.Controls.Add(lblCustomer);
            card.Controls.Add(lblDate);
            card.Controls.Add(lblTotal);

            // Pay button — hidden if fully paid
            var totalPaid = order.Payments?.Sum(p => p.Amount) ?? 0m;
            bool isFullyPaid = totalPaid >= order.TotalAmount && order.TotalAmount > 0;

            var btnPay = new Button
            {
                Text = isFullyPaid ? "Paid" : "Pay",
                Size = new Size(80, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = isFullyPaid ? Theme.SuccessColor : Theme.AccentColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = isFullyPaid ? Cursors.Default : Cursors.Hand,
                Enabled = !isFullyPaid
            };
            btnPay.FlatAppearance.BorderSize = 0;
            btnPay.Click += (s, e) => PayOrderRequested?.Invoke(this, order.OrderId);
            card.Controls.Add(btnPay);

            Action layoutRight = () =>
            {
                lblStatus.Location = new Point(card.Width - lblStatus.Width - 24, 18);
                lblTotal.Location  = new Point(card.Width - lblTotal.Width - 24, 84);
                btnPay.Location = new Point(card.Width - btnPay.Width - 24, 46);
            };
            card.Resize += (s, e) => layoutRight();
            layoutRight();

            // Card click → open the order directly
            // Card click → open the order directly (unless locked)
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

        private static Color GetStatusColor(string? statusName)
        {
            return statusName?.ToLowerInvariant() switch
            {
                "pending" => Theme.WarningColor,
                "in progress" => Theme.AccentColor,
                "processing" => Theme.AccentColor,
                "ready" => Theme.SuccessColor,
                "picked up" => Theme.PrimaryColor,
                "completed" => Theme.SuccessColor,
                "delivered" => Theme.SuccessColor,
                "cancelled" => Theme.DangerColor,
                _ => Theme.TextLightColor
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
