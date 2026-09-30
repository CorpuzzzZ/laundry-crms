using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRM.WinForms.Services;
using CRM.WinForms.UI;

namespace CRM.WinForms.Forms
{
    public class DashboardControl : UserControl
    {
        private Label lblWelcome = null!;
        private Panel pnlCards = null!;

        public DashboardControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.BackgroundColor;
            Padding = new Padding(20);

            lblWelcome = new Label
            {
                Text = "Welcome back!",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Theme.TextDarkColor,
                Dock = DockStyle.Top,
                Height = 40
            };

            if (SessionManager.CurrentUser != null)
                lblWelcome.Text = $"Welcome back, {SessionManager.CurrentUser.FullName}!";

            pnlCards = new Panel
            {
                Dock = DockStyle.Top,
                Height = 160,
                BackColor = Color.Transparent
            };

            Controls.Add(pnlCards);
            Controls.Add(lblWelcome);

            // Delay card creation until we know the width
            Load += (s, e) =>
            {
                BuildCards();
            };
        }

        private void BuildCards()
        {
            pnlCards.Controls.Clear();

            int cardWidth = 280;
            int cardHeight = 130;
            int spacing = 20;

            AddCard("📊", "Today's Orders", "0", Theme.AccentColor, 0, cardWidth, cardHeight, spacing);
            AddCard("⏳", "Pending Orders", "0", Theme.WarningColor, 1, cardWidth, cardHeight, spacing);
            AddCard("💰", "Today's Revenue", "₱ 0.00", Theme.SuccessColor, 2, cardWidth, cardHeight, spacing);
            AddCard("👥", "Total Customers", "0", Theme.PrimaryColor, 3, cardWidth, cardHeight, spacing);
        }

        private void AddCard(string icon, string title, string value, Color color,
            int index, int width, int height, int spacing)
        {
            var card = new CRM.UI.Controls.RoundedCard
            {
                Size = new Size(width, height),
                Location = new Point(index * (width + spacing), 0),
                CornerRadius = 14,
                FillColor = CRM.UI.Colors.Surface,
                BorderColor = CRM.UI.Colors.Border,
                HoverBorderColor = color,
                AccentColor = color,
                AccentWidth = 4,
                EnableHover = true,
                ShowShadow = true,
                Padding = new Padding(20, 16, 20, 16)
            };

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 22F),
                Location = new Point(18, 18),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = CRM.UI.Colors.TextSecondary,
                Location = new Point(68, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = CRM.UI.Colors.TextPrimary,
                Location = new Point(68, 48),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            card.Controls.AddRange(new Control[] { lblValue, lblTitle, lblIcon });
            pnlCards.Controls.Add(card);
        }
    }
}