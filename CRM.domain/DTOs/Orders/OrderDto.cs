using System;
using System.Collections.Generic;

namespace CRM.Domain.DTOs.Orders
{
    // READ DTO - What API returns
    public class OrderDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string OrderType { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string StatusCode { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? TotalWeight { get; set; }
        public int TotalItems { get; set; }
        public string? SpecialInstructions { get; set; }
        public DateTime? PickupDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? ActualPickupDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public List<OrderPaymentDto> Payments { get; set; } = new();
    }
}