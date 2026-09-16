using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Domain.DTOs.Orders
{
    public class UpdateOrderDto
    {
        [Required]
        public int CustomerId { get; set; }

        public string Priority { get; set; } = "Normal";

        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TaxAmount { get; set; }

        public decimal? TotalWeight { get; set; }

        public string? SpecialInstructions { get; set; }

        public DateTime? PickupDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public string? Notes { get; set; }

        public List<UpdateOrderItemDto>? Items { get; set; }
    }

    public class UpdateOrderItemDto
    {
        public int? OrderItemId { get; set; }

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
        public decimal DiscountAmount { get; set; }

        public string? SpecialInstructions { get; set; }
    }
}