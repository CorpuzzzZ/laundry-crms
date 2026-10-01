using System;
using System.Collections.Generic;

namespace CRM.Domain.DTOs.Dashboard
{
    public class CrewDashboardDto
    {
        // Operational (range-filtered)
        public int TodayOrderCount { get; set; }
        public int PendingOrderCount { get; set; }      // StatusCode = "PE"
        public int ReadyOrderCount { get; set; }        // StatusCode = "RD"
        public int PickedUpCount { get; set; }          // StatusCode = "PU" within range

        // Revenue (range-filtered)
        public decimal TotalRevenue { get; set; }
        public int PaymentCount { get; set; }
        public decimal AveragePayment { get; set; }

        // Loyalty (range-filtered)
        public int PointsIssued { get; set; }
        public int PointsRedeemed { get; set; }
        public decimal LoyaltyDiscountGiven { get; set; }

        // Recent orders (top 10 by OrderDate desc, all-time)
        public List<RecentOrderDto> RecentOrders { get; set; } = new();

        // Echo the range for UI display
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }

        // ===== Chart data =====
        // Orders per hour (0-23) for the range
        public List<HourlyCountDto> OrdersByHour { get; set; } = new();
        // Revenue for the last 7 days (includes today)
        public List<DailyRevenueDto> RevenueByDay { get; set; } = new();
        // Order counts by status for the range
        public List<StatusCountDto> OrdersByStatus { get; set; } = new();
    }

    public class HourlyCountDto
    {
        public int Hour { get; set; }
        public int Count { get; set; }
    }

    public class DailyRevenueDto
    {
        public DateTime Day { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class StatusCountDto
    {
        public string StatusCode { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
    }
}

