using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CRM.UI;
using CRM.WinForms.Models;

namespace CRM.UI.Controls
{
    /// <summary>
    /// Shows a before/after diff of customer changes for confirmation.
    /// </summary>
    public class CustomerEditConfirmationDialog : Form
    {
        private readonly List<(string Field, string Before, string After)> _changes;

        // Layout constants
        private const int DialogWidth = 780;
        private const int FieldColWidth = 130;
        private const int ValueColWidth = 260;
        private const int RowHeight = 56;
        private const int RowPadding = 16;

        public CustomerEditConfirmationDialog(
            CustomerModel? original,
            CustomerModel updated)
        {
            _changes = ComputeChanges(original, updated);

            BuildUI();
        }

        private static List<(string, string, string)> ComputeChanges(
            CustomerModel? original, CustomerModel updated)
        {
            var list = new List<(string, string, string)>();

            void Compare(string label, string? before, string? after)
            {
                var b = string.IsNullOrWhiteSpace(before) ? "(empty)" : before.Trim();
                var a = string.IsNullOrWhiteSpace(after)  ? "(empty)" : after.Trim();

                if (!string.Equals(b, a, StringComparison.Ordinal))
                    list.Add((label, b, a));
            }

            Compare("First Name",  original?.FirstName,    updated.FirstName);
            Compare("Last Name",   original?.LastName,     updated.LastName);
            Compare("Email",       original?.Email,        updated.Email);
            Compare("Phone",       original?.PhonePrimary, updated.PhonePrimary);
            Compare("Street",      original?.Street,       updated.Street);
            Compare("Village",     original?.Village,      updated.Village);
            Compare("City",        original?.City,         updated.City);
            Compare("State",       original?.State,        updated.State);
            Compare("Postal Code", original?.PostalCode,   updated.PostalCode);
            Compare("Country",     original?.Country,      updated.Country);
            Compare("Notes",       original?.Notes,        updated.Notes);

            return list;
        }

        private void BuildUI()
        {
            Text = "Confirm Changes";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Colors.Background;
            Font = Typography.Body;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            int headerHeight = 100;
            int footerHeight = 76;

            // Dialog size — grows with content up to a max, then scrolls
            int contentHeight = _changes.Count * RowHeight + 40;
            int bodyHeight = Math.Min(contentHeight, 420);
            int totalHeight = headerHeight + bodyHeight + footerHeight;

            ClientSize = new Size(DialogWidth, totalHeight);

            // ── HEADER ────────────────────────────────────────
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = headerHeight,
                BackColor = Colors.Surface,
                Padding = new Padding(32, 20, 32, 20)
            };
            header.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
            };

            var title = new Label
            {
                Text = "Confirm Changes",
                Font = Typography.H1,
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(32, 22)
            };
            header.Controls.Add(title);

            var subtitle = new Label
            {
                Text = _changes.Count == 0
                    ? "No changes detected."
                    : $"Review {_changes.Count} change{(_changes.Count == 1 ? "" : "s")} before saving.",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(32, 64)
            };
            header.Controls.Add(subtitle);

            // ── BODY ──────────────────────────────────────────
            var body = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Colors.Background,
                Padding = new Padding(32, 0, 32, 0)
            };

            if (_changes.Count == 0)
            {
                var noChanges = new Label
                {
                    Text = "No changes were made.",
                    Font = Typography.H2,
                    ForeColor = Colors.TextMuted,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                body.Controls.Add(noChanges);
            }
            else
            {
                // Container: fixed header + scrollable list, both use same column widths
                var container = new Panel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BackColor = Colors.Background
                };

                // ── Column header row ─────────────────────────
                var headerRow = BuildHeaderRow();
                headerRow.Dock = DockStyle.Top;

                // ── Data rows (FlowLayoutPanel) ───────────────
                var list = new FlowLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    BackColor = Colors.Background,
                    Padding = new Padding(0),
                    Margin = new Padding(0)
                };

                foreach (var (field, before, after) in _changes)
                {
                    list.Controls.Add(BuildDiffRow(field, before, after));
                }

                // Order: add list first (bottom), then header (top)
                container.Controls.Add(list);
                container.Controls.Add(headerRow);

                body.Controls.Add(container);
            }

            // ── FOOTER ────────────────────────────────────────
            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = footerHeight,
                BackColor = Colors.Surface,
                Padding = new Padding(32, 16, 32, 16)
            };
            footer.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                Width = 130,
                Height = 44,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnCancel.FlatAppearance.BorderColor = Colors.Border;
            btnCancel.Location = new Point(footer.Width - 310, 16);
            btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            var btnConfirm = new Button
            {
                Text = _changes.Count == 0 ? "Close" : "Confirm & Save",
                Width = 160,
                Height = 44,
                FlatStyle = FlatStyle.Flat,
                BackColor = _changes.Count == 0 ? Colors.TextMuted : Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = _changes.Count > 0 ? Cursors.Hand : Cursors.Default,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Enabled = _changes.Count > 0
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.Location = new Point(footer.Width - 170, 16);
            btnConfirm.Click += (s, e) =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };

            footer.Controls.Add(btnCancel);
            footer.Controls.Add(btnConfirm);

            // ── ASSEMBLE ──────────────────────────────────────
            Controls.Add(body);
            Controls.Add(footer);
            Controls.Add(header);
        }

        // ============================================================
        // HEADER ROW (column titles)
        // ============================================================
        private Panel BuildHeaderRow()
        {
            int contentWidth = DialogWidth - 64 - SystemInformation.VerticalScrollBarWidth;

            var header = new Panel
            {
                Width = contentWidth,
                Height = 36,
                BackColor = Colors.Background
            };
            header.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
            };

            int x = 0;

            var lblField = new Label
            {
                Text = "FIELD",
                Font = Typography.TinyUpper,
                ForeColor = Colors.TextMuted,
                AutoSize = false,
                Width = FieldColWidth,
                Height = 24,
                Location = new Point(x, 8),
                TextAlign = ContentAlignment.MiddleLeft
            };
            header.Controls.Add(lblField);
            x += FieldColWidth;

            var lblBefore = new Label
            {
                Text = "BEFORE",
                Font = Typography.TinyUpper,
                ForeColor = Colors.TextMuted,
                AutoSize = false,
                Width = ValueColWidth,
                Height = 24,
                Location = new Point(x, 8),
                TextAlign = ContentAlignment.MiddleLeft
            };
            header.Controls.Add(lblBefore);
            x += ValueColWidth;

            var lblAfter = new Label
            {
                Text = "AFTER",
                Font = Typography.TinyUpper,
                ForeColor = Colors.Primary,
                AutoSize = false,
                Width = ValueColWidth,
                Height = 24,
                Location = new Point(x, 8),
                TextAlign = ContentAlignment.MiddleLeft
            };
            header.Controls.Add(lblAfter);

            return header;
        }

        // ============================================================
        // DATA ROW (one field's before/after)
        // ============================================================
        private Panel BuildDiffRow(string field, string before, string after)
        {
            int contentWidth = DialogWidth - 64 - SystemInformation.VerticalScrollBarWidth;

            var row = new Panel
            {
                Width = contentWidth,
                Height = RowHeight,
                BackColor = Colors.Surface,
                Margin = new Padding(0, 0, 0, 2)
            };
            row.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.BorderLight);
                e.Graphics.DrawLine(pen, 0, row.Height - 1, row.Width, row.Height - 1);
            };

            int x = 0;
            int innerY = 10;
            int textHeight = RowHeight - 20;

            // Field label
            var lblField = new Label
            {
                Text = field,
                Font = Typography.SmallBold,
                ForeColor = Colors.TextSecondary,
                AutoSize = false,
                Width = FieldColWidth,
                Height = textHeight,
                Location = new Point(x, innerY),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            row.Controls.Add(lblField);
            x += FieldColWidth;

            // Before value (muted, may be empty)
            var lblBefore = new Label
            {
                Text = before,
                Font = Typography.Body,
                ForeColor = Colors.TextMuted,
                AutoSize = false,
                Width = ValueColWidth,
                Height = textHeight,
                Location = new Point(x, innerY),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            row.Controls.Add(lblBefore);
            x += ValueColWidth;

            // After value (highlighted)
            var lblAfter = new Label
            {
                Text = after,
                Font = Typography.BodyBold,
                ForeColor = Colors.Primary,
                AutoSize = false,
                Width = ValueColWidth,
                Height = textHeight,
                Location = new Point(x, innerY),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            row.Controls.Add(lblAfter);

            return row;
        }
    }
}