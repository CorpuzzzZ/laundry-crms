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

        public CRM.WinForms.Models.AvailedSubscriptionModel? AvailedSubscription { get; set; }

        // Assigned Branch for Manager and Staff Operations
        public int? AssignedBranchId { get; set; }
        public string? AssignedBranchName { get; set; }
        public System.Collections.Generic.List<int> BranchIds { get; set; } = new();

        // Module Access Permissions gated by Role and Subscription Plan
        public bool CanAccessBranches => !IsSuperAdmin && IsAdmin && (AvailedSubscription?.CanAccessBranches ?? true);
        public bool CanAccessServices => !IsSuperAdmin && (IsAdmin || IsManager) && (AvailedSubscription?.CanAccessServices ?? true);
        public bool CanAccessUsers => !IsSuperAdmin && (IsAdmin || IsManager) && (AvailedSubscription?.CanAccessUsers ?? true);
        public bool CanAccessCustomers => !IsSuperAdmin && (AvailedSubscription?.CanAccessCustomers ?? true);
        public bool CanAccessLoyalty => !IsSuperAdmin && (AvailedSubscription?.CanAccessLoyalty ?? true);
        public bool CanAccessOrders => !IsSuperAdmin && (IsAdmin || IsManager || IsCrew) && (AvailedSubscription?.CanAccessOrders ?? true);

        // Reports: SuperAdmin has access; Crew does not; Admin/Manager access is gated by Plan (Plan 2 has NO Reports)
        public bool CanAccessReports
        {
            get
            {
                if (IsSuperAdmin) return true;
                if (IsCrew) return false;
                if (AvailedSubscription != null) return AvailedSubscription.CanAccessReports;
                return true;
            }
        }

        // Orders: Admin is READ-ONLY. Manager + Crew can create/edit/delete/change-status.
        public bool CanModifyOrders => !IsSuperAdmin && (IsManager || IsCrew);

        // Subscription: SuperAdmin manages all; Admin can view their availed subscription plan.
        public bool CanAccessSubscription => IsSuperAdmin || IsAdmin;

        // Terms: SuperAdmin, Admin, and Crew can access terms. In Manager role there is NO Terms and condition.
        public bool CanAccessTerms => !IsManager;

        // Terms modification: Admin can create/edit/delete their company's terms; Crew can only view.
        public bool CanModifyTerms => IsSuperAdmin || IsAdmin;
    }

    public static class SessionManager
    {
        public static CurrentUser? CurrentUser { get; private set; }
        public static void StartSession(CurrentUser user) => CurrentUser = user;
        public static void EndSession() => CurrentUser = null;
        public static bool IsLoggedIn => CurrentUser != null;
    }
}