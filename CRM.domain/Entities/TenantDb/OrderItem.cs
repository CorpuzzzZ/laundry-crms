using System;
using System.ComponentModel.DataAnnotations;

namespace CRM.Domain.Entities.TenantDb
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ServiceId { get; set; }
        public int? GarmentTypeId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public string? SpecialInstructions { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedAt { get; set; }

        // Navigation Properties
        public virtual Order? Order { get; set; }
        public virtual Service? Service { get; set; }
        public virtual GarmentType? GarmentType { get; set; }
    }
}