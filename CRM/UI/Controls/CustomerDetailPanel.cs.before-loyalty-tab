using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CRM.UI;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.UI.Controls
{
    public class CustomerDetailPanel : UserControl
    {
        public event EventHandler<int>? EditRequested;
        public event EventHandler? BackRequested;

        private readonly int _customerId;
        private readonly CustomerInteractionApiService _interactionService;

        private Panel _tabContent = null!;
        private Panel _profileTab = null!;
        private Panel _ordersTab = null!;
        private Panel _interactionsTab = null!;

        private Button _tabProfileBtn = null!;
        private Button _tabOrdersBtn = null!;
        private Button _tabInteractionsBtn = null!;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string CustomerName { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string CustomerCode { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string CustomerType { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string Email { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string PhonePrimary { get; set; } = "";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string Street { get; set; } = "";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string Village { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string City { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string State { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string PostalCode { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string Country { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string Notes { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int LoyaltyPoints { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public decimal LifetimeSpend { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int TotalOrders { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public DateTime? LastOrderDate { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public DateTime CreatedAt { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool IsActive { get; set; } = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string InitialTab { get; set; } = "profile";

        public CustomerDetailPanel(int customerId)
        {
            _customerId = customerId;
            Dock = DockStyle.Fill;
            BackColor = Colors.Background;
            _interactionService = new CustomerInteractionApiService(ApiClient.Instance);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            BuildUI();
        }

        private void BuildUI()
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 16, 24, 16)
            };
            header.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
            };

            var btnBack = new Button
            {
                Text = "< Back to list",
                Location = new Point(24, 18),
                Width = 130,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.Primary,
                Font = Typography.Body,
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderColor = Colors.Border;
            btnBack.Click += (s, e) => BackRequested?.Invoke(this, EventArgs.Empty);

            var btnEdit = new Button
            {
                Text = "Edit Profile",
                Width = 110,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += (s, e) => EditRequested?.Invoke(this, _customerId);

            // Gate visibility: Admin + Crew only
            var currentUser = CRM.WinForms.Services.SessionManager.CurrentUser;
            bool canEditCustomer = currentUser?.IsAdmin == true || currentUser?.IsCrew == true;
            btnEdit.Visible = canEditCustomer;

            header.Controls.Add(btnBack);
            header.Controls.Add(btnEdit);
            header.Resize += (s, e) =>
            {
                btnEdit.Location = new Point(header.Width - btnEdit.Width - 24, 18);
            };

            var identity = new Panel
            {
                Dock = DockStyle.Top,
                Height = 140,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 16, 24, 0)
            };

            var avatar = new Label
            {
                Text = GetInitials(CustomerName),
                Font = new Font(Typography.Family, 20f, FontStyle.Bold),
                ForeColor = Colors.Primary,
                BackColor = Colors.PrimaryLight,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(64, 64),
                Location = new Point(24, 16)
            };
            identity.Controls.Add(avatar);

            var nameLbl = new Label
            {
                Text = CustomerName,
                Font = new Font(Typography.Family, 16f, FontStyle.Bold),
                ForeColor = Colors.TextPrimary,
                AutoSize = false,
                Width = 500,
                Height = 28,
                Location = new Point(104, 20)
            };
            identity.Controls.Add(nameLbl);

            var subLbl = new Label
            {
                Text = CustomerCode + "  |  " + CustomerType,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = false,
                Width = 500,
                Height = 20,
                Location = new Point(104, 48)
            };
            identity.Controls.Add(subLbl);

            var statusBadge = new Label
            {
                Text = IsActive ? "Active" : "Inactive",
                Font = Typography.SmallBold,
                ForeColor = IsActive ? Colors.Success : Colors.TextMuted,
                BackColor = IsActive ? Colors.SuccessLight : Color.FromArgb(0xEC, 0xF0, 0xF1),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(80, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            statusBadge.Location = new Point(identity.Width - 104, 24);
            identity.Controls.Add(statusBadge);

            var tabs = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                BackColor = Colors.Surface
            };

            _tabProfileBtn = CreateTab("Profile", "profile", 24);
            _tabOrdersBtn = CreateTab("Orders", "orders", 124);
            _tabInteractionsBtn = CreateTab("Interactions", "interactions", 224);

            tabs.Controls.Add(_tabProfileBtn);
            tabs.Controls.Add(_tabOrdersBtn);
            tabs.Controls.Add(_tabInteractionsBtn);
            tabs.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawLine(pen, 0, tabs.Height - 1, tabs.Width, tabs.Height - 1);
            };

            identity.Controls.Add(tabs);

            _tabContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Background,
                Padding = new Padding(24)
            };

            _profileTab = BuildProfileTab();
            _ordersTab = BuildOrdersTab();
            _interactionsTab = BuildInteractionsTab();

            _profileTab.Dock = DockStyle.Fill;
            _ordersTab.Dock = DockStyle.Fill;
            _interactionsTab.Dock = DockStyle.Fill;

            _tabContent.Controls.Add(_profileTab);
            _tabContent.Controls.Add(_ordersTab);
            _tabContent.Controls.Add(_interactionsTab);

            Controls.Add(_tabContent);
            Controls.Add(identity);
            Controls.Add(header);

            SwitchTab(string.IsNullOrEmpty(InitialTab) ? "profile" : InitialTab);
        }

        private Button CreateTab(string text, string key, int left)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(left, 0),
                Width = 100,
                Height = 44,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextSecondary,
                Font = Typography.Body,
                Cursor = Cursors.Hand,
                Tag = key
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => SwitchTab(key);
            return btn;
        }

        private void SwitchTab(string key)
        {
            _profileTab.Visible = key == "profile";
            _ordersTab.Visible = key == "orders";
            _interactionsTab.Visible = key == "interactions";

            _tabProfileBtn.ForeColor = key == "profile" ? Colors.Primary : Colors.TextSecondary;
            _tabOrdersBtn.ForeColor = key == "orders" ? Colors.Primary : Colors.TextSecondary;
            _tabInteractionsBtn.ForeColor = key == "interactions" ? Colors.Primary : Colors.TextSecondary;

            _tabProfileBtn.Font = key == "profile" ? Typography.BodyBold : Typography.Body;
            _tabOrdersBtn.Font = key == "orders" ? Typography.BodyBold : Typography.Body;
            _tabInteractionsBtn.Font = key == "interactions" ? Typography.BodyBold : Typography.Body;

            if (key == "orders") _ = LoadOrdersAsync();
            else if (key == "interactions") _ = LoadInteractionsAsync();
        }
        private Panel BuildProfileTab()
        {
            var host = new Panel { BackColor = Colors.Background, AutoScroll = true };

            var card = new Panel
            {
                BackColor = Colors.Surface,
                Padding = new Padding(24),
                Width = 900,
                Height = 800,
                Location = new Point(0, 0)
            };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            int y = 24;
            int labelWidth = 160;

            AddSection(card, "Contact", ref y);
            AddField(card, "Email",             Email, ref y, labelWidth);
            AddField(card, "Phone (primary)",   PhonePrimary, ref y, labelWidth);
            AddField(card, "Street", Street, ref y, labelWidth);
            AddField(card, "Village", Village, ref y, labelWidth);
            y += 20;

            AddSection(card, "Address", ref y);
            AddField(card, "City",        City, ref y, labelWidth);
            AddField(card, "State",       State, ref y, labelWidth);
            AddField(card, "Postal Code", PostalCode, ref y, labelWidth);
            AddField(card, "Country",     Country, ref y, labelWidth);
            y += 20;

            AddSection(card, "Loyalty and Stats", ref y);
            AddField(card, "Loyalty Points",  LoyaltyPoints.ToString("N0"), ref y, labelWidth);
            AddField(card, "Lifetime Spend",  "P " + LifetimeSpend.ToString("N2"), ref y, labelWidth);
            AddField(card, "Total Orders",    TotalOrders.ToString("N0"), ref y, labelWidth);
            AddField(card, "Last Order",      LastOrderDate?.ToString("yyyy-MM-dd") ?? "Never", ref y, labelWidth);
            AddField(card, "Customer Since",  CreatedAt.ToString("yyyy-MM-dd"), ref y, labelWidth);
            y += 20;

            AddSection(card, "Notes", ref y);
            var notes = new Label
            {
                Text = string.IsNullOrWhiteSpace(Notes) ? "(no notes)" : Notes,
                Font = Typography.Body,
                ForeColor = string.IsNullOrWhiteSpace(Notes) ? Colors.TextMuted : Colors.TextBody,
                AutoSize = false,
                Width = card.Width - 48,
                Height = 80,
                Location = new Point(24, y)
            };
            card.Controls.Add(notes);

            card.Height = y + 100;

            host.Controls.Add(card);
            return host;
        }

        private Panel BuildOrdersTab()
        {
            var host = new Panel { BackColor = Colors.Background };

            var header = new Label
            {
                Name = "OrdersHeader",
                Text = "Loading order history...",
                Font = Typography.H2,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 36,
                Padding = new Padding(0, 6, 0, 0)
            };
            host.Controls.Add(header);

            var list = new FlowLayoutPanel
            {
                Name = "OrdersList",
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Colors.Background
            };
            host.Controls.Add(list);

            return host;
        }

        private async System.Threading.Tasks.Task LoadOrdersAsync()
        {
            var header = _ordersTab.Controls["OrdersHeader"] as Label;
            var list = _ordersTab.Controls["OrdersList"] as FlowLayoutPanel;
            if (header == null || list == null) return;

            list.Controls.Clear();
            header.Text = "Loading...";

            try
            {
                var orders = await ApiClient.Instance.GetDataAsync<List<OrderModel>>(
                    "/api/v1/orders?customerId=" + _customerId + "&page=1&pageSize=100");

                if (orders == null || orders.Count == 0)
                {
                    header.Text = "No orders found for this customer.";
                    return;
                }

                header.Text = "Order History (" + orders.Count + ")";

                foreach (var order in orders)
                {
                    var card = new Panel
                    {
                        BackColor = Colors.Surface,
                        Height = 84,
                        Width = list.ClientSize.Width - 20,
                        Padding = new Padding(16),
                        Margin = new Padding(0, 0, 0, 8),
                        Anchor = AnchorStyles.Left | AnchorStyles.Right
                    };
                    card.Paint += (s, e) =>
                    {
                        using var pen = new Pen(Colors.Border);
                        e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                    };

                    var no = new Label
                    {
                        Text = order.OrderNumber,
                        Font = Typography.BodyBold,
                        ForeColor = Colors.Primary,
                        AutoSize = true,
                        Location = new Point(16, 12)
                    };
                    card.Controls.Add(no);

                    var date = new Label
                    {
                        Text = order.OrderDate.ToString("yyyy-MM-dd HH:mm"),
                        Font = Typography.Small,
                        ForeColor = Colors.TextSecondary,
                        AutoSize = true,
                        Location = new Point(16, 38)
                    };
                    card.Controls.Add(date);

                    var status = new Label
                    {
                        Text = order.StatusName,
                        Font = Typography.SmallBold,
                        ForeColor = Colors.TextPrimary,
                        AutoSize = true,
                        Location = new Point(220, 24)
                    };
                    card.Controls.Add(status);

                    var total = new Label
                    {
                        Text = "P " + order.TotalAmount.ToString("N2"),
                        Font = Typography.BodyBold,
                        ForeColor = Colors.TextPrimary,
                        AutoSize = true,
                        Anchor = AnchorStyles.Top | AnchorStyles.Right
                    };
                    total.Location = new Point(card.Width - 120, 26);
                    card.Controls.Add(total);

                    list.Controls.Add(card);
                }
            }
            catch (Exception ex)
            {
                header.Text = "Failed to load orders: " + ex.Message;
            }
        }

        private Panel BuildInteractionsTab()
        {
            var host = new Panel { BackColor = Colors.Background };

            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Colors.Background
            };

            var btnInquiry = MakeActionButton("+ Record Inquiry", Colors.Primary, 0);
            btnInquiry.Click += (s, e) => OpenRecordDialog(InteractionType.Inquiry);

            var btnComplaint = MakeActionButton("+ Record Complaint", Colors.Danger, 160);
            btnComplaint.Click += (s, e) => OpenRecordDialog(InteractionType.Complaint);

            var btnFeedback = MakeActionButton("+ Record Feedback", Colors.Success, 330);
            btnFeedback.Click += (s, e) => OpenRecordDialog(InteractionType.Feedback);

            toolbar.Controls.Add(btnInquiry);
            toolbar.Controls.Add(btnComplaint);
            toolbar.Controls.Add(btnFeedback);

            var header = new Label
            {
                Name = "InteractionsHeader",
                Text = "Loading...",
                Font = Typography.H2,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 36,
                Padding = new Padding(0, 6, 0, 0)
            };

            var list = new FlowLayoutPanel
            {
                Name = "InteractionsList",
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Colors.Background
            };
            list.Resize += (s, e) =>
            {
                int w = list.ClientSize.Width - 24;
                if (w < 200) return;
                foreach (Control c in list.Controls)
                {
                    if (c is Panel p && p.Width != w)
                        p.Width = w;
                }
            };
            list.Resize += (s, e) =>
            {
                int w = list.ClientSize.Width - 24;
                if (w < 100) return;
                foreach (Control c in list.Controls)
                {
                    if (c is Panel p && c.Width != w)
                        p.Width = w;
                }
            };

            host.Controls.Add(list);
            host.Controls.Add(header);
            host.Controls.Add(toolbar);

            return host;
        }

        private static Button MakeActionButton(string text, Color color, int left)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(left, 10),
                Width = 150,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private async System.Threading.Tasks.Task LoadInteractionsAsync()
        {
            var header = _interactionsTab.Controls["InteractionsHeader"] as Label;
            var list = _interactionsTab.Controls["InteractionsList"] as FlowLayoutPanel;
            if (header == null || list == null) return;

            list.Controls.Clear();
            header.Text = "Loading...";

            var items = await _interactionService.GetForCustomerAsync(_customerId);

            if (items.Count == 0)
            {
                header.Text = "No interactions recorded yet.";
                return;
            }

            header.Text = "Interactions (" + items.Count + ")";

            foreach (var item in items)
            {
                var card = BuildInteractionCard(item);
                card.Width = list.ClientSize.Width - 24;
                list.Controls.Add(card);
            }
        }

        private Panel BuildInteractionCard(CustomerInteractionModel interaction)
        {
            var typeColor = interaction.InteractionType == 1 ? Colors.Primary
                          : interaction.InteractionType == 2 ? Colors.Danger
                          : interaction.InteractionType == 3 ? Colors.Success
                          : Colors.TextMuted;

            var statusColor = interaction.Status == 1 ? Colors.Warning
                            : interaction.Status == 2 ? Colors.Primary
                            : interaction.Status == 3 ? Colors.Success
                            : Colors.TextMuted;

            var card = new RoundedCard
            {
                Height = 108,
                Width = 600,
                Margin = new Padding(0, 0, 0, 12),
                Cursor = Cursors.Hand,
                CornerRadius = 8,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                HoverBorderColor = Colors.Primary,
                AccentColor = typeColor,
                AccentWidth = 4,
                EnableHover = true,
                ShowShadow = true,
                Padding = new Padding(20, 14, 20, 14)
            };

            var typeLbl = new Label
            {
                Text = interaction.InteractionTypeName.ToUpperInvariant(),
                Font = Typography.TinyUpper,
                ForeColor = typeColor,
                AutoSize = true,
                Location = new Point(20, 14),
                BackColor = Color.Transparent
            };
            card.Controls.Add(typeLbl);

            var priorityLbl = new Label
            {
                Text = interaction.PriorityName,
                Font = Typography.Tiny,
                ForeColor = Colors.TextMuted,
                AutoSize = true,
                Location = new Point(typeLbl.Right + 12, 16),
                BackColor = Color.Transparent
            };
            card.Controls.Add(priorityLbl);

            var statusBadge = new Label
            {
                Text = interaction.StatusName,
                Font = Typography.Tiny,
                ForeColor = statusColor,
                BackColor = Color.FromArgb(30, statusColor),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(90, 22),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            statusBadge.Location = new Point(card.Width - 110, 14);
            card.Controls.Add(statusBadge);

            var subject = new Label
            {
                Text = interaction.Subject,
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                AutoSize = false,
                Height = 20,
                Width = card.Width - 40,
                Location = new Point(20, 42),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            card.Controls.Add(subject);

            var preview = interaction.Description.Length > 110
                ? interaction.Description.Substring(0, 110) + "..."
                : interaction.Description;

            var previewLbl = new Label
            {
                Text = preview,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = false,
                Height = 18,
                Width = card.Width - 40,
                Location = new Point(20, 64),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            card.Controls.Add(previewLbl);

            var dateLbl = new Label
            {
                Text = interaction.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                Font = Typography.Tiny,
                ForeColor = Colors.TextMuted,
                AutoSize = true,
                Location = new Point(20, 86),
                BackColor = Color.Transparent
            };
            card.Controls.Add(dateLbl);

            EventHandler onClick = (s, e) => OpenInteractionDetail(interaction);
            AttachClickRecursive(card, onClick);

            return card;
        }

        private static void AttachClickRecursive(Control parent, EventHandler handler)
        {
            parent.Click += handler;
            foreach (Control child in parent.Controls)
                AttachClickRecursive(child, handler);
        }

        private void OpenRecordDialog(InteractionType type)
        {
            using var dlg = new CustomerInteractionDialog(_customerId, type);
            if (dlg.ShowDialog(this) == DialogResult.OK)
                _ = LoadInteractionsAsync();
        }

        private void OpenInteractionDetail(CustomerInteractionModel interaction)
        {
            try
            {
                Control? parent = this.Parent;
                while (parent != null && !(parent is CRM.WinForms.Forms.CustomersView))
                    parent = parent.Parent;

                if (parent is CRM.WinForms.Forms.CustomersView view)
                {
                    view.ShowInteractionDetail(interaction);
                }
                else
                {
                    MessageBox.Show("Could not find the parent CustomersView.", "Navigation error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to open interaction detail: " + ex.GetType().Name + ": " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static void AddSection(Panel parent, string title, ref int y)
        {
            var section = new Label
            {
                Text = title.ToUpperInvariant(),
                Font = Typography.TinyUpper,
                ForeColor = Colors.TextMuted,
                AutoSize = true,
                Location = new Point(24, y)
            };
            parent.Controls.Add(section);
            y += 26;
        }

        private static void AddField(Panel parent, string label, string value, ref int y, int labelWidth)
        {
            var lbl = new Label
            {
                Text = label,
                Font = Typography.Body,
                ForeColor = Colors.TextSecondary,
                AutoSize = false,
                Width = labelWidth,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(24, y)
            };
            parent.Controls.Add(lbl);

            var val = new Label
            {
                Text = string.IsNullOrWhiteSpace(value) ? "(not set)" : value,
                Font = Typography.BodyBold,
                ForeColor = string.IsNullOrWhiteSpace(value) ? Colors.TextMuted : Colors.TextPrimary,
                AutoSize = false,
                Width = parent.Width - 48 - labelWidth - 24,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(24 + labelWidth, y)
            };
            parent.Controls.Add(val);

            y += 28;
        }

        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpperInvariant();
        }
    }
}
