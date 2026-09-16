using System;

namespace CRM.WinForms.Services
{
    public class CurrentUser
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();

        public string FullName => $"{FirstName} {LastName}".Trim();
        public string PrimaryRole => Roles.Length > 0 ? Roles[0] : "Unknown";

        public bool IsSuperAdmin => Array.Exists(Roles, r => r.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase));
        public bool IsAdmin => Array.Exists(Roles, r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase));
        public bool IsManager => Array.Exists(Roles, r => r.Equals("Manager", StringComparison.OrdinalIgnoreCase));
        public bool IsCrew => Array.Exists(Roles, r => r.Equals("Crew", StringComparison.OrdinalIgnoreCase));

        // Super Admin has NO access to Orders
        public bool CanAccessOrders => IsAdmin || IsManager || IsCrew;

        // Orders: Admin is READ-ONLY. Manager + Crew can create/edit/delete/change-status.
        public bool CanModifyOrders => IsManager || IsCrew;
    }

    public static class SessionManager
    {
        public static CurrentUser? CurrentUser { get; private set; }
        public static void StartSession(CurrentUser user) => CurrentUser = user;
        public static void EndSession() => CurrentUser = null;
        public static bool IsLoggedIn => CurrentUser != null;
    }
}