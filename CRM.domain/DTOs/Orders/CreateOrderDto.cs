using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Domain.DTOs.Orders
{
    public class CreateOrderDto
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int BranchId { get; set; }

        public string OrderType { get; set; } = "WalkIn";

        public string Priority { get; set; } = "Normal";

        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal TaxAmount { get; set; } = 0;

        public decimal? TotalWeight { get; set; }

        public string? SpecialInstructions { get; set; }

        public DateTime? PickupDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public string? Notes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one order item is required")]
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        [Required]
        public int ServiceId { get; set; }

        public int? GarmentTypeId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; } = 0;

        public string? SpecialInstructions { get; set; }

        public List<string> AddOns { get; set; } = new();
        public decimal? WeightKg { get; set; }
        public string? CategoryName { get; set; }
    }
}
