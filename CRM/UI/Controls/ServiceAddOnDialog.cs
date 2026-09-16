using System;
using System.Drawing;
using System.Windows.Forms;
using CRM.UI;
using CRM.WinForms.Models;

namespace CRM.UI.Controls
{
    /// <summary>
    /// Small modal for adding/editing a service add-on.
    /// </summary>
    public class ServiceAddOnDialog : Form
    {
        public ServiceAddOnModel? Result { get; private set; }

        private TextBox txtName = null!;
        private TextBox txtDescription = null!;
        private NumericUpDown numPrice = null!;
        private CheckBox chkActive = null!;
        private NumericUpDown numSortOrder = null!;
        private Label lblStatus = null!;

        public ServiceAddOnDialog(ServiceAddOnModel? existing)
        {
            Text = existing == null ? "Add Add-On" : "Edit Add-On";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(460, 340);
            BackColor = Colors.Background;
            Font = Typography.Body;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            int left = Spacing.Xl;
            int fieldWidth = 400;
            int y = Spacing.Xl;

            // Header
            Controls.Add(new Label
            {
                Text = existing == null ? "Add Add-On" : "Edit Add-On",
                Font = Typography.H1,
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(left, y)
            });
            y += 44;

            // Name
            AddFieldLabel("Add-On Name *", left, y);
            txtName = new TextBox
            {
                Location = new Point(left, y + 22),
                Width = fieldWidth,
                Height = 32,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle,
                Text = existing?.AddOnName ?? ""
            };
            Controls.Add(txtName);
            y += 62;

            // Description
            AddFieldLabel("Description", left, y);
            txtDescription = new TextBox
            {
                Location = new Point(left, y + 22),
                Width = fieldWidth,
                Height = 32,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle,
                Text = existing?.Description ?? ""
            };
            Controls.Add(txtDescription);
            y += 62;

            // Price + Sort
            AddFieldLabel("Additional Price (₱) *", left, y);
            numPrice = new NumericUpDown
            {
                Location = new Point(left, y + 22),
                Width = 180,
                Height = 32,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle,
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 1000000,
                Value = existing?.AdditionalPrice ?? 0
            };
            Controls.Add(numPrice);

            AddFieldLabel("Sort Order", left + 200, y);
            numSortOrder = new NumericUpDown
            {
                Location = new Point(left + 200, y + 22),
                Width = 200,
                Height = 32,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle,
                Minimum = 0,
                Maximum = 9999,
                Value = existing?.SortOrder ?? 0
            };
            Controls.Add(numSortOrder);
            y += 62;

            // Active
            chkActive = new CheckBox
            {
                Text = "Active",
                Font = Typography.Body,
                ForeColor = Colors.TextPrimary,
                Checked = existing?.IsActive ?? true,
                AutoSize = true,
                Location = new Point(left, y)
            };
            Controls.Add(chkActive);
            y += 34;

            // Status
            lblStatus = new Label
            {
                Location = new Point(left, y),
                Width = fieldWidth,
                Height = 22,
                Font = Typography.Small,
                ForeColor = Colors.Danger
            };
            Controls.Add(lblStatus);
            y += 32;

            // Buttons
            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(ClientSize.Width - Spacing.Xl - 240, ClientSize.Height - 60),
                Width = 110,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderColor = Colors.Border;
            Controls.Add(btnCancel);

            var btnSave = new Button
            {
                Text = "OK",
                Location = new Point(ClientSize.Width - Spacing.Xl - 120, ClientSize.Height - 60),
                Width = 120,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => Save(existing);
            Controls.Add(btnSave);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void AddFieldLabel(string text, int x, int y)
        {
            Controls.Add(new Label
            {
                Text = text,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(x, y)
            });
        }

        private void Save(ServiceAddOnModel? existing)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                lblStatus.Text = "Add-On Name is required.";
                txtName.Focus();
                return;
            }

            Result = new ServiceAddOnModel
            {
                ServiceAddOnId = existing?.ServiceAddOnId ?? 0,
                ServiceId = existing?.ServiceId ?? 0,
                AddOnName = txtName.Text.Trim(),
                Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim(),
                AdditionalPrice = numPrice.Value,
                IsActive = chkActive.Checked,
                SortOrder = (int)numSortOrder.Value
            };

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}