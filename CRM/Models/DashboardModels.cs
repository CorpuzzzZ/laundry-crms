using System;
using System.Collections.Generic;

namespace CRM.WinForms.Models
{
    public class CrewDashboardModel
    {
        public int TodayOrderCount { get; set; }
        public int PendingOrderCount { get; set; }
        public int ReadyOrderCount { get; set; }
        public int PickedUpCount { get; set; }

        public decimal TotalRevenue { get; set; }
        public int PaymentCount { get; set; }
        public decimal AveragePayment { get; set; }

        public int PointsIssued { get; set; }
        public int PointsRedeemed { get; set; }
        public decimal LoyaltyDiscountGiven { get; set; }

        public List<RecentOrderModel> RecentOrders { get; set; } = new();

        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }

        // ===== Chart data =====
        public List<HourlyCountModel> OrdersByHour { get; set; } = new();
        public List<DailyRevenueModel> RevenueByDay { get; set; } = new();
        public List<StatusCountModel> OrdersByStatus { get; set; } = new();
    }

    public class HourlyCountModel
    {
        public int Hour { get; set; }
        public int Count { get; set; }
    }

    public class DailyRevenueModel
    {
        public DateTime Day { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class StatusCountModel
    {
        public string StatusCode { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class RecentOrderModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }

        public string TotalAmountText => $"PHP {TotalAmount:N2}";
        public string OrderDateText => OrderDate.ToLocalTime().ToString("HH:mm");
        public string DayText => OrderDate.ToLocalTime().ToString("MMM dd");
    }

    public class CrewDashboardEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public CrewDashboardModel? Data { get; set; }
    }
}


namespace CRM.WinForms.Models
{
    public class AdminDashboardModel
    {
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

        public List<DailyRevenueModel> RevenueByDay { get; set; } = new();
        public List<DailyOrderCountModel> OrdersByDay { get; set; } = new();

        public List<TopCustomerModel> TopCustomers { get; set; } = new();
        public List<TopServiceModel> TopServices { get; set; } = new();

        public int PointsIssuedThisMonth { get; set; }
        public int PointsRedeemedThisMonth { get; set; }
        public decimal RedemptionRatePct { get; set; }
        public List<TierDistributionModel> TierDistribution { get; set; } = new();

        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }

        // Display helpers
        public string RevenueChangeText => RevenueChangePct >= 0
            ? $"▲ +{RevenueChangePct:N1}%"
            : $"▼ {RevenueChangePct:N1}%";
        public string OrdersChangeText => OrdersChangePct >= 0
            ? $"▲ +{OrdersChangePct:N1}%"
            : $"▼ {OrdersChangePct:N1}%";
        public string CustomersChangeText => CustomersChangePct >= 0
            ? $"▲ +{CustomersChangePct:N1}%"
            : $"▼ {CustomersChangePct:N1}%";
        public string AvgOrderChangeText => AvgOrderChangePct >= 0
            ? $"▲ +{AvgOrderChangePct:N1}%"
            : $"▼ {AvgOrderChangePct:N1}%";
    }

    public class DailyOrderCountModel
    {
        public DateTime Day { get; set; }
        public int Count { get; set; }
    }

    public class TopCustomerModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public string TotalRevenueText => $"PHP {TotalRevenue:N2}";
    }

    public class TopServiceModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int OrderItemCount { get; set; }
        public decimal Revenue { get; set; }
        public string RevenueText => $"PHP {Revenue:N2}";
    }

    public class TierDistributionModel
    {
        public int? LoyaltyTierId { get; set; }
        public string TierName { get; set; } = string.Empty;
        public int CustomerCount { get; set; }
    }

    public class AdminDashboardEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public AdminDashboardModel? Data { get; set; }
    }
}
