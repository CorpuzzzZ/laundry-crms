using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CRM.UI.Controls
{
    /// <summary>
    /// Circular floating action button (FAB) pinned to bottom-right of parent.
    /// The square background is filled with the parent's color so the circular
    /// button appears to float without any visible box.
    /// </summary>
    public class FloatingActionButton : Control
    {
        private bool _hover;
        private bool _pressed;

        private const int Diameter = 56;
        private const int MarginPx = 24;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Color NormalColor { get; set; } = Colors.Primary;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Color HoverColor { get; set; } = Colors.PrimaryHover;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Color PressedColor { get; set; } = Colors.PrimaryActive;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Color GlyphColor { get; set; } = Color.White;

        public FloatingActionButton()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Opaque,
                true);

            Size = new Size(Diameter, Diameter);
            Cursor = Cursors.Hand;
            TabStop = false;

            ParentChanged += (s, e) =>
            {
                if (Parent != null)
                {
                    Parent.Resize -= Parent_Resize;
                    Parent.Resize += Parent_Resize;
                    PinToParentBottomRight();
                }
            };
        }

        private void Parent_Resize(object? sender, EventArgs e)
        {
            PinToParentBottomRight();
        }

        public void PinToParentBottomRight()
        {
            if (Parent == null) return;
            Left = Parent.ClientSize.Width - Width - MarginPx;
            Top  = Parent.ClientSize.Height - Height - MarginPx;
            BringToFront();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            _pressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _pressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _pressed = false;
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Deliberately empty - OnPaint fills with the parent's color
            // so the corners of the FAB's square area blend seamlessly.
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Fill the full square with the parent's actual background color
            // so no visible square patch appears behind the circle.
            Color bg = Color.White;
            g.Clear(bg);

            // Determine circle fill color
            var fill = _pressed ? PressedColor : (_hover ? HoverColor : NormalColor);

            // Circle shrinks slightly on hover/press for tactile feedback
            var size = _pressed ? Diameter - 4 : (_hover ? Diameter - 2 : Diameter);
            var offset = (Diameter - size) / 2;
            var circleRect = new Rectangle(offset, offset, size, size);

            // Soft drop shadow (two offset layers for depth)
            using (var shadow = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                g.FillEllipse(shadow, circleRect.X + 1, circleRect.Y + 3, size, size);
            using (var shadow = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                g.FillEllipse(shadow, circleRect.X + 2, circleRect.Y + 4, size, size);

            // Main circle
            using (var brush = new SolidBrush(fill))
                g.FillEllipse(brush, circleRect);

            // Plus glyph
            int cx = circleRect.X + size / 2;
            int cy = circleRect.Y + size / 2;
            int arm = 12;
            int thick = 3;

            using var pen = new Pen(GlyphColor, thick)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            // Horizontal bar
            g.DrawLine(pen, cx - arm / 2, cy, cx + arm / 2, cy);
            // Vertical bar
            g.DrawLine(pen, cx, cy - arm / 2, cx, cy + arm / 2);
        }
    }
}