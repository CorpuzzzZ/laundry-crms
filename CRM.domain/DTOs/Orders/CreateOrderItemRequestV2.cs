using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Domain.DTOs.Orders
{
    /// <summary>
    /// New order item request — carries category + weight so backend can apply detergent rules.
    /// </summary>
    public class CreateOrderItemRequestV2
    {
        [Required]
        public string Category { get; set; } = string.Empty;      // "Clothes" or "Beddings"

        [Required]
        public int ServiceId { get; set; }

        [Required]
        [Range(0.1, 100)]
        public decimal Weight { get; set; }

        public List<ChemicalAddOnDto> ChemicalAddOns { get; set; } = new();
        public List<MachineAddOnDto> MachineAddOns { get; set; } = new();

        public string? SpecialInstructions { get; set; }
    }

    public class ChemicalAddOnDto
    {
        public int AddOnId { get; set; }
        public int Quantity { get; set; }
    }

    public class MachineAddOnDto
    {
        public int AddOnId { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Full create-order request with new item structure.
    /// </summary>
    public class CreateOrderRequestV2
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int BranchId { get; set; }

        public string OrderType { get; set; } = "WalkIn";
        public string Priority { get; set; } = "Normal";
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public string? Notes { get; set; }

        [Required]
        [MinLength(1)]
        public List<CreateOrderItemRequestV2> Items { get; set; } = new();
    }

    /// <summary>
    /// Response from the API — includes computed values from rules.
    /// </summary>
    public class OrderItemResponseV2
    {
        public string Category { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal DetergentGrams { get; set; }
        public decimal LoadCount { get; set; }
        public decimal ServiceLineTotal { get; set; }
        public decimal ChemicalTotal { get; set; }
        public decimal MachineTotal { get; set; }
        public decimal LineTotal { get; set; }
    }
}