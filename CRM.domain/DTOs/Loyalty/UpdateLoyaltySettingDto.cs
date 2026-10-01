using System.ComponentModel.DataAnnotations;

namespace CRM.Domain.DTOs.Loyalty
{
    public class UpdateLoyaltySettingDto
    {
        [Required, Range(0, 10000)]
        public decimal PointsPerOrder { get; set; }

        [Required, Range(0, 10000)]
        public decimal PointsPerDollar { get; set; }

        [Required, Range(1, 1000000)]
        public decimal RedeemPointsRequired { get; set; }

        [Required, Range(0, 1000000)]
        public decimal RedeemDiscountAmount { get; set; }

        [Range(1, 36500)]
        public int? PointsExpiryDays { get; set; }

        public bool IsActive { get; set; }
    }
}
