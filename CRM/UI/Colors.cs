using System.Drawing;

namespace CRM.UI
{
    /// <summary>
    /// Light blue + white professional theme.
    /// Only reference these values — never inline Color.FromArgb() in forms.
    /// </summary>
    public static class Colors
    {
        // ── Brand: Modern Royal Blue ──────────────────────────
        public static readonly Color Primary        = Color.FromArgb(37, 99, 235);     // #2563EB
        public static readonly Color PrimaryHover   = Color.FromArgb(29, 78, 216);     // #1D4ED8
        public static readonly Color PrimaryActive  = Color.FromArgb(30, 64, 175);     // #1E40AF
        public static readonly Color PrimaryLight   = Color.FromArgb(239, 246, 255);   // #EFF6FF
        public static readonly Color PrimaryLighter = Color.FromArgb(248, 250, 252);   // #F8FAFC

        // ── Neutrals (Modern Tailwind Slate) ────────────────────
        public static readonly Color Background     = Color.FromArgb(248, 250, 252);   // #F8FAFC
        public static readonly Color Surface        = Color.White;
        public static readonly Color SurfaceAlt     = Color.FromArgb(241, 245, 249);   // #F1F5F9

        public static readonly Color Border         = Color.FromArgb(226, 232, 240);   // #E2E8F0
        public static readonly Color BorderLight    = Color.FromArgb(241, 245, 249);   // #F1F5F9

        public static readonly Color TextPrimary    = Color.FromArgb(15, 23, 42);      // #0F172A
        public static readonly Color TextBody       = Color.FromArgb(51, 65, 85);      // #334155
        public static readonly Color TextSecondary  = Color.FromArgb(100, 116, 139);   // #64748B
        public static readonly Color TextMuted      = Color.FromArgb(148, 163, 184);   // #94A3B8
        public static readonly Color TextOnPrimary  = Color.White;

        // ── Semantic ───────────────────────────────────────────
        public static readonly Color Success        = Color.FromArgb(16, 185, 129);    // #10B981
        public static readonly Color SuccessLight   = Color.FromArgb(236, 253, 245);   // #ECFDF5
        public static readonly Color Warning        = Color.FromArgb(245, 158, 11);    // #F59E0B
        public static readonly Color WarningLight   = Color.FromArgb(254, 243, 199);   // #FEF3C7
        public static readonly Color Danger         = Color.FromArgb(239, 68, 68);     // #EF4444
        public static readonly Color DangerLight    = Color.FromArgb(254, 242, 242);   // #FEF2F2

        // ── Sidebar (Executive Deep Slate 900) ──────────────────
        public static readonly Color SidebarBg          = Color.FromArgb(15, 23, 42);      // #0F172A
        public static readonly Color SidebarBgDark      = Color.FromArgb(10, 15, 29);      // #0A0F1D
        public static readonly Color SidebarHover       = Color.FromArgb(30, 41, 59);      // #1E293B
        public static readonly Color SidebarActive      = Color.FromArgb(37, 99, 235);     // #2563EB
        public static readonly Color SidebarText        = Color.FromArgb(148, 163, 184);   // #94A3B8
        public static readonly Color SidebarTextMuted   = Color.FromArgb(100, 116, 139);   // #64748B
        public static readonly Color SidebarTextActive  = Color.White;
        public static readonly Color SidebarAccent      = Color.FromArgb(96, 165, 250);    // #60A5FA
        public static readonly Color SidebarDivider     = Color.FromArgb(30, 41, 59);      // #1E293B
    }
}