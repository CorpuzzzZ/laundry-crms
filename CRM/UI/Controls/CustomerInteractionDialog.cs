using System;
using System.Drawing;
using System.Windows.Forms;
using CRM.UI;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.UI.Controls
{
    public class CustomerInteractionDialog : Form
    {
        private readonly int _customerId;
        private readonly InteractionType _type;
        private readonly CustomerInteractionApiService _service;

        private TextBox _txtSubject = null!;
        private TextBox _txtDescription = null!;
        private ComboBox _cmbPriority = null!;
        private ComboBox _cmbRating = null!;
        private Button _btnSave = null!;
        private Button _btnCancel = null!;
        private Label _lblError = null!;

        public CustomerInteractionDialog(int customerId, InteractionType type)
        {
            _customerId = customerId;
            _type = type;
            _service = new CustomerInteractionApiService(ApiClient.Instance);

            BuildUI();
        }

        private void BuildUI()
        {
            Text = "Record " + _type;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(520, 520);
            BackColor = Colors.Background;
            Font = Typography.Body;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            Color typeColor = _type == InteractionType.Inquiry ? Colors.Primary
                            : _type == InteractionType.Complaint ? Colors.Danger
                            : Colors.Success;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 16, 24, 16)
            };
            header.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
            };

            var title = new Label
            {
                Text = "Record " + _type,
                Font = Typography.H2,
                ForeColor = typeColor,
                AutoSize = true,
                Location = new Point(24, 20)
            };
            header.Controls.Add(title);

            var body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                AutoScroll = true,
                BackColor = Colors.Background
            };

            int y = 20;
            int fieldWidth = 424;

            AddLabel(body, "Subject", 24, y);
            _txtSubject = new TextBox
            {
                Location = new Point(24, y + 22),
                Width = fieldWidth,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle
            };
            body.Controls.Add(_txtSubject);
            y += 66;

            AddLabel(body, "Description", 24, y);
            _txtDescription = new TextBox
            {
                Location = new Point(24, y + 22),
                Width = fieldWidth,
                Height = 140,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle
            };
            body.Controls.Add(_txtDescription);
            y += 186;

            AddLabel(body, "Priority", 24, y);
            _cmbPriority = new ComboBox
            {
                Location = new Point(24, y + 22),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Typography.Body
            };
            _cmbPriority.Items.AddRange(new object[] { "Low", "Normal", "High", "Urgent" });
            _cmbPriority.SelectedIndex = 1;
            body.Controls.Add(_cmbPriority);
            y += 66;

            if (_type == InteractionType.Feedback)
            {
                AddLabel(body, "Rating (1-5)", 24, y);
                _cmbRating = new ComboBox
                {
                    Location = new Point(24, y + 22),
                    Width = 200,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = Typography.Body
                };
                _cmbRating.Items.AddRange(new object[]
                {
                    "1 - Poor", "2 - Fair", "3 - Good", "4 - Very Good", "5 - Excellent"
                });
                _cmbRating.SelectedIndex = 4;
                body.Controls.Add(_cmbRating);
                y += 66;
            }

            _lblError = new Label
            {
                Location = new Point(24, y),
                Width = fieldWidth,
                Height = 40,
                ForeColor = Colors.Danger,
                Font = Typography.Small,
                AutoSize = false,
                Visible = false
            };
            body.Controls.Add(_lblError);

            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 12, 24, 12)
            };
            footer.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
            };

            _btnCancel = new Button
            {
                Text = "Cancel",
                Width = 100,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.Body,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnCancel.FlatAppearance.BorderColor = Colors.Border;
            _btnCancel.Location = new Point(footer.Width - 224, 14);
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            _btnSave = new Button
            {
                Text = "Save",
                Width = 100,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = typeColor,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Location = new Point(footer.Width - 114, 14);
            _btnSave.Click += async (s, e) => await SaveAsync();

            footer.Controls.Add(_btnCancel);
            footer.Controls.Add(_btnSave);

            Controls.Add(body);
            Controls.Add(footer);
            Controls.Add(header);
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            _lblError.Visible = false;

            if (string.IsNullOrWhiteSpace(_txtSubject.Text))
            {
                ShowError("Subject is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtDescription.Text))
            {
                ShowError("Description is required.");
                return;
            }

            _btnSave.Enabled = false;
            _btnSave.Text = "Saving...";

            try
            {
                int? rating = null;
                if (_type == InteractionType.Feedback && _cmbRating != null)
                    rating = _cmbRating.SelectedIndex + 1;

                var req = new CreateCustomerInteractionRequest
                {
                    CustomerId = _customerId,
                    InteractionType = (int)_type,
                    Subject = _txtSubject.Text.Trim(),
                    Description = _txtDescription.Text.Trim(),
                    Priority = _cmbPriority.SelectedIndex + 1,
                    Rating = rating
                };

                var result = await _service.CreateAsync(req);

                if (result == null)
                {
                    ShowError("Failed to save. Please try again.");
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                ShowError("Error: " + ex.Message);
            }
            finally
            {
                _btnSave.Enabled = true;
                _btnSave.Text = "Save";
            }
        }

        private void ShowError(string message)
        {
            _lblError.Text = message;
            _lblError.Visible = true;
        }

        private static void AddLabel(Panel parent, string text, int x, int y)
        {
            var lbl = new Label
            {
                Text = text,
                Font = Typography.SmallBold,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(x, y)
            };
            parent.Controls.Add(lbl);
        }
    }
}