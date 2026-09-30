using System;
using System.Drawing;
using System.Windows.Forms;
using CRM.UI;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.WinForms.Forms
{
    public class CreatePlanDialog : Form
    {
        private readonly SuperAdminApiService _api = new(ApiClient.Instance);
        private readonly SubscriptionPlanModel? _existing;

        private TextBox txtName = null!;
        private TextBox txtCode = null!;
        private TextBox txtDesc = null!;
        private NumericUpDown numPriceMo = null!;
        private NumericUpDown numPriceYr = null!;
        private NumericUpDown numUsers = null!;
        private NumericUpDown numBranches = null!;
        private NumericUpDown numOrders = null!;
        private CheckBox chkActive = null!;
        private Label lblError = null!;
        private Button btnSave = null!;

        public SubscriptionPlanModel? SavedPlan { get; private set; }

        public CreatePlanDialog(SubscriptionPlanModel? existing = null)
        {
            _existing = existing;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = _existing == null ? "New Subscription Plan" : "Edit Subscription Plan";
            Size = new Size(500, 580);
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
                Text = _existing == null ? "Create Subscription Plan" : "Edit Plan Details",
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
                Text = _existing == null ? "Create Plan" : "Save Changes",
                Width = 130,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(326, 12)
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
                Location = new Point(226, 12),
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
            AddLabel(pnlBody, "Plan Name *", 24, y);
            txtName = AddTextBox(pnlBody, 24, y + 20, 210);

            AddLabel(pnlBody, "Plan Code *", 254, y);
            txtCode = AddTextBox(pnlBody, 254, y + 20, 200);
            y += 62;

            AddLabel(pnlBody, "Description", 24, y);
            txtDesc = AddTextBox(pnlBody, 24, y + 20, 430);
            y += 62;

            AddLabel(pnlBody, "Price / Month (PHP) *", 24, y);
            numPriceMo = new NumericUpDown
            {
                Location = new Point(24, y + 20),
                Width = 200,
                Maximum = 100000,
                DecimalPlaces = 2,
                Font = Typography.Body,
                Value = 1499
            };
            numPriceMo.ValueChanged += (s, e) =>
            {
                if (numPriceYr.Value == 0 || numPriceYr.Value == numPriceMo.Value * 12)
                    numPriceYr.Value = numPriceMo.Value * 10; // 2 months discount by default
            };
            pnlBody.Controls.Add(numPriceMo);

            AddLabel(pnlBody, "Price / Year (PHP)", 244, y);
            numPriceYr = new NumericUpDown
            {
                Location = new Point(244, y + 20),
                Width = 210,
                Maximum = 1000000,
                DecimalPlaces = 2,
                Font = Typography.Body,
                Value = 14990
            };
            pnlBody.Controls.Add(numPriceYr);
            y += 62;

            AddLabel(pnlBody, "Max Users", 24, y);
            numUsers = new NumericUpDown
            {
                Location = new Point(24, y + 20),
                Width = 120,
                Minimum = 1,
                Maximum = 500,
                Font = Typography.Body,
                Value = 5
            };
            pnlBody.Controls.Add(numUsers);

            AddLabel(pnlBody, "Max Branches", 164, y);
            numBranches = new NumericUpDown
            {
                Location = new Point(164, y + 20),
                Width = 120,
                Minimum = 1,
                Maximum = 100,
                Font = Typography.Body,
                Value = 1
            };
            pnlBody.Controls.Add(numBranches);

            AddLabel(pnlBody, "Max Orders / Mo", 304, y);
            numOrders = new NumericUpDown
            {
                Location = new Point(304, y + 20),
                Width = 150,
                Minimum = 0,
                Maximum = 100000,
                Font = Typography.Body,
                Value = 1000
            };
            pnlBody.Controls.Add(numOrders);
            y += 62;

            chkActive = new CheckBox
            {
                Text = "Plan is Active and Available for Tenants",
                Location = new Point(24, y),
                AutoSize = true,
                Checked = true,
                Font = Typography.Body
            };
            pnlBody.Controls.Add(chkActive);
            y += 34;

            lblError = new Label
            {
                Location = new Point(24, y),
                Width = 430,
                Height = 30,
                ForeColor = Colors.Danger,
                Font = Typography.Small,
                Text = ""
            };
            pnlBody.Controls.Add(lblError);

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);

            // Populate if editing
            if (_existing != null)
            {
                txtName.Text = _existing.PlanName;
                txtCode.Text = _existing.PlanCode;
                txtDesc.Text = _existing.Description ?? "";
                numPriceMo.Value = _existing.PricePerMonth;
                numPriceYr.Value = _existing.PricePerYear ?? (_existing.PricePerMonth * 10);
                numUsers.Value = Math.Max(1, _existing.MaxUsers);
                numBranches.Value = Math.Max(1, _existing.MaxBranches);
                numOrders.Value = _existing.MaxOrdersPerMonth ?? 0;
                chkActive.Checked = _existing.IsActive;
            }
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            lblError.Text = "";

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                lblError.Text = "Please enter Plan Name.";
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                lblError.Text = "Please enter Plan Code.";
                txtCode.Focus();
                return;
            }

            btnSave.Enabled = false;
            btnSave.Text = "Saving...";

            var req = new CreateSubscriptionPlanRequest
            {
                PlanName = txtName.Text.Trim(),
                PlanCode = txtCode.Text.Trim().ToUpperInvariant(),
                Description = txtDesc.Text.Trim(),
                PricePerMonth = numPriceMo.Value,
                PricePerYear = numPriceYr.Value,
                MaxUsers = (int)numUsers.Value,
                MaxBranches = (int)numBranches.Value,
                MaxOrdersPerMonth = numOrders.Value > 0 ? (int)numOrders.Value : null,
                IsActive = chkActive.Checked
            };

            SubscriptionPlanModel? result;
            if (_existing == null)
            {
                result = await _api.CreatePlanAsync(req);
            }
            else
            {
                result = await _api.UpdatePlanAsync(_existing.PlanId, req);
            }

            if (result != null)
            {
                SavedPlan = result;
                MessageBox.Show($"Subscription Plan '{result.PlanName}' saved successfully.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblError.Text = "Failed to save plan. Please ensure Plan Code is unique.";
                btnSave.Enabled = true;
                btnSave.Text = _existing == null ? "Create Plan" : "Save Changes";
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
    }
}
