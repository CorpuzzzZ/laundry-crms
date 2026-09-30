using System;
using System.Drawing;
using System.Windows.Forms;
using CRM.UI;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.WinForms.Forms
{
    public class TermsEditDialog : Form
    {
        private readonly SuperAdminApiService _api = new(ApiClient.Instance);
        private readonly TermsModel? _existing;

        private TextBox txtVersion = null!;
        private TextBox txtTitle = null!;
        private DateTimePicker dtEffective = null!;
        private DateTimePicker dtExpiry = null!;
        private CheckBox chkHasExpiry = null!;
        private CheckBox chkActive = null!;
        private CheckBox chkMandatory = null!;
        private TextBox txtContent = null!;
        private Label lblError = null!;
        private Button btnSave = null!;

        public TermsModel? SavedTerms { get; private set; }

        public TermsEditDialog(TermsModel? existing = null)
        {
            _existing = existing;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = _existing == null ? "New Terms and Conditions" : "Edit Terms and Conditions";
            Size = new Size(680, 680);
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
            var lblHead = new Label
            {
                Text = _existing == null ? "Create Terms & Conditions Version" : "Edit Terms & Conditions",
                Font = Typography.H2,
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(24, 16)
            };
            pnlTop.Controls.Add(lblHead);

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(24, 12, 24, 12)
            };

            btnSave = new Button
            {
                Text = _existing == null ? "Publish Terms" : "Save Changes",
                Width = 140,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(496, 12)
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
                Location = new Point(396, 12),
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderColor = Colors.Border;

            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Controls.Add(btnSave);

            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 12, 24, 12)
            };

            int y = 8;
            AddLabel(pnlBody, "Version (e.g. v1.1, v2.0) *", 24, y);
            txtVersion = AddTextBox(pnlBody, 24, y + 20, 140);
            txtVersion.Text = _existing?.Version ?? "v1.1";

            AddLabel(pnlBody, "Document Title *", 180, y);
            txtTitle = AddTextBox(pnlBody, 180, y + 20, 450);
            txtTitle.Text = _existing?.Title ?? "Platform Terms and Conditions";
            y += 62;

            AddLabel(pnlBody, "Effective Date", 24, y);
            dtEffective = new DateTimePicker
            {
                Location = new Point(24, y + 20),
                Width = 180,
                Format = DateTimePickerFormat.Short,
                Font = Typography.Body
            };
            pnlBody.Controls.Add(dtEffective);

            chkHasExpiry = new CheckBox
            {
                Text = "Set Expiration Date",
                Location = new Point(220, y + 2),
                AutoSize = true,
                Font = Typography.SmallBold,
                ForeColor = Colors.TextSecondary
            };
            chkHasExpiry.CheckedChanged += (s, e) => dtExpiry.Enabled = chkHasExpiry.Checked;
            pnlBody.Controls.Add(chkHasExpiry);

            dtExpiry = new DateTimePicker
            {
                Location = new Point(220, y + 20),
                Width = 180,
                Format = DateTimePickerFormat.Short,
                Font = Typography.Body,
                Enabled = false
            };
            pnlBody.Controls.Add(dtExpiry);

            chkActive = new CheckBox
            {
                Text = "Active Version",
                Location = new Point(420, y + 20),
                AutoSize = true,
                Checked = true,
                Font = Typography.Body
            };
            pnlBody.Controls.Add(chkActive);

            chkMandatory = new CheckBox
            {
                Text = "Mandatory Acceptance",
                Location = new Point(530, y + 20),
                AutoSize = true,
                Checked = true,
                Font = Typography.Body
            };
            pnlBody.Controls.Add(chkMandatory);
            y += 62;

            AddLabel(pnlBody, "Terms and Conditions Content (Markdown supported) *", 24, y);
            y += 20;

            txtContent = new TextBox
            {
                Location = new Point(24, y),
                Size = new Size(606, 320),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 10f),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlBody.Controls.Add(txtContent);
            y += 326;

            lblError = new Label
            {
                Location = new Point(24, y),
                Width = 606,
                Height = 24,
                ForeColor = Colors.Danger,
                Font = Typography.Small,
                Text = ""
            };
            pnlBody.Controls.Add(lblError);

            Controls.Add(pnlBody);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);

            if (_existing != null)
            {
                txtVersion.Text = _existing.Version;
                txtTitle.Text = _existing.Title;
                txtContent.Text = _existing.Content;
                dtEffective.Value = _existing.EffectiveDate;
                if (_existing.ExpiryDate.HasValue)
                {
                    chkHasExpiry.Checked = true;
                    dtExpiry.Enabled = true;
                    dtExpiry.Value = _existing.ExpiryDate.Value;
                }
                chkActive.Checked = _existing.IsActive;
                chkMandatory.Checked = _existing.IsMandatory;
            }
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            lblError.Text = "";

            if (string.IsNullOrWhiteSpace(txtVersion.Text))
            {
                lblError.Text = "Please enter a Version.";
                txtVersion.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                lblError.Text = "Please enter a Title.";
                txtTitle.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtContent.Text))
            {
                lblError.Text = "Please enter the Terms and Conditions content.";
                txtContent.Focus();
                return;
            }

            btnSave.Enabled = false;
            btnSave.Text = "Saving...";

            if (_existing == null)
            {
                var req = new CreateTermsRequest
                {
                    Version = txtVersion.Text.Trim(),
                    Title = txtTitle.Text.Trim(),
                    Content = txtContent.Text.Trim(),
                    EffectiveDate = dtEffective.Value,
                    ExpiryDate = chkHasExpiry.Checked ? dtExpiry.Value : null,
                    IsActive = chkActive.Checked,
                    IsMandatory = chkMandatory.Checked
                };

                var created = await _api.CreateTermsAsync(req);
                if (created != null)
                {
                    SavedTerms = created;
                    MessageBox.Show("Terms & Conditions created successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    lblError.Text = "Failed to create Terms & Conditions.";
                    btnSave.Enabled = true;
                    btnSave.Text = "Publish Terms";
                }
            }
            else
            {
                var req = new UpdateTermsRequest
                {
                    Version = txtVersion.Text.Trim(),
                    Title = txtTitle.Text.Trim(),
                    Content = txtContent.Text.Trim(),
                    EffectiveDate = dtEffective.Value,
                    ExpiryDate = chkHasExpiry.Checked ? dtExpiry.Value : null,
                    IsActive = chkActive.Checked,
                    IsMandatory = chkMandatory.Checked
                };

                var updated = await _api.UpdateTermsAsync(_existing.TermsId, req);
                if (updated != null)
                {
                    SavedTerms = updated;
                    MessageBox.Show("Terms & Conditions updated successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    lblError.Text = "Failed to update Terms & Conditions.";
                    btnSave.Enabled = true;
                    btnSave.Text = "Save Changes";
                }
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
