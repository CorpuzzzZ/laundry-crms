using System;

namespace CRM.Domain.Entities.TenantDb
{
    public class InventoryTransaction
    {
        public int InventoryTransactionId { get; set; }
        public int InventoryItemId { get; set; }
        public string TransactionType { get; set; } = "Receive";
        public decimal QuantityChange { get; set; }
        public decimal QuantityBalance { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }

        public virtual InventoryItem? InventoryItem { get; set; }
    }
}
