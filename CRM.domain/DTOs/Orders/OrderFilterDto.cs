using System;

namespace CRM.Domain.DTOs.Orders
{
    public class OrderFilterDto
    {
        public string? SearchTerm { get; set; }
        public int? CustomerId { get; set; }
        public string? StatusCode { get; set; }
        public string? Priority { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string SortBy { get; set; } = "OrderDate";
        public string SortDirection { get; set; } = "DESC";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}