using System;

namespace CRM.Domain.Entities.TenantDb
{
    public class CustomerLoyalty
    {
        public int CustomerLoyaltyId { get; set; }
        public int CustomerId { get; set; }
        public int? LoyaltyTierId { get; set; }
        public int CurrentPoints { get; set; } = 0;
        public int TotalPointsEarned { get; set; } = 0;
        public int TotalPointsRedeemed { get; set; } = 0;
        public decimal LifetimeSpend { get; set; } = 0;
        public DateTime? LastActivityDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Customer? Customer { get; set; }
        public virtual LoyaltyTier? LoyaltyTier { get; set; }
    }
}
