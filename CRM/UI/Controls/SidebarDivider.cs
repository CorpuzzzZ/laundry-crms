using System.Drawing;
using System.Windows.Forms;

namespace CRM.UI.Controls
{
    public class SidebarDivider : Panel
    {
        public SidebarDivider()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
            Height = 1;
            BackColor = Colors.SidebarBg;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using var brush = new SolidBrush(Colors.SidebarBg);
            e.Graphics.FillRectangle(brush, ClientRectangle);

            using var pen = new Pen(Colors.SidebarDivider);
            e.Graphics.DrawLine(pen, Spacing.Xl, 0, Width - Spacing.Xl, 0);
        }
    }
}