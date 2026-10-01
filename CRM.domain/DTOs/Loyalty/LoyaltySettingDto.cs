namespace CRM.Domain.DTOs.Loyalty
{
    public class LoyaltySettingDto
    {
        public int LoyaltySettingId { get; set; }
        public decimal PointsPerOrder { get; set; }
        public decimal PointsPerDollar { get; set; }
        public decimal RedeemPointsRequired { get; set; }
        public decimal RedeemDiscountAmount { get; set; }
        public int? PointsExpiryDays { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime UpdatedAt { get; set; }
    }
}
