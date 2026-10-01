using System;

namespace CRM.Domain.DTOs.Loyalty
{
    public class LoyaltyTransactionDto
    {
        public int LoyaltyTransactionId { get; set; }
        public int CustomerId { get; set; }
        public int? OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public int PointsChange { get; set; }
        public int PointsBalance { get; set; }
        public string? Description { get; set; }
        public string? Reference { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
