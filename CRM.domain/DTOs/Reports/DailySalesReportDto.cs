using System;
using System.Collections.Generic;

namespace CRM.Domain.DTOs.Reports
{
    public class DailySalesReportDto
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailySalesRow> Rows { get; set; } = new();
    }

    public class DailySalesRow
    {
        public DateTime Day { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}
