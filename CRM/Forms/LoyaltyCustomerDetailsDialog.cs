using System;
using System.Drawing;
using System.Windows.Forms;
using CRM.UI;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.WinForms.Forms
{
    public class LoyaltyCustomerDetailsDialog : Form
    {
        private readonly LoyaltyApiService _api;
        private readonly LoyaltyCustomerModel _customer;

        private DataGridView _dgvTransactions = null!;
        private Label _lblTitle = null!;
        private Button _btnClose = null!;

        public LoyaltyCustomerDetailsDialog(LoyaltyApiService api, LoyaltyCustomerModel customer)
        {
            _api = api;
            _customer = customer;
            Build();
            Load += async (s, e) => await LoadTransactionsAsync();
        }

        private void Build()
        {
            Text = "Loyalty - " + _customer.CustomerName;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(860, 620);
            MinimumSize = new Size(620, 420);
            BackColor = Colors.Background;
            Font = Typography.Body;

            // ===== Header =====
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 84,
                BackColor = Colors.Surface,
                Padding = new Padding(20, 14, 20, 14)
            };

            _lblTitle = new Label
            {
                Text = _customer.CustomerName,
                Left = 20, Top = 12,
                AutoSize = true,
                Font = Typography.H2,
                ForeColor = Colors.TextPrimary
            };
            header.Controls.Add(_lblTitle);

            header.Controls.Add(new Label
            {
                Text = $"{_customer.CustomerCode}   |   {(_customer.Phone ?? "no phone")}",
                Left = 22, Top = 46,
                AutoSize = true,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary
            });

            // ===== Summary row =====
            var summary = new Panel
            {
                Dock = DockStyle.Top,
                Height = 76,
                BackColor = Colors.Background,
                Padding = new Padding(20, 10, 20, 10)
            };

            summary.Controls.Add(MakeSummaryCard("Current Points", _customer.CurrentPoints.ToString(), 0));
            summary.Controls.Add(MakeSummaryCard("Total Earned", _customer.TotalPointsEarned.ToString(), 1));
            summary.Controls.Add(MakeSummaryCard("Total Redeemed", _customer.TotalPointsRedeemed.ToString(), 2));
            summary.Controls.Add(MakeSummaryCard("Tier", _customer.TierDisplay, 3));
            summary.Controls.Add(MakeSummaryCard("Lifetime Spend", $"PHP {_customer.LifetimeSpend:0.00}", 4));

            // ===== Transactions grid =====
            var gridHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Background,
                Padding = new Padding(20, 4, 20, 8)
            };

            var lblHist = new Label
            {
                Text = "POINTS HISTORY",
                Dock = DockStyle.Top,
                Height = 24,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary
            };
            gridHost.Controls.Add(lblHist);

            _dgvTransactions = new DataGridView
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
                RowTemplate = { Height = 32 }
            };

            _dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Date", DataPropertyName = "DateText", Width = 150 });
            _dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Type", DataPropertyName = "TransactionType", Width = 90 });
            _dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Change", DataPropertyName = "PointsChangeText", Width = 90 });
            _dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Balance", DataPropertyName = "PointsBalance", Width = 90 });
            _dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Order", DataPropertyName = "OrderNumber", Width = 160 });
            _dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Description", DataPropertyName = "Description", Width = 260 });

            gridHost.Controls.Add(_dgvTransactions);
            _dgvTransactions.BringToFront();

            // ===== Footer =====
            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Colors.Surface,
                Padding = new Padding(20, 12, 20, 12)
            };

            _btnClose = new Button
            {
                Text = "Close",
                Width = 110, Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.Body,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            _btnClose.FlatAppearance.BorderSize = 1;
            _btnClose.FlatAppearance.BorderColor = Colors.Border;
            _btnClose.Click += (s, e) => Close();
            footer.Controls.Add(_btnClose);
            footer.Resize += (s, e) =>
            {
                _btnClose.Left = footer.ClientSize.Width - 130;
                _btnClose.Top = 12;
            };

            // ===== Compose =====
            Controls.Add(gridHost);
            Controls.Add(summary);
            Controls.Add(header);
            Controls.Add(footer);

            AcceptButton = _btnClose;
            CancelButton = _btnClose;
        }

        private static Panel MakeSummaryCard(string caption, string value, int index)
        {
            var card = new Panel
            {
                Left = 20 + index * 162,
                Top = 0,
                Width = 154,
                Height = 56,
                BackColor = Colors.Surface,
                Padding = new Padding(12, 8, 12, 8)
            };
            card.Controls.Add(new Label
            {
                Text = caption,
                Left = 12, Top = 8,
                AutoSize = true,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary
            });
            card.Controls.Add(new Label
            {
                Text = value,
                Left = 12, Top = 26,
                AutoSize = true,
                Font = Typography.H3,
                ForeColor = Colors.TextPrimary
            });
            return card;
        }

        private async System.Threading.Tasks.Task LoadTransactionsAsync()
        {
            try
            {
                var list = await _api.GetCustomerTransactionsAsync(_customer.CustomerId);
                _dgvTransactions.DataSource = null;
                _dgvTransactions.DataSource = list;

                if (list.Count == 0)
                {
                    var lblEmpty = new Label
                    {
                        Text = "No loyalty transactions yet.",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = Typography.Body,
                        ForeColor = Colors.TextMuted
                    };
                    _dgvTransactions.Controls.Add(lblEmpty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load transactions.\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
