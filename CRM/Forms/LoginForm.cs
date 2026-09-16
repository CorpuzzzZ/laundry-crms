using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CRM.WinForms.Config;
using CRM.WinForms.Models;
using CRM.WinForms.Services;
using CRM.WinForms.UI;

namespace CRM.WinForms.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtEmail = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Button btnExit = null!;
        private Label lblStatus = null!;
        private CheckBox chkShowPassword = null!;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = $"{AppConfig.AppName} - Login";
            Size = new Size(460, 580);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Theme.BackgroundColor;
            Font = Theme.BodyFont;

            // ===== HEADER =====
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 160,
                BackColor = Theme.PrimaryColor
            };

            var lblTitle = new Label
            {
                Text = "LAUNDRY CRM",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 25, 0, 0)
            };

            var lblSubtitle = new Label
            {
                Text = "Customer Relationship Management",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(189, 195, 199),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblVersion = new Label
            {
                Text = $"v{AppConfig.AppVersion}",
                Font = Theme.SmallFont,
                ForeColor = Color.FromArgb(149, 165, 166),
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var btnClose = new Button
            {
                Text = "✕",
                Size = new Size(35, 35),
                Location = new Point(415, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Theme.PrimaryColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Application.Exit();

            pnlHeader.Controls.Add(btnClose);
            pnlHeader.Controls.Add(lblVersion);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);

            // ===== BODY =====
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(40, 30, 40, 20)
            };

            // Email
            var lblEmail = new Label
            {
                Text = "Email Address",
                Font = Theme.SmallFont,
                ForeColor = Theme.TextLightColor,
                Location = new Point(40, 30),
                AutoSize = true
            };

            txtEmail = new TextBox
            {
                Location = new Point(40, 52),
                Width = 360,
                Font = new Font("Segoe UI", 11F),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "superadmin@crm.com"
            };

            // Password
            var lblPassword = new Label
            {
                Text = "Password",
                Font = Theme.SmallFont,
                ForeColor = Theme.TextLightColor,
                Location = new Point(40, 105),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(40, 127),
                Width = 360,
                Font = new Font("Segoe UI", 11F),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true,
                Text = "Admin@123456"
            };
            txtPassword.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    e.Handled = true;
                    _ = LoginAsync();
                }
            };

            // Show Password
            chkShowPassword = new CheckBox
            {
                Text = "Show password",
                Font = Theme.SmallFont,
                ForeColor = Theme.TextLightColor,
                Location = new Point(40, 168),
                AutoSize = true
            };
            chkShowPassword.CheckedChanged += (s, e) =>
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;

            // Status
            lblStatus = new Label
            {
                Text = "",
                Font = Theme.SmallFont,
                ForeColor = Theme.DangerColor,
                Location = new Point(40, 200),
                Width = 360,
                Height = 40
            };

            // Login Button
            btnLogin = new Button
            {
                Text = "LOGIN",
                Location = new Point(40, 250),
                Width = 360,
                Height = 45,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            Theme.StylePrimaryButton(btnLogin);
            btnLogin.Click += async (s, e) => await LoginAsync();

            // Exit Button
            btnExit = new Button
            {
                Text = "Exit",
                Location = new Point(40, 305),
                Width = 360,
                Height = 35
            };
            Theme.StyleSecondaryButton(btnExit);
            btnExit.Click += (s, e) => Application.Exit();

            pnlBody.Controls.AddRange(new Control[]
            {
                lblEmail, txtEmail,
                lblPassword, txtPassword,
                chkShowPassword, lblStatus,
                btnLogin, btnExit
            });

            Controls.Add(pnlBody);
            Controls.Add(pnlHeader);

            // Drag support
            pnlHeader.MouseDown += (s, e) =>
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
                SetStatus("Please enter your email.", Theme.DangerColor);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                SetStatus("Please enter your password.", Theme.DangerColor);
                return;
            }

            btnLogin.Enabled = false;
            btnExit.Enabled = false;
            btnLogin.Text = "LOGGING IN...";
            SetStatus("Connecting...", Theme.AccentColor);

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
                    SessionManager.StartSession(user);

                    SetStatus($"Welcome, {user.FullName}!", Theme.SuccessColor);
                    await Task.Delay(500);

                    var mainForm = new MainForm();
                    Hide();
                    mainForm.ShowDialog();
                    Close();
                }
                else
                {
                    SetStatus(response.ErrorMessage ?? "Login failed.", Theme.DangerColor);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}", Theme.DangerColor);
            }
            finally
            {
                btnLogin.Enabled = true;
                btnExit.Enabled = true;
                btnLogin.Text = "LOGIN";
            }
        }

        private void SetStatus(string message, Color color)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(() => SetStatus(message, color)));
                return;
            }
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
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