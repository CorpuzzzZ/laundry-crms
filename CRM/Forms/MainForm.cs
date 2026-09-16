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
        private Label _userInfo = null!;
        private Label _roleInfo = null!;

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
                    var u = SessionManager.CurrentUser;
                    _userInfo.Text = u.FullName;
                    _roleInfo.Text = u.PrimaryRole;
                    _sidebar.SetUser(u.FullName, u.PrimaryRole);
                }
                NavigateTo("dashboard");
            };
        }

        // ============================================================
        // TOP BAR - lives INSIDE _content, docks at top of content area
        // ============================================================
        private void BuildTopBar()
        {
            _topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Colors.Surface,
                Padding = new Padding(Spacing.Xl, 0, Spacing.Xl, 0)
            };

            // Bottom border
            _topBar.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawLine(pen, 0, _topBar.Height - 1, _topBar.Width, _topBar.Height - 1);
            };

            _pageTitle = new Label
            {
                Text = "Dashboard",
                Font = Typography.H1,
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(Spacing.Xl, 16)
            };

            _userInfo = new Label
            {
                Text = "User",
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Size = new Size(220, 20)
            };

            _roleInfo = new Label
            {
                Text = "",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Size = new Size(220, 18)
            };

            _topBar.Controls.Add(_pageTitle);
            _topBar.Controls.Add(_userInfo);
            _topBar.Controls.Add(_roleInfo);

            _topBar.Resize += (s, e) =>
            {
                int rightOffset = Spacing.Xl;
                _userInfo.Location = new Point(
                    _topBar.Width - _userInfo.Width - rightOffset, 12);
                _roleInfo.Location = new Point(
                    _topBar.Width - _roleInfo.Width - rightOffset, 34);
            };
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

            _sidebar.AddItem(SidebarIcon.Customers, "Customers", "customers");

            if (user?.IsSuperAdmin == true || user?.IsAdmin == true || user?.IsManager == true)
                _sidebar.AddItem(SidebarIcon.Services, "Services", "services");

            _sidebar.AddItem(SidebarIcon.Loyalty, "Loyalty", "loyalty");

            if (user?.IsSuperAdmin == true || user?.IsAdmin == true)
            {
                _sidebar.AddSection("Admin");
                _sidebar.AddItem(SidebarIcon.Branches, "Branches", "branches");
                _sidebar.AddItem(SidebarIcon.Users, "Users", "users");
            }

            if (user?.IsCrew != true)
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

            _contentBody.Controls.Clear();
            _sidebar.SetActive(pageKey);

            switch (pageKey)
            {
                case "dashboard":
                    _pageTitle.Text = "Dashboard";
                    _contentBody.Controls.Add(new DashboardControl { Dock = DockStyle.Fill });
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
                    ShowPlaceholder("Loyalty");
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
                    _pageTitle.Text = "Reports";
                    ShowPlaceholder("Reports");
                    break;

                case "subscription":
                    _pageTitle.Text = "Subscription";
                    ShowPlaceholder("Subscription");
                    break;

                case "terms":
                    _pageTitle.Text = "Terms & Conditions";
                    ShowPlaceholder("Terms");
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
            view.BringToFront();

            // Update page title
            _pageTitle.Text = orderId == 0 ? "New Order" : "Edit Order";
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
