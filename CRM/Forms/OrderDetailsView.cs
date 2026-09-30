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
    /// Read-only details view for Picked Up / Delivered orders.
    /// Shows order summary, items, billing, payment history (who received),
    /// and status history (who changed).
    /// </summary>
    public class OrderDetailsView : UserControl
    {
        public event EventHandler? Cancelled;

        private readonly OrderApiService _orderService = new();
        private readonly int _orderId;
        private OrderModel? _order;

        // Header
        private Button btnBack = null!;
        private Label lblTitle = null!;

        // Order summary
        private Label lblOrderNumber = null!;
        private Label lblOrderDate = null!;
        private Label lblCustomer = null!;
        private Label lblCustomerPhone = null!;
        private Label lblStatus = null!;
        private Label lblPriority = null!;

        // Billing
        private Label lblSubtotal = null!;
        private Label lblAddOns = null!;
        private Label lblTotal = null!;
        private Label lblPaid = null!;
        private Label lblBalance = null!;

        // Footer
        private Button btnClose = null!;

        public OrderDetailsView(int orderId)
        {
            _orderId = orderId;
            InitializeComponent();
            Load += async (s, e) => await LoadAsync();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Background;

            // ─── FOOTER ───
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

            btnClose = new Button
            {
                Text = "Close",
                Size = new Size(120, 42),
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Cancelled?.Invoke(this, EventArgs.Empty);
            pnlFooter.Controls.Add(btnClose);
            pnlFooter.Resize += (s, e) =>
            {
                btnClose.Location = new Point(pnlFooter.Width - btnClose.Width - Spacing.Xl, 14);
            };

            Controls.Add(pnlFooter);

            // ─── HEADER ───
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
                Text = "Order Details",
                Font = Typography.H1,
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(230, 20)
            };
            pnlPageHeader.Controls.Add(lblTitle);

            Controls.Add(pnlPageHeader);

            // ─── BODY ───
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Colors.Background,
                Padding = new Padding(Spacing.Xl, Spacing.Lg, Spacing.Xl, Spacing.Lg)
            };
            Controls.Add(pnlBody);
            pnlBody.BringToFront();

            var pnlStack = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1400,
                BackColor = Colors.Background
            };
            pnlBody.Controls.Add(pnlStack);

            // Order summary card
            var cardOrder = new RoundedCard
            {
                Dock = DockStyle.Top,
                Height = 180,
                Padding = new Padding(Spacing.Xl),
                CornerRadius = 8,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = false,
                Margin = new Padding(0, 0, 0, Spacing.Xl)
            };
            pnlStack.Controls.Add(cardOrder);
            BuildOrderCard(cardOrder);

            // Items card
            var cardItems = new RoundedCard
            {
                Dock = DockStyle.Top,
                Height = 220,
                Padding = new Padding(Spacing.Xl),
                CornerRadius = 8,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = false,
                Margin = new Padding(0, 0, 0, Spacing.Xl)
            };
            pnlStack.Controls.Add(cardItems);
            BuildItemsCard(cardItems);

            // Billing card
            var cardBilling = new RoundedCard
            {
                Dock = DockStyle.Top,
                Height = 260,
                Padding = new Padding(Spacing.Xl),
                CornerRadius = 8,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = false,
                Margin = new Padding(0, 0, 0, Spacing.Xl)
            };
            pnlStack.Controls.Add(cardBilling);
            BuildBillingCard(cardBilling);

            // Payments card
            var cardPayments = new RoundedCard
            {
                Dock = DockStyle.Top,
                Height = 240,
                Padding = new Padding(Spacing.Xl),
                CornerRadius = 8,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = false,
                Margin = new Padding(0, 0, 0, Spacing.Xl)
            };
            pnlStack.Controls.Add(cardPayments);
            BuildPaymentsCard(cardPayments);

            // Status history card
            var cardStatus = new RoundedCard
            {
                Dock = DockStyle.Top,
                Height = 240,
                Padding = new Padding(Spacing.Xl),
                CornerRadius = 8,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = false,
                Margin = new Padding(0, 0, 0, Spacing.Lg)
            };
            pnlStack.Controls.Add(cardStatus);
            BuildStatusHistoryCard(cardStatus);
        }

        private void BuildOrderCard(RoundedCard card)
        {
            int cx = Spacing.Lg;
            int y = Spacing.Lg;

            card.Controls.Add(new Label
            {
                Text = "ORDER SUMMARY",
                Font = Typography.TinyUpper,
                ForeColor = Colors.TextMuted,
                Location = new Point(cx, y),
                AutoSize = true
            });
            y += 32;

            card.Controls.Add(MakeLabel("Order #", cx, y));
            lblOrderNumber = MakeValue(cx, y + 22);
            card.Controls.Add(lblOrderNumber);

            card.Controls.Add(MakeLabel("Order Date", cx + 320, y));
            lblOrderDate = MakeValue(cx + 320, y + 22);
            card.Controls.Add(lblOrderDate);

            card.Controls.Add(MakeLabel("Status", cx + 640, y));
            lblStatus = MakeValue(cx + 640, y + 22);
            card.Controls.Add(lblStatus);

            y += 64;

            card.Controls.Add(MakeLabel("Customer", cx, y));
            lblCustomer = MakeValue(cx, y + 22);
            card.Controls.Add(lblCustomer);

            card.Controls.Add(MakeLabel("Phone", cx + 320, y));
            lblCustomerPhone = MakeValue(cx + 320, y + 22);
            card.Controls.Add(lblCustomerPhone);

            card.Controls.Add(MakeLabel("Priority", cx + 640, y));
            lblPriority = MakeValue(cx + 640, y + 22);
            card.Controls.Add(lblPriority);
        }

        private void BuildItemsCard(RoundedCard card)
        {
            int cx = Spacing.Lg;
            int y = Spacing.Lg;

            card.Controls.Add(new Label
            {
                Text = "ITEMS",
                Font = Typography.TinyUpper,
                ForeColor = Colors.TextMuted,
                Location = new Point(cx, y),
                AutoSize = true
            });
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
            lblAddOns = AddBillRow(card, "Add-Ons", y + 34);
            lblTotal = AddBillRow(card, "TOTAL", y + 68, bold: true);
            lblPaid = AddBillRow(card, "Amount Paid", y + 108);
            lblBalance = AddBillRow(card, "Balance", y + 142, bold: true, highlight: true);
        }

        private void BuildPaymentsCard(RoundedCard card)
        {
            int cx = Spacing.Lg;
            int y = Spacing.Lg;

            card.Controls.Add(new Label
            {
                Text = "PAYMENT HISTORY",
                Font = Typography.TinyUpper,
                ForeColor = Colors.TextMuted,
                Location = new Point(cx, y),
                AutoSize = true,
                Tag = "payments-title"
            });
        }

        private void BuildStatusHistoryCard(RoundedCard card)
        {
            int cx = Spacing.Lg;
            int y = Spacing.Lg;

            card.Controls.Add(new Label
            {
                Text = "STATUS HISTORY",
                Font = Typography.TinyUpper,
                ForeColor = Colors.TextMuted,
                Location = new Point(cx, y),
                AutoSize = true,
                Tag = "status-title"
            });
        }

        private Label AddBillRow(Control parent, string caption, int y, bool bold = false, bool highlight = false)
        {
            parent.Controls.Add(new Label
            {
                Text = caption,
                Font = bold ? new Font("Segoe UI", 10F, FontStyle.Bold) : Typography.Body,
                ForeColor = highlight ? Colors.Primary : (bold ? Colors.TextPrimary : Colors.TextSecondary),
                Location = new Point(Spacing.Lg, y),
                AutoSize = true
            });

            var value = new Label
            {
                Text = "PHP 0.00",
                Font = bold ? new Font("Segoe UI", 11F, FontStyle.Bold) : Typography.BodyBold,
                ForeColor = highlight ? Colors.Primary : Colors.TextPrimary,
                Location = new Point(Spacing.Lg + 400, y),
                Width = 300,
                Height = 22,
                TextAlign = ContentAlignment.MiddleRight
            };
            parent.Controls.Add(value);
            return value;
        }

        private static Label MakeLabel(string text, int x, int y) => new Label
        {
            Text = text,
            Font = Typography.Small,
            ForeColor = Colors.TextSecondary,
            Location = new Point(x, y),
            AutoSize = true
        };

        private static Label MakeValue(int x, int y) => new Label
        {
            Text = "-",
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Colors.TextPrimary,
            Location = new Point(x, y),
            AutoSize = true
        };

        private async Task LoadAsync()
        {
            try
            {
                _order = await _orderService.GetOrderByIdAsync(_orderId);

                if (_order == null)
                {
                    MessageBox.Show($"Order {_orderId} not found.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Cancelled?.Invoke(this, EventArgs.Empty);
                    return;
                }

                ApplyOrderToUi();
                BuildItemsList();
                BuildPaymentsList();
                BuildStatusHistoryList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load order: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ApplyOrderToUi()
        {
            if (_order == null) return;

            lblTitle.Text = $"Order Details — {_order.OrderNumber}";
            lblOrderNumber.Text = _order.OrderNumber;
            lblOrderDate.Text = _order.OrderDate.ToString("yyyy-MM-dd HH:mm");
            lblCustomer.Text = _order.CustomerName;
            lblCustomerPhone.Text = _order.CustomerPhone;
            lblStatus.Text = _order.StatusName;
            lblPriority.Text = _order.Priority;
        }

        private void BuildItemsList()
        {
            var card = FindCard("ITEMS");
            if (card == null || _order == null) return;

            int y = Spacing.Lg + 32;
            int cx = Spacing.Lg;

            foreach (var item in _order.Items)
            {
                var addOnsDisplay = item.AddOns != null && item.AddOns.Count > 0
                    ? string.Join(", ", item.AddOns)
                    : "(none)";

                card.Controls.Add(new Label
                {
                    Text = item.ServiceName,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Colors.TextPrimary,
                    Location = new Point(cx, y),
                    AutoSize = true
                });

                card.Controls.Add(new Label
                {
                    Text = $"Category: {item.CategoryName ?? "-"}    Weight: {(item.WeightKg.HasValue ? item.WeightKg.Value.ToString("0.##") + " kg" : "-")}",
                    Font = Typography.Small,
                    ForeColor = Colors.TextSecondary,
                    Location = new Point(cx, y + 24),
                    AutoSize = true
                });

                card.Controls.Add(new Label
                {
                    Text = $"Add-ons: {addOnsDisplay}",
                    Font = Typography.Small,
                    ForeColor = Colors.TextSecondary,
                    Location = new Point(cx, y + 46),
                    AutoSize = true
                });

                card.Controls.Add(new Label
                {
                    Text = $"PHP {item.LineTotal:N2}",
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    ForeColor = Colors.Primary,
                    Location = new Point(cx + 700, y + 24),
                    Size = new Size(220, 24),
                    TextAlign = ContentAlignment.MiddleRight
                });

                y += 76;
            }

            // Billing
            decimal subtotal = _order.Items?.Sum(i => i.LineTotal) ?? 0m;
            decimal addOns = 0m;
            foreach (var item in _order.Items ?? new List<OrderItemModel>())
                addOns += GetAddOnsTotal(item.AddOns);

            decimal total = _order.TotalAmount;
            decimal paid = _order.Payments?.Sum(p => p.Amount) ?? 0m;
            decimal balance = Math.Max(0, total - paid);

            lblSubtotal.Text = $"PHP {subtotal:N2}";
            lblAddOns.Text = $"PHP {addOns:N2}";
            lblTotal.Text = $"PHP {total:N2}";
            lblPaid.Text = $"PHP {paid:N2}";
            lblBalance.Text = $"PHP {balance:N2}";
        }

        private void BuildPaymentsList()
        {
            var card = FindCard("PAYMENT HISTORY");
            if (card == null || _order == null) return;

            int y = Spacing.Lg + 32;
            int cx = Spacing.Lg;

            if (_order.Payments == null || _order.Payments.Count == 0)
            {
                card.Controls.Add(new Label
                {
                    Text = "No payments recorded.",
                    Font = Typography.Body,
                    ForeColor = Colors.TextMuted,
                    Location = new Point(cx, y),
                    AutoSize = true
                });
                return;
            }

            foreach (var payment in _order.Payments.OrderBy(p => p.PaymentDate))
            {
                card.Controls.Add(new Label
                {
                    Text = payment.PaymentMethodName,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Colors.TextPrimary,
                    Location = new Point(cx, y),
                    AutoSize = true
                });

                card.Controls.Add(new Label
                {
                    Text = $"PHP {payment.Amount:N2}",
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    ForeColor = Colors.Primary,
                    Location = new Point(cx + 700, y),
                    Size = new Size(220, 22),
                    TextAlign = ContentAlignment.MiddleRight
                });

                card.Controls.Add(new Label
                {
                    Text = $"Received by: {payment.ReceivedByName ?? "(unknown)"}   |   {payment.PaymentDate:yyyy-MM-dd HH:mm}",
                    Font = Typography.Small,
                    ForeColor = Colors.TextSecondary,
                    Location = new Point(cx, y + 24),
                    AutoSize = true
                });

                if (!string.IsNullOrWhiteSpace(payment.ReferenceNumber))
                {
                    card.Controls.Add(new Label
                    {
                        Text = $"Reference: {payment.ReferenceNumber}",
                        Font = Typography.Small,
                        ForeColor = Colors.TextSecondary,
                        Location = new Point(cx, y + 44),
                        AutoSize = true
                    });
                    y += 68;
                }
                else
                {
                    y += 56;
                }
            }
        }

        private void BuildStatusHistoryList()
        {
            var card = FindCard("STATUS HISTORY");
            if (card == null || _order == null) return;

            int y = Spacing.Lg + 32;
            int cx = Spacing.Lg;

            if (_order.StatusHistory == null || _order.StatusHistory.Count == 0)
            {
                card.Controls.Add(new Label
                {
                    Text = "No status history recorded.",
                    Font = Typography.Body,
                    ForeColor = Colors.TextMuted,
                    Location = new Point(cx, y),
                    AutoSize = true
                });
                return;
            }

            foreach (var entry in _order.StatusHistory.OrderBy(h => h.ChangedAt))
            {
                var color = GetStatusColor(entry.StatusName);

                var badge = new Label
                {
                    Text = entry.StatusName,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = color,
                    Location = new Point(cx, y),
                    Size = new Size(110, 24),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                card.Controls.Add(badge);

                card.Controls.Add(new Label
                {
                    Text = $"Changed by: {entry.ChangedByName ?? "(unknown)"}   |   {entry.ChangedAt:yyyy-MM-dd HH:mm}",
                    Font = Typography.Small,
                    ForeColor = Colors.TextSecondary,
                    Location = new Point(cx + 130, y + 4),
                    AutoSize = true
                });

                y += 34;
            }
        }

        private RoundedCard? FindCard(string title)
        {
            foreach (Control c in Controls)
            {
                if (c is Panel body)
                {
                    foreach (Control inner in body.Controls)
                    {
                        if (inner is Panel stack)
                        {
                            foreach (Control card in stack.Controls)
                            {
                                if (card is RoundedCard rc)
                                {
                                    foreach (Control label in rc.Controls)
                                    {
                                        if (label is Label lbl && lbl.Text == title)
                                            return rc;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static decimal GetAddOnsTotal(List<string>? addOns)
        {
            if (addOns == null || addOns.Count == 0) return 0m;

            decimal total = 0m;
            foreach (var name in addOns)
            {
                total += name switch
                {
                    "Fabcon 1" => 10m,
                    "Fabcon 2" => 10m,
                    "Fabcon 3" => 10m,
                    "Cologne 1" => 10m,
                    "Cologne 2" => 10m,
                    "Dry 10min" => 45m,
                    "Dry 20min" => 60m,
                    "Dry 30min" => 70m,
                    "Spin" => 20m,
                    _ => 0m
                };
            }
            return total;
        }

        private static Color GetStatusColor(string? statusName)
        {
            return statusName?.ToLowerInvariant() switch
            {
                "pending" => Colors.Warning,
                "processing" => Colors.Primary,
                "ready" => Colors.Success,
                "picked up" => Colors.Primary,
                "delivered" => Colors.Success,
                "cancelled" => Colors.Danger,
                _ => Colors.TextMuted
            };
        }
    }
}
