using System;
using System.Collections.Generic;

namespace CRM.Domain.Entities.TenantDb
{
    public class Order
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int BranchId { get; set; }
        public string OrderType { get; set; } = "WalkIn";
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public int StatusId { get; set; }
        public string Priority { get; set; } = "Normal";
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; }
        public decimal? TotalWeight { get; set; }
        public int TotalItems { get; set; } = 0;
        public string? SpecialInstructions { get; set; }
        public DateTime? PickupDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? ActualPickupDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public string? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Customer? Customer { get; set; }
        public virtual OrderStatus? Status { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
        public virtual ICollection<OrderPayment> Payments { get; set; } = new List<OrderPayment>();
        public virtual ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();
    }
}
