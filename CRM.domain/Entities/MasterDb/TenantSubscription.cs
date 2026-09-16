using System;

namespace CRM.Domain.Entities.MasterDb
{
    public class TenantSubscription
    {
        public int TenantSubscriptionId { get; set; }
        public int CompanyId { get; set; }
        public int PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string PaymentStatus { get; set; } = "Pending";
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public decimal? AmountPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        public bool AutoRenew { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Company? Company { get; set; }
        public virtual SubscriptionPlan? Plan { get; set; }
    }
}
