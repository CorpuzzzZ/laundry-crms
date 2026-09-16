using System;
using System.Collections.Generic;

namespace CRM.Domain.Entities.TenantDb
{
    public class InventoryItem
    {
        public int InventoryItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; } = 0;
        public decimal? ReorderLevel { get; set; }
        public decimal? ReorderQuantity { get; set; }
        public decimal? CostPerUnit { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierContact { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    }
}
