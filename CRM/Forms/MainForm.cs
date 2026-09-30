using System;
using System.Drawing;
using System.Windows.Forms;
using CRM.UI;
using CRM.UI.Controls;
using CRM.WinForms.Config;
using CRM.WinForms.Services;

namespace CRM.WinForms.Forms
{
    public class MainForm : Form
    {
        private Panel _topBar = null!;
        private Sidebar _sidebar = null!;
        private Panel _content = null!; 
        private Panel _contentBody = null!;
        private Label _pageTitle = null!;

        // TopBar Modern SaaS Elements
        private Panel _pnlTenantBadge = null!;
        private Panel _pnlCloudStatus = null!;
        private Panel _pnlDateBadge = null!;
        private Panel _pnlUserProfile = null!;
        private Button _btnLogoutTop = null!;
        private Label _lblUserName = null!;
        private Label _lblUserRole = null!;
        private Panel _pnlAvatar = null!;

        private string _userInitials = "U";
        private Color _userRoleColor = Color.FromArgb(52, 152, 219);
        private string _tenantName = "👑 Master Platform (Central Cloud)";
        private Color _tenantBgColor = Color.FromArgb(238, 242, 255);
        private Color _tenantBorderColor = Color.FromArgb(199, 210, 254);
        private Color _tenantTextColor = Color.FromArgb(67, 56, 202);

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // ===== Form setup =====
            Text = AppConfig.AppName;
            Size = new Size(1400, 850);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1200, 700);
            Shown += (s, e) => { PerformLayout(); _contentBody?.PerformLayout(); _content?.PerformLayout(); };
            BackColor = Colors.Background;
            Font = Typography.Body;

            // ===== Build UI =====
            BuildTopBar();
            BuildSidebar();
            BuildContent();

            // (comment cleaned)
            // LAYOUT: Sidebar spans full height on the left.
            // Top bar sits only above the content area.
            //
            // Structure:
            //   Form
        //   _sidebar (Dock=Left, full height)
        // CONTENT - two-panel structure:
        //       _topBar    (Dock=Top)
        // CONTENT - two-panel structure:
            // (comment cleaned)
            Controls.Add(_content);   // Fill first - bottom of z-order
            Controls.Add(_sidebar);   // Left last - sits beside content

            // ===== Load event =====
            Load += (s, e) =>
            {
                if (SessionManager.CurrentUser != null)
                {
                    ApplySessionUser(SessionManager.CurrentUser);
                }
                NavigateTo("dashboard");
            };
        }

        // ============================================================
        // TOP BAR - Modern Executive SaaS Header
        // ============================================================
        private void BuildTopBar()
        {
            _topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Colors.Surface,
                Padding = new Padding(0)
            };

            // Bottom subtle divider
            _topBar.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border, 1f);
                e.Graphics.DrawLine(pen, 0, _topBar.Height - 1, _topBar.Width, _topBar.Height - 1);
            };

            // 1. Page Title
            _pageTitle = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(24, 18)
            };
            _pageTitle.TextChanged += (s, e) => UpdateTopBarLayout();

            // 2. Active Tenant / Enterprise Database Pill Badge
            _pnlTenantBadge = new Panel
            {
                Size = new Size(260, 32),
                BackColor = Color.Transparent,
                Cursor = Cursors.Default
            };
            _pnlTenantBadge.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, _pnlTenantBadge.Width - 1, _pnlTenantBadge.Height - 1);
                using var path = DashboardCard.GetRoundedPath(r, 6);
                using var bg = new SolidBrush(_tenantBgColor);
                using var borderPen = new Pen(_tenantBorderColor, 1f);
                using var textBrush = new SolidBrush(_tenantTextColor);
                using var fontBold = new Font("Segoe UI", 9f, FontStyle.Bold);
                g.FillPath(bg, path);
                g.DrawPath(borderPen, path);
                g.DrawString(_tenantName, fontBold, textBrush, 10, 7);
            };

            // 3. Cloud Server Status Badge
            _pnlCloudStatus = new Panel
            {
                Size = new Size(160, 34),
                BackColor = Color.Transparent,
                Cursor = Cursors.Default
            };
            _pnlCloudStatus.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, _pnlCloudStatus.Width - 1, _pnlCloudStatus.Height - 1);
                using var path = DashboardCard.GetRoundedPath(r, 6);
                using var bg = new SolidBrush(Color.FromArgb(248, 250, 252));
                using var borderPen = new Pen(Colors.Border, 1f);
                g.FillPath(bg, path);
                g.DrawPath(borderPen, path);

                // Online green dot
                using var dotBrush = new SolidBrush(Color.FromArgb(39, 174, 96));
                g.FillEllipse(dotBrush, 12, 13, 8, 8);

                using var font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                using var textBrush = new SolidBrush(Colors.TextPrimary);
                g.DrawString("MonsterASP Cloud", font, textBrush, 26, 9);
            };

            // 4. Live Date Chip
            _pnlDateBadge = new Panel
            {
                Size = new Size(130, 34),
                BackColor = Color.Transparent,
                Cursor = Cursors.Default
            };
            _pnlDateBadge.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, _pnlDateBadge.Width - 1, _pnlDateBadge.Height - 1);
                using var path = DashboardCard.GetRoundedPath(r, 6);
                using var bg = new SolidBrush(Color.FromArgb(248, 250, 252));
                using var borderPen = new Pen(Colors.Border, 1f);
                g.FillPath(bg, path);
                g.DrawPath(borderPen, path);

                using var font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
                using var textBrush = new SolidBrush(Colors.TextSecondary);
                g.DrawString($"📅  {DateTime.Now:MMM dd, yyyy}", font, textBrush, 10, 9);
            };

            // 5. User Profile Card
            _pnlUserProfile = new Panel
            {
                Size = new Size(190, 44),
                BackColor = Color.Transparent,
                Cursor = Cursors.Default
            };
            _pnlUserProfile.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, _pnlUserProfile.Width - 1, _pnlUserProfile.Height - 1);
                using var path = DashboardCard.GetRoundedPath(r, 8);
                using var bg = new SolidBrush(Color.FromArgb(250, 252, 255));
                using var borderPen = new Pen(Colors.Border, 1f);
                g.FillPath(bg, path);
                g.DrawPath(borderPen, path);
            };

            _pnlAvatar = new Panel
            {
                Size = new Size(32, 32),
                Location = new Point(6, 6),
                BackColor = Color.Transparent
            };
            _pnlAvatar.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var avatarBrush = new SolidBrush(_userRoleColor);
                g.FillEllipse(avatarBrush, 0, 0, 31, 31);

                using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(_userInitials, font, Brushes.White, new RectangleF(0, 0, 32, 32), sf);
            };

            _lblUserName = new Label
            {
                Text = "User",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Colors.TextPrimary,
                Location = new Point(44, 5),
                Size = new Size(138, 17),
                AutoEllipsis = true
            };

            _lblUserRole = new Label
            {
                Text = "ROLE",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = _userRoleColor,
                Location = new Point(44, 22),
                Size = new Size(138, 16),
                AutoEllipsis = true
            };

            _pnlUserProfile.Controls.Add(_pnlAvatar);
            _pnlUserProfile.Controls.Add(_lblUserName);
            _pnlUserProfile.Controls.Add(_lblUserRole);

            // 6. Sign Out Button
            _btnLogoutTop = new Button
            {
                Size = new Size(36, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Color.FromArgb(120, 130, 145),
                Text = "⎋",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnLogoutTop.FlatAppearance.BorderSize = 1;
            _btnLogoutTop.FlatAppearance.BorderColor = Colors.Border;
            _btnLogoutTop.FlatAppearance.MouseOverBackColor = Color.FromArgb(254, 242, 242);
            _btnLogoutTop.FlatAppearance.MouseDownBackColor = Color.FromArgb(254, 226, 226);
            _btnLogoutTop.Click += (s, e) => Logout();

            var toolTip = new ToolTip();
            toolTip.SetToolTip(_btnLogoutTop, "Sign out of workspace");

            _topBar.Controls.Add(_pageTitle);
            _topBar.Controls.Add(_pnlTenantBadge);
            _topBar.Controls.Add(_pnlCloudStatus);
            _topBar.Controls.Add(_pnlDateBadge);
            _topBar.Controls.Add(_pnlUserProfile);
            _topBar.Controls.Add(_btnLogoutTop);

            _topBar.Resize += (s, e) => UpdateTopBarLayout();
            UpdateTopBarLayout();
        }

        private void UpdateTopBarLayout()
        {
            if (_topBar == null || _pageTitle == null || _pnlTenantBadge == null) return;

            _pageTitle.Location = new Point(24, 18);
            _pnlTenantBadge.Location = new Point(_pageTitle.Right + 14, 19);

            int x = _topBar.ClientSize.Width - 24;

            _btnLogoutTop.Location = new Point(x - _btnLogoutTop.Width, 17);
            x = _btnLogoutTop.Left - 10;

            _pnlUserProfile.Location = new Point(x - _pnlUserProfile.Width, 13);
            x = _pnlUserProfile.Left - 10;

            _pnlDateBadge.Location = new Point(x - _pnlDateBadge.Width, 18);
            x = _pnlDateBadge.Left - 10;

            _pnlCloudStatus.Location = new Point(x - _pnlCloudStatus.Width, 18);
        }

        private void ApplySessionUser(CurrentUser u)
        {
            _lblUserName.Text = u.FullName;
            _lblUserRole.Text = u.PrimaryRole.ToUpperInvariant();
            _userInitials = GetInitials(u.FullName);

            if (u.IsSuperAdmin)
            {
                _userRoleColor = Color.FromArgb(79, 70, 229); // Royal Indigo
                _tenantName = "👑 Master Platform (Central Cloud)";
                _tenantBgColor = Color.FromArgb(238, 242, 255);
                _tenantBorderColor = Color.FromArgb(199, 210, 254);
                _tenantTextColor = Color.FromArgb(67, 56, 202);
            }
            else if (u.CompanyId == 1)
            {
                _userRoleColor = u.IsAdmin ? Color.FromArgb(37, 99, 235) : (u.IsManager ? Color.FromArgb(13, 148, 136) : Color.FromArgb(217, 119, 6));
                _tenantName = "🏢 CRM Solutions Inc. (Tenant 1)";
                _tenantBgColor = Color.FromArgb(236, 253, 245);
                _tenantBorderColor = Color.FromArgb(167, 243, 208);
                _tenantTextColor = Color.FromArgb(4, 120, 87);
            }
            else if (u.CompanyId == 2)
            {
                _userRoleColor = u.IsAdmin ? Color.FromArgb(37, 99, 235) : (u.IsManager ? Color.FromArgb(13, 148, 136) : Color.FromArgb(217, 119, 6));
                _tenantName = "🏢 SwiftWash Laundry (Tenant 2)";
                _tenantBgColor = Color.FromArgb(239, 246, 255);
                _tenantBorderColor = Color.FromArgb(191, 219, 254);
                _tenantTextColor = Color.FromArgb(29, 78, 216);
            }
            else if (u.CompanyId == 3)
            {
                _userRoleColor = u.IsAdmin ? Color.FromArgb(37, 99, 235) : (u.IsManager ? Color.FromArgb(13, 148, 136) : Color.FromArgb(217, 119, 6));
                _tenantName = "🏢 Sparkle Cleaners (Tenant 3)";
                _tenantBgColor = Color.FromArgb(250, 245, 255);
                _tenantBorderColor = Color.FromArgb(233, 213, 255);
                _tenantTextColor = Color.FromArgb(126, 34, 206);
            }
            else
            {
                _userRoleColor = Color.FromArgb(52, 152, 219);
                _tenantName = $"🏢 Company #{u.CompanyId} (Tenant)";
                _tenantBgColor = Color.FromArgb(241, 245, 249);
                _tenantBorderColor = Color.FromArgb(203, 213, 225);
                _tenantTextColor = Color.FromArgb(51, 65, 85);
            }

            _lblUserRole.ForeColor = _userRoleColor;
            _sidebar.SetUser(u.FullName, u.PrimaryRole);
            _pnlAvatar.Invalidate();
            _pnlTenantBadge.Invalidate();
            _pnlUserProfile.Invalidate();
            UpdateTopBarLayout();
        }

        private static string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "U";
            var parts = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                return parts[0].Length >= 2 
                    ? parts[0].Substring(0, 2).ToUpperInvariant() 
                    : parts[0].ToUpperInvariant();
            }
            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpperInvariant();
        }

        // ============================================================
        // SIDEBAR - docks left, spans full height
        // ============================================================
        private void BuildSidebar()
        {
            _sidebar = new Sidebar();
            _sidebar.NavigationRequested += OnSidebarNavigation;

            var user = SessionManager.CurrentUser;

            _sidebar.AddSection("Main");
            _sidebar.AddItem(SidebarIcon.Dashboard, "Dashboard", "dashboard");

            if (user?.CanAccessOrders == true)
                _sidebar.AddItem(SidebarIcon.Orders, "Orders", "orders");

            if (user?.CanAccessCustomers == true)
                _sidebar.AddItem(SidebarIcon.Customers, "Customers", "customers");

            if (user?.CanAccessServices == true)
                _sidebar.AddItem(SidebarIcon.Services, "Services", "services");

            if (user?.CanAccessLoyalty == true)
                _sidebar.AddItem(SidebarIcon.Loyalty, "Loyalty", "loyalty");

            if (user?.CanAccessBranches == true || user?.CanAccessUsers == true)
            {
                _sidebar.AddSection("Admin");
                if (user?.CanAccessBranches == true)
                    _sidebar.AddItem(SidebarIcon.Branches, "Branches", "branches");
                if (user?.CanAccessUsers == true)
                    _sidebar.AddItem(SidebarIcon.Users, "Users", "users");
            }

            if (user?.CanAccessReports == true)
            {
                _sidebar.AddSection("Insights");
                _sidebar.AddItem(SidebarIcon.Reports, "Reports", "reports");
            }

            if (user?.IsSuperAdmin == true)
            {
                _sidebar.AddSection("System");
                _sidebar.AddItem(SidebarIcon.Subscription, "Subscription", "subscription");
                _sidebar.AddItem(SidebarIcon.Terms, "Terms", "terms");
            }
            else
            {
                _sidebar.AddSection("Company");
                if (user?.CanAccessSubscription == true)
                    _sidebar.AddItem(SidebarIcon.Subscription, "Subscription", "subscription");

                if (user?.CanAccessTerms == true)
                    _sidebar.AddItem(SidebarIcon.Terms, "Terms", "terms");
            }
        }

        // ============================================================
        // CONTENT - two-panel structure:
        //   _content (Dock=Fill) contains:
            // (comment cleaned)
        // CONTENT - two-panel structure:
        // ============================================================
        private void BuildContent()
        {
            _content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Background,
                Padding = new Padding(0)   // no padding - topbar must reach edges
            };

            _contentBody = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Background,
                Padding = new Padding(Spacing.Xl)   // 24px padding inside content area
            };

            // Add in correct dock order inside _content:
            //   _contentBody (Fill) first - bottom of z-order
            //   _topBar (Top) second - sits above, at top
            _content.Controls.Add(_contentBody);
            _content.Controls.Add(_topBar);
        }

        // ============================================================
        // NAVIGATION
        // ============================================================
        private void OnSidebarNavigation(object? sender, string pageKey)
        {
            NavigateTo(pageKey);
        }

        private void NavigateTo(string pageKey)
        {
            if (pageKey == "__logout__")
            {
                Logout();
                return;
            }

            // Access gate: Super Admin has NO access to operational modules
            var user = SessionManager.CurrentUser;
            if (user?.IsSuperAdmin == true)
            {
                switch (pageKey.ToLowerInvariant())
                {
                    case "orders":
                    case "customers":
                    case "services":
                    case "loyalty":
                    case "branches":
                    case "users":
                        MessageBox.Show("Access Denied: Super Admin does not have access to this module.",
                            "Access Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                }
            }
            else
            {
                if (pageKey.Equals("subscription", StringComparison.OrdinalIgnoreCase) && user?.CanAccessSubscription != true)
                {
                    MessageBox.Show("Access Denied: Only Admin and Super Admin can view Subscription details.",
                        "Access Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (pageKey.Equals("terms", StringComparison.OrdinalIgnoreCase) && user?.CanAccessTerms != true)
                {
                    MessageBox.Show("Access Denied: You do not have access to Terms & Conditions.",
                        "Access Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (pageKey.Equals("reports", StringComparison.OrdinalIgnoreCase) && user?.CanAccessReports != true)
                {
                    MessageBox.Show("Access Denied: Your current subscription plan does not include Reports.",
                        "Access Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (pageKey.Equals("branches", StringComparison.OrdinalIgnoreCase) && user?.CanAccessBranches != true)
                {
                    MessageBox.Show("Access Denied: Your current subscription plan does not include Branch Management.",
                        "Access Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (pageKey.Equals("loyalty", StringComparison.OrdinalIgnoreCase) && user?.CanAccessLoyalty != true)
                {
                    MessageBox.Show("Access Denied: Your current subscription plan does not include Loyalty Management.",
                        "Access Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            _contentBody.Controls.Clear();
            _sidebar.SetActive(pageKey);

            switch (pageKey.ToLowerInvariant())
            {
                case "dashboard":
                    var _dashUser = CRM.WinForms.Services.SessionManager.CurrentUser;
                    if (_dashUser?.IsSuperAdmin == true)
                    {
                        _pageTitle.Text = "Platform Dashboard";
                        _contentBody.Controls.Add(new SuperAdminDashboardControl { Dock = DockStyle.Fill });
                    }
                    else if (_dashUser?.IsCrew == true && _dashUser?.IsAdmin != true && _dashUser?.IsManager != true)
                    {
                        _pageTitle.Text = "Crew Dashboard";
                        _contentBody.Controls.Add(new CrewDashboardControl { Dock = DockStyle.Fill });
                    }
                    else
                    {
                        _pageTitle.Text = "Admin Dashboard";
                        _contentBody.Controls.Add(new AdminDashboardControl { Dock = DockStyle.Fill });
                    }
                    break;

                case "orders":
                    _pageTitle.Text = "Order Management";
                    ShowOrdersList();
                    break;

                case "customers":
                    _pageTitle.Text = "Customer Management";
                    _contentBody.Controls.Add(new CustomersView { Dock = DockStyle.Fill });
                    break;

                case "services":
                    _pageTitle.Text = "Service Management";
                    _contentBody.Controls.Add(new ServicesView { Dock = DockStyle.Fill });
                    break;

                case "loyalty":
                    _pageTitle.Text = "Loyalty Program";
                    _contentBody.Controls.Add(new LoyaltyView { Dock = DockStyle.Fill });
                    break;

                case "branches":
                    _pageTitle.Text = "Branch Management";
                    _contentBody.Controls.Add(new BranchesView { Dock = DockStyle.Fill });
                    break;

                case "users":
                    _pageTitle.Text = "User Management";
                    _contentBody.Controls.Add(new UsersView { Dock = DockStyle.Fill });
                    break;

                case "reports":
                    var _repUser = CRM.WinForms.Services.SessionManager.CurrentUser;
                    if (_repUser?.IsSuperAdmin == true)
                    {
                        _pageTitle.Text = "Platform Reports";
                        _contentBody.Controls.Add(new SuperAdminReportsView { Dock = DockStyle.Fill });
                    }
                    else
                    {
                        _pageTitle.Text = "Reports";
                        _contentBody.Controls.Add(new ReportsView { Dock = DockStyle.Fill });
                    }
                    break;

                case "subscription":
                    if (user?.IsSuperAdmin == true)
                    {
                        _pageTitle.Text = "Subscription Management";
                        _contentBody.Controls.Add(new SubscriptionView { Dock = DockStyle.Fill });
                    }
                    else
                    {
                        _pageTitle.Text = "My Subscription Plan";
                        _contentBody.Controls.Add(new AdminSubscriptionView { Dock = DockStyle.Fill });
                    }
                    break;

                case "terms":
                    _pageTitle.Text = user?.IsSuperAdmin == true
                        ? "Platform Terms & Conditions"
                        : (user?.CanModifyTerms == true ? "Company Terms & Conditions" : "Company Terms & Conditions (Read-Only)");
                    _contentBody.Controls.Add(new TermsView { Dock = DockStyle.Fill });
                    break;

                default:
                    _pageTitle.Text = ToTitleCase(pageKey);
                    ShowPlaceholder(ToTitleCase(pageKey));
                    break;
            }

        }

                private void ShowOrdersList()
        {
            // Clear all existing content
            _contentBody.Controls.Clear();

            var ordersForm = new OrdersForm
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };

            // Wire navigation events
            ordersForm.NewOrderRequested += (s, e) => ShowOrderEdit(0);
            ordersForm.EditOrderRequested += (s, e) => ShowOrderEdit(e);
            ordersForm.PayOrderRequested += (s, e) => ShowPayment(e);
            ordersForm.ViewOrderDetailsRequested += (s, e) => ShowOrderDetails(e);

            _contentBody.Controls.Add(ordersForm);
            ordersForm.Show();
            ordersForm.BringToFront();
        }

        private void ShowOrderEdit(int orderId)
        {
            // Clear content
            _contentBody.Controls.Clear();

            var view = new OrderEditView(orderId)
            {
                Dock = DockStyle.Fill
            };

            // Navigation
            view.Saved += (s, e) => ShowOrdersList();
            view.Cancelled += (s, e) => ShowOrdersList();

            _contentBody.Controls.Add(view);
            view.Dock = DockStyle.None;
            view.Bounds = _contentBody.ClientRectangle;
            view.Dock = DockStyle.Fill;
            view.PerformLayout();
            _contentBody.PerformLayout();
            System.IO.File.AppendAllText(@"C:\temp\order_debug.log", $"[MainForm after add view] _contentBody.Bounds={_contentBody.Bounds} _content.Bounds={_content.Bounds} this.ClientSize={this.ClientSize} this.WindowState={this.WindowState}`r`n");
            view.BringToFront();

            // Update page title
            _pageTitle.Text = orderId == 0 ? "New Order" : "Edit Order";
        }

        private void ShowPayment(int orderId)
        {
            _contentBody.Controls.Clear();

            var view = new PaymentView(orderId)
            {
                Dock = DockStyle.Fill
            };

            view.Saved += (s, e) => ShowOrdersList();
            view.Cancelled += (s, e) => ShowOrdersList();

            _contentBody.Controls.Add(view);
            view.Dock = DockStyle.None;
            view.Bounds = _contentBody.ClientRectangle;
            view.Dock = DockStyle.Fill;
            view.PerformLayout();
            _contentBody.PerformLayout();
            view.BringToFront();

            _pageTitle.Text = "Process Payment";
        }

        private void ShowOrderDetails(int orderId)
        {
            _contentBody.Controls.Clear();

            var view = new OrderDetailsView(orderId)
            {
                Dock = DockStyle.Fill
            };

            view.Cancelled += (s, e) => ShowOrdersList();

            _contentBody.Controls.Add(view);
            view.Dock = DockStyle.None;
            view.Bounds = _contentBody.ClientRectangle;
            view.Dock = DockStyle.Fill;
            view.PerformLayout();
            _contentBody.PerformLayout();
            view.BringToFront();

            _pageTitle.Text = "Order Details";
        }

        private void ShowPlaceholder(string name)
        {
            var lbl = new Label
            {
                Text = $"{name} Module\n\n(Coming Soon)",
                Font = Typography.H2,
                ForeColor = Colors.TextMuted,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            _contentBody.Controls.Add(lbl);
        }

        private static string ToTitleCase(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        // ============================================================
        // LOGOUT
        // ============================================================
        private void Logout()
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            ApiClient.Instance.ClearToken();
            SessionManager.EndSession();

            var login = new LoginForm();
            Hide();
            login.ShowDialog();
            Close();
        }
    }
}



