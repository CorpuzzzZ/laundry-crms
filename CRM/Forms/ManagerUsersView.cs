using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRM.UI;
using CRM.UI.Controls;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.WinForms.Forms
{
    public class ManagerUsersView : UserControl
    {
        private readonly UserApiService _userService = new(ApiClient.Instance);

        // Header controls
        private Label _lblBranchBadge = null!;
        private Label _lblCrewCount = null!;
        private Label _lblActiveCount = null!;
        private Label _lblAvailableCount = null!;

        // Search & Filter controls
        private TextBox _txtSearch = null!;
        private Button _btnFilterBranchOnly = null!;
        private Button _btnFilterAllCrew = null!;
        private Button _btnFilterUnassigned = null!;
        private string _activeFilter = "branch"; // "branch", "all", "unassigned"

        // Grid
        private DataGridView _dgvUsers = null!;
        private Button _btnAddCrew = null!;
        private Button _btnRefresh = null!;

        // Data caches
        private List<UserModel> _allUsers = new();
        private int _managerBranchId = 1;
        private string _managerBranchName = "Main Branch";

        public ManagerUsersView()
        {
            InitializeComponent();
            Load += async (s, e) =>
            {
                ResolveManagerBranch();
                await LoadUsersAsync();
            };
        }

        private void ResolveManagerBranch()
        {
            var user = SessionManager.CurrentUser;
            if (user?.AssignedBranchId.HasValue == true)
            {
                _managerBranchId = user.AssignedBranchId.Value;
                _managerBranchName = !string.IsNullOrWhiteSpace(user.AssignedBranchName)
                    ? user.AssignedBranchName
                    : $"Branch #{_managerBranchId}";
            }
            else
            {
                _managerBranchId = 1;
                _managerBranchName = "Branch 1";
            }
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Background;
            Font = Typography.Body;
            Padding = new Padding(24);

            // ===== 1. HEADER BANNER =====
            var pnlHeader = new DashboardCard
            {
                Dock = DockStyle.Top,
                Height = 84,
                CornerRadius = 14,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true,
                Padding = new Padding(20, 14, 20, 14)
            };

            var lblTitle = new Label
            {
                Text = "Branch Crew Management",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Colors.TextPrimary,
                Location = new Point(18, 14),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "Manage laundry staff, assign crew members to your branch, and monitor on-duty crew.",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                Location = new Point(20, 48),
                AutoSize = true
            };

            _lblBranchBadge = new Label
            {
                Text = "📍 LOADING BRANCH...",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 148, 136),
                BackColor = Color.FromArgb(240, 253, 250),
                Padding = new Padding(12, 6, 12, 6),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _lblBranchBadge.Location = new Point(pnlHeader.Width - 320, 24);
            _lblBranchBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(_lblBranchBadge);

            // ===== 2. METRIC CARDS ROW =====
            var pnlMetrics = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 90,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0, 16, 0, 16),
                Padding = new Padding(0, 10, 0, 10)
            };
            pnlMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            pnlMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            pnlMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            var cardBranchCrew = CreateSummaryCard("CREW ASSIGNED TO BRANCH", "0", Color.FromArgb(13, 148, 136), out _lblCrewCount);
            var cardActive = CreateSummaryCard("ACTIVE ON-DUTY CREW", "0", Color.FromArgb(16, 185, 129), out _lblActiveCount);
            var cardAvailable = CreateSummaryCard("UNASSIGNED CREW AVAILABLE", "0", Color.FromArgb(59, 130, 246), out _lblAvailableCount);

            pnlMetrics.Controls.Add(cardBranchCrew, 0, 0);
            pnlMetrics.Controls.Add(cardActive, 1, 0);
            pnlMetrics.Controls.Add(cardAvailable, 2, 0);

            // ===== 3. ACTION & FILTER BAR =====
            var pnlControls = new DashboardCard
            {
                Dock = DockStyle.Top,
                Height = 64,
                CornerRadius = 14,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true,
                Padding = new Padding(16, 12, 16, 12)
            };

            // Search Box
            var pnlSearch = new Panel
            {
                Location = new Point(16, 14),
                Size = new Size(260, 36),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            pnlSearch.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var path = DashboardCard.GetRoundedPath(new Rectangle(0, 0, pnlSearch.Width - 1, pnlSearch.Height - 1), 8);
                using var pen = new Pen(Colors.Border, 1f);
                g.DrawPath(pen, path);
            };

            var lblSearchIcon = new Label
            {
                Text = "🔍",
                Font = new Font("Segoe UI Emoji", 10f),
                Location = new Point(8, 7),
                Size = new Size(22, 22),
                ForeColor = Colors.TextSecondary
            };

            _txtSearch = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(248, 250, 252),
                Font = Typography.Body,
                ForeColor = Colors.TextBody,
                Location = new Point(34, 8),
                Width = 215,
                PlaceholderText = "Search staff name or email..."
            };
            _txtSearch.TextChanged += (s, e) => ApplyFilterAndDisplay();

            pnlSearch.Controls.Add(lblSearchIcon);
            pnlSearch.Controls.Add(_txtSearch);
            pnlControls.Controls.Add(pnlSearch);

            // Filter Tabs
            int fx = pnlSearch.Right + 16;
            _btnFilterBranchOnly = CreateFilterChip("📍 Assigned to My Branch", "branch", ref fx, pnlControls);
            _btnFilterAllCrew = CreateFilterChip("👥 All Crew Staff", "all", ref fx, pnlControls);
            _btnFilterUnassigned = CreateFilterChip("⚡ Unassigned Crew", "unassigned", ref fx, pnlControls);

            // Right Action Buttons
            _btnAddCrew = new Button
            {
                Text = "+ Add New Crew Member",
                Dock = DockStyle.Right,
                Width = 190,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand
            };
            _btnAddCrew.FlatAppearance.BorderSize = 0;
            _btnAddCrew.Click += async (s, e) => await OpenAddCrewDialogAsync();

            _btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Dock = DockStyle.Right,
                Width = 90,
                Height = 36,
                Margin = new Padding(0, 0, 8, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.SmallBold,
                Cursor = Cursors.Hand
            };
            _btnRefresh.FlatAppearance.BorderSize = 1;
            _btnRefresh.FlatAppearance.BorderColor = Colors.Border;
            _btnRefresh.Click += async (s, e) => await LoadUsersAsync();

            pnlControls.Controls.Add(_btnRefresh);
            pnlControls.Controls.Add(_btnAddCrew);

            // ===== 4. DATA GRID CARD =====
            var cardGrid = new DashboardCard
            {
                Dock = DockStyle.Fill,
                CornerRadius = 14,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true,
                Padding = new Padding(16),
                Margin = new Padding(0, 16, 0, 0)
            };

            _dgvUsers = new DataGridView
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
                ColumnHeadersHeight = 38,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 250, 252),
                    ForeColor = Colors.TextSecondary,
                    Font = Typography.SmallBold,
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 0, 10, 0)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Colors.TextBody,
                    SelectionBackColor = Colors.PrimaryLight,
                    SelectionForeColor = Colors.TextPrimary,
                    Padding = new Padding(10, 0, 10, 0)
                },
                RowTemplate = { Height = 46 }
            };

            // Columns
            _dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Full Name",
                DataPropertyName = "FullName",
                Width = 200
            });

            _dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Email Address",
                DataPropertyName = "Email",
                Width = 220
            });

            _dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Phone",
                DataPropertyName = "PhoneNumber",
                Width = 130
            });

            _dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Role",
                DataPropertyName = "Role",
                Width = 100
            });

            _dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Branch Status",
                DataPropertyName = "BranchAssignmentStatus",
                Width = 220
            });

            _dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Account Status",
                DataPropertyName = "ActiveStatusText",
                Width = 130
            });

            // Action Button Column (Assign / Remove Branch)
            var btnAssignCol = new DataGridViewButtonColumn
            {
                Name = "ColActionAssign",
                HeaderText = "Branch Assignment",
                Text = "Assign to Branch",
                UseColumnTextForButtonValue = false,
                Width = 160,
                FlatStyle = FlatStyle.Flat
            };
            _dgvUsers.Columns.Add(btnAssignCol);

            // Action Button Column (Toggle Active/Inactive)
            var btnToggleCol = new DataGridViewButtonColumn
            {
                Name = "ColActionToggle",
                HeaderText = "Status Toggle",
                Text = "Toggle Status",
                UseColumnTextForButtonValue = false,
                Width = 130,
                FlatStyle = FlatStyle.Flat
            };
            _dgvUsers.Columns.Add(btnToggleCol);

            _dgvUsers.CellFormatting += DgvUsers_CellFormatting;
            _dgvUsers.CellContentClick += async (s, e) => await DgvUsers_CellContentClick(e.RowIndex, e.ColumnIndex);

            cardGrid.Controls.Add(_dgvUsers);

            // Container stacking
            var pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 16, 0, 0)
            };
            pnlMain.Controls.Add(cardGrid);

            Controls.Add(pnlMain);
            Controls.Add(pnlControls);
            Controls.Add(pnlMetrics);
            Controls.Add(pnlHeader);

            UpdateFilterChipStyles();
        }

        private DashboardCard CreateSummaryCard(string title, string initialValue, Color accentColor, out Label valueLabel)
        {
            var card = new DashboardCard
            {
                Dock = DockStyle.Fill,
                CornerRadius = 12,
                FillColor = Colors.Surface,
                BorderColor = Colors.Border,
                ShowShadow = true,
                Padding = new Padding(16, 12, 16, 12),
                Margin = new Padding(4)
            };

            var lblT = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Colors.TextSecondary,
                Dock = DockStyle.Top,
                Height = 18
            };

            valueLabel = new Label
            {
                Text = initialValue,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = accentColor,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            card.Controls.Add(valueLabel);
            card.Controls.Add(lblT);
            return card;
        }

        private Button CreateFilterChip(string text, string filterKey, ref int xPos, Panel container)
        {
            var btn = new Button
            {
                Text = text,
                Tag = filterKey,
                Location = new Point(xPos, 16),
                Height = 32,
                AutoSize = true,
                Padding = new Padding(12, 0, 12, 0),
                FlatStyle = FlatStyle.Flat,
                Font = Typography.SmallBold,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.Click += (s, e) =>
            {
                _activeFilter = filterKey;
                UpdateFilterChipStyles();
                ApplyFilterAndDisplay();
            };

            container.Controls.Add(btn);
            xPos += btn.PreferredSize.Width + 8;
            return btn;
        }

        private void UpdateFilterChipStyles()
        {
            Button[] chips = { _btnFilterBranchOnly, _btnFilterAllCrew, _btnFilterUnassigned };
            foreach (var chip in chips)
            {
                bool active = (string)chip.Tag! == _activeFilter;
                chip.BackColor = active ? Colors.PrimaryLight : Color.Transparent;
                chip.ForeColor = active ? Colors.Primary : Colors.TextSecondary;
                chip.FlatAppearance.BorderColor = active ? Colors.Primary : Colors.Border;
            }
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                ResolveManagerBranch();
                _lblBranchBadge.Text = $"📍 {_managerBranchName.ToUpperInvariant()} (ID: #{_managerBranchId})";

                _btnRefresh.Enabled = false;
                var allUsers = await _userService.GetAllAsync();

                // Managers can manage crew members
                _allUsers = allUsers.Where(u => u.Role.Equals("Crew", StringComparison.OrdinalIgnoreCase)).ToList();

                // Calculate metrics
                int branchCount = _allUsers.Count(u => u.BranchIds.Contains(_managerBranchId));
                int activeCount = _allUsers.Count(u => u.BranchIds.Contains(_managerBranchId) && u.IsActive);
                int unassignedCount = _allUsers.Count(u => u.BranchIds.Count == 0);

                _lblCrewCount.Text = branchCount.ToString();
                _lblActiveCount.Text = activeCount.ToString();
                _lblAvailableCount.Text = unassignedCount.ToString();

                ApplyFilterAndDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load crew members: {ex.Message}", "Crew Management", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnRefresh.Enabled = true;
            }
        }

        private void ApplyFilterAndDisplay()
        {
            var search = _txtSearch.Text.Trim().ToLowerInvariant();

            var query = _allUsers.AsEnumerable();

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.FullName.ToLowerInvariant().Contains(search) ||
                    u.Email.ToLowerInvariant().Contains(search) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
            }

            // Tab Filter
            switch (_activeFilter)
            {
                case "branch":
                    query = query.Where(u => u.BranchIds.Contains(_managerBranchId));
                    break;
                case "unassigned":
                    query = query.Where(u => u.BranchIds.Count == 0);
                    break;
                case "all":
                default:
                    break;
            }

            var list = query.Select(u => new CrewRowViewModel(u, _managerBranchId)).ToList();
            _dgvUsers.DataSource = list;
        }

        private void DgvUsers_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _dgvUsers.Rows.Count) return;

            var rowItem = _dgvUsers.Rows[e.RowIndex].DataBoundItem as CrewRowViewModel;
            if (rowItem == null) return;

            var colName = _dgvUsers.Columns[e.ColumnIndex].Name;

            // Branch Status coloring
            if (colName == "BranchAssignmentStatus" || e.ColumnIndex == 4)
            {
                if (rowItem.IsAssignedToThisBranch)
                {
                    e.CellStyle!.ForeColor = Color.FromArgb(13, 148, 136);
                    e.CellStyle.Font = Typography.BodyBold;
                }
                else
                {
                    e.CellStyle!.ForeColor = Colors.TextSecondary;
                }
            }

            // Account status coloring
            if (colName == "ActiveStatusText" || e.ColumnIndex == 5)
            {
                e.CellStyle!.ForeColor = rowItem.IsActive ? Color.FromArgb(16, 185, 129) : Color.FromArgb(239, 68, 68);
                e.CellStyle.Font = Typography.SmallBold;
            }

            // Action button texts
            if (colName == "ColActionAssign")
            {
                e.Value = rowItem.IsAssignedToThisBranch ? "✖ Remove from Branch" : "✚ Assign to Branch";
            }
            else if (colName == "ColActionToggle")
            {
                e.Value = rowItem.IsActive ? "Deactivate" : "Activate";
            }
        }

        private async Task DgvUsers_CellContentClick(int rowIndex, int colIndex)
        {
            if (rowIndex < 0 || rowIndex >= _dgvUsers.Rows.Count) return;

            var rowItem = _dgvUsers.Rows[rowIndex].DataBoundItem as CrewRowViewModel;
            if (rowItem == null) return;

            var colName = _dgvUsers.Columns[colIndex].Name;

            // 1. ASSIGN / REMOVE FROM BRANCH
            if (colName == "ColActionAssign")
            {
                if (rowItem.IsAssignedToThisBranch)
                {
                    // Confirm removal
                    var confirm = MessageBox.Show(
                        $"Are you sure you want to remove '{rowItem.FullName}' from {_managerBranchName}?",
                        "Unassign Branch", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirm != DialogResult.Yes) return;

                    var newBranchIds = rowItem.User.BranchIds.Where(id => id != _managerBranchId).ToList();
                    await _userService.SetBranchesAsync(rowItem.User.Id, new SetUserBranchesRequest
                    {
                        BranchIds = newBranchIds,
                        PrimaryBranchId = newBranchIds.FirstOrDefault()
                    });
                }
                else
                {
                    // Add branch
                    var newBranchIds = new List<int>(rowItem.User.BranchIds);
                    if (!newBranchIds.Contains(_managerBranchId))
                    {
                        newBranchIds.Add(_managerBranchId);
                    }

                    await _userService.SetBranchesAsync(rowItem.User.Id, new SetUserBranchesRequest
                    {
                        BranchIds = newBranchIds,
                        PrimaryBranchId = _managerBranchId
                    });
                }

                await LoadUsersAsync();
            }
            // 2. TOGGLE ACTIVE/INACTIVE
            else if (colName == "ColActionToggle")
            {
                await _userService.ToggleActiveAsync(rowItem.User.Id);
                await LoadUsersAsync();
            }
        }

        // ============================================================
        // ADD NEW CREW MODAL DIALOG
        // ============================================================
        private async Task OpenAddCrewDialogAsync()
        {
            using var dlg = new Form
            {
                Text = "Register New Crew Member",
                Size = new Size(460, 480),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Colors.Surface,
                Font = Typography.Body
            };

            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };

            var lblDlgTitle = new Label
            {
                Text = "Add Crew Member",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Colors.TextPrimary,
                Location = new Point(24, 16),
                AutoSize = true
            };

            var lblDlgSub = new Label
            {
                Text = $"The new crew member will be automatically assigned to {_managerBranchName}.",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                Location = new Point(24, 46),
                Size = new Size(390, 32)
            };

            int y = 84;
            var txtFirst = AddField(pnl, "First Name", ref y);
            var txtLast = AddField(pnl, "Last Name", ref y);
            var txtEmail = AddField(pnl, "Email Address", ref y);
            var txtPhone = AddField(pnl, "Phone Number", ref y);
            var txtPass = AddField(pnl, "Initial Password", ref y, isPassword: true);

            var btnSubmit = new Button
            {
                Text = "Create Crew Member",
                Location = new Point(24, y + 10),
                Size = new Size(392, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand
            };
            btnSubmit.FlatAppearance.BorderSize = 0;

            btnSubmit.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtFirst.Text) || string.IsNullOrWhiteSpace(txtLast.Text))
                {
                    MessageBox.Show("Please enter the first and last name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtEmail.Text) || !txtEmail.Text.Contains("@"))
                {
                    MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtPass.Text) || txtPass.Text.Length < 6)
                {
                    MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnSubmit.Enabled = false;
                btnSubmit.Text = "Creating Account...";

                try
                {
                    var created = await _userService.CreateAsync(new CreateUserRequest
                    {
                        FirstName = txtFirst.Text.Trim(),
                        LastName = txtLast.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        PhoneNumber = txtPhone.Text.Trim(),
                        Password = txtPass.Text,
                        Role = "Crew",
                        BranchIds = new List<int> { _managerBranchId }
                    });

                    if (created != null)
                    {
                        MessageBox.Show($"Crew member '{created.FullName}' created successfully and assigned to {_managerBranchName}!",
                            "Crew Registered", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dlg.DialogResult = DialogResult.OK;
                        dlg.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to create crew member:\n\n{ex.Message}", "Creation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnSubmit.Enabled = true;
                    btnSubmit.Text = "Create Crew Member";
                }
            };

            pnl.Controls.Add(lblDlgTitle);
            pnl.Controls.Add(lblDlgSub);
            pnl.Controls.Add(btnSubmit);
            dlg.Controls.Add(pnl);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                await LoadUsersAsync();
            }
        }

        private TextBox AddField(Panel pnl, string label, ref int y, bool isPassword = false)
        {
            var lbl = new Label
            {
                Text = label,
                Font = Typography.SmallBold,
                ForeColor = Colors.TextSecondary,
                Location = new Point(24, y),
                AutoSize = true
            };

            var txt = new TextBox
            {
                Location = new Point(24, y + 20),
                Size = new Size(392, 28),
                Font = Typography.Body,
                UseSystemPasswordChar = isPassword
            };

            pnl.Controls.Add(lbl);
            pnl.Controls.Add(txt);
            y += 56;
            return txt;
        }

        // View Model for DataGridView Row
        private class CrewRowViewModel
        {
            public UserModel User { get; }
            public int TargetBranchId { get; }

            public CrewRowViewModel(UserModel user, int targetBranchId)
            {
                User = user;
                TargetBranchId = targetBranchId;
            }

            public string FullName => User.FullName;
            public string Email => User.Email;
            public string PhoneNumber => string.IsNullOrWhiteSpace(User.PhoneNumber) ? "--" : User.PhoneNumber;
            public string Role => User.Role;
            public bool IsActive => User.IsActive;
            public string ActiveStatusText => User.IsActive ? "● Active" : "○ Inactive";

            public bool IsAssignedToThisBranch => User.BranchIds.Contains(TargetBranchId);

            public string BranchAssignmentStatus
            {
                get
                {
                    if (IsAssignedToThisBranch)
                    {
                        return $"✓ Assigned ({string.Join(", ", User.BranchNames)})";
                    }
                    if (User.BranchNames.Count > 0)
                    {
                        return $"Assigned to: {string.Join(", ", User.BranchNames)}";
                    }
                    return "⚡ Unassigned (Available)";
                }
            }
        }
    }
}
