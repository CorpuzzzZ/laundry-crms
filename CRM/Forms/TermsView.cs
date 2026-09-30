using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRM.UI;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.WinForms.Forms
{
    public class TermsView : UserControl
    {
        private readonly SuperAdminApiService _api = new(ApiClient.Instance);

        private TextBox _txtSearch = null!;
        private DataGridView _dgvTerms = null!;
        private List<TermsModel> _terms = new();

        public TermsView()
        {
            InitializeComponent();
            Load += async (s, e) => await LoadTermsAsync();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Background;
            Font = Typography.Body;
            Padding = new Padding(16);

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Colors.Surface,
                Padding = new Padding(16, 12, 16, 12)
            };

            var lblTitle = new Label
            {
                Text = "Terms & Conditions Management",
                Font = Typography.H2,
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(16, 18)
            };
            pnlTop.Controls.Add(lblTitle);

            var lblSearch = new Label
            {
                Text = "Search:",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(340, 20)
            };
            pnlTop.Controls.Add(lblSearch);

            _txtSearch = new TextBox
            {
                Location = new Point(390, 16),
                Width = 180,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtSearch.TextChanged += (s, e) => FilterTerms();
            pnlTop.Controls.Add(_txtSearch);

            var pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 4, 0, 0)
            };

            var btnNew = new Button
            {
                Text = "+ New Version",
                Width = 130,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnNew.FlatAppearance.BorderSize = 0;
            btnNew.Click += async (s, e) =>
            {
                var dlg = new TermsEditDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                    await LoadTermsAsync();
            };

            var btnView = new Button
            {
                Text = "👁 View",
                Width = 80,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.Body,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnView.FlatAppearance.BorderColor = Colors.Border;
            btnView.Click += (s, e) => ViewSelected();

            var btnEdit = new Button
            {
                Text = "✎ Edit",
                Width = 80,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.Body,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnEdit.FlatAppearance.BorderColor = Colors.Border;
            btnEdit.Click += async (s, e) =>
            {
                var sel = GetSelected();
                if (sel == null)
                {
                    MessageBox.Show("Please select a document from the list.", "Select Terms", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                var dlg = new TermsEditDialog(sel);
                if (dlg.ShowDialog() == DialogResult.OK)
                    await LoadTermsAsync();
            };

            var btnDelete = new Button
            {
                Text = "🗑 Delete",
                Width = 90,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.Danger,
                Font = Typography.Body,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnDelete.FlatAppearance.BorderColor = Colors.Border;
            btnDelete.Click += async (s, e) =>
            {
                var sel = GetSelected();
                if (sel == null) return;
                var res = MessageBox.Show($"Delete Terms & Conditions version '{sel.Version}' - '{sel.Title}'?\n\nThis cannot be undone.",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (res == DialogResult.Yes)
                {
                    await _api.DeleteTermsAsync(sel.TermsId);
                    await LoadTermsAsync();
                }
            };

            var btnRefresh = new Button
            {
                Text = "⟳ Refresh",
                Width = 90,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.Body,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 0)
            };
            btnRefresh.FlatAppearance.BorderColor = Colors.Border;
            btnRefresh.Click += async (s, e) => await LoadTermsAsync();

            pnlButtons.Controls.AddRange(new Control[] { btnNew, btnView, btnEdit, btnDelete, btnRefresh });
            pnlTop.Controls.Add(pnlButtons);

            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(16)
            };

            _dgvTerms = new DataGridView
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
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Colors.BorderLight,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeight = 36,
                EnableHeadersVisualStyles = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 36 },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 250, 252),
                    ForeColor = Colors.TextSecondary,
                    Font = Typography.SmallBold,
                    Padding = new Padding(10, 0, 10, 0),
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Colors.Surface,
                    ForeColor = Colors.TextBody,
                    SelectionBackColor = Colors.PrimaryLight,
                    SelectionForeColor = Colors.TextPrimary,
                    Font = Typography.Body,
                    Padding = new Padding(10, 0, 10, 0),
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                }
            };
            _dgvTerms.CellDoubleClick += (s, e) => ViewSelected();

            _dgvTerms.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Version", DataPropertyName = "Version", FillWeight = 12 });
            _dgvTerms.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Document Title", DataPropertyName = "Title", FillWeight = 38 });
            _dgvTerms.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Effective Date", DataPropertyName = "EffectiveDateText", FillWeight = 15 });
            _dgvTerms.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Expiry Date", DataPropertyName = "ExpiryDateText", FillWeight = 15 });
            _dgvTerms.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mandatory", DataPropertyName = "MandatoryText", FillWeight = 10 });
            _dgvTerms.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "StatusText", FillWeight = 10 });

            pnlGrid.Controls.Add(_dgvTerms);

            Controls.Add(pnlGrid);
            Controls.Add(pnlTop);
        }

        private async Task LoadTermsAsync()
        {
            _terms = await _api.GetTermsAsync();
            FilterTerms();
        }

        private void FilterTerms()
        {
            string q = _txtSearch?.Text.Trim().ToLowerInvariant() ?? "";
            var filtered = string.IsNullOrEmpty(q)
                ? _terms
                : _terms.Where(t => t.Title.ToLowerInvariant().Contains(q) ||
                                    t.Version.ToLowerInvariant().Contains(q)).ToList();

            _dgvTerms.DataSource = null;
            _dgvTerms.DataSource = filtered.Select(t => new
            {
                t.TermsId,
                t.Version,
                t.Title,
                EffectiveDateText = t.EffectiveDate.ToString("MMM dd, yyyy"),
                ExpiryDateText = t.ExpiryDate?.ToString("MMM dd, yyyy") ?? "None",
                MandatoryText = t.IsMandatory ? "Yes" : "Optional",
                StatusText = t.IsActive ? "Active" : "Archived"
            }).ToList();
        }

        private TermsModel? GetSelected()
        {
            if (_dgvTerms.CurrentRow?.DataBoundItem == null) return null;
            var item = _dgvTerms.CurrentRow.DataBoundItem;
            var prop = item.GetType().GetProperty("TermsId");
            if (prop == null) return null;
            int id = (int)prop.GetValue(item)!;
            return _terms.FirstOrDefault(t => t.TermsId == id);
        }

        private void ViewSelected()
        {
            var sel = GetSelected();
            if (sel == null)
            {
                MessageBox.Show("Please select a document to view.", "View Terms", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var dlg = new TermsViewDialog(sel);
            dlg.ShowDialog();
        }
    }
}
