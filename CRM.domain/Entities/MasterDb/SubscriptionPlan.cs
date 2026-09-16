using System;
using System.Collections.Generic;

namespace CRM.Domain.Entities.MasterDb
{
    public class SubscriptionPlan
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string PlanCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal PricePerMonth { get; set; }
        public decimal? PricePerYear { get; set; }
        public int MaxUsers { get; set; }
        public int MaxBranches { get; set; }
        public int? MaxOrdersPerMonth { get; set; }
        public int? MaxStorageGB { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<TenantSubscription> TenantSubscriptions { get; set; } = new List<TenantSubscription>();
        public virtual ICollection<SubscriptionFeature> SubscriptionFeatures { get; set; } = new List<SubscriptionFeature>();
    }
}
