using System.Drawing;

namespace CRM.UI
{
    /// <summary>
    /// Light blue + white professional theme.
    /// Only reference these values — never inline Color.FromArgb() in forms.
    /// </summary>
    public static class Colors
    {
        // ── Brand: Light Blue ──────────────────────────────────
        public static readonly Color Primary        = Color.FromArgb(0x34, 0x98, 0xDB);
        public static readonly Color PrimaryHover   = Color.FromArgb(0x2E, 0x86, 0xC1);
        public static readonly Color PrimaryActive  = Color.FromArgb(0x28, 0x74, 0xA8);
        public static readonly Color PrimaryLight   = Color.FromArgb(0xEB, 0xF5, 0xFB);
        public static readonly Color PrimaryLighter = Color.FromArgb(0xF4, 0xFA, 0xFE);

        // ── Neutrals ───────────────────────────────────────────
        public static readonly Color Background     = Color.FromArgb(0xF5, 0xF7, 0xFA);
        public static readonly Color Surface        = Color.White;
        public static readonly Color SurfaceAlt     = Color.FromArgb(0xFA, 0xFB, 0xFC);

        public static readonly Color Border         = Color.FromArgb(0xE1, 0xE4, 0xE8);
        public static readonly Color BorderLight    = Color.FromArgb(0xEE, 0xF0, 0xF3);

        public static readonly Color TextPrimary    = Color.FromArgb(0x1F, 0x2D, 0x3D);
        public static readonly Color TextBody       = Color.FromArgb(0x3E, 0x4C, 0x5C);
        public static readonly Color TextSecondary  = Color.FromArgb(0x6B, 0x7A, 0x8A);
        public static readonly Color TextMuted      = Color.FromArgb(0x9A, 0xA5, 0xB1);
        public static readonly Color TextOnPrimary  = Color.White;

        // ── Semantic ───────────────────────────────────────────
        public static readonly Color Success        = Color.FromArgb(0x27, 0xAE, 0x60);
        public static readonly Color SuccessLight   = Color.FromArgb(0xE8, 0xF8, 0xEF);
        public static readonly Color Warning        = Color.FromArgb(0xF3, 0x9C, 0x12);
        public static readonly Color WarningLight   = Color.FromArgb(0xFD, 0xF3, 0xE3);
        public static readonly Color Danger         = Color.FromArgb(0xE7, 0x4C, 0x3C);
        public static readonly Color DangerLight    = Color.FromArgb(0xFD, 0xEC, 0xEA);

        // ── Sidebar (deep blue — creates hierarchy vs content) ─
        public static readonly Color SidebarBg          = Color.FromArgb(0x21, 0x6D, 0xA0);
        public static readonly Color SidebarBgDark      = Color.FromArgb(0x1A, 0x55, 0x7D);
        public static readonly Color SidebarHover       = Color.FromArgb(0x2A, 0x7F, 0xB8);
        public static readonly Color SidebarActive      = Color.FromArgb(0x18, 0x54, 0x7D);
        public static readonly Color SidebarText        = Color.FromArgb(0xD6, 0xEA, 0xF8);
        public static readonly Color SidebarTextMuted   = Color.FromArgb(0x8F, 0xB8, 0xD4);
        public static readonly Color SidebarTextActive  = Color.White;
        public static readonly Color SidebarAccent      = Color.FromArgb(0x5D, 0xAD, 0xE2);
        public static readonly Color SidebarDivider     = Color.FromArgb(0x2F, 0x7C, 0xAF);
    }
}