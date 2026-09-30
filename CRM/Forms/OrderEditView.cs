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
    /// Full-page order editor â€” replaces the modal OrderDetailsForm dialog.
    /// Handles both create (orderId = 0) and edit modes.
    /// Uses docked layout throughout for professional, responsive rendering.
    /// </summary>
    public class OrderEditView : UserControl
    {
        public event EventHandler? Saved;
        public event EventHandler? Cancelled;

        private readonly OrderApiService _orderService = new();
        private readonly ServiceApiService _serviceApi = new(ApiClient.Instance);
        private readonly CustomerApiService _customerApi = new();
        private readonly int _orderId;

        // Header
        private Button btnBack = null!;
        private Label lblTitle = null!;
        private Button btnMarkReady = null!;
        private Button btnMarkPickedUp = null!;
        private string _currentStatusCode = string.Empty;

        // Basic info
        private ComboBox cmbCustomer = null!;
        private Label lblOrderDate = null!;
        private TextBox txtNotes = null!;

        // Items
        private Panel pnlItems = null!;
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

        // Edit mode: add-ons that were on the existing order (locked, can't uncheck)
        private readonly List<string> _editModeAddOns = new();

        private string? _editModeCategory;
        private decimal? _editModeWeight;
        private int _editModeServiceId;
        private CRM.WinForms.Models.OrderModel? _loadedOrder;
        private bool _isLocked = false;

        public OrderEditView(int orderId)
        {
            _orderId = orderId;
            InitializeComponent();
            System.IO.File.AppendAllText(@"C:\temp\order_debug.log", $"[OrderEditView ctor] this.Bounds={this.Bounds}, this.Dock={this.Dock}, parent={this.Parent?.GetType().Name}, parentBounds={this.Parent?.Bounds}`r`n");
            Load += async (s, e) => await LoadAsync();
        }

        private bool IsCreate => _orderId == 0;


        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;

            // Force a reasonable starting size so docked children lay out correctly
            // before MainForm parents us (which will resize us to fill anyway)
            Width = 1200;
            Height = 800;
            BackColor = Colors.Background;

            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
            // FOOTER (Docked Bottom) â€” Cancel + Save Order
            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
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

            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
            // HEADER (Docked Top) â€” Back button + title
            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
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

            // Status workflow buttons (right-aligned in header)
            btnMarkPickedUp = new Button
            {
                Text = "Mark as Picked Up",
                Size = new Size(170, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.Body,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Visible = false
            };
            btnMarkPickedUp.FlatAppearance.BorderSize = 0;
            btnMarkPickedUp.Click += async (s, e) => await ChangeStatusAsync("PU");
            pnlPageHeader.Controls.Add(btnMarkPickedUp);

            btnMarkReady = new Button
            {
                Text = "Mark as Ready",
                Size = new Size(150, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Success,
                ForeColor = Color.White,
                Font = Typography.Body,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Visible = false
            };
            btnMarkReady.FlatAppearance.BorderSize = 0;
            btnMarkReady.Click += async (s, e) => await ChangeStatusAsync("RD");
            pnlPageHeader.Controls.Add(btnMarkReady);

            Action positionHeaderButtons = () =>
            {
                int rightEdge = pnlPageHeader.Width - Spacing.Xl;
                btnMarkPickedUp.Location = new Point(rightEdge - btnMarkPickedUp.Width, 15);
                btnMarkReady.Location = new Point(rightEdge - btnMarkReady.Width, 15);
            };
            pnlPageHeader.Resize += (s, e) => positionHeaderButtons();
            positionHeaderButtons();

            Controls.Add(pnlPageHeader);

            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
            // BODY (Docked Fill) â€” scrollable content
            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Colors.Background,
                Padding = new Padding(Spacing.Xl, Spacing.Lg, Spacing.Xl, Spacing.Lg)
            };
            Controls.Add(pnlBody);
            pnlBody.BringToFront();

            // Layout helper â€” the body is a vertical stack of cards,
            // each docked Top so they auto-size
            var pnlStack = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1020,
                BackColor = Colors.Background
            };
            pnlBody.Controls.Add(pnlStack);

            // â”€â”€ Customer & Order card â”€â”€
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

            // â”€â”€ Items card â”€â”€
            var cardItems = new RoundedCard
            {
                Dock = DockStyle.Top,
                Height = 500,
                Padding = new Padding(0),
                CornerRadius = 8,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true,
                Margin = new Padding(0, 0, 0, Spacing.Xl)
            };
            pnlStack.Controls.Add(cardItems);
            BuildItemsCard(cardItems);

            // â”€â”€ Billing card â”€â”€
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
            // Current add order: customer, items, billing â†’ renders bottom-up
            // Fix by reversing the order: billing, items, customer
            // ... but we already added them. Instead, use Dock=Top with explicit order.
            // Simplify: re-add in reverse
            pnlStack.Controls.Clear();
            pnlStack.Controls.Add(cardBilling);
            pnlStack.Controls.Add(cardItems);
            pnlStack.Controls.Add(cardCustomer);
            System.IO.File.AppendAllText(@"C:\temp\order_debug.log", $"[view chain] view={this.Bounds} pnlBody={pnlBody.Bounds} pnlStack={pnlStack.Bounds} cardItems={cardItems.Bounds}\r\n");
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
            // â”€â”€ Top bar (Docked Top) â”€â”€
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

            // â”€â”€ Items area (Docked Fill) â”€â”€
            pnlItems = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Colors.Background,
                Padding = new Padding(Spacing.Lg, Spacing.Lg, Spacing.Lg, Spacing.Lg)
            };
            card.Controls.Add(pnlItems);
            pnlItems.BringToFront();

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
            lblChemical = AddBillRow(card, "Add-Ons", y + 34);

            // Total row
            var lblTotalCaption = new Label
            {
                Text = "TOTAL",
                Font = Typography.H3,
                ForeColor = Colors.TextPrimary,
                Location = new Point(cx, y + 82),
                AutoSize = true
            };
            card.Controls.Add(lblTotalCaption);

            lblTotal = new Label
            {
                Text = "PHP 0.00",
                Font = Typography.H2,
                ForeColor = Colors.Primary,
                Location = new Point(cx + 400, y + 80),
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
            // Customer list — loaded from CustomerApiService
            try
            {
                var custResp = await _customerApi.GetCustomersAsync(page: 1, pageSize: 200, isActive: true);
                cmbCustomer.Items.Clear();
                foreach (var c in custResp.Data)
                    cmbCustomer.Items.Add(new ComboItem(c.CustomerId, FormatCustomer(c)));
                if (cmbCustomer.Items.Count > 0)
                    cmbCustomer.SelectedIndex = 0;
                else
                    cmbCustomer.Items.Add(new ComboItem(0, "(no customers found)"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load customers: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Edit mode: load existing order
            if (!IsCreate)
            {
                try
                {
                    var order = await _orderService.GetOrderByIdAsync(_orderId);
                    if (order != null)
                    {
                        lblTitle.Text = $"Edit Order #{order.OrderNumber}";
                        _currentStatusCode = order.StatusCode;
                        _loadedOrder = order;
                        txtNotes.Text = order.Notes ?? "";

                        // Extract add-ons from the first order item
                        _editModeAddOns.Clear();
                        if (order.Items != null && order.Items.Count > 0)
                        {
                            _editModeAddOns.AddRange(order.Items[0].AddOns);
                            _editModeCategory = order.Items[0].CategoryName;
                            _editModeWeight = order.Items[0].WeightKg;
                            _editModeServiceId = order.Items[0].ServiceId;
                        }

                        // Select the order's customer in the dropdown
                        for (int i = 0; i < cmbCustomer.Items.Count; i++)
                        {
                            var item = cmbCustomer.Items[i] as ComboItem;
                            if (item != null && item.Value == order.CustomerId)
                            {
                                cmbCustomer.SelectedIndex = i;
                                break;
                            }
                        }

                        lblOrderDate.Text = order.OrderDate.ToString("yyyy-MM-dd HH:mm");

                        UpdateStatusButtons();

                        // Lock the view if order is Picked Up or Delivered
                        if (_currentStatusCode == "PU" || _currentStatusCode == "DE")
                        {
                            _isLocked = true;
                            cmbCustomer.Enabled = false;
                            txtNotes.ReadOnly = true;
                            btnAddItem.Enabled = false;
                            if (btnSave != null) btnSave.Visible = false;
                            lblTitle.Text = $"Order {order.OrderNumber} — {order.StatusName} (Locked)";
                        }
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

            // Edit mode: lock the customer field
            if (!IsCreate)
            {
                cmbCustomer.Enabled = false;
            }
        }

        private async Task AddItemAsync()
        {
            var card = new OrderItemCard();
            if (!IsCreate)
            {
                card.IsEditMode = true;
                card.PreCheckedAddOns.AddRange(_editModeAddOns);
                card.PreSetCategory = _editModeCategory;
                card.PreSetWeight = _editModeWeight;
            }
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

            _itemCards.Add(card);
            pnlItems.Controls.Add(card);
            System.IO.File.AppendAllText(@"C:\temp\order_debug.log", $"[after add] card.Bounds={card.Bounds} card.Dock={card.Dock} card.Parent={card.Parent?.GetType().Name} card.ParentBounds={card.Parent?.Bounds} pnlItems.Bounds={pnlItems.Bounds} pnlItems.Parent={pnlItems.Parent?.GetType().Name} pnlItems.ParentBounds={pnlItems.Parent?.Bounds}`r`n");

            // Re-apply after add

            await card.InitializeDataAsync(_serviceApi);

            card.ApplyEditMode();
            System.IO.File.AppendAllText(@"C:\temp\order_debug.log", $"[edit-apply] IsEditMode={card.IsEditMode} PreSetCategory={card.PreSetCategory} PreSetWeight={card.PreSetWeight} checkedAddOns={string.Join(",", card.PreCheckedAddOns)}`r`n");
        }

        private void RecalculateBilling()
        {
            decimal sub = _itemCards.Sum(c => c.ServiceLineTotal);
            decimal chem = _itemCards.Sum(c => c.ChemicalTotal);
            decimal total = sub + chem;

            lblSubtotal.Text = $"PHP {sub:N2}";
            lblChemical.Text = $"PHP {chem:N2}";
            lblTotal.Text = $"PHP {total:N2}";
        }

        private void UpdateStatusButtons()
        {
            if (IsCreate)
            {
                btnMarkReady.Visible = false;
                btnMarkPickedUp.Visible = false;
                return;
            }

            var user = SessionManager.CurrentUser;
            bool canModify = user?.CanModifyOrders == true;

            bool showReady = canModify && (_currentStatusCode == "PE" || _currentStatusCode == "PR");
            decimal paidAmount = _loadedOrder?.Payments?.Sum(p => p.Amount) ?? 0m;
            bool isFullyPaid = _loadedOrder != null && _loadedOrder.TotalAmount > 0 && paidAmount >= _loadedOrder.TotalAmount;
            bool showPickedUp = canModify && _currentStatusCode == "RD" && isFullyPaid;

            btnMarkReady.Visible = showReady;
            btnMarkPickedUp.Visible = showPickedUp;
        }

        private async Task ChangeStatusAsync(string targetStatusCode)
        {
            int statusId = targetStatusCode switch
            {
                "RD" => 3,
                "PU" => 4,
                _ => 0
            };

            if (statusId == 0) return;

            string prompt = targetStatusCode == "RD"
                ? "Mark this order as Ready for Pickup?"
                : "Mark this order as Picked Up by the customer?";

            var confirm = MessageBox.Show(prompt, "Confirm Status Change",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var request = new OrderStatusChangeRequest
            {
                StatusId = statusId,
                Notes = targetStatusCode == "RD"
                    ? "Order ready for pickup"
                    : "Order picked up by customer",
                ChangedByName = SessionManager.CurrentUser?.FullName
            };

            var (success, _, error) = await _orderService.ChangeStatusAsync(_orderId, request);

            if (success)
            {
                MessageBox.Show($"Order status updated successfully.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Saved?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                MessageBox.Show($"Failed to update status: {error}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                    ServiceId = card.ServiceId,
                    Quantity = 1,
                    UnitPrice = card.LineTotal,
                    DiscountAmount = 0,
                    AddOns = card.GetCheckedAddOns(),
                    WeightKg = card.CurrentWeight,
                    CategoryName = card.CategoryName
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
                    var updateRequest = new UpdateOrderRequest
                    {
                        CustomerId = (cmbCustomer.SelectedItem as ComboItem)?.Value ?? 1,
                        Priority = "Normal",
                        Notes = txtNotes.Text,
                        PickupDate = now,
                        DeliveryDate = now.AddDays(2),
                        Items = items.Select(i => new UpdateOrderItemRequest
                        {
                            ServiceId = i.ServiceId,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice,
                            DiscountAmount = i.DiscountAmount,
                            AddOns = i.AddOns,
                            WeightKg = i.WeightKg,
                            CategoryName = i.CategoryName
                        }).ToList()
                    };

                    var (updateSuccess, _, updateError) = await _orderService.UpdateOrderAsync(_orderId, updateRequest);
                    if (updateSuccess)
                    {
                        MessageBox.Show($"Order #{_orderId} updated successfully.",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Saved?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to update: {updateError}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            finally
            {
                btnSave.Enabled = true;
                btnSave.Text = "Save Order";
            }
        }

        private static string FormatCustomer(CustomerModel c)
        {
            var name = !string.IsNullOrWhiteSpace(c.CompanyName)
                ? c.CompanyName
                : c.FullName;

            if (!string.IsNullOrWhiteSpace(c.PhonePrimary))
                name += $"  ({c.PhonePrimary})";

            return name;
        }
    }
}
