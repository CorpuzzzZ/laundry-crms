using System.ComponentModel.DataAnnotations;

namespace CRM.Domain.DTOs.Loyalty
{
    public class CreateLoyaltyTierDto
    {
        [Required, MaxLength(50)]
        public string TierName { get; set; } = string.Empty;

        [Required, Range(0, int.MaxValue)]
        public int MinPoints { get; set; }

        [Range(0, int.MaxValue)]
        public int? MaxPoints { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; }

        [Range(0.1, 100)]
        public decimal PointsMultiplier { get; set; } = 1.0m;

        [MaxLength(2000)]
        public string? BenefitsJSON { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;
    }
}
