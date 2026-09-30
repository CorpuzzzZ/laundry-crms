using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CRM.UI;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.WinForms.Forms
{
    public class AddCompanyDialog : Form
    {
        private readonly SuperAdminApiService _api = new(ApiClient.Instance);
        private readonly List<SubscriptionPlanModel> _plans;

        private TextBox txtName = null!;
        private TextBox txtCode = null!;
        private TextBox txtLegalName = null!;
        private TextBox txtIndustry = null!;
        private TextBox txtTaxId = null!;
        private TextBox txtWebsite = null!;
        private ComboBox cmbPlan = null!;
        private ComboBox cmbBilling = null!;
        private NumericUpDown numAmount = null!;
        private ComboBox cmbMethod = null!;
        private Label lblError = null!;
        private Button btnSave = null!;

        public CompanySubscriptionModel? CreatedCompany { get; private set; }

        public AddCompanyDialog(List<SubscriptionPlanModel> plans)
        {
            _plans = plans;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Register Company & Subscription";
            Size = new Size(540, 680);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Colors.Surface;
            Font = Typography.Body;

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 16, 24, 10)
            };
            var lblTitle = new Label
            {
                Text = "Register Company & Assign Plan",
                Font = Typography.H2,
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(24, 16)
            };
            pnlTop.Controls.Add(lblTitle);

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(24, 12, 24, 12)
            };

            btnSave = new Button
            {
                Text = "Register Company",
                Width = 150,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(346, 12)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += async (s, e) => await SaveAsync();

            var btnCancel = new Button
            {
                Text = "Cancel",
                Width = 90,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextSecondary,
                Font = Typography.Body,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(246, 12),
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderColor = Colors.Border;

            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Controls.Add(btnSave);

            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(24, 12, 24, 12)
            };

            int y = 10;
            AddLabel(pnlBody, "Company Name *", 24, y);
            txtName = AddTextBox(pnlBody, 24, y + 20, 470);
            txtName.TextChanged += (s, e) => AutoSuggestCode();
            y += 62;

            AddLabel(pnlBody, "Company Code (Unique Identifier) *", 24, y);
            txtCode = AddTextBox(pnlBody, 24, y + 20, 225);

            AddLabel(pnlBody, "Industry", 269, y);
            txtIndustry = AddTextBox(pnlBody, 269, y + 20, 225);
            txtIndustry.Text = "Laundry Services";
            y += 62;

            AddLabel(pnlBody, "Legal / Registered Name", 24, y);
            txtLegalName = AddTextBox(pnlBody, 24, y + 20, 470);
            y += 62;

            AddLabel(pnlBody, "Tax ID / TIN", 24, y);
            txtTaxId = AddTextBox(pnlBody, 24, y + 20, 225);

            AddLabel(pnlBody, "Website", 269, y);
            txtWebsite = AddTextBox(pnlBody, 269, y + 20, 225);
            y += 62;

            // Section divider
            var sep = new Panel { Location = new Point(24, y), Width = 470, Height = 1, BackColor = Colors.BorderLight };
            pnlBody.Controls.Add(sep);
            y += 12;

            var lblSubHead = new Label
            {
                Text = "SUBSCRIPTION PLAN DETAILS",
                Font = Typography.SmallBold,
                ForeColor = Colors.Primary,
                Location = new Point(24, y),
                AutoSize = true
            };
            pnlBody.Controls.Add(lblSubHead);
            y += 24;

            AddLabel(pnlBody, "Subscription Plan *", 24, y);
            cmbPlan = new ComboBox
            {
                Location = new Point(24, y + 20),
                Width = 225,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Typography.Body
            };
            foreach (var p in _plans)
                cmbPlan.Items.Add(new PlanItem(p));
            if (cmbPlan.Items.Count > 0) cmbPlan.SelectedIndex = 0;
            cmbPlan.SelectedIndexChanged += (s, e) => UpdateCalculatedPrice();
            pnlBody.Controls.Add(cmbPlan);

            AddLabel(pnlBody, "Billing Cycle *", 269, y);
            cmbBilling = new ComboBox
            {
                Location = new Point(269, y + 20),
                Width = 225,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Typography.Body
            };
            cmbBilling.Items.AddRange(new object[] { "Monthly", "Yearly" });
            cmbBilling.SelectedIndex = 0;
            cmbBilling.SelectedIndexChanged += (s, e) => UpdateCalculatedPrice();
            pnlBody.Controls.Add(cmbBilling);
            y += 62;

            AddLabel(pnlBody, "Initial Payment Amount (PHP)", 24, y);
            numAmount = new NumericUpDown
            {
                Location = new Point(24, y + 20),
                Width = 225,
                Maximum = 1000000,
                DecimalPlaces = 2,
                Font = Typography.Body
            };
            pnlBody.Controls.Add(numAmount);

            AddLabel(pnlBody, "Payment Method", 269, y);
            cmbMethod = new ComboBox
            {
                Location = new Point(269, y + 20),
                Width = 225,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Typography.Body
            };
            cmbMethod.Items.AddRange(new object[] { "Manual", "Bank Transfer", "GCash", "Credit Card", "Cash" });
            cmbMethod.SelectedIndex = 0;
            pnlBody.Controls.Add(cmbMethod);
            y += 62;

            lblError = new Label
            {
                Location = new Point(24, y),
                Width = 470,
                Height = 32,
                ForeColor = Colors.Danger,
                Font = Typography.Small,
                Text = ""
            };
            pnlBody.Controls.Add(lblError);

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);

            UpdateCalculatedPrice();
        }

        private void AutoSuggestCode()
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || txtCode.Tag?.ToString() == "auto")
            {
                var clean = new string(txtName.Text.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
                if (clean.Length > 8) clean = clean.Substring(0, 8);
                txtCode.Text = clean;
                txtCode.Tag = "auto";
            }
        }

        private void UpdateCalculatedPrice()
        {
            if (cmbPlan.SelectedItem is PlanItem item)
            {
                bool isYearly = cmbBilling.SelectedIndex == 1;
                decimal price = isYearly
                    ? (item.Plan.PricePerYear ?? (item.Plan.PricePerMonth * 10))
                    : item.Plan.PricePerMonth;
                numAmount.Value = price;
            }
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            lblError.Text = "";

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                lblError.Text = "Please enter a Company Name.";
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                lblError.Text = "Please enter a Company Code.";
                txtCode.Focus();
                return;
            }

            if (cmbPlan.SelectedItem is not PlanItem planItem)
            {
                lblError.Text = "Please select a Subscription Plan.";
                return;
            }

            btnSave.Enabled = false;
            btnSave.Text = "Saving...";

            var req = new CreateCompanyWithPlanRequest
            {
                CompanyName = txtName.Text.Trim(),
                CompanyCode = txtCode.Text.Trim(),
                LegalName = string.IsNullOrWhiteSpace(txtLegalName.Text) ? txtName.Text.Trim() : txtLegalName.Text.Trim(),
                Industry = txtIndustry.Text.Trim(),
                TaxId = txtTaxId.Text.Trim(),
                Website = txtWebsite.Text.Trim(),
                PlanId = planItem.Plan.PlanId,
                BillingCycle = cmbBilling.SelectedItem?.ToString() ?? "Monthly",
                AmountPaid = numAmount.Value,
                PaymentMethod = cmbMethod.SelectedItem?.ToString() ?? "Manual"
            };

            var result = await _api.CreateCompanyWithPlanAsync(req);
            if (result != null)
            {
                CreatedCompany = result;
                MessageBox.Show($"Company '{result.CompanyName}' registered and activated under {result.CurrentPlanName}!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblError.Text = "Failed to create company. Ensure Company Code is unique.";
                btnSave.Enabled = true;
                btnSave.Text = "Register Company";
            }
        }

        private static void AddLabel(Panel parent, string text, int x, int y)
        {
            var lbl = new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = Typography.SmallBold,
                ForeColor = Colors.TextSecondary
            };
            parent.Controls.Add(lbl);
        }

        private static TextBox AddTextBox(Panel parent, int x, int y, int width)
        {
            var txt = new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle
            };
            parent.Controls.Add(txt);
            return txt;
        }

        private class PlanItem
        {
            public SubscriptionPlanModel Plan { get; }
            public PlanItem(SubscriptionPlanModel plan) => Plan = plan;
            public override string ToString() => $"{Plan.PlanName} (PHP {Plan.PricePerMonth:N0}/mo)";
        }
    }
}
