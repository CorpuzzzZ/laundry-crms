using System;

namespace CRM.Domain.DTOs.Orders
{
    public class OrderStatusHistoryDto
    {
        public int OrderStatusHistoryId { get; set; }
        public int OrderId { get; set; }
        public int StatusId { get; set; }
        public string StatusCode { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string? ChangedByUserId { get; set; }
        public string? ChangedByName { get; set; }
        public string? Notes { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
