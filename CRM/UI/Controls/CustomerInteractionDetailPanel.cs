using System;
using System.Drawing;
using System.Windows.Forms;
using CRM.UI;
using CRM.WinForms.Models;
using CRM.WinForms.Services;

namespace CRM.UI.Controls
{
    public class CustomerInteractionDetailPanel : UserControl
    {
        public event EventHandler? BackRequested;

        private readonly CustomerInteractionModel _interaction;
        private readonly CustomerInteractionApiService _service;

        public bool Changed { get; private set; }

        private ComboBox _cmbStatus = null!;
        private TextBox _txtStatusNotes = null!;
        private Button _btnApplyStatus = null!;
        private FlowLayoutPanel _historyList = null!;
        private Label _lblStatus = null!;

        public CustomerInteractionDetailPanel(
            CustomerInteractionModel interaction,
            CustomerInteractionApiService service)
        {
            _interaction = interaction;
            _service = service;

            Dock = DockStyle.Fill;
            BackColor = Colors.Background;

            BuildUI();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _ = LoadHistoryAsync();
        }

        private void BuildUI()
        {
            Color typeColor = _interaction.InteractionType == 1 ? Colors.Primary
                            : _interaction.InteractionType == 2 ? Colors.Danger
                            : Colors.Success;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 16, 24, 16)
            };
            header.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
            };

            var btnBack = new Button
            {
                Text = "< Back to interactions",
                Location = new Point(24, 18),
                Width = 170,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.Primary,
                Font = Typography.Body,
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderColor = Colors.Border;
            btnBack.Click += (s, e) => BackRequested?.Invoke(this, EventArgs.Empty);

            header.Controls.Add(btnBack);

            var identity = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 16, 24, 16)
            };
            identity.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawLine(pen, 0, identity.Height - 1, identity.Width, identity.Height - 1);
            };

            var typeBadge = new Label
            {
                Text = _interaction.InteractionTypeName.ToUpperInvariant(),
                Font = Typography.TinyUpper,
                ForeColor = typeColor,
                BackColor = Color.FromArgb(30, typeColor),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(100, 24),
                Location = new Point(24, 16)
            };
            identity.Controls.Add(typeBadge);

            var priorityLbl = new Label
            {
                Text = _interaction.PriorityName + " priority",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(136, 20)
            };
            identity.Controls.Add(priorityLbl);

            var subject = new Label
            {
                Text = _interaction.Subject,
                Font = Typography.H1,
                ForeColor = Colors.TextPrimary,
                AutoSize = false,
                Height = 32,
                Width = identity.Width - 48,
                Location = new Point(24, 48),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            identity.Controls.Add(subject);

            var body = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Colors.Background,
                Padding = new Padding(24)
            };

            int y = 16;

            y = AddMetaRow(body, y, "Created", _interaction.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
            if (_interaction.Rating.HasValue)
                y = AddMetaRow(body, y, "Rating", _interaction.Rating.Value + " / 5");
            y += 12;

            AddSectionHeader(body, "Description", y);
            y += 26;
            var desc = new TextBox
            {
                Text = _interaction.Description,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = Typography.Body,
                BackColor = Colors.Surface,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(24, y),
                Width = body.Width - 48,
                Height = 120,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            body.Controls.Add(desc);
            y += 140;

            // Change Status: only for Manager and Admin
            var currentUser = CRM.WinForms.Services.SessionManager.CurrentUser;
            bool canUpdateStatus = currentUser?.IsManager == true || currentUser?.IsAdmin == true;

            if (canUpdateStatus)
            {
            AddSectionHeader(body, "Change Status", y);
            y += 26;

            _cmbStatus = new ComboBox
            {
                Location = new Point(24, y),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Typography.Body
            };
            _cmbStatus.Items.AddRange(new object[] { "Open", "In Progress", "Resolved", "Closed" });
            int selIdx = Math.Max(0, _interaction.Status - 1);
            _cmbStatus.SelectedIndex = selIdx;
            body.Controls.Add(_cmbStatus);

            _lblStatus = new Label
            {
                Text = "Current: " + _interaction.StatusName,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(236, y + 4)
            };
            body.Controls.Add(_lblStatus);

            y += 44;

            _txtStatusNotes = new TextBox
            {
                Location = new Point(24, y),
                Width = body.Width - 48,
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            body.Controls.Add(_txtStatusNotes);
            y += 74;

            _btnApplyStatus = new Button
            {
                Text = "Apply Status Change",
                Location = new Point(24, y),
                Width = 180,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand
            };
            _btnApplyStatus.FlatAppearance.BorderSize = 0;
            _btnApplyStatus.Click += async (s, e) => await ApplyStatusChangeAsync();
            body.Controls.Add(_btnApplyStatus);

            y += 60;

            }  // end canUpdateStatus

            AddSectionHeader(body, "Status History (Audit Log)", y);
            y += 26;

            _historyList = new FlowLayoutPanel
            {
                Location = new Point(24, y),
                Width = body.Width - 48,
                Height = 260,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Colors.Surface,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            body.Controls.Add(_historyList);

            Controls.Add(body);
            Controls.Add(identity);
            Controls.Add(header);
        }

        private async System.Threading.Tasks.Task ApplyStatusChangeAsync()
        {
            int newStatus = _cmbStatus.SelectedIndex + 1;

            if (newStatus == _interaction.Status)
            {
                MessageBox.Show("Status is unchanged.", "No change",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _btnApplyStatus.Enabled = false;
            _btnApplyStatus.Text = "Applying...";

            try
            {
                var req = new ChangeInteractionStatusRequest
                {
                    NewStatus = newStatus,
                    Notes = string.IsNullOrWhiteSpace(_txtStatusNotes.Text)
                        ? null
                        : _txtStatusNotes.Text.Trim()
                };

                var result = await _service.ChangeStatusAsync(
                    _interaction.CustomerId, _interaction.InteractionId, req);

                if (result == null)
                {
                    MessageBox.Show("Failed to change status.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _interaction.Status = result.Status;
                _interaction.StatusName = result.StatusName;
                _lblStatus.Text = "Current: " + result.StatusName;
                _txtStatusNotes.Clear();
                Changed = true;

                await LoadHistoryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnApplyStatus.Enabled = true;
                _btnApplyStatus.Text = "Apply Status Change";
            }
        }

        private async System.Threading.Tasks.Task LoadHistoryAsync()
        {
            _historyList.Controls.Clear();

            var history = await _service.GetHistoryAsync(
                _interaction.CustomerId, _interaction.InteractionId);

            if (history.Count == 0)
            {
                var lbl = new Label
                {
                    Text = "No history yet.",
                    Font = Typography.Small,
                    ForeColor = Colors.TextMuted,
                    AutoSize = false,
                    Height = 40,
                    Width = _historyList.ClientSize.Width - 20,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Margin = new Padding(0, 20, 0, 0)
                };
                _historyList.Controls.Add(lbl);
                return;
            }

            for (int i = 0; i < history.Count; i++)
            {
                var entry = history[i];
                bool isLast = i == history.Count - 1;

                var row = new Panel
                {
                    Width = _historyList.ClientSize.Width - 24,
                    Height = 56,
                    BackColor = Colors.Surface,
                    Margin = new Padding(8, 4, 8, 4),
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };

                var dot = new Panel
                {
                    Location = new Point(12, 20),
                    Size = new Size(10, 10),
                    BackColor = isLast ? Colors.Primary : Colors.TextMuted
                };
                row.Controls.Add(dot);

                var fromTo = entry.FromStatus.HasValue
                    ? entry.FromStatusName + "  ->  " + entry.ToStatusName
                    : "Created as " + entry.ToStatusName;

                var textLbl = new Label
                {
                    Text = fromTo,
                    Font = Typography.SmallBold,
                    ForeColor = Colors.TextPrimary,
                    AutoSize = false,
                    Height = 20,
                    Width = row.Width - 200,
                    Location = new Point(34, 8)
                };
                row.Controls.Add(textLbl);

                var dateLbl = new Label
                {
                    Text = entry.ChangedAt.ToString("yyyy-MM-dd HH:mm"),
                    Font = Typography.Tiny,
                    ForeColor = Colors.TextSecondary,
                    AutoSize = false,
                    Height = 20,
                    Width = row.Width - 200,
                    Location = new Point(34, 26)
                };
                row.Controls.Add(dateLbl);

                if (!string.IsNullOrWhiteSpace(entry.Notes))
                {
                    var notesLbl = new Label
                    {
                        Text = entry.Notes,
                        Font = Typography.Tiny,
                        ForeColor = Colors.TextMuted,
                        AutoSize = false,
                        Height = 16,
                        Width = row.Width - 200,
                        Location = new Point(34, 42)
                    };
                    row.Controls.Add(notesLbl);
                }

                _historyList.Controls.Add(row);
            }
        }

        private static int AddMetaRow(Panel parent, int y, string label, string value)
        {
            var lbl = new Label
            {
                Text = label,
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = false,
                Width = 100,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(24, y)
            };
            parent.Controls.Add(lbl);

            var val = new Label
            {
                Text = value,
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                AutoSize = false,
                Width = parent.Width - 148,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(130, y),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            parent.Controls.Add(val);

            return y + 26;
        }

        private static void AddSectionHeader(Panel parent, string title, int y)
        {
            var header = new Label
            {
                Text = title.ToUpperInvariant(),
                Font = Typography.TinyUpper,
                ForeColor = Colors.TextMuted,
                AutoSize = true,
                Location = new Point(24, y)
            };
            parent.Controls.Add(header);
        }
    }
}
