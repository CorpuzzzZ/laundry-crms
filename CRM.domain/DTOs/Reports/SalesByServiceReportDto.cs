using System;
using System.Collections.Generic;

namespace CRM.Domain.DTOs.Reports
{
    public class SalesByServiceReportDto
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalItems { get; set; }
        public List<SalesByServiceRow> Rows { get; set; } = new();
    }

    public class SalesByServiceRow
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceCode { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal RevenuePct { get; set; }
    }
}
