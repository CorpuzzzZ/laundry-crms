using System.Drawing;

namespace CRM.UI
{
    public static class Typography
    {
        public const string Family = "Segoe UI";
        public const string MonoFamily = "Cascadia Mono";

        public static readonly Font Display   = new Font(Family, 26f, FontStyle.Bold);
        public static readonly Font H1        = new Font(Family, 20f, FontStyle.Bold);
        public static readonly Font H2        = new Font(Family, 15f, FontStyle.Bold);
        public static readonly Font H3        = new Font(Family, 12f, FontStyle.Bold);
        public static readonly Font Body      = new Font(Family, 10f, FontStyle.Regular);
        public static readonly Font BodyBold  = new Font(Family, 10f, FontStyle.Bold);
        public static readonly Font Small     = new Font(Family, 9f,  FontStyle.Regular);
        public static readonly Font SmallBold = new Font(Family, 9f,  FontStyle.Bold);
        public static readonly Font Tiny      = new Font(Family, 8f,  FontStyle.Regular);
        public static readonly Font TinyUpper = new Font(Family, 8f,  FontStyle.Bold);
        public static readonly Font Mono      = new Font(MonoFamily, 10f);
    }
}