using System.Collections.Generic;

namespace CRM.Domain.Entities.TenantDb
{
    public class OrderStatus
    {
        public int StatusId { get; set; }
        public string StatusCode { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsFinal { get; set; } = false;
        public int SortOrder { get; set; } = 0;
        public string? ColorCode { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    }
}
