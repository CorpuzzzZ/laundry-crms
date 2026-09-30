using System.Drawing;
using System.Windows.Forms;

namespace CRM.WinForms.UI
{
    public static class Theme
    {
        public static readonly Color PrimaryColor = Color.FromArgb(15, 23, 42);       // Slate 900
        public static readonly Color AccentColor = Color.FromArgb(37, 99, 235);        // Royal Blue
        public static readonly Color SuccessColor = Color.FromArgb(16, 185, 129);      // Emerald Green
        public static readonly Color WarningColor = Color.FromArgb(245, 158, 11);      // Amber
        public static readonly Color DangerColor = Color.FromArgb(239, 68, 68);        // Red
        public static readonly Color BackgroundColor = Color.FromArgb(248, 250, 252);  // #F8FAFC
        public static readonly Color CardColor = Color.White;
        public static readonly Color TextDarkColor = Color.FromArgb(15, 23, 42);
        public static readonly Color TextLightColor = Color.FromArgb(100, 116, 139);
        public static readonly Color BorderColor = Color.FromArgb(226, 232, 240);
        public static readonly Color SidebarColor = Color.FromArgb(15, 23, 42);

        public static readonly Font TitleFont = new Font("Segoe UI", 20F, FontStyle.Bold);
        public static readonly Font HeadingFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        public static readonly Font BodyFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        public static readonly Font SmallFont = new Font("Segoe UI", 9F, FontStyle.Regular);
        public static readonly Font MetricFont = new Font("Segoe UI", 24F, FontStyle.Bold);

        public const int ButtonHeight = 40;
        public const int SidebarWidth = 230;
        public const int TopBarHeight = 60;

        public static void StylePrimaryButton(Button btn)
        {
            btn.BackColor = AccentColor;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(41, 128, 185);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(31, 97, 141);
            btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btn.Height = ButtonHeight;
            btn.Cursor = Cursors.Hand;
        }

        public static void StyleSuccessButton(Button btn)
        {
            btn.BackColor = SuccessColor;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 174, 96);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 132, 73);
            btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btn.Height = ButtonHeight;
            btn.Cursor = Cursors.Hand;
        }

        public static void StyleDangerButton(Button btn)
        {
            btn.BackColor = DangerColor;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(192, 57, 43);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 40, 27);
            btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
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
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 247, 250);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(235, 238, 242);
            btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
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
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.FromArgb(240, 243, 246);
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            dgv.ColumnHeadersHeight = 42;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.RowTemplate.Height = 38;

            // Sleek modern header
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(246, 248, 250);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);

            // Row styling with calm, readable selection
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 242, 253);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
            dgv.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);

            // Subtle zebra striping
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 242, 253);
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
            dgv.AlternatingRowsDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);

            // Reflection-based double-buffering to prevent scroll flicker
            try
            {
                typeof(DataGridView).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.SetProperty,
                    null, dgv, new object[] { true });
            }
            catch { }
        }
    }
}