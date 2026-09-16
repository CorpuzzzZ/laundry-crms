using System.Collections.Generic;

namespace CRM.Domain.Common
{
    public static class RolePermissions
    {
        public static Dictionary<string, string[]> GetDefaultPermissions()
        {
            return new Dictionary<string, string[]>
            {
                // ============================================================
                // SUPER ADMIN
                //   Full system control EXCEPT:
                //     - Orders (by design)
                //     - Customer mutations (only read)
                // ============================================================
                ["SuperAdmin"] = new[]
                {
                    Permissions.CustomerInteractionRead,
                    Permissions.CustomerOrderHistoryRead,
                    Permissions.SubscriptionManage,
                    Permissions.TermsManage,

                    Permissions.ReportsRead,
                    Permissions.ReportsCreate,
                    Permissions.ReportsExport,

                    Permissions.DashboardRead,

                    Permissions.BranchRead,
                    Permissions.BranchCreate,
                    Permissions.BranchUpdate,
                    Permissions.BranchDelete,

                    Permissions.ServiceRead,
                    Permissions.ServiceCreate,
                    Permissions.ServiceUpdate,
                    Permissions.ServiceDelete,

                    Permissions.UserRead,
                    Permissions.UserCreate,
                    Permissions.UserUpdate,
                    Permissions.UserDelete,

                    // Customer: READ ONLY
                    Permissions.CustomerRead,
                    // NO CustomerCreate
                    // NO CustomerUpdate
                    // NO CustomerDelete

                    Permissions.LoyaltyRead,
                    Permissions.LoyaltyRedeem,
                    Permissions.LoyaltyManage,

                    // NO Order permissions
                },

                // ============================================================
                // ADMIN
                //   Full business ops EXCEPT:
                //     - Subscription/Terms
                //     - Customer mutations (only read)
                // ============================================================
                ["Admin"] = new[]
                {
                    // ── Interactions (full CRUD) ─────────────────
                    Permissions.CustomerInteractionRead,
                    Permissions.CustomerInteractionUpdate,
                    Permissions.CustomerInteractionDelete,
                    Permissions.CustomerOrderHistoryRead,

                    // ── Reports ──────────────────────────────────
                    Permissions.ReportsRead,
                    Permissions.ReportsCreate,
                    Permissions.ReportsExport,
                    Permissions.DashboardRead,

                    // ── Branches (full CRUD) ─────────────────────
                    Permissions.BranchRead,
                    Permissions.BranchCreate,
                    Permissions.BranchUpdate,
                    Permissions.BranchDelete,

                    // ── Services (full CRUD) ─────────────────────
                    Permissions.ServiceRead,
                    Permissions.ServiceCreate,
                    Permissions.ServiceUpdate,
                    Permissions.ServiceDelete,

                    // ── Users (full CRUD within company) ─────────
                    Permissions.UserRead,
                    Permissions.UserCreate,
                    Permissions.UserUpdate,
                    Permissions.UserDelete,

                    // ── Customers (FULL CRUD) ────────────────────
                    Permissions.CustomerRead,
                    Permissions.CustomerCreate,
                    Permissions.CustomerUpdate,
                    Permissions.CustomerDelete,

                    // ── Loyalty ──────────────────────────────────
                    Permissions.LoyaltyRead,
                    Permissions.LoyaltyRedeem,
                    Permissions.LoyaltyManage,

                    // ── Orders (READ ONLY) ───────────────────────
                    Permissions.OrderRead,
                    // NO OrderCreate
                    // NO OrderUpdate
                    // NO OrderDelete
                    // NO OrderChangeStatus
                    // NO OrderProcessPayment
                },

                // ============================================================
                // MANAGER
                //   Branch operations + full order management
                //   Customer: READ + DEACTIVATE only (no add, no edit)
                // ============================================================
                ["Manager"] = new[]
                {
                    Permissions.CustomerInteractionRead,
                    Permissions.CustomerInteractionCreate,
                    Permissions.CustomerInteractionUpdate,
                    Permissions.CustomerOrderHistoryRead,
                    Permissions.ReportsRead,
                    Permissions.DashboardRead,

                    Permissions.ServiceRead,
                    Permissions.ServiceCreate,
                    Permissions.ServiceUpdate,

                    // Customer: Read + Activate/Deactivate only
                    Permissions.CustomerRead,
                    // NO CustomerCreate
                    // NO CustomerUpdate
                    Permissions.CustomerDelete, // â† "Deactivate" permission

                    Permissions.LoyaltyRead,
                    Permissions.LoyaltyRedeem,

                    Permissions.OrderRead,
                    Permissions.OrderCreate,
                    Permissions.OrderUpdate,
                    Permissions.OrderChangeStatus,
                    Permissions.OrderProcessPayment,
                },

                // ============================================================
                // CREW
                //   Basic ops + ONLY role that can ADD/EDIT customers
                // ============================================================
                ["Crew"] = new[]
                {
                    Permissions.CustomerInteractionRead,
                    Permissions.CustomerInteractionCreate,
                    Permissions.CustomerOrderHistoryRead,
                    Permissions.DashboardRead,

                    // Customer: Read + Create + Update (no delete)
                    Permissions.CustomerRead,
                    Permissions.CustomerCreate,
                    Permissions.CustomerUpdate,
                    // NO CustomerDelete

                    Permissions.LoyaltyRead,
                    Permissions.LoyaltyRedeem,

                    Permissions.ServiceRead,

                    Permissions.OrderRead,
                    Permissions.OrderCreate,
                    Permissions.OrderUpdate,
                    Permissions.OrderChangeStatus,
                    Permissions.OrderProcessPayment,
                }
            };
        }
    }
}
