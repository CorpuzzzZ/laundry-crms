namespace CRM.Domain.Common
{
    public static class Permissions
    {
        // Order Management Module
        public const string OrderRead = "OrderManagement:Read";
        public const string OrderCreate = "OrderManagement:Create";
        public const string OrderUpdate = "OrderManagement:Update";
        public const string OrderDelete = "OrderManagement:Delete";
        public const string OrderChangeStatus = "OrderManagement:ChangeStatus";
        public const string OrderProcessPayment = "OrderManagement:ProcessPayment";

        // Customer Management Module
        public const string CustomerRead = "CustomerManagement:Read";
        public const string CustomerCreate = "CustomerManagement:Create";
        public const string CustomerUpdate = "CustomerManagement:Update";
        public const string CustomerDelete = "CustomerManagement:Delete";

        // Service Management Module
        public const string ServiceRead = "ServiceManagement:Read";
        public const string ServiceCreate = "ServiceManagement:Create";
        public const string ServiceUpdate = "ServiceManagement:Update";
        public const string ServiceDelete = "ServiceManagement:Delete";

        // Loyalty Management Module
        public const string LoyaltyRead = "LoyaltyManagement:Read";
        public const string LoyaltyRedeem = "LoyaltyManagement:Redeem";
        public const string LoyaltyManage = "LoyaltyManagement:Manage";

        // Branch Management Module
        public const string BranchRead = "BranchManagement:Read";
        public const string BranchCreate = "BranchManagement:Create";
        public const string BranchUpdate = "BranchManagement:Update";
        public const string BranchDelete = "BranchManagement:Delete";

        // User Management Module
        public const string UserRead = "UserManagement:Read";
        public const string UserCreate = "UserManagement:Create";
        public const string UserUpdate = "UserManagement:Update";
        public const string UserDelete = "UserManagement:Delete";

        // Reports Module
        public const string ReportsRead = "Reports:Read";
        public const string ReportsCreate = "Reports:Create";
        public const string ReportsExport = "Reports:Export";

        // Dashboard Module
        public const string DashboardRead = "Dashboard:Read";

        // Subscription (Super Admin only)
        public const string SubscriptionManage = "SubscriptionManagement:Manage";

        // Terms (Super Admin only)
        public const string TermsManage = "TermsAndConditions:Manage";
        // ── Customer Interactions (inquiries, complaints, feedback) ──
        public const string CustomerInteractionRead    = "CustomerInteraction:Read";
        public const string CustomerInteractionCreate  = "CustomerInteraction:Create";
        public const string CustomerInteractionUpdate  = "CustomerInteraction:Update";
        public const string CustomerInteractionDelete  = "CustomerInteraction:Delete";

        // ── Customer Order History ──
        public const string CustomerOrderHistoryRead   = "CustomerOrderHistory:Read";
    }
}