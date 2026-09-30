using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRM.UI;
using CRM.WinForms.Config;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.WinForms.Forms
{
    public class LoginForm : Form
    {
        private Panel pnlLeft = null!;
        private Panel pnlRight = null!;

        private TextBox txtEmail = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Button btnClose = null!;
        private Button btnMinimize = null!;
        private Button btnShowPassword = null!;
        private Label lblStatus = null!;
        private Panel pnlStatus = null!;
        private Panel pnlEmailContainer = null!;
        private Panel pnlPasswordContainer = null!;

        private Button btnPresetSuperAdmin = null!;
        private Button btnPresetTenant1 = null!;
        private Button btnPresetTenant2 = null!;
        private Button btnPresetTenant3 = null!;

        private bool _isPasswordMasked = true;

        [System.Runtime.InteropServices.DllImport("gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Width > 0 && Height > 0)
            {
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassStyle |= 0x20000; // CS_DROPSHADOW
                return cp;
            }
        }

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = $"{AppConfig.AppName} - Secure Login";
            Size = new Size(880, 580);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Colors.Surface;
            DoubleBuffered = true;

            // ============================================================
            // 1. LEFT BRAND PANEL (360px wide, rich gradient)
            // ============================================================
            pnlLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 360,
                Padding = new Padding(36, 40, 36, 36)
            };

            pnlLeft.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Rich modern slate gradient
                using (var brush = new LinearGradientBrush(
                    pnlLeft.ClientRectangle,
                    Color.FromArgb(15, 23, 42),
                    Color.FromArgb(30, 41, 59),
                    LinearGradientMode.ForwardDiagonal))
                {
                    g.FillRectangle(brush, pnlLeft.ClientRectangle);
                }

                // Subtle diagonal decorative accent lines in background
                using (var pen = new Pen(Color.FromArgb(15, 255, 255, 255), 1.5f))
                {
                    g.DrawLine(pen, 0, 180, 360, 320);
                    g.DrawLine(pen, 0, 240, 360, 380);
                }
            };

            // Logo Icon Badge
            var pnlLogoBadge = new Panel
            {
                Size = new Size(54, 54),
                Location = new Point(36, 40),
                BackColor = Color.Transparent
            };
            pnlLogoBadge.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = GetRoundedRectPath(new Rectangle(0, 0, 54, 54), 14);
                using var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, 54, 54),
                    Color.FromArgb(52, 152, 219),
                    Color.FromArgb(33, 110, 160),
                    LinearGradientMode.ForwardDiagonal);
                g.FillPath(brush, path);

                // Laundry basket / sparkle symbol
                using var font = new Font("Segoe UI Emoji", 22f);
                using var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString("🧺", font, Brushes.White, new RectangleF(0, 0, 54, 54), sf);
            };

            var lblBrandTitle = new Label
            {
                Text = "LAUNDRY CRM",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(102, 43),
                AutoSize = true
            };

            var lblBrandSub = new Label
            {
                Text = "ENTERPRISE MULTI-TENANT CLOUD",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 180, 230),
                Location = new Point(104, 69),
                AutoSize = true
            };

            // Catchy headline
            var lblHeroHeadline = new Label
            {
                Text = "Intelligent Operations & Complete Data Isolation",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(36, 130),
                Size = new Size(288, 70)
            };

            // Feature List with icons
            var pnlFeatures = new Panel
            {
                Location = new Point(36, 215),
                Size = new Size(288, 200),
                BackColor = Color.Transparent
            };

            AddFeatureBullet(pnlFeatures, 0, "⚡", "Isolated Database-per-Tenant Architecture");
            AddFeatureBullet(pnlFeatures, 48, "🌐", "Live MonsterASP.NET Cloud Synchronization");
            AddFeatureBullet(pnlFeatures, 96, "👔", "Strict Multi-Role Access Control");
            AddFeatureBullet(pnlFeatures, 144, "📊", "Financial Analytics & Automated Operations");

            // Cloud Status Badge (Bottom)
            var pnlCloudBadge = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                BackColor = Color.Transparent
            };
            pnlCloudBadge.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using var path = GetRoundedRectPath(new Rectangle(0, 0, pnlCloudBadge.Width - 1, 54), 10);
                using var bg = new SolidBrush(Color.FromArgb(40, 255, 255, 255));
                using var borderPen = new Pen(Color.FromArgb(60, 255, 255, 255), 1f);
                g.FillPath(bg, path);
                g.DrawPath(borderPen, path);

                // Green glow indicator
                using var dotBrush = new SolidBrush(Color.FromArgb(46, 204, 113));
                g.FillEllipse(dotBrush, 14, 21, 10, 10);

                using var fontBold = new Font("Segoe UI", 9f, FontStyle.Bold);
                using var fontSmall = new Font("Segoe UI", 8f, FontStyle.Regular);
                using var subTextBrush = new SolidBrush(Color.FromArgb(170, 210, 240));
                g.DrawString("Cloud Online: MonsterASP.NET", fontBold, Brushes.White, 32, 11);
                g.DrawString("4 Dedicated SQL Databases Connected", fontSmall, subTextBrush, 32, 29);
            };

            pnlLeft.Controls.Add(pnlLogoBadge);
            pnlLeft.Controls.Add(lblBrandTitle);
            pnlLeft.Controls.Add(lblBrandSub);
            pnlLeft.Controls.Add(lblHeroHeadline);
            pnlLeft.Controls.Add(pnlFeatures);
            pnlLeft.Controls.Add(pnlCloudBadge);

            // ============================================================
            // 2. RIGHT AUTHENTICATION PANEL (520px wide)
            // ============================================================
            pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(48, 20, 48, 30)
            };

            // Window control buttons (Top-Right)
            var pnlWindowControls = new Panel
            {
                Size = new Size(80, 36),
                Location = new Point(pnlRight.Width - 90, 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.Transparent
            };

            btnMinimize = new Button
            {
                Text = "—",
                Size = new Size(34, 30),
                Location = new Point(6, 2),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Colors.TextSecondary,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Click += (s, e) => WindowState = FormWindowState.Minimized;

            btnClose = new Button
            {
                Text = "✕",
                Size = new Size(34, 30),
                Location = new Point(44, 2),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Colors.TextSecondary,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.MouseEnter += (s, e) => { btnClose.BackColor = Colors.Danger; btnClose.ForeColor = Color.White; };
            btnClose.MouseLeave += (s, e) => { btnClose.BackColor = Color.Transparent; btnClose.ForeColor = Colors.TextSecondary; };
            btnClose.Click += (s, e) => Application.Exit();

            pnlWindowControls.Controls.Add(btnMinimize);
            pnlWindowControls.Controls.Add(btnClose);

            // Sign In Header
            var lblSignInTitle = new Label
            {
                Text = "Sign In",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Colors.TextPrimary,
                Location = new Point(48, 38),
                AutoSize = true
            };

            var lblSignInSub = new Label
            {
                Text = "Access your company workspace and operational portal.",
                Font = Typography.Body,
                ForeColor = Colors.TextSecondary,
                Location = new Point(48, 74),
                AutoSize = true
            };

            // Preset Account Chips Header
            var lblPresetTitle = new Label
            {
                Text = "QUICK ACCOUNT SELECTOR",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Colors.TextMuted,
                Location = new Point(48, 110),
                AutoSize = true
            };

            // Preset Buttons Container
            var pnlPresets = new FlowLayoutPanel
            {
                Location = new Point(48, 128),
                Size = new Size(424, 36),
                BackColor = Color.Transparent,
                WrapContents = false
            };

            btnPresetSuperAdmin = CreatePresetChip("👑 Super Admin", "superadmin@crm.com", "Admin@123456", true);
            btnPresetTenant1 = CreatePresetChip("🏢 Tenant 1", "admin@crm.com", "Admin@123456", false);
            btnPresetTenant2 = CreatePresetChip("🏢 Tenant 2", "admin2@crm.com", "Admin@123456", false);
            btnPresetTenant3 = CreatePresetChip("🏢 Tenant 3", "admin3@crm.com", "Admin@123456", false);

            pnlPresets.Controls.Add(btnPresetSuperAdmin);
            pnlPresets.Controls.Add(btnPresetTenant1);
            pnlPresets.Controls.Add(btnPresetTenant2);
            pnlPresets.Controls.Add(btnPresetTenant3);

            // Email Field
            var lblEmail = new Label
            {
                Text = "EMAIL ADDRESS",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Colors.TextSecondary,
                Location = new Point(48, 178),
                AutoSize = true
            };

            pnlEmailContainer = new Panel
            {
                Location = new Point(48, 198),
                Size = new Size(424, 44),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            pnlEmailContainer.Paint += (s, e) => DrawInputBorder(s as Panel, e.Graphics, txtEmail.Focused);

            var lblEmailIcon = new Label
            {
                Text = "✉",
                Font = new Font("Segoe UI", 12f),
                ForeColor = Colors.TextMuted,
                Location = new Point(10, 11),
                Size = new Size(24, 24),
                TextAlign = ContentAlignment.MiddleCenter
            };

            txtEmail = new TextBox
            {
                Text = "superadmin@crm.com",
                Location = new Point(40, 11),
                Width = 370,
                Font = new Font("Segoe UI", 10.5f),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Colors.TextPrimary
            };
            txtEmail.GotFocus += (s, e) => pnlEmailContainer.Invalidate();
            txtEmail.LostFocus += (s, e) => pnlEmailContainer.Invalidate();

            pnlEmailContainer.Controls.Add(lblEmailIcon);
            pnlEmailContainer.Controls.Add(txtEmail);

            // Password Field
            var lblPassword = new Label
            {
                Text = "PASSWORD",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Colors.TextSecondary,
                Location = new Point(48, 254),
                AutoSize = true
            };

            pnlPasswordContainer = new Panel
            {
                Location = new Point(48, 274),
                Size = new Size(424, 44),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            pnlPasswordContainer.Paint += (s, e) => DrawInputBorder(s as Panel, e.Graphics, txtPassword.Focused);

            var lblLockIcon = new Label
            {
                Text = "🔒",
                Font = new Font("Segoe UI", 11f),
                ForeColor = Colors.TextMuted,
                Location = new Point(10, 11),
                Size = new Size(24, 24),
                TextAlign = ContentAlignment.MiddleCenter
            };

            txtPassword = new TextBox
            {
                Text = "Admin@123456",
                Location = new Point(40, 11),
                Width = 338,
                Font = new Font("Segoe UI", 10.5f),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Colors.TextPrimary,
                UseSystemPasswordChar = true
            };
            txtPassword.GotFocus += (s, e) => pnlPasswordContainer.Invalidate();
            txtPassword.LostFocus += (s, e) => pnlPasswordContainer.Invalidate();
            txtPassword.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    e.Handled = true;
                    _ = LoginAsync();
                }
            };

            btnShowPassword = new Button
            {
                Text = "👁",
                Size = new Size(32, 28),
                Location = new Point(384, 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Colors.TextMuted,
                Font = new Font("Segoe UI", 10f),
                Cursor = Cursors.Hand
            };
            btnShowPassword.FlatAppearance.BorderSize = 0;
            btnShowPassword.Click += (s, e) =>
            {
                _isPasswordMasked = !_isPasswordMasked;
                txtPassword.UseSystemPasswordChar = _isPasswordMasked;
                btnShowPassword.ForeColor = _isPasswordMasked ? Colors.TextMuted : Colors.Primary;
            };

            pnlPasswordContainer.Controls.Add(lblLockIcon);
            pnlPasswordContainer.Controls.Add(txtPassword);
            pnlPasswordContainer.Controls.Add(btnShowPassword);

            // Status feedback banner
            pnlStatus = new Panel
            {
                Location = new Point(48, 330),
                Size = new Size(424, 38),
                Visible = false
            };
            pnlStatus.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = GetRoundedRectPath(new Rectangle(0, 0, pnlStatus.Width - 1, pnlStatus.Height - 1), 8);
                using var bg = new SolidBrush(pnlStatus.BackColor);
                using var pen = new Pen(Color.FromArgb(40, lblStatus.ForeColor), 1f);
                g.FillPath(bg, path);
                g.DrawPath(pen, path);
            };

            lblStatus = new Label
            {
                Text = "",
                Font = Typography.SmallBold,
                ForeColor = Colors.Danger,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 12, 0),
                BackColor = Color.Transparent
            };
            pnlStatus.Controls.Add(lblStatus);

            // Login Button
            btnLogin = new Button
            {
                Text = "SIGN IN TO WORKSPACE  ➔",
                Location = new Point(48, 380),
                Size = new Size(424, 48),
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Colors.PrimaryHover;
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Colors.Primary;
            btnLogin.Click += async (s, e) => await LoginAsync();

            // Footer note
            var lblFooter = new Label
            {
                Text = "🔒 Encrypted Cloud Session  •  Isolated Tenant Routing  •  v" + AppConfig.AppVersion,
                Font = Typography.Tiny,
                ForeColor = Colors.TextMuted,
                Location = new Point(48, 442),
                Size = new Size(424, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlRight.Controls.Add(pnlWindowControls);
            pnlRight.Controls.Add(lblSignInTitle);
            pnlRight.Controls.Add(lblSignInSub);
            pnlRight.Controls.Add(lblPresetTitle);
            pnlRight.Controls.Add(pnlPresets);
            pnlRight.Controls.Add(lblEmail);
            pnlRight.Controls.Add(pnlEmailContainer);
            pnlRight.Controls.Add(lblPassword);
            pnlRight.Controls.Add(pnlPasswordContainer);
            pnlRight.Controls.Add(pnlStatus);
            pnlRight.Controls.Add(btnLogin);
            pnlRight.Controls.Add(lblFooter);

            Controls.Add(pnlRight);
            Controls.Add(pnlLeft);

            // Enable smooth form dragging
            EnableDrag(pnlLeft);
            EnableDrag(pnlRight);
            EnableDrag(this);
        }

        private Button CreatePresetChip(string title, string email, string password, bool isSelected)
        {
            var btn = new Button
            {
                Text = title,
                Height = 32,
                AutoSize = true,
                Margin = new Padding(0, 0, 8, 0),
                Padding = new Padding(12, 0, 12, 0),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = isSelected ? Colors.PrimaryLight : Color.FromArgb(245, 247, 250),
                ForeColor = isSelected ? Colors.Primary : Colors.TextSecondary
            };
            btn.FlatAppearance.BorderColor = isSelected ? Colors.Primary : Colors.Border;
            btn.FlatAppearance.BorderSize = 1;

            btn.Click += (s, e) =>
            {
                txtEmail.Text = email;
                txtPassword.Text = password;
                HighlightSelectedChip(btn);
            };

            return btn;
        }

        private void HighlightSelectedChip(Button selected)
        {
            Button[] chips = { btnPresetSuperAdmin, btnPresetTenant1, btnPresetTenant2, btnPresetTenant3 };
            foreach (var chip in chips)
            {
                bool active = (chip == selected);
                chip.BackColor = active ? Colors.PrimaryLight : Color.FromArgb(245, 247, 250);
                chip.ForeColor = active ? Colors.Primary : Colors.TextSecondary;
                chip.FlatAppearance.BorderColor = active ? Colors.Primary : Colors.Border;
            }
        }

        private void DrawInputBorder(Panel? panel, Graphics g, bool isFocused)
        {
            if (panel == null) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            using var path = GetRoundedRectPath(rect, 8);
            using var bg = new SolidBrush(panel.BackColor);
            g.FillPath(bg, path);

            var borderColor = isFocused ? Colors.Primary : Colors.Border;
            float borderWidth = isFocused ? 2f : 1f;

            using var pen = new Pen(borderColor, borderWidth);
            g.DrawPath(pen, path);
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void EnableDrag(Control control)
        {
            control.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    NativeMethods.ReleaseCapture();
                    NativeMethods.SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                SetStatus("Please enter your email address.", Colors.Danger, Colors.DangerLight);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                SetStatus("Please enter your password.", Colors.Danger, Colors.DangerLight);
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "AUTHENTICATING & CONNECTING...";
            btnLogin.BackColor = Colors.TextMuted;
            SetStatus("Connecting to cloud database...", Colors.Primary, Colors.PrimaryLight);

            try
            {
                var request = new LoginRequest
                {
                    Email = txtEmail.Text.Trim(),
                    Password = txtPassword.Text
                };

                var url = AppConfig.ApiV1Url("auth/login");
                var response = await ApiClient.Instance.PostAsync<LoginResponse>(url, request);

                if (response.Success && response.Data != null)
                {
                    var d = response.Data;
                    var user = new CurrentUser
                    {
                        Id = d.User.Id,
                        Email = d.User.Email,
                        FirstName = d.User.FirstName,
                        LastName = d.User.LastName,
                        CompanyId = d.User.CompanyId,
                        Token = d.Token,
                        RefreshToken = d.RefreshToken,
                        Expiration = d.Expiration,
                        Roles = d.User.Roles.ToArray()
                    };

                    ApiClient.Instance.SetToken(d.Token);

                    if (user.IsAdmin)
                    {
                        try
                        {
                            var tenantApi = new TenantApiService(ApiClient.Instance);
                            user.AvailedSubscription = await tenantApi.GetMyAvailedSubscriptionAsync();
                        }
                        catch { }
                    }

                    SessionManager.StartSession(user);

                    SetStatus($"✓ Authenticated! Welcome, {user.FullName}", Colors.Success, Colors.SuccessLight);
                    await Task.Delay(400);

                    var mainForm = new MainForm();
                    Hide();
                    mainForm.ShowDialog();
                    Close();
                }
                else
                {
                    SetStatus(response.ErrorMessage ?? "Invalid credentials. Please verify and try again.", Colors.Danger, Colors.DangerLight);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Connection error: {ex.Message}", Colors.Danger, Colors.DangerLight);
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "SIGN IN TO WORKSPACE  ➔";
                btnLogin.BackColor = Colors.Primary;
            }
        }

        private void SetStatus(string message, Color textCol, Color bgCol)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(() => SetStatus(message, textCol, bgCol)));
                return;
            }

            pnlStatus.Visible = !string.IsNullOrEmpty(message);
            pnlStatus.BackColor = bgCol;
            lblStatus.Text = message;
            lblStatus.ForeColor = textCol;
            pnlStatus.Invalidate();
        }

        private void AddFeatureBullet(Panel container, int yPos, string icon, string text)
        {
            var pnl = new Panel
            {
                Location = new Point(0, yPos),
                Size = new Size(container.Width, 38),
                BackColor = Color.Transparent
            };

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 11f),
                Location = new Point(0, 0),
                Size = new Size(28, 32),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblText = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(220, 235, 250),
                Location = new Point(32, 2),
                Size = new Size(250, 34)
            };

            pnl.Controls.Add(lblIcon);
            pnl.Controls.Add(lblText);
            container.Controls.Add(pnl);
        }
    }

    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    }
}