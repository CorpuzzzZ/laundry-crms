namespace CRM.Domain.DTOs.Loyalty
{
    public class RedeemableInfoDto
    {
        public int CustomerId { get; set; }
        public int CurrentPoints { get; set; }
        public int PointsRequired { get; set; }
        public decimal DiscountPerBatch { get; set; }

        public int RedeemableBatches { get; set; }
        public int MaxRedeemablePoints { get; set; }
        public decimal MaxDiscount { get; set; }

        public bool CanRedeem => RedeemableBatches > 0;
    }
}
