using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CRM.UI.Controls
{
    /// <summary>
    /// A small uppercase section label inside the sidebar.
    /// </summary>
    public class SidebarSection : Panel
    {
        private string _text = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new string Text
        {
            get => _text;
            set { _text = value; Invalidate(); }
        }

        public SidebarSection()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Height = 30;
            BackColor = Colors.SidebarBg;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            using (var brush = new SolidBrush(Colors.SidebarBg))
                g.FillRectangle(brush, ClientRectangle);

            TextRenderer.DrawText(g, Text.ToUpperInvariant(),
                Typography.TinyUpper,
                new Rectangle(Spacing.Xl, 0, Width - Spacing.Xl, Height),
                Colors.SidebarTextMuted,
                TextFormatFlags.Left | TextFormatFlags.Bottom);
        }
    }
}