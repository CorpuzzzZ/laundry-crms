using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRM.WinForms.Models;
using CRM.WinForms.Services;
using CRM.WinForms.UI;
using CRM.UI;
using CRM.UI.Controls;

namespace CRM.WinForms.Forms
{
    /// <summary>
    /// Full-page order editor — replaces the modal OrderDetailsForm dialog.
    /// Handles both create (orderId = 0) and edit modes.
    /// Uses docked layout throughout for professional, responsive rendering.
    /// </summary>
    public class OrderEditView : UserControl
    {
        public event EventHandler? Saved;
        public event EventHandler? Cancelled;

        private readonly OrderApiService _orderService = new();
        private readonly ServiceApiService _serviceApi = new(ApiClient.Instance);
        private readonly int _orderId;

        // Header
        private Button btnBack = null!;
        private Label lblTitle = null!;

        // Basic info
        private ComboBox cmbCustomer = null!;
        private Label lblOrderDate = null!;
        private TextBox txtNotes = null!;

        // Items
        private FlowLayoutPanel pnlItems = null!;
        private Button btnAddItem = null!;

        // Billing
        private Label lblSubtotal = null!;
        private Label lblChemical = null!;
        private Label lblMachine = null!;
        private Label lblTotal = null!;

        // Footer
        private Button btnCancel = null!;
        private Button btnSave = null!;

        private readonly List<OrderItemCard> _itemCards = new();

        public OrderEditView(int orderId)
        {
            _orderId = orderId;
            InitializeComponent();
            Load += async (s, e) => await LoadAsync();
        }

        private bool IsCreate => _orderId == 0;

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Background;

            // ═══════════════════════════════════════════════════════
            // FOOTER (Docked Bottom) — Cancel + Save Order
            // ═══════════════════════════════════════════════════════
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Colors.Surface,
                Padding = new Padding(Spacing.Xl, 0, Spacing.Xl, 0)
            };
            pnlFooter.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(120, 42),
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnCancel.FlatAppearance.BorderColor = Colors.Border;
            btnCancel.Click += (s, e) => Cancelled?.Invoke(this, EventArgs.Empty);
            pnlFooter.Controls.Add(btnCancel);

            btnSave = new Button
            {
                Text = "Save Order",
                Size = new Size(150, 42),
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += async (s, e) => await SaveAsync();
            pnlFooter.Controls.Add(btnSave);

            pnlFooter.Resize += (s, e) =>
            {
                btnSave.Location = new Point(pnlFooter.Width - btnSave.Width - Spacing.Xl, 14);
                btnCancel.Location = new Point(btnSave.Left - btnCancel.Width - Spacing.Sm, 14);
            };

            Controls.Add(pnlFooter);

            // ═══════════════════════════════════════════════════════
            // HEADER (Docked Top) — Back button + title
            // ═══════════════════════════════════════════════════════
            var pnlPageHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Colors.Surface
            };
            pnlPageHeader.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawLine(pen, 0, pnlPageHeader.Height - 1, pnlPageHeader.Width, pnlPageHeader.Height - 1);
            };

            btnBack = new Button
            {
                Text = "\u2190 Back to Orders",
                Location = new Point(Spacing.Xl, 16),
                Size = new Size(180, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextSecondary,
                Font = Typography.Body,
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderColor = Colors.Border;
            btnBack.Click += (s, e) => Cancelled?.Invoke(this, EventArgs.Empty);
            pnlPageHeader.Controls.Add(btnBack);

            lblTitle = new Label
            {
                Text = IsCreate ? "New Order" : "Edit Order",
                Font = Typography.H1,
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(230, 20)
            };
            pnlPageHeader.Controls.Add(lblTitle);

            Controls.Add(pnlPageHeader);

            // ═══════════════════════════════════════════════════════
            // BODY (Docked Fill) — scrollable content
            // ═══════════════════════════════════════════════════════
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Colors.Background,
                Padding = new Padding(Spacing.Xl, Spacing.Lg, Spacing.Xl, Spacing.Lg)
            };
            Controls.Add(pnlBody);
            pnlBody.BringToFront();

            // Layout helper — the body is a vertical stack of cards,
            // each docked Top so they auto-size
            var pnlStack = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Colors.Background
            };
            pnlBody.Controls.Add(pnlStack);

            // ── Customer & Order card ──
            var cardCustomer = new RoundedCard
            {
                Dock = DockStyle.Top,
                Height = 180,
                Padding = new Padding(Spacing.Xl),
                CornerRadius = 8,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true,
                Margin = new Padding(0, 0, 0, Spacing.Xl)
            };
            pnlStack.Controls.Add(cardCustomer);
            BuildCustomerCard(cardCustomer);

            // ── Items card ──
            var cardItems = new RoundedCard
            {
                Dock = DockStyle.Top,
                Height = 520,
                Padding = new Padding(0),
                CornerRadius = 8,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true,
                Margin = new Padding(0, 0, 0, Spacing.Xl)
            };
            pnlStack.Controls.Add(cardItems);
            BuildItemsCard(cardItems);

            // ── Billing card ──
            var cardBilling = new RoundedCard
            {
                Dock = DockStyle.Top,
                Height = 240,
                Padding = new Padding(Spacing.Xl),
                CornerRadius = 8,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true,
                Margin = new Padding(0, 0, 0, Spacing.Lg)
            };
            pnlStack.Controls.Add(cardBilling);
            BuildBillingCard(cardBilling);

            // Order matters: Dock=Top stacks in REVERSE add order
            // We want: customer (top), items (middle), billing (bottom)
            // So add in reverse: billing first, then items, then customer
            // Actually WinForms docks the LAST-added control to the top.
            // Current add order: customer, items, billing → renders bottom-up
            // Fix by reversing the order: billing, items, customer
            // ... but we already added them. Instead, use Dock=Top with explicit order.
            // Simplify: re-add in reverse
            pnlStack.Controls.Clear();
            pnlStack.Controls.Add(cardBilling);
            pnlStack.Controls.Add(cardItems);
            pnlStack.Controls.Add(cardCustomer);
        }

        private void BuildCustomerCard(RoundedCard card)
        {
            int cx = Spacing.Lg;
            int y = Spacing.Lg;

            card.Controls.Add(new Label
            {
                Text = "CUSTOMER & ORDER",
                Font = Typography.TinyUpper,
                ForeColor = Colors.TextMuted,
                Location = new Point(cx, y),
                AutoSize = true
            });
            y += 32;

            // Row 1: Customer | Order Date
            int col1W = 380;
            int col2X = cx + col1W + Spacing.Xl;
            int col2W = 260;

            card.Controls.Add(MakeLabel("Customer *", cx, y));
            cmbCustomer = new ComboBox
            {
                Location = new Point(cx, y + 20),
                Width = col1W,
                Font = Typography.Body,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            card.Controls.Add(cmbCustomer);

            card.Controls.Add(MakeLabel("Order Date", col2X, y));
            lblOrderDate = new Label
            {
                Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                Location = new Point(col2X, y + 20),
                Size = new Size(col2W, 32),
                Font = Typography.Body,
                ForeColor = Colors.TextPrimary,
                BackColor = Colors.Background,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 0, 0)
            };
            card.Controls.Add(lblOrderDate);
            y += 70;

            // Row 2: Notes (full width)
            card.Controls.Add(MakeLabel("Notes (optional)", cx, y));
            txtNotes = new TextBox
            {
                Location = new Point(cx, y + 20),
                Width = card.Width - cx * 2 - 20,
                Height = 32,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(txtNotes);
        }

        private void BuildItemsCard(RoundedCard card)
        {
            // ── Top bar (Docked Top) ──
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Colors.Surface,
                Padding = new Padding(Spacing.Lg, 14, Spacing.Lg, 14)
            };

            pnlHeader.Controls.Add(new Label
            {
                Text = "ITEMS",
                Font = Typography.TinyUpper,
                ForeColor = Colors.TextMuted,
                Location = new Point(Spacing.Lg, 20),
                AutoSize = true
            });

            btnAddItem = new Button
            {
                Text = "+ Add Item",
                Size = new Size(130, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Success,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnAddItem.FlatAppearance.BorderSize = 0;
            btnAddItem.Click += async (s, e) => await AddItemAsync();
            pnlHeader.Controls.Add(btnAddItem);
            pnlHeader.Resize += (s, e) =>
            {
                btnAddItem.Location = new Point(pnlHeader.Width - btnAddItem.Width - Spacing.Lg, 10);
            };

            card.Controls.Add(pnlHeader);

            // ── Items area (Docked Fill) ──
            pnlItems = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Colors.Background,
                Padding = new Padding(Spacing.Lg, Spacing.Lg, Spacing.Lg, Spacing.Lg)
            };
            card.Controls.Add(pnlItems);
            pnlItems.BringToFront();

            pnlItems.Resize += (s, e) =>
            {
                int targetWidth = Math.Max(600, pnlItems.ClientSize.Width - 20);
                foreach (Control c in pnlItems.Controls)
                {
                    if (c is OrderItemCard ic)
                        ic.Width = targetWidth;
                }
            };
        }

        private void BuildBillingCard(RoundedCard card)
        {
            int cx = Spacing.Lg;
            int y = Spacing.Lg;

            card.Controls.Add(new Label
            {
                Text = "BILLING SUMMARY",
                Font = Typography.TinyUpper,
                ForeColor = Colors.TextMuted,
                Location = new Point(cx, y),
                AutoSize = true
            });
            y += 32;

            lblSubtotal = AddBillRow(card, "Items Subtotal", y);
            lblChemical = AddBillRow(card, "Chemical Add-ons", y + 34);
            lblMachine = AddBillRow(card, "Machine Add-ons", y + 68);

            // Total row
            var lblTotalCaption = new Label
            {
                Text = "TOTAL",
                Font = Typography.H3,
                ForeColor = Colors.TextPrimary,
                Location = new Point(cx, y + 116),
                AutoSize = true
            };
            card.Controls.Add(lblTotalCaption);

            lblTotal = new Label
            {
                Text = "PHP 0.00",
                Font = Typography.H2,
                ForeColor = Colors.Primary,
                Location = new Point(cx + 400, y + 114),
                Width = 300,
                Height = 30,
                TextAlign = ContentAlignment.MiddleRight
            };
            card.Controls.Add(lblTotal);
        }

        private Label AddBillRow(Control parent, string caption, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = caption,
                Font = Typography.Body,
                ForeColor = Colors.TextSecondary,
                Location = new Point(Spacing.Lg, y),
                AutoSize = true
            });

            var value = new Label
            {
                Text = "PHP 0.00",
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                Location = new Point(Spacing.Lg + 400, y),
                Width = 300,
                Height = 22,
                TextAlign = ContentAlignment.MiddleRight
            };
            parent.Controls.Add(value);
            return value;
        }

        private Label MakeLabel(string text, int x, int y) => new Label
        {
            Text = text,
            Font = Typography.Small,
            ForeColor = Colors.TextSecondary,
            Location = new Point(x, y),
            AutoSize = true
        };

        private async Task LoadAsync()
        {
            // Customer list (placeholder until CustomerApiService is wired)
            cmbCustomer.Items.Clear();
            cmbCustomer.Items.Add(new ComboItem(1, "Walk-in Customer"));
            cmbCustomer.SelectedIndex = 0;

            // Edit mode: load existing order
            if (!IsCreate)
            {
                try
                {
                    var order = await _orderService.GetOrderByIdAsync(_orderId);
                    if (order != null)
                    {
                        lblTitle.Text = $"Edit Order #{order.OrderNumber}";
                        txtNotes.Text = order.Notes ?? "";
                        lblOrderDate.Text = order.OrderDate.ToString("yyyy-MM-dd HH:mm");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load order: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            await AddItemAsync();
            RecalculateBilling();

            // Apply permission gates
            var user = SessionManager.CurrentUser;
            bool canModify = user?.CanModifyOrders == true;
            btnSave.Visible = canModify;
            if (!canModify)
            {
                cmbCustomer.Enabled = false;
                txtNotes.ReadOnly = true;
                btnAddItem.Enabled = false;
            }
        }

        private async Task AddItemAsync()
        {
            var card = new OrderItemCard();
            card.ItemChanged += (s, e) => RecalculateBilling();
            card.RemoveRequested += (s, e) =>
            {
                if (_itemCards.Count <= 1)
                {
                    MessageBox.Show("At least one item is required.", "Cannot Remove",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _itemCards.Remove(card);
                pnlItems.Controls.Remove(card);
                RecalculateBilling();
            };

            // Set width BEFORE adding so it fits
            card.Width = Math.Max(600, pnlItems.ClientSize.Width - 20);

            _itemCards.Add(card);
            pnlItems.Controls.Add(card);

            // Re-apply after add
            card.Width = Math.Max(600, pnlItems.ClientSize.Width - 20);

            await card.InitializeDataAsync(_serviceApi);
        }

        private void RecalculateBilling()
        {
            decimal sub = _itemCards.Sum(c => c.ServiceLineTotal);
            decimal chem = _itemCards.Sum(c => c.ChemicalTotal);
            decimal mach = _itemCards.Sum(c => c.MachineTotal);
            decimal total = sub + chem + mach;

            lblSubtotal.Text = $"PHP {sub:N2}";
            lblChemical.Text = $"PHP {chem:N2}";
            lblMachine.Text = $"PHP {mach:N2}";
            lblTotal.Text = $"PHP {total:N2}";
        }

        private async Task SaveAsync()
        {
            if (cmbCustomer.SelectedItem == null)
            {
                MessageBox.Show("Please select a customer.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_itemCards.Count == 0)
            {
                MessageBox.Show("Please add at least one item.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var items = new List<CreateOrderItemRequest>();
            foreach (var card in _itemCards)
            {
                if (!card.IsValid(out string err))
                {
                    MessageBox.Show(err, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                items.Add(new CreateOrderItemRequest
                {
                    ServiceId = 1,
                    Quantity = 1,
                    UnitPrice = card.ServiceLineTotal,
                    DiscountAmount = 0
                });
            }

            var now = DateTime.UtcNow;
            var request = new CreateOrderRequest
            {
                CustomerId = (cmbCustomer.SelectedItem as ComboItem)?.Value ?? 1,
                BranchId = 1,
                OrderType = "WalkIn",
                Priority = "Normal",
                Notes = txtNotes.Text,
                PickupDate = now,
                DeliveryDate = now.AddDays(2),
                Items = items
            };

            btnSave.Enabled = false;
            btnSave.Text = "Saving...";

            try
            {
                if (IsCreate)
                {
                    var (success, order, error) = await _orderService.CreateOrderAsync(request);
                    if (success)
                    {
                        MessageBox.Show($"Order {order?.OrderNumber} created successfully.",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Saved?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        MessageBox.Show($"Failed: {error}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Update not yet implemented.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            finally
            {
                btnSave.Enabled = true;
                btnSave.Text = "Save Order";
            }
        }
    }
}