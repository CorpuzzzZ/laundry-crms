using System;

namespace CRM.Domain.Entities.TenantDb
{
    public class LoyaltySetting
    {
        public int LoyaltySettingId { get; set; }
        public decimal PointsPerOrder { get; set; } = 10.00m;
        public decimal PointsPerDollar { get; set; } = 1.00m;
        public decimal RedeemPointsRequired { get; set; } = 100.00m;
        public decimal RedeemDiscountAmount { get; set; } = 10.00m;
        public int? PointsExpiryDays { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }
    }
}
