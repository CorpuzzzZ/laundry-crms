using System;
using System.Collections.Generic;

namespace CRM.Domain.DTOs.Reports
{
    public class PeakHoursReportDto
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public int TotalOrders { get; set; }
        public int BusiestHour { get; set; }
        public int BusiestHourCount { get; set; }
        public List<PeakHourRow> Rows { get; set; } = new();
    }

    public class PeakHourRow
    {
        public int Hour { get; set; }        // 0-23
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
