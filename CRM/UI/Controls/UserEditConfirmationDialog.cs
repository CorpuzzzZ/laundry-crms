using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CRM.UI;
using CRM.WinForms.Models;

namespace CRM.UI.Controls
{
    /// <summary>
    /// Shows a before/after diff of user changes for confirmation.
    /// Includes Is Active toggle + branch assignment changes.
    /// </summary>
    public class UserEditConfirmationDialog : Form
    {
        private readonly List<(string Field, string Before, string After)> _changes;

        private const int DialogWidth = 780;
        private const int FieldColWidth = 130;
        private const int ValueColWidth = 260;
        private const int RowHeight = 56;
        private const int RowPadding = 16;

        public UserEditConfirmationDialog(UserModel? original, UserModel updated)
        {
            _changes = ComputeChanges(original, updated);
            BuildUI();
        }

        private static List<(string, string, string)> ComputeChanges(
            UserModel? original, UserModel updated)
        {
            var list = new List<(string, string, string)>();

            void Compare(string label, string? before, string? after)
            {
                var b = string.IsNullOrWhiteSpace(before) ? "(empty)" : before.Trim();
                var a = string.IsNullOrWhiteSpace(after)  ? "(empty)" : after.Trim();

                if (!string.Equals(b, a, StringComparison.Ordinal))
                    list.Add((label, b, a));
            }

            Compare("First Name",  original?.FirstName,  updated.FirstName);
            Compare("Last Name",   original?.LastName,   updated.LastName);
            Compare("Phone",       original?.PhoneNumber, updated.PhoneNumber);
            Compare("Role",        original?.Role,       updated.Role);

            // Bool → human-readable
            var beforeActive = original?.IsActive == true ? "Active" : "Inactive";
            var afterActive  = updated.IsActive == true ? "Active" : "Inactive";
            if (beforeActive != afterActive)
                list.Add(("Is Active", beforeActive, afterActive));

            // Branch assignments — compare as sorted lists
            var beforeBranches = (original?.BranchNames != null && original.BranchNames.Count > 0)
                ? string.Join(", ", original.BranchNames)
                : "(none)";
            var afterBranches = (updated.BranchNames != null && updated.BranchNames.Count > 0)
                ? string.Join(", ", updated.BranchNames)
                : "(none)";
            if (beforeBranches != afterBranches)
                list.Add(("Branches", beforeBranches, afterBranches));

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

            int contentHeight = _changes.Count * RowHeight + 40;
            int bodyHeight = Math.Min(contentHeight, 420);
            int totalHeight = headerHeight + bodyHeight + footerHeight;

            ClientSize = new Size(DialogWidth, totalHeight);

            // ── Header ─────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = headerHeight,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 20, 24, 20)
            };

            pnlHeader.Controls.Add(new Label
            {
                Text = "Confirm Changes",
                Font = Typography.H1,
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(24, 20)
            });

            pnlHeader.Controls.Add(new Label
            {
                Text = _changes.Count == 1
                    ? "Review 1 change before saving."
                    : $"Review {_changes.Count} changes before saving.",
                Font = Typography.Body,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(24, 56)
            });

            // ── Body (scrollable) ──────────────────────────
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Colors.Background,
                Padding = new Padding(24, 16, 24, 16)
            };

            int y = 16;

            // Column headers
            pnlBody.Controls.Add(new Label
            {
                Text = "FIELD",
                Font = Typography.TinyUpper,
                ForeColor = Colors.TextMuted,
                AutoSize = true,
                Location = new Point(24, y)
            });
            pnlBody.Controls.Add(new Label
            {
                Text = "BEFORE",
                Font = Typography.TinyUpper,
                ForeColor = Colors.TextMuted,
                AutoSize = true,
                Location = new Point(24 + FieldColWidth + 20, y)
            });
            pnlBody.Controls.Add(new Label
            {
                Text = "AFTER",
                Font = Typography.TinyUpper,
                ForeColor = Colors.TextMuted,
                AutoSize = true,
                Location = new Point(24 + FieldColWidth + ValueColWidth + 40, y)
            });
            y += 36;

            foreach (var (field, before, after) in _changes)
            {
                // Field name
                pnlBody.Controls.Add(new Label
                {
                    Text = field,
                    Font = Typography.BodyBold,
                    ForeColor = Colors.TextPrimary,
                    AutoSize = false,
                    Size = new Size(FieldColWidth, 24),
                    Location = new Point(24, y)
                });

                // Before
                pnlBody.Controls.Add(new Label
                {
                    Text = before,
                    Font = Typography.Body,
                    ForeColor = Colors.TextSecondary,
                    AutoSize = false,
                    Size = new Size(ValueColWidth, 24),
                    Location = new Point(24 + FieldColWidth + 20, y)
                });

                // After
                pnlBody.Controls.Add(new Label
                {
                    Text = after,
                    Font = Typography.BodyBold,
                    ForeColor = Colors.Primary,
                    AutoSize = false,
                    Size = new Size(ValueColWidth, 24),
                    Location = new Point(24 + FieldColWidth + ValueColWidth + 40, y)
                });

                y += RowHeight - RowPadding;
            }

            // ── Footer ─────────────────────────────────────
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = footerHeight,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 16, 24, 16)
            };
            pnlFooter.Paint += (s, e) =>
            {
                using var pen = new Pen(Colors.Border);
                e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(120, 42),
                Location = new Point(DialogWidth - 24 - 120 - 12 - 170, 16),
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderColor = Colors.Border;
            pnlFooter.Controls.Add(btnCancel);

            var btnConfirm = new Button
            {
                Text = "Confirm & Save",
                Size = new Size(170, 42),
                Location = new Point(DialogWidth - 24 - 170, 16),
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Primary,
                ForeColor = Color.White,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            pnlFooter.Controls.Add(btnConfirm);

            AcceptButton = btnConfirm;
            CancelButton = btnCancel;

            // Order matters — Fill needs to be added last for Dock
            Controls.Add(pnlBody);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
        }
    }
}