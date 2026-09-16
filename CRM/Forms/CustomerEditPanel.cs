using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRM.UI;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.WinForms.Forms
{
    /// <summary>
    /// Customer add/edit panel - embeddable, no popup.
    /// Modern card-based layout with Basic Information, Address, and Notes sections.
    /// </summary>
    public class CustomerEditPanel : UserControl
    {
        public event EventHandler? Saved;
        public event EventHandler? Cancelled;

        private readonly CustomerApiService _customerService = new();
        private readonly int _customerId;
        private CustomerModel? _existing;

        // Inputs
        private TextBox txtFirstName = null!;
        private TextBox txtLastName = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPhonePrimary = null!;

        private TextBox txtStreet = null!;
        private TextBox txtVillage = null!;
        private TextBox txtCity = null!;
        private TextBox txtState = null!;
        private TextBox txtPostalCode = null!;
        private TextBox txtCountry = null!;

        private TextBox txtNotes = null!;
        private CheckBox chkIsActive = null!;

        private Label lblCustomerCode = null!;
        private Label lblTitle = null!;
        private Label lblStatus = null!;
        private Button btnSave = null!;
        private Button btnArchive = null!;
        private Button btnRestore = null!;
        private Button btnCancel = null!;

        public CustomerEditPanel(int customerId)
        {
            _customerId = customerId;
            InitializeComponent();
            Load += async (s, e) => await LoadAsync();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Background;
            AutoScroll = true;
            Padding = new Padding(Spacing.Xl, Spacing.Lg, Spacing.Xl, Spacing.Xl);

            int cardWidth = 900;
            int contentLeft = Spacing.Lg;
            int twoColWidth = (cardWidth - Spacing.Xl * 2 - Spacing.Md) / 2;  // ~410
            int col2Left = contentLeft + twoColWidth + Spacing.Md;

            int y = 0;

            // ── HEADER ─────────────────────────────────────────
            lblTitle = new Label
            {
                Text = _customerId == 0 ? "New Customer" : "Edit Customer",
                Font = Typography.H1,
                ForeColor = Colors.TextPrimary,
                Location = new Point(0, y),
                AutoSize = true
            };
            Controls.Add(lblTitle);

            lblCustomerCode = new Label
            {
                Text = _customerId == 0 ? "(auto-generated)" : "",
                Font = Typography.BodyBold,
                ForeColor = Colors.Success,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            lblCustomerCode.Location = new Point(cardWidth - lblCustomerCode.Width, y + 6);
            Controls.Add(lblCustomerCode);

            y += lblTitle.Height + Spacing.Md;

            // ── BASIC INFORMATION CARD ─────────────────────────
            var cardBasic = new CRM.UI.Controls.RoundedCard
            {
                Location = new Point(0, y),
                Width = cardWidth,
                Padding = new Padding(Spacing.Xl),
                CornerRadius = 8,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true
            };
            Controls.Add(cardBasic);

            int cy = Spacing.Xl;
            AddSectionTitle(cardBasic, "BASIC INFORMATION", Spacing.Lg, cy);
            cy += 32;

            // Row 1: First Name | Last Name
            AddFieldLabel(cardBasic, "First Name", contentLeft, cy);
            txtFirstName = AddCardTextBox(cardBasic, contentLeft, cy + 22, twoColWidth);

            AddFieldLabel(cardBasic, "Last Name", col2Left, cy);
            txtLastName = AddCardTextBox(cardBasic, col2Left, cy + 22, twoColWidth);
            cy += 62;

            // Row 2: Email | Phone
            AddFieldLabel(cardBasic, "Email", contentLeft, cy);
            txtEmail = AddCardTextBox(cardBasic, contentLeft, cy + 22, twoColWidth);

            AddFieldLabel(cardBasic, "Phone", col2Left, cy);
            txtPhonePrimary = AddCardTextBox(cardBasic, col2Left, cy + 22, twoColWidth);
            cy += 62;

            cardBasic.Height = cy + Spacing.Lg;
            y += cardBasic.Height + Spacing.Xl;

            // ── ADDRESS CARD ───────────────────────────────────
            var cardAddress = new CRM.UI.Controls.RoundedCard
            {
                Location = new Point(0, y),
                Width = cardWidth,
                Padding = new Padding(Spacing.Xl),
                CornerRadius = 8,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true
            };
            Controls.Add(cardAddress);

            cy = Spacing.Xl;
            AddSectionTitle(cardAddress, "ADDRESS", Spacing.Lg, cy);
            cy += 32;

            // Row 1: Street | Village
            AddFieldLabel(cardAddress, "Street", contentLeft, cy);
            txtStreet = AddCardTextBox(cardAddress, contentLeft, cy + 22, twoColWidth);

            AddFieldLabel(cardAddress, "Village", col2Left, cy);
            txtVillage = AddCardTextBox(cardAddress, col2Left, cy + 22, twoColWidth);
            cy += 62;

            // Row 2: City | State
            AddFieldLabel(cardAddress, "City", contentLeft, cy);
            txtCity = AddCardTextBox(cardAddress, contentLeft, cy + 22, twoColWidth);

            AddFieldLabel(cardAddress, "State / Province", col2Left, cy);
            txtState = AddCardTextBox(cardAddress, col2Left, cy + 22, twoColWidth);
            cy += 62;

            // Row 3: Postal Code | Country
            AddFieldLabel(cardAddress, "Postal Code", contentLeft, cy);
            txtPostalCode = AddCardTextBox(cardAddress, contentLeft, cy + 22, twoColWidth);

            AddFieldLabel(cardAddress, "Country", col2Left, cy);
            txtCountry = AddCardTextBox(cardAddress, col2Left, cy + 22, twoColWidth);
            txtCountry.Text = "Philippines";
            cy += 62;

            cardAddress.Height = cy + Spacing.Lg;
            y += cardAddress.Height + Spacing.Xl;
            // ── NOTES CARD ─────────────────────────────────────
            var cardNotes = new CRM.UI.Controls.RoundedCard
            {
                Location = new Point(0, y),
                Width = cardWidth,
                Padding = new Padding(Spacing.Xl),
                CornerRadius = 8,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true
            };
            Controls.Add(cardNotes);

            cy = Spacing.Xl;
            AddSectionTitle(cardNotes, "NOTES", Spacing.Lg, cy);
            cy += 32;

            txtNotes = new TextBox
            {
                Location = new Point(contentLeft, cy),
                Width = cardWidth - Spacing.Xl * 2,
                Height = 90,
                Font = Typography.Body,
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Colors.Surface
            };
            cardNotes.Controls.Add(txtNotes);
            cy += 100;

            if (_customerId > 0)
            {
                chkIsActive = new CheckBox
                {
                    Location = new Point(contentLeft, cy),
                    Text = "Customer is active",
                    Font = Typography.Body,
                    ForeColor = Colors.TextBody,
                    Checked = true,
                    AutoSize = true
                };
                cardNotes.Controls.Add(chkIsActive);
                cy += 30;
            }

            cardNotes.Height = cy + Spacing.Lg;
            y += cardNotes.Height + Spacing.Xl;

            // ── STATUS ─────────────────────────────────────────
            lblStatus = new Label
            {
                Location = new Point(0, y),
                Width = cardWidth,
                Height = 24,
                Font = Typography.Small,
                ForeColor = Colors.Danger,
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(lblStatus);
            y += 32;

            // ── BUTTONS ────────────────────────────────────────
            btnArchive = new Button
            {
                Text = "Archive Customer",
                Location = new Point(cardWidth - 540, y),
                Width = 160,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.Danger,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnArchive.FlatAppearance.BorderColor = Colors.Danger;
            btnArchive.Click += async (s, e) => await ArchiveAsync();
            Controls.Add(btnArchive);

            btnRestore = new Button
            {
                Text = "Restore Customer",
                Location = new Point(cardWidth - 320, y),
                Width = 180,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Success,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnRestore.FlatAppearance.BorderSize = 0;
            btnRestore.Click += async (s, e) => await RestoreAsync();
            Controls.Add(btnRestore);

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(cardWidth - 260, y),
                Width = 120,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Colors.Border;
            btnCancel.Click += (s, e) => Cancelled?.Invoke(this, EventArgs.Empty);
            Controls.Add(btnCancel);

            btnSave = new Button
            {
                Text = _customerId == 0 ? "Create Customer" : "Save Changes",
                Location = new Point(cardWidth - 130, y),
                Width = 130,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += async (s, e) => await SaveAsync();
            Controls.Add(btnSave);
        }

        // ============================================================
        // HELPERS
        // ============================================================
        private static void AddSectionTitle(Control parent, string text, int x, int y)
        {
            var lbl = new Label
            {
                Text = text,
                Font = Typography.TinyUpper,
                ForeColor = Colors.TextMuted,
                Location = new Point(x, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);
        }

        private static void AddFieldLabel(Control parent, string text, int x, int y)
        {
            var lbl = new Label
            {
                Text = text,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                Location = new Point(x, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);
        }

        private static TextBox AddCardTextBox(Control parent, int x, int y, int width)
        {
            var txt = new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                Height = 32,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Colors.Surface
            };
            parent.Controls.Add(txt);
            return txt;
        }

        // ============================================================
        // LOAD
        // ============================================================
        private async Task LoadAsync()
        {
            if (_customerId <= 0) return;

            var customer = await _customerService.GetCustomerByIdAsync(_customerId);
            if (customer == null)
            {
                MessageBox.Show("Customer not found.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Cancelled?.Invoke(this, EventArgs.Empty);
                return;
            }

            _existing = customer;

            lblCustomerCode.Text = customer.CustomerCode;
            txtFirstName.Text = customer.FirstName;
            txtLastName.Text = customer.LastName;
            txtEmail.Text = customer.Email ?? "";
            txtPhonePrimary.Text = customer.PhonePrimary;

            txtStreet.Text = customer.Street ?? "";
            txtVillage.Text = customer.Village ?? "";
            txtCity.Text = customer.City ?? "";
            txtState.Text = customer.State ?? "";
            txtPostalCode.Text = customer.PostalCode ?? "";
            txtCountry.Text = customer.Country ?? "Philippines";
            txtNotes.Text = customer.Notes ?? "";

            if (chkIsActive != null)
                chkIsActive.Checked = customer.IsActive;

            // ── Archive state ─────────────────────────────
            var currentUser = SessionManager.CurrentUser;
            if (customer.IsArchived)
            {
                if (btnSave != null) btnSave.Visible = false;
                if (btnArchive != null) btnArchive.Visible = false;
                if (btnRestore != null && currentUser?.IsAdmin == true)
                    btnRestore.Visible = true;
                MakeReadOnly();
                SetStatus("This customer is archived. Restore to make changes.", Colors.Warning);
            }
            else
            {
                if (btnArchive != null && currentUser?.IsAdmin == true)
                    btnArchive.Visible = true;
            }
        }

        private void MakeReadOnly()
        {
            foreach (var tb in new[] { txtFirstName, txtLastName, txtEmail, txtPhonePrimary,
                                       txtStreet, txtVillage, txtCity, txtState, txtPostalCode, txtCountry, txtNotes })
            {
                if (tb != null)
                {
                    tb.ReadOnly = true;
                    tb.BackColor = Colors.Background;
                    tb.ForeColor = Colors.TextMuted;
                }
            }
            if (chkIsActive != null) chkIsActive.Enabled = false;
        }

        private async Task ArchiveAsync()
        {
            var currentUser = SessionManager.CurrentUser;
            if (currentUser?.IsAdmin != true)
            {
                SetStatus("Only Admin can archive customers.", Colors.Danger);
                return;
            }

            var display = _existing?.FullName ?? "this customer";
            var confirm = MessageBox.Show(
                $"Archive customer \"{display}\"?\n\nThe customer will be hidden from the main list.",
                "Confirm Archive", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.Yes) return;

            btnArchive.Enabled = false;
            btnArchive.Text = "Archiving...";
            try
            {
                var (success, error) = await _customerService.DeleteCustomerAsync(_customerId);
                if (!success)
                {
                    SetStatus("Failed to archive customer: " + (error ?? ""), Colors.Danger);
                    return;
                }
                MessageBox.Show("Customer archived successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Saved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex) { SetStatus("Error: " + ex.Message, Colors.Danger); }
            finally { btnArchive.Enabled = true; btnArchive.Text = "Archive Customer"; }
        }

        private async Task RestoreAsync()
        {
            var currentUser = SessionManager.CurrentUser;
            if (currentUser?.IsAdmin != true)
            {
                SetStatus("Only Admin can restore customers.", Colors.Danger);
                return;
            }

            var display = _existing?.FullName ?? "this customer";
            var confirm = MessageBox.Show(
                $"Restore customer \"{display}\"?\n\nThe customer will become active again.",
                "Confirm Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            btnRestore.Enabled = false;
            btnRestore.Text = "Restoring...";
            try
            {
                var (success, _, error) = await _customerService.RestoreAsync(_customerId);
                if (!success)
                {
                    SetStatus("Failed to restore: " + (error ?? ""), Colors.Danger);
                    return;
                }
                MessageBox.Show("Customer restored successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Saved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex) { SetStatus("Error: " + ex.Message, Colors.Danger); }
            finally { btnRestore.Enabled = true; btnRestore.Text = "Restore Customer"; }
        }

        // ============================================================
        // SAVE
        // ============================================================
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                SetStatus("First Name is required.", Colors.Danger);
                txtFirstName.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                SetStatus("Last Name is required.", Colors.Danger);
                txtLastName.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtPhonePrimary.Text))
            {
                SetStatus("Phone is required.", Colors.Danger);
                txtPhonePrimary.Focus();
                return;
            }

            btnSave.Enabled = false;
            btnSave.Text = "Saving...";
            SetStatus("", Colors.TextPrimary);

            try
            {
                if (_customerId == 0)
                {
                    var request = new CreateCustomerRequest
                    {
                        FirstName = txtFirstName.Text.Trim(),
                        LastName = txtLastName.Text.Trim(),
                        CustomerType = "Individual",
                        PhonePrimary = txtPhonePrimary.Text.Trim(),
                        Email = NullIfEmpty(txtEmail.Text),
                        Street = NullIfEmpty(txtStreet.Text),
                        Village = NullIfEmpty(txtVillage.Text),
                        City = NullIfEmpty(txtCity.Text),
                        State = NullIfEmpty(txtState.Text),
                        PostalCode = NullIfEmpty(txtPostalCode.Text),
                        Country = NullIfEmpty(txtCountry.Text),
                        Notes = NullIfEmpty(txtNotes.Text)
                    };

                    var (success, customer, error) = await _customerService.CreateCustomerAsync(request);

                    if (success)
                    {
                        MessageBox.Show(
                            "Customer created successfully.\n\nCode: " + (customer?.CustomerCode ?? ""),
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Saved?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        SetStatus(error ?? "Failed to create customer.", Colors.Danger);
                    }
                }
                else
                {
                    // ── Show before/after confirmation for edits ──
                    var updated = BuildCurrentModel();
                    using (var confirmDlg = new CRM.UI.Controls.CustomerEditConfirmationDialog(_existing, updated))
                    {
                        var confirmResult = confirmDlg.ShowDialog(this);
                        if (confirmResult != DialogResult.OK)
                        {
                            SetStatus("Changes discarded.", Colors.TextMuted);
                            btnSave.Enabled = true;
                            btnSave.Text = "Save Changes";
                            return;
                        }
                    }

                    var request = new UpdateCustomerRequest
                    {
                        FirstName = txtFirstName.Text.Trim(),
                        LastName = txtLastName.Text.Trim(),
                        CustomerType = "Individual",
                        PhonePrimary = txtPhonePrimary.Text.Trim(),
                        Email = NullIfEmpty(txtEmail.Text),
                        Street = NullIfEmpty(txtStreet.Text),
                        Village = NullIfEmpty(txtVillage.Text),
                        City = NullIfEmpty(txtCity.Text),
                        State = NullIfEmpty(txtState.Text),
                        PostalCode = NullIfEmpty(txtPostalCode.Text),
                        Country = NullIfEmpty(txtCountry.Text),
                        Notes = NullIfEmpty(txtNotes.Text),
                        IsActive = chkIsActive?.Checked ?? true
                    };

                    var (success, customer, error) = await _customerService.UpdateCustomerAsync(_customerId, request);

                    if (success)
                    {
                        MessageBox.Show("Customer updated successfully.",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Saved?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        SetStatus(error ?? "Failed to update customer.", Colors.Danger);
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatus("Error: " + ex.Message, Colors.Danger);
            }
            finally
            {
                btnSave.Enabled = true;
                btnSave.Text = _customerId == 0 ? "Create Customer" : "Save Changes";
            }
        }

        private CustomerModel BuildCurrentModel()
        {
            return new CustomerModel
            {
                CustomerId      = _customerId,
                CustomerCode    = _existing?.CustomerCode ?? "",
                CustomerType    = _existing?.CustomerType ?? "Individual",
                FirstName       = txtFirstName.Text.Trim(),
                LastName        = txtLastName.Text.Trim(),
                Email           = NullIfEmpty(txtEmail.Text),
                PhonePrimary    = txtPhonePrimary.Text.Trim(),
                Street          = NullIfEmpty(txtStreet.Text),
                Village         = NullIfEmpty(txtVillage.Text),
                City            = NullIfEmpty(txtCity.Text),
                State           = NullIfEmpty(txtState.Text),
                PostalCode      = NullIfEmpty(txtPostalCode.Text),
                Country         = NullIfEmpty(txtCountry.Text),
                Notes           = NullIfEmpty(txtNotes.Text),
                IsActive        = chkIsActive?.Checked ?? true
            };
        }

        private void SetStatus(string text, Color color)
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = color;
        }

        private static string? NullIfEmpty(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}