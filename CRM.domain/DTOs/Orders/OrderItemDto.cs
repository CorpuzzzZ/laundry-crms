namespace CRM.Domain.DTOs.Orders
{
    public class OrderItemDto
    {
        public int OrderItemId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int? GarmentTypeId { get; set; }
        public string? GarmentTypeName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal LineTotal { get; set; }
        public string? SpecialInstructions { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public System.Collections.Generic.List<string> AddOns { get; set; } = new();
        public decimal? WeightKg { get; set; }
        public string? CategoryName { get; set; }
    }
}
