using System;

namespace CRM.Domain.Entities.TenantDb
{
    public class LoyaltyTransaction
    {
        public int LoyaltyTransactionId { get; set; }
        public int CustomerId { get; set; }
        public int? OrderId { get; set; }
        public string TransactionType { get; set; } = "Earn";
        public int PointsChange { get; set; }
        public int PointsBalance { get; set; }
        public string? Description { get; set; }
        public string? Reference { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }

        public virtual Customer? Customer { get; set; }
        public virtual Order? Order { get; set; }
    }
}
