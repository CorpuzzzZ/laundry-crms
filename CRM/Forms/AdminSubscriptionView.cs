using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRM.UI;
using CRM.UI.Controls;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.WinForms.Forms
{
    /// <summary>
    /// Read-only Subscription View for Tenant Admins.
    /// Displays exclusively the subscription plan availed by the company.
    /// </summary>
    public class AdminSubscriptionView : UserControl
    {
        private readonly TenantApiService _api = new(ApiClient.Instance);

        private Panel _scroller = null!;
        private Panel _contentStack = null!;

        // Header Hero Banner
        private DashboardCard _cardHero = null!;
        private Label _lblPlanName = null!;
        private Label _lblPlanDesc = null!;
        private Panel _pnlStatusBadge = null!;
        private Label _lblPrice = null!;

        // 4 Spec Cards
        private DashboardMetricCard _cardUsers = null!;
        private DashboardMetricCard _cardBranches = null!;
        private DashboardMetricCard _cardOrders = null!;
        private DashboardMetricCard _cardStorage = null!;

        // Details Card
        private DashboardCard _cardDetails = null!;
        private Label _lblCompanyVal = null!;
        private Label _lblStartDateVal = null!;
        private Label _lblEndDateVal = null!;
        private Label _lblDaysRemainingVal = null!;
        private Label _lblPaymentVal = null!;
        private Label _lblRenewalVal = null!;

        // Info Banner
        private Panel _pnlNotice = null!;

        public AdminSubscriptionView()
        {
            InitializeComponent();
            Load += async (s, e) =>
            {
                ArrangeLayout();
                await LoadDataAsync();
            };
            Resize += (s, e) => ArrangeLayout();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Background;
            Font = Typography.Body;

            // 1. Header Toolbar
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 14, 24, 14)
            };

            pnlTop.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border, 1f);
                e.Graphics.DrawLine(pen, 0, pnlTop.Height - 1, pnlTop.Width, pnlTop.Height - 1);
            };

            var lblTitle = new Label
            {
                Text = "My Company Subscription",
                Font = Typography.H2,
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(24, 18)
            };
            pnlTop.Controls.Add(lblTitle);

            var btnRefresh = new Button
            {
                Text = "⟳  Refresh Plan",
                Width = 130,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlTop.Width - 154, 14)
            };
            btnRefresh.FlatAppearance.BorderColor = Colors.Border;
            btnRefresh.Click += async (s, e) => await LoadDataAsync();
            pnlTop.Controls.Add(btnRefresh);

            pnlTop.Resize += (s, e) =>
            {
                btnRefresh.Left = pnlTop.ClientSize.Width - 24 - btnRefresh.Width;
            };

            // 2. Scrollable Body
            _scroller = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Colors.Background
            };

            _contentStack = new Panel
            {
                Width = 1100,
                AutoSize = false,
                BackColor = Colors.Background,
                Padding = new Padding(24, 20, 24, 24)
            };
            _scroller.Controls.Add(_contentStack);

            // 3. Hero Card
            _cardHero = new DashboardCard
            {
                Height = 130,
                AccentColor = Colors.Primary,
                AccentHeight = 4,
                CornerRadius = 14
            };

            _lblPlanName = new Label
            {
                Text = "Loading Subscription...",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Colors.TextPrimary,
                Location = new Point(24, 20),
                AutoSize = true
            };

            _lblPlanDesc = new Label
            {
                Text = "Active multi-tenant SaaS plan details for your business workspace.",
                Font = Typography.Body,
                ForeColor = Colors.TextSecondary,
                Location = new Point(26, 60),
                Size = new Size(580, 44),
                AutoEllipsis = true
            };

            _lblPrice = new Label
            {
                Text = "---",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Colors.Primary,
                TextAlign = ContentAlignment.TopRight,
                Size = new Size(260, 36)
            };

            _pnlStatusBadge = new Panel
            {
                Size = new Size(110, 28),
                BackColor = Color.Transparent
            };
            _pnlStatusBadge.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, _pnlStatusBadge.Width - 1, _pnlStatusBadge.Height - 1);
                using var path = DashboardCard.GetRoundedPath(r, 6);
                using var bg = new SolidBrush(Color.FromArgb(236, 253, 245));
                using var border = new Pen(Color.FromArgb(167, 243, 208), 1f);
                g.FillPath(bg, path);
                g.DrawPath(border, path);

                using var dot = new SolidBrush(Color.FromArgb(16, 185, 129));
                g.FillEllipse(dot, 10, 10, 8, 8);

                using var font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.FromArgb(4, 120, 87));
                g.DrawString("ACTIVE", font, textBrush, 24, 6);
            };

            _cardHero.Controls.Add(_lblPlanName);
            _cardHero.Controls.Add(_lblPlanDesc);
            _cardHero.Controls.Add(_lblPrice);
            _cardHero.Controls.Add(_pnlStatusBadge);

            _cardHero.Resize += (s, e) =>
            {
                int rX = _cardHero.ClientSize.Width - 28;
                _lblPrice.Location = new Point(rX - _lblPrice.Width, 22);
                _pnlStatusBadge.Location = new Point(rX - _pnlStatusBadge.Width, 68);
            };

            // 4. Metric Spec Cards
            _cardUsers = new DashboardMetricCard
            {
                Caption = "Max Users",
                Value = "0",
                IconText = "👥",
                DeltaText = "Allocated Seats",
                IconBgColor = Colors.PrimaryLight,
                IconFgColor = Colors.Primary,
                AccentColor = Colors.Primary,
                CornerRadius = 14
            };

            _cardBranches = new DashboardMetricCard
            {
                Caption = "Branch Quota",
                Value = "0",
                IconText = "🏪",
                DeltaText = "Branch Locations",
                IconBgColor = Colors.SuccessLight,
                IconFgColor = Colors.Success,
                AccentColor = Colors.Success,
                CornerRadius = 14
            };

            _cardOrders = new DashboardMetricCard
            {
                Caption = "Monthly Orders",
                Value = "Unlimited",
                IconText = "📦",
                DeltaText = "Monthly Quota",
                IconBgColor = Color.FromArgb(238, 242, 255),
                IconFgColor = Color.FromArgb(79, 70, 229),
                AccentColor = Color.FromArgb(79, 70, 229),
                CornerRadius = 14
            };

            _cardStorage = new DashboardMetricCard
            {
                Caption = "Storage Limit",
                Value = "10 GB",
                IconText = "💾",
                DeltaText = "Cloud Storage",
                IconBgColor = Color.FromArgb(254, 243, 199),
                IconFgColor = Color.FromArgb(217, 119, 6),
                AccentColor = Color.FromArgb(217, 119, 6),
                CornerRadius = 14
            };

            // 5. Details Card
            _cardDetails = new DashboardCard
            {
                Height = 220,
                CornerRadius = 14
            };

            var lblDetailsHeading = new Label
            {
                Text = "Plan Specifications & Billing Status",
                Font = Typography.H3,
                ForeColor = Colors.TextPrimary,
                Location = new Point(24, 18),
                AutoSize = true
            };
            _cardDetails.Controls.Add(lblDetailsHeading);

            // Left column
            AddDetailRow(_cardDetails, "Subscriber Company:", out _lblCompanyVal, 24, 60);
            AddDetailRow(_cardDetails, "Subscription Started:", out _lblStartDateVal, 24, 100);
            AddDetailRow(_cardDetails, "Renewal / Expiry Date:", out _lblEndDateVal, 24, 140);

            // Right column
            AddDetailRow(_cardDetails, "Days Remaining:", out _lblDaysRemainingVal, 480, 60);
            AddDetailRow(_cardDetails, "Payment Status & Method:", out _lblPaymentVal, 480, 100);
            AddDetailRow(_cardDetails, "Auto-Renewal Status:", out _lblRenewalVal, 480, 140);

            // 6. Platform Notice Box
            _pnlNotice = new Panel
            {
                Height = 64,
                BackColor = Color.FromArgb(238, 242, 255)
            };
            _pnlNotice.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, _pnlNotice.Width - 1, _pnlNotice.Height - 1);
                using var path = DashboardCard.GetRoundedPath(r, 8);
                using var bg = new SolidBrush(Color.FromArgb(238, 242, 255));
                using var border = new Pen(Color.FromArgb(199, 210, 254), 1f);
                g.FillPath(bg, path);
                g.DrawPath(border, path);

                using var fontBold = new Font("Segoe UI", 9f, FontStyle.Bold);
                using var fontRegular = new Font("Segoe UI", 8.5f, FontStyle.Regular);
                using var textBrush = new SolidBrush(Color.FromArgb(67, 56, 202));

                g.DrawString("ℹ️  Platform Subscription Management", fontBold, textBrush, 16, 12);
                g.DrawString("Subscription plans and billing quotas are managed centrally. To upgrade tiers or expand branch licenses, contact platform support.", fontRegular, textBrush, 16, 34);
            };

            _contentStack.Controls.Add(_cardHero);
            _contentStack.Controls.Add(_cardUsers);
            _contentStack.Controls.Add(_cardBranches);
            _contentStack.Controls.Add(_cardOrders);
            _contentStack.Controls.Add(_cardStorage);
            _contentStack.Controls.Add(_cardDetails);
            _contentStack.Controls.Add(_pnlNotice);

            Controls.Add(_scroller);
            Controls.Add(pnlTop);
        }

        private void AddDetailRow(Panel parent, string labelText, out Label valLabel, int x, int y)
        {
            var lbl = new Label
            {
                Text = labelText,
                Font = Typography.SmallBold,
                ForeColor = Colors.TextSecondary,
                Location = new Point(x, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);

            valLabel = new Label
            {
                Text = "---",
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                Location = new Point(x, y + 18),
                AutoSize = true
            };
            parent.Controls.Add(valLabel);
        }

        private void ArrangeLayout()
        {
            if (_contentStack == null) return;

            int containerWidth = Math.Max(700, _scroller.ClientSize.Width - 30);
            _contentStack.Width = containerWidth;

            // Row 1: Hero Card
            _cardHero.Location = new Point(24, 20);
            _cardHero.Width = containerWidth - 48;

            // Row 2: 4 Spec Cards
            int y2 = _cardHero.Bottom + 18;
            int totalSpacing = 16 * 3;
            int cardW = Math.Max(150, (_cardHero.Width - totalSpacing) / 4);

            _cardUsers.Location = new Point(24, y2);
            _cardUsers.Size = new Size(cardW, 115);

            _cardBranches.Location = new Point(_cardUsers.Right + 16, y2);
            _cardBranches.Size = new Size(cardW, 115);

            _cardOrders.Location = new Point(_cardBranches.Right + 16, y2);
            _cardOrders.Size = new Size(cardW, 115);

            _cardStorage.Location = new Point(_cardOrders.Right + 16, y2);
            _cardStorage.Size = new Size(cardW, 115);

            // Row 3: Details Card
            int y3 = _cardUsers.Bottom + 18;
            _cardDetails.Location = new Point(24, y3);
            _cardDetails.Width = _cardHero.Width;

            // Row 4: Notice
            int y4 = _cardDetails.Bottom + 18;
            _pnlNotice.Location = new Point(24, y4);
            _pnlNotice.Width = _cardHero.Width;

            _contentStack.Height = _pnlNotice.Bottom + 40;
        }

        private async Task LoadDataAsync()
        {
            var plan = await _api.GetMyAvailedSubscriptionAsync();
            if (plan == null)
            {
                _lblPlanName.Text = "Standard Plan";
                _lblPlanDesc.Text = "No active subscription plan records were returned by the cloud server.";
                _lblPrice.Text = "---";
                return;
            }

            _lblPlanName.Text = plan.PlanName;
            _lblPlanDesc.Text = !string.IsNullOrWhiteSpace(plan.Description) 
                ? plan.Description 
                : $"Multi-tenant SaaS tier configured for {plan.CompanyName}.";

            _lblPrice.Text = $"₱{plan.PricePerMonth:N0} / mo";

            if (plan.PlanCode == "PLAN1" || plan.PlanName.Contains("Plan 1", StringComparison.OrdinalIgnoreCase))
            {
                _cardUsers.Value = "Unlimited";
                _cardBranches.Value = "Unlimited";
            }
            else if (plan.PlanCode == "PLAN2" || plan.PlanName.Contains("Plan 2", StringComparison.OrdinalIgnoreCase))
            {
                _cardUsers.Value = "1 Mgr, 1 Crew";
                _cardBranches.Value = "1 Branch";
            }
            else if (plan.PlanCode == "PLAN3" || plan.PlanName.Contains("Plan 3", StringComparison.OrdinalIgnoreCase))
            {
                _cardUsers.Value = $"{plan.MaxUsers} Users";
                _cardBranches.Value = "Not Included";
            }
            else
            {
                _cardUsers.Value = plan.MaxUsers.ToString();
                _cardBranches.Value = plan.MaxBranches.ToString();
            }

            _cardOrders.Value = plan.MaxOrdersPerMonth.HasValue ? $"{plan.MaxOrdersPerMonth:N0}" : "Unlimited";
            _cardStorage.Value = plan.MaxStorageGB.HasValue ? $"{plan.MaxStorageGB} GB" : "Unlimited";

            _lblCompanyVal.Text = $"{plan.CompanyName} ({plan.CompanyCode})";
            _lblStartDateVal.Text = plan.StartDate.ToString("MMM dd, yyyy");
            _lblEndDateVal.Text = plan.EndDate.HasValue ? plan.EndDate.Value.ToString("MMM dd, yyyy") : "Perpetual / Ongoing";

            if (plan.DaysRemaining > 0)
                _lblDaysRemainingVal.Text = $"{plan.DaysRemaining} Days Remaining";
            else if (plan.IsExpired)
                _lblDaysRemainingVal.Text = "Plan Expired";
            else
                _lblDaysRemainingVal.Text = "Active (Ongoing)";

            _lblPaymentVal.Text = $"{plan.PaymentStatus} ({plan.PaymentMethod ?? "Cloud Billing"})";
            _lblRenewalVal.Text = plan.AutoRenew ? "Enabled (Auto-Renew)" : "Manual Renewal";
        }
    }
}
