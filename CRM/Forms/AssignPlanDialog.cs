using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CRM.UI;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.WinForms.Forms
{
    public class AssignPlanDialog : Form
    {
        private readonly SuperAdminApiService _api = new(ApiClient.Instance);
        private readonly CompanySubscriptionModel _company;
        private readonly List<SubscriptionPlanModel> _plans;

        private ComboBox cmbPlan = null!;
        private ComboBox cmbBilling = null!;
        private NumericUpDown numAmount = null!;
        private ComboBox cmbMethod = null!;
        private Label lblError = null!;
        private Button btnSave = null!;

        public bool PlanAssigned { get; private set; }

        public AssignPlanDialog(CompanySubscriptionModel company, List<SubscriptionPlanModel> plans)
        {
            _company = company;
            _plans = plans;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Change Subscription Plan";
            Size = new Size(460, 480);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Colors.Surface;
            Font = Typography.Body;

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 14, 24, 8)
            };
            var lblTitle = new Label
            {
                Text = $"Assign Plan: {_company.CompanyName}",
                Font = Typography.H2,
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(24, 14)
            };
            var lblSub = new Label
            {
                Text = $"Code: {_company.CompanyCode} | Current Plan: {_company.CurrentPlanName}",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(24, 38)
            };
            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(lblSub);

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(24, 12, 24, 12)
            };

            btnSave = new Button
            {
                Text = "Update Plan",
                Width = 120,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(296, 12)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += async (s, e) => await SaveAsync();

            var btnCancel = new Button
            {
                Text = "Cancel",
                Width = 80,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextSecondary,
                Font = Typography.Body,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(206, 12),
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderColor = Colors.Border;

            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Controls.Add(btnSave);

            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 16, 24, 16)
            };

            int y = 10;
            AddLabel(pnlBody, "Choose New Subscription Plan *", 24, y);
            cmbPlan = new ComboBox
            {
                Location = new Point(24, y + 20),
                Width = 390,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Typography.Body
            };
            foreach (var p in _plans)
                cmbPlan.Items.Add(new PlanItem(p));
            if (cmbPlan.Items.Count > 0) cmbPlan.SelectedIndex = 0;
            cmbPlan.SelectedIndexChanged += (s, e) => UpdatePrice();
            pnlBody.Controls.Add(cmbPlan);
            y += 62;

            AddLabel(pnlBody, "Billing Cycle *", 24, y);
            cmbBilling = new ComboBox
            {
                Location = new Point(24, y + 20),
                Width = 190,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Typography.Body
            };
            cmbBilling.Items.AddRange(new object[] { "Monthly", "Yearly" });
            cmbBilling.SelectedIndex = 0;
            cmbBilling.SelectedIndexChanged += (s, e) => UpdatePrice();
            pnlBody.Controls.Add(cmbBilling);

            AddLabel(pnlBody, "Amount Paid (PHP)", 224, y);
            numAmount = new NumericUpDown
            {
                Location = new Point(224, y + 20),
                Width = 190,
                Maximum = 1000000,
                DecimalPlaces = 2,
                Font = Typography.Body
            };
            pnlBody.Controls.Add(numAmount);
            y += 62;

            AddLabel(pnlBody, "Payment Method", 24, y);
            cmbMethod = new ComboBox
            {
                Location = new Point(24, y + 20),
                Width = 390,
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
                Width = 390,
                Height = 30,
                ForeColor = Colors.Danger,
                Font = Typography.Small,
                Text = ""
            };
            pnlBody.Controls.Add(lblError);

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);

            UpdatePrice();
        }

        private void UpdatePrice()
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
            if (cmbPlan.SelectedItem is not PlanItem planItem) return;

            btnSave.Enabled = false;
            btnSave.Text = "Updating...";

            var req = new AssignCompanyPlanRequest
            {
                CompanyId = _company.CompanyId,
                PlanId = planItem.Plan.PlanId,
                BillingCycle = cmbBilling.SelectedItem?.ToString() ?? "Monthly",
                AmountPaid = numAmount.Value,
                PaymentMethod = cmbMethod.SelectedItem?.ToString() ?? "Manual"
            };

            var ok = await _api.AssignPlanAsync(req);
            if (ok)
            {
                PlanAssigned = true;
                MessageBox.Show($"Subscription plan updated to '{planItem.Plan.PlanName}' for {_company.CompanyName}.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblError.Text = "Failed to update plan. Please try again.";
                btnSave.Enabled = true;
                btnSave.Text = "Update Plan";
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

        private class PlanItem
        {
            public SubscriptionPlanModel Plan { get; }
            public PlanItem(SubscriptionPlanModel plan) => Plan = plan;
            public override string ToString() => $"{Plan.PlanName} (PHP {Plan.PricePerMonth:N0}/mo)";
        }
    }
}
