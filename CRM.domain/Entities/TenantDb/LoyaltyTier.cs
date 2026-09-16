using System;
using System.Collections.Generic;

namespace CRM.Domain.Entities.TenantDb
{
    public class LoyaltyTier
    {
        public int LoyaltyTierId { get; set; }
        public string TierName { get; set; } = string.Empty;
        public int MinPoints { get; set; }
        public int? MaxPoints { get; set; }
        public decimal DiscountPercentage { get; set; } = 0;
        public decimal PointsMultiplier { get; set; } = 1.0m;
        public string? BenefitsJSON { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<CustomerLoyalty> CustomerLoyalties { get; set; } = new List<CustomerLoyalty>();
    }
}
