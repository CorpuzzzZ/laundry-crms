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
    public class SubscriptionView : UserControl
    {
        private readonly SuperAdminApiService _api = new(ApiClient.Instance);

        private TabControl _tabs = null!;

        // Tab 1: Companies
        private TextBox _txtSearchCompanies = null!;
        private DataGridView _dgvCompanies = null!;
        private List<CompanySubscriptionModel> _companies = new();
        private List<SubscriptionPlanModel> _plans = new();

        // Tab 2: Plans
        private DataGridView _dgvPlans = null!;

        public SubscriptionView()
        {
            InitializeComponent();
            Load += async (s, e) =>
            {
                await LoadPlansAsync();
                await LoadCompaniesAsync();
            };
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Background;
            Font = Typography.Body;

            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(16, 8),
                Font = Typography.Body
            };

            _tabs.TabPages.Add(BuildCompaniesTab());
            _tabs.TabPages.Add(BuildPlansTab());
            _tabs.SelectedIndexChanged += async (s, e) =>
            {
                if (_tabs.SelectedIndex == 0) await LoadCompaniesAsync();
                else await LoadPlansAsync();
            };

            Controls.Add(_tabs);
        }

        // ============================================================
        // TAB 1: COMPANIES & SUBSCRIPTIONS
        // ============================================================
        private TabPage BuildCompaniesTab()
        {
            var tab = new TabPage("🏢 Companies & Subscriptions");
            tab.BackColor = Colors.Background;
            tab.Padding = new Padding(16);

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Colors.Surface,
                Padding = new Padding(16, 12, 16, 12)
            };

            var lblSearch = new Label
            {
                Text = "Search:",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(16, 20)
            };
            pnlTop.Controls.Add(lblSearch);

            _txtSearchCompanies = new TextBox
            {
                Location = new Point(70, 16),
                Width = 220,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtSearchCompanies.TextChanged += (s, e) => FilterCompanies();
            pnlTop.Controls.Add(_txtSearchCompanies);

            var pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 4, 0, 0)
            };

            var btnAddCompany = new Button
            {
                Text = "+ Add Company & Plan",
                Width = 180,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnAddCompany.FlatAppearance.BorderSize = 0;
            btnAddCompany.Click += async (s, e) =>
            {
                if (_plans.Count == 0) await LoadPlansAsync();
                var dlg = new AddCompanyDialog(_plans);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    await LoadCompaniesAsync();
                    await LoadPlansAsync();
                }
            };

            var btnAssignPlan = new Button
            {
                Text = "⇄ Change Plan",
                Width = 120,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.Body,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnAssignPlan.FlatAppearance.BorderColor = Colors.Border;
            btnAssignPlan.Click += async (s, e) =>
            {
                var sel = GetSelectedCompany();
                if (sel == null)
                {
                    MessageBox.Show("Please select a company from the table.", "Select Company", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (_plans.Count == 0) await LoadPlansAsync();
                var dlg = new AssignPlanDialog(sel, _plans);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    await LoadCompaniesAsync();
                    await LoadPlansAsync();
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
            btnRefresh.Click += async (s, e) => await LoadCompaniesAsync();

            pnlButtons.Controls.AddRange(new Control[] { btnAddCompany, btnAssignPlan, btnRefresh });
            pnlTop.Controls.Add(pnlButtons);

            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(16)
            };

            _dgvCompanies = MakeGrid();
            _dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Code", DataPropertyName = "CompanyCode", FillWeight = 12 });
            _dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Company Name", DataPropertyName = "CompanyName", FillWeight = 26 });
            _dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Industry", DataPropertyName = "Industry", FillWeight = 16 });
            _dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Current Plan", DataPropertyName = "CurrentPlanName", FillWeight = 20 });
            _dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Monthly Price", DataPropertyName = "PriceText", FillWeight = 14 });
            _dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Renewal Date", DataPropertyName = "RenewalText", FillWeight = 14 });
            _dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Payment", DataPropertyName = "PaymentStatus", FillWeight = 12 });
            _dgvCompanies.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "StatusText", FillWeight = 10 });

            pnlGrid.Controls.Add(_dgvCompanies);

            tab.Controls.Add(pnlGrid);
            tab.Controls.Add(pnlTop);

            return tab;
        }

        // ============================================================
        // TAB 2: SUBSCRIPTION PLANS
        // ============================================================
        private TabPage BuildPlansTab()
        {
            var tab = new TabPage("💳 Subscription Plans");
            tab.BackColor = Colors.Background;
            tab.Padding = new Padding(16);

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Colors.Surface,
                Padding = new Padding(16, 12, 16, 12)
            };

            var lblDesc = new Label
            {
                Text = "Manage platform subscription tiers, pricing, and resource allocations",
                Font = Typography.Body,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(16, 18)
            };
            pnlTop.Controls.Add(lblDesc);

            var pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 4, 0, 0)
            };

            var btnNewPlan = new Button
            {
                Text = "+ Create New Plan",
                Width = 160,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnNewPlan.FlatAppearance.BorderSize = 0;
            btnNewPlan.Click += async (s, e) =>
            {
                var dlg = new CreatePlanDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                    await LoadPlansAsync();
            };

            var btnEditPlan = new Button
            {
                Text = "✎ Edit Plan",
                Width = 100,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.Body,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnEditPlan.FlatAppearance.BorderColor = Colors.Border;
            btnEditPlan.Click += async (s, e) =>
            {
                var sel = GetSelectedPlan();
                if (sel == null)
                {
                    MessageBox.Show("Please select a plan from the table.", "Select Plan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                var dlg = new CreatePlanDialog(sel);
                if (dlg.ShowDialog() == DialogResult.OK)
                    await LoadPlansAsync();
            };

            var btnDeletePlan = new Button
            {
                Text = "🗑 Delete / Toggle",
                Width = 130,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.Danger,
                Font = Typography.Body,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnDeletePlan.FlatAppearance.BorderColor = Colors.Border;
            btnDeletePlan.Click += async (s, e) =>
            {
                var sel = GetSelectedPlan();
                if (sel == null) return;
                var res = MessageBox.Show($"Are you sure you want to deactivate or remove '{sel.PlanName}'?",
                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (res == DialogResult.Yes)
                {
                    await _api.DeletePlanAsync(sel.PlanId);
                    await LoadPlansAsync();
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
            btnRefresh.Click += async (s, e) => await LoadPlansAsync();

            pnlButtons.Controls.AddRange(new Control[] { btnNewPlan, btnEditPlan, btnDeletePlan, btnRefresh });
            pnlTop.Controls.Add(pnlButtons);

            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(16)
            };

            _dgvPlans = MakeGrid();
            _dgvPlans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Plan Name", DataPropertyName = "PlanName", FillWeight = 22 });
            _dgvPlans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Plan Code", DataPropertyName = "PlanCode", FillWeight = 14 });
            _dgvPlans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Monthly Price", DataPropertyName = "PriceMoText", FillWeight = 14 });
            _dgvPlans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Annual Price", DataPropertyName = "PriceYrText", FillWeight = 14 });
            _dgvPlans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Max Users", DataPropertyName = "MaxUsers", FillWeight = 10 });
            _dgvPlans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Max Branches", DataPropertyName = "MaxBranches", FillWeight = 10 });
            _dgvPlans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Max Orders", DataPropertyName = "OrdersText", FillWeight = 12 });
            _dgvPlans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Active Tenants", DataPropertyName = "CompanyCount", FillWeight = 12 });
            _dgvPlans.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "StatusText", FillWeight = 10 });

            pnlGrid.Controls.Add(_dgvPlans);

            tab.Controls.Add(pnlGrid);
            tab.Controls.Add(pnlTop);

            return tab;
        }

        private static DataGridView MakeGrid() => new()
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

        private async Task LoadCompaniesAsync()
        {
            _companies = await _api.GetCompaniesAsync();
            FilterCompanies();
        }

        private void FilterCompanies()
        {
            string q = _txtSearchCompanies?.Text.Trim().ToLowerInvariant() ?? "";
            var filtered = string.IsNullOrEmpty(q)
                ? _companies
                : _companies.Where(c => c.CompanyName.ToLowerInvariant().Contains(q) ||
                                       c.CompanyCode.ToLowerInvariant().Contains(q) ||
                                       (c.CurrentPlanName?.ToLowerInvariant().Contains(q) ?? false)).ToList();

            _dgvCompanies.DataSource = null;
            _dgvCompanies.DataSource = filtered.Select(c => new
            {
                c.CompanyId,
                c.CompanyCode,
                c.CompanyName,
                Industry = c.Industry ?? "Laundry Services",
                CurrentPlanName = c.CurrentPlanName ?? "Unassigned",
                PriceText = c.PricePerMonth.HasValue ? $"PHP {c.PricePerMonth.Value:N0}/mo" : "—",
                RenewalText = c.EndDate?.ToString("MMM dd, yyyy") ?? "—",
                PaymentStatus = c.PaymentStatus ?? "None",
                StatusText = c.IsActive ? "Active" : "Inactive"
            }).ToList();
        }

        private async Task LoadPlansAsync()
        {
            _plans = await _api.GetPlansAsync();
            _dgvPlans.DataSource = null;
            _dgvPlans.DataSource = _plans.Select(p => new
            {
                p.PlanId,
                p.PlanName,
                p.PlanCode,
                PriceMoText = $"PHP {p.PricePerMonth:N0}/mo",
                PriceYrText = p.PricePerYear.HasValue ? $"PHP {p.PricePerYear.Value:N0}/yr" : "—",
                p.MaxUsers,
                p.MaxBranches,
                OrdersText = p.MaxOrdersPerMonth.HasValue ? p.MaxOrdersPerMonth.Value.ToString("N0") : "Unlimited",
                p.CompanyCount,
                StatusText = p.IsActive ? "Active" : "Inactive"
            }).ToList();
        }

        private CompanySubscriptionModel? GetSelectedCompany()
        {
            if (_dgvCompanies.CurrentRow?.DataBoundItem == null) return null;
            var item = _dgvCompanies.CurrentRow.DataBoundItem;
            var prop = item.GetType().GetProperty("CompanyId");
            if (prop == null) return null;
            int id = (int)prop.GetValue(item)!;
            return _companies.FirstOrDefault(c => c.CompanyId == id);
        }

        private SubscriptionPlanModel? GetSelectedPlan()
        {
            if (_dgvPlans.CurrentRow?.DataBoundItem == null) return null;
            var item = _dgvPlans.CurrentRow.DataBoundItem;
            var prop = item.GetType().GetProperty("PlanId");
            if (prop == null) return null;
            int id = (int)prop.GetValue(item)!;
            return _plans.FirstOrDefault(p => p.PlanId == id);
        }
    }
}
