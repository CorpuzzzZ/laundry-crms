using System;

namespace CRM.Domain.DTOs.Loyalty
{
    public class LoyaltyTierDto
    {
        public int LoyaltyTierId { get; set; }
        public string TierName { get; set; } = string.Empty;
        public int MinPoints { get; set; }
        public int? MaxPoints { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal PointsMultiplier { get; set; }
        public string? BenefitsJSON { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
