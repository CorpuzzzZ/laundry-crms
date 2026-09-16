using System;

namespace CRM.Domain.Entities.TenantDb
{
    public class OrderStatusHistory
    {
        public int OrderStatusHistoryId { get; set; }
        public int OrderId { get; set; }
        public int StatusId { get; set; }
        public string? ChangedByUserId { get; set; }
        public string? Notes { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public virtual Order? Order { get; set; }
        public virtual OrderStatus? Status { get; set; }
    }
}
