using System;

namespace CRM.Domain.DTOs.Loyalty
{
    public class LoyaltyCustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public int? LoyaltyTierId { get; set; }
        public string? TierName { get; set; }

        public int CurrentPoints { get; set; }
        public int TotalPointsEarned { get; set; }
        public int TotalPointsRedeemed { get; set; }
        public decimal LifetimeSpend { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public bool IsActive { get; set; }
    }
}
