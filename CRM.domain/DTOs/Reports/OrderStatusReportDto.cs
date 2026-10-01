using System;
using System.Collections.Generic;

namespace CRM.Domain.DTOs.Reports
{
    public class OrderStatusReportDto
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public int TotalOrders { get; set; }
        public List<OrderStatusRow> Rows { get; set; } = new();
    }

    public class OrderStatusRow
    {
        public int StatusId { get; set; }
        public string StatusCode { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal PercentOfTotal { get; set; }
    }
}
