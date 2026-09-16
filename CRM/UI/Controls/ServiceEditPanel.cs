using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRM.UI;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.UI.Controls
{
    /// <summary>
    /// Ultra-simple create/edit form for services.
    /// Same form for Base / Chemical Add-On / Machine Add-On.
    /// Fields: Service Type, Service Name, Price, Note.
    /// </summary>
    public class ServiceEditPanel : UserControl
    {
        public event EventHandler? Saved;
        public event EventHandler? Cancelled;

        private readonly ServiceApiService _serviceApi = new(ApiClient.Instance);
        private readonly int _serviceId;
        private ServiceModel? _existing;

        private Label lblTitle = null!;
        private Label lblStatus = null!;
        private Label lblServiceCode = null!;

        private ComboBox cmbServiceType = null!;
        private TextBox txtServiceName = null!;
        private NumericUpDown numPrice = null!;
        private TextBox txtNote = null!;

        private Button btnSave = null!;
        private Button btnArchive = null!;
        private Button btnRestore = null!;
        private Button btnCancel = null!;

        public ServiceEditPanel(int serviceId)
        {
            _serviceId = serviceId;
            InitializeComponent();
            Load += (s, e) => _ = LoadAsync();
        }

        private bool IsCreate => _serviceId == 0;

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Background;
            AutoScroll = true;
            Padding = new Padding(Spacing.Xl, Spacing.Lg, Spacing.Xl, Spacing.Xl);

            int cardWidth = 700;
            int contentLeft = Spacing.Xl;
            int fieldWidth = cardWidth - Spacing.Xl * 2;
            int y = 0;

            // ── HEADER ─────────────────────────────────────────
            lblTitle = new Label
            {
                Text = IsCreate ? "New Service" : "Edit Service",
                Font = Typography.H1,
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(0, y)
            };
            Controls.Add(lblTitle);
            y += lblTitle.Height + Spacing.Md;

            // ── MAIN CARD ──────────────────────────────────────
            var card = new RoundedCard
            {
                Location = new Point(0, y),
                Width = cardWidth,
                Padding = new Padding(Spacing.Xl),
                CornerRadius = 8,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true
            };
            Controls.Add(card);

            int cy = Spacing.Xl;

            // Row 1: Service Type
            AddFieldLabel(card, "Service Type *", contentLeft, cy);
            cmbServiceType = new ComboBox
            {
                Location = new Point(contentLeft, cy + 22),
                Width = fieldWidth,
                Font = Typography.Body,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbServiceType.Items.AddRange(new object[] { "Base Service", "Chemical Add-On", "Machine Add-On" });
            cmbServiceType.SelectedIndex = 0;
            card.Controls.Add(cmbServiceType);
            cy += 66;

            // Row 2: Service Code (read-only)
            AddFieldLabel(card, "Service Code", contentLeft, cy);
            lblServiceCode = new Label
            {
                Location = new Point(contentLeft, cy + 22),
                Width = fieldWidth,
                Height = 32,
                Font = Typography.BodyBold,
                ForeColor = Colors.Primary,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = IsCreate ? "(auto-generated)" : "",
                BackColor = Colors.Surface
            };
            card.Controls.Add(lblServiceCode);
            cy += 66;

            // Row 3: Service Name
            AddFieldLabel(card, "Service Name *", contentLeft, cy);
            txtServiceName = new TextBox
            {
                Location = new Point(contentLeft, cy + 22),
                Width = fieldWidth,
                Height = 32,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Colors.Surface
            };
            card.Controls.Add(txtServiceName);
            cy += 66;

            // Row 4: Price
            AddFieldLabel(card, "Price (₱) *", contentLeft, cy);
            numPrice = new NumericUpDown
            {
                Location = new Point(contentLeft, cy + 22),
                Width = fieldWidth,
                Height = 32,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Colors.Surface,
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 1000000
            };
            card.Controls.Add(numPrice);
            cy += 66;

            // Row 5: Note
            AddFieldLabel(card, "Note", contentLeft, cy);
            txtNote = new TextBox
            {
                Location = new Point(contentLeft, cy + 22),
                Width = fieldWidth,
                Height = 60,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Colors.Surface,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            card.Controls.Add(txtNote);
            cy += 92;

            card.Height = cy + Spacing.Lg;
            y += card.Height + Spacing.Xl;

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
                Text = "Archive Service",
                Location = new Point(0, y),
                Width = 140,
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
                Text = "Restore Service",
                Location = new Point(0, y),
                Width = 160,
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
                Text = IsCreate ? "Create Service" : "Save Changes",
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

        private static void AddFieldLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                Location = new Point(x, y),
                AutoSize = true
            });
        }

        // ═══════════════════════════════════════════════════════
        // LOAD
        // ═══════════════════════════════════════════════════════
        private async Task LoadAsync()
        {
            if (IsCreate) return;

            try
            {
                var svc = await _serviceApi.GetByIdAsync(_serviceId);
                if (svc == null)
                {
                    MessageBox.Show("Service not found.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Cancelled?.Invoke(this, EventArgs.Empty);
                    return;
                }

                _existing = svc;
                lblServiceCode.Text = svc.ServiceCode;
                txtServiceName.Text = svc.ServiceName;
                numPrice.Value = Math.Min(svc.BasePrice, numPrice.Maximum);
                txtNote.Text = svc.Description ?? "";

                // Set type dropdown (0=Base, 1=Chemical, 2=Machine)
                cmbServiceType.SelectedIndex = Math.Max(0, Math.Min(2, svc.ServiceType - 1));

                // Archive mode
                var user = SessionManager.CurrentUser;
                if (svc.IsArchived)
                {
                    lblTitle.Text = "Archived Service";
                    lblTitle.ForeColor = Colors.TextMuted;
                    btnSave.Visible = false;
                    btnArchive.Visible = false;
                    if (user?.IsAdmin == true) btnRestore.Visible = true;
                    MakeReadOnly();
                    SetStatus("This service is archived. Restore to make changes.", Colors.Warning);
                }
                else if (user?.IsAdmin == true)
                {
                    btnArchive.Visible = true;
                }
            }
            catch (Exception ex)
            {
                SetStatus("Load error: " + ex.Message, Colors.Danger);
            }
        }

        private void MakeReadOnly()
        {
            if (cmbServiceType != null) cmbServiceType.Enabled = false;
            if (txtServiceName != null) { txtServiceName.ReadOnly = true; txtServiceName.BackColor = Colors.Background; txtServiceName.ForeColor = Colors.TextMuted; }
            if (numPrice != null) numPrice.Enabled = false;
            if (txtNote != null) { txtNote.ReadOnly = true; txtNote.BackColor = Colors.Background; txtNote.ForeColor = Colors.TextMuted; }
        }

        // ═══════════════════════════════════════════════════════
        // SAVE
        // ═══════════════════════════════════════════════════════
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(txtServiceName.Text))
            { SetStatus("Service Name is required.", Colors.Danger); txtServiceName.Focus(); return; }

            btnSave.Enabled = false;
            btnSave.Text = "Saving...";
            SetStatus("", Colors.TextPrimary);

            try
            {
                int serviceType = cmbServiceType.SelectedIndex + 1;   // 1/2/3

                if (IsCreate)
                {
                    var request = new CreateServiceRequest
                    {
                        ServiceType = serviceType,
                        ServiceCategoryId = null,
                        ServiceName = txtServiceName.Text.Trim(),
                        Description = NullIfEmpty(txtNote.Text),
                        BasePrice = numPrice.Value,
                        PricePerUnit = null,
                        UnitOfMeasure = "Item",
                        ProcessingTimeHours = 24,
                        IsExpressService = false,
                        ExpressMultiplier = null,
                        RequiresSpecialHandling = false,
                        HandlingInstructions = null,
                        IsActive = true,
                        SortOrder = 0
                    };

                    var result = await _serviceApi.CreateAsync(request);
                    if (result == null)
                    {
                        SetStatus("Failed to create service. Check server logs.", Colors.Danger);
                        return;
                    }

                    MessageBox.Show(
                        $"Service created successfully.\n\nCode: {result.ServiceCode}",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Saved?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    var request = new UpdateServiceRequest
                    {
                        ServiceType = serviceType,
                        ServiceCategoryId = _existing?.ServiceCategoryId,
                        ServiceName = txtServiceName.Text.Trim(),
                        Description = NullIfEmpty(txtNote.Text),
                        BasePrice = numPrice.Value,
                        PricePerUnit = null,
                        UnitOfMeasure = "Item",
                        ProcessingTimeHours = 24,
                        IsExpressService = false,
                        ExpressMultiplier = null,
                        RequiresSpecialHandling = false,
                        HandlingInstructions = null,
                        IsActive = true,
                        SortOrder = 0
                    };

                    var result = await _serviceApi.UpdateAsync(_serviceId, request);
                    if (result == null)
                    {
                        SetStatus("Failed to update service.", Colors.Danger);
                        return;
                    }

                    MessageBox.Show("Service updated successfully.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Saved?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                SetStatus("Error: " + ex.Message, Colors.Danger);
            }
            finally
            {
                btnSave.Enabled = true;
                btnSave.Text = IsCreate ? "Create Service" : "Save Changes";
            }
        }

        // ═══════════════════════════════════════════════════════
        // ARCHIVE / RESTORE
        // ═══════════════════════════════════════════════════════
        private async Task ArchiveAsync()
        {
            var user = SessionManager.CurrentUser;
            if (user?.IsAdmin != true) { SetStatus("Only Admin can archive services.", Colors.Danger); return; }

            var confirm = MessageBox.Show(
                $"Archive service \"{_existing?.ServiceName}\"?\n\nThe service will be hidden from the main list.",
                "Confirm Archive", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.Yes) return;

            btnArchive.Enabled = false;
            btnArchive.Text = "Archiving...";
            try
            {
                var ok = await _serviceApi.ArchiveAsync(_serviceId);
                if (!ok) { SetStatus("Failed to archive.", Colors.Danger); return; }
                MessageBox.Show("Service archived successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Saved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex) { SetStatus("Error: " + ex.Message, Colors.Danger); }
            finally { btnArchive.Enabled = true; btnArchive.Text = "Archive Service"; }
        }

        private async Task RestoreAsync()
        {
            var user = SessionManager.CurrentUser;
            if (user?.IsAdmin != true) { SetStatus("Only Admin can restore services.", Colors.Danger); return; }

            var confirm = MessageBox.Show(
                $"Restore service \"{_existing?.ServiceName}\"?",
                "Confirm Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            btnRestore.Enabled = false;
            btnRestore.Text = "Restoring...";
            try
            {
                var result = await _serviceApi.RestoreAsync(_serviceId);
                if (result == null) { SetStatus("Failed to restore.", Colors.Danger); return; }
                MessageBox.Show("Service restored successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Saved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex) { SetStatus("Error: " + ex.Message, Colors.Danger); }
            finally { btnRestore.Enabled = true; btnRestore.Text = "Restore Service"; }
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