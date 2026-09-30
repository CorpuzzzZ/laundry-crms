using System;
using System.Drawing;
using System.Windows.Forms;
using CRM.UI;
using CRM.WinForms.Models;

namespace CRM.WinForms.Forms
{
    public class TermsViewDialog : Form
    {
        public TermsViewDialog(TermsModel terms)
        {
            Text = $"Terms & Conditions: {terms.Title}";
            Size = new Size(650, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Colors.Surface;
            Font = Typography.Body;

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 14, 24, 8)
            };

            var lblTitle = new Label
            {
                Text = terms.Title,
                Font = Typography.H2,
                ForeColor = Colors.TextPrimary,
                AutoSize = true,
                Location = new Point(24, 14)
            };
            var lblMeta = new Label
            {
                Text = $"Version: {terms.Version} | Effective: {terms.EffectiveDate:MMM dd, yyyy} | Mandatory: {(terms.IsMandatory ? "Yes" : "No")} | Status: {(terms.IsActive ? "Active" : "Inactive")}",
                Font = Typography.Small,
                ForeColor = Colors.TextSecondary,
                AutoSize = true,
                Location = new Point(24, 40)
            };
            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(lblMeta);

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(24, 10, 24, 10)
            };
            var btnClose = new Button
            {
                Text = "Close",
                Width = 90,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Font = Typography.BodyBold,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(530, 12),
                DialogResult = DialogResult.OK
            };
            btnClose.FlatAppearance.BorderColor = Colors.Border;
            pnlBottom.Controls.Add(btnClose);

            var txtBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Colors.Background,
                ForeColor = Colors.TextPrimary,
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.None,
                Text = terms.Content,
                Padding = new Padding(16)
            };

            var host = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 12, 24, 12)
            };
            host.Controls.Add(txtBox);

            Controls.Add(host);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);
        }
    }
}
