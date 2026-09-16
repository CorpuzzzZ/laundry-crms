using System.Drawing;
using System.Windows.Forms;

namespace CRM.WinForms.UI
{
    public static class Theme
    {
        public static readonly Color PrimaryColor = Color.FromArgb(44, 62, 80);
        public static readonly Color AccentColor = Color.FromArgb(52, 152, 219);
        public static readonly Color SuccessColor = Color.FromArgb(46, 204, 113);
        public static readonly Color WarningColor = Color.FromArgb(243, 156, 18);
        public static readonly Color DangerColor = Color.FromArgb(231, 76, 60);
        public static readonly Color BackgroundColor = Color.FromArgb(236, 240, 241);
        public static readonly Color CardColor = Color.White;
        public static readonly Color TextDarkColor = Color.FromArgb(44, 62, 80);
        public static readonly Color TextLightColor = Color.FromArgb(127, 140, 141);
        public static readonly Color BorderColor = Color.FromArgb(220, 220, 220);
        public static readonly Color SidebarColor = Color.FromArgb(52, 73, 94);

        public static readonly Font TitleFont = new Font("Segoe UI", 22F, FontStyle.Bold);
        public static readonly Font HeadingFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        public static readonly Font BodyFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        public static readonly Font SmallFont = new Font("Segoe UI", 9F, FontStyle.Regular);
        public static readonly Font MetricFont = new Font("Segoe UI", 28F, FontStyle.Bold);

        public const int ButtonHeight = 40;
        public const int SidebarWidth = 230;
        public const int TopBarHeight = 60;

        public static void StylePrimaryButton(Button btn)
        {
            btn.BackColor = AccentColor;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = BodyFont;
            btn.Height = ButtonHeight;
            btn.Cursor = Cursors.Hand;
        }

        public static void StyleSuccessButton(Button btn)
        {
            btn.BackColor = SuccessColor;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = BodyFont;
            btn.Height = ButtonHeight;
            btn.Cursor = Cursors.Hand;
        }

        public static void StyleDangerButton(Button btn)
        {
            btn.BackColor = DangerColor;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = BodyFont;
            btn.Height = ButtonHeight;
            btn.Cursor = Cursors.Hand;
        }

        public static void StyleSecondaryButton(Button btn)
        {
            btn.BackColor = Color.White;
            btn.ForeColor = TextDarkColor;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = BorderColor;
            btn.Font = BodyFont;
            btn.Height = ButtonHeight;
            btn.Cursor = Cursors.Hand;
        }

        public static void StyleTextBox(TextBox txt)
        {
            txt.Font = BodyFont;
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.BackColor = Color.White;
            txt.ForeColor = TextDarkColor;
        }

        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AllowUserToAddRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.Font = BodyFont;
            dgv.ColumnHeadersHeight = 40;
            dgv.RowTemplate.Height = 35;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = PrimaryColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = HeadingFont;
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        }
    }
}