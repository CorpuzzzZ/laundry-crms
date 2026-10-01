using System;
using System.Collections.Generic;

namespace CRM.Domain.DTOs.Dashboard
{
    public class AdminDashboardDto
    {
        // ===== Section 1 — Overview (this month vs last month) =====
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueLastMonth { get; set; }
        public decimal RevenueChangePct { get; set; }

        public int OrdersThisMonth { get; set; }
        public int OrdersLastMonth { get; set; }
        public decimal OrdersChangePct { get; set; }

        public int NewCustomersThisMonth { get; set; }
        public int NewCustomersLastMonth { get; set; }
        public decimal CustomersChangePct { get; set; }

        public decimal AverageOrderValue { get; set; }
        public decimal AverageOrderValueLastMonth { get; set; }
        public decimal AvgOrderChangePct { get; set; }

        // ===== Section 2 — Trends (last 30 days) =====
        public List<DailyRevenueDto> RevenueByDay { get; set; } = new();
        public List<DailyOrderCountDto> OrdersByDay { get; set; } = new();

        // ===== Section 3 — Top performers (this month) =====
        public List<TopCustomerDto> TopCustomers { get; set; } = new();
        public List<TopServiceDto> TopServices { get; set; } = new();

        // ===== Section 4 — Loyalty health (this month) =====
        public int PointsIssuedThisMonth { get; set; }
        public int PointsRedeemedThisMonth { get; set; }
        public decimal RedemptionRatePct { get; set; }
        public List<TierDistributionDto> TierDistribution { get; set; } = new();

        // ===== Meta =====
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public DateTime LastMonthFromUtc { get; set; }
        public DateTime LastMonthToUtc { get; set; }
    }

    public class DailyOrderCountDto
    {
        public DateTime Day { get; set; }
        public int Count { get; set; }
    }

    public class TopCustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int OrderItemCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TierDistributionDto
    {
        public int? LoyaltyTierId { get; set; }
        public string TierName { get; set; } = "No Tier";
        public int CustomerCount { get; set; }
    }
}
