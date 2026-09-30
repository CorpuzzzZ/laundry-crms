using System;
using System.Collections.Generic;

namespace CRM.WinForms.Models
{
    // ===== Daily Sales =====
    public class DailySalesReportModel
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailySalesRowModel> Rows { get; set; } = new();
    }

    public class DailySalesRowModel
    {
        public DateTime Day { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageOrderValue { get; set; }

        public string DayText => Day.ToLocalTime().ToString("yyyy-MM-dd");
        public string RevenueText => $"PHP {Revenue:N2}";
        public string AvgText => $"PHP {AverageOrderValue:N2}";
    }

    public class DailySalesEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public DailySalesReportModel? Data { get; set; }
    }

    // ===== Sales by Service =====
    public class SalesByServiceReportModel
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalItems { get; set; }
        public List<SalesByServiceRowModel> Rows { get; set; } = new();
    }

    public class SalesByServiceRowModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceCode { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal RevenuePct { get; set; }

        public string RevenueText => $"PHP {Revenue:N2}";
        public string PctText => $"{RevenuePct:N1}%";
    }

    public class SalesByServiceEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public SalesByServiceReportModel? Data { get; set; }
    }

    // ===== Sales by Payment Method =====
    public class SalesByPaymentMethodReportModel
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalPayments { get; set; }
        public List<SalesByPaymentMethodRowModel> Rows { get; set; } = new();
    }

    public class SalesByPaymentMethodRowModel
    {
        public int PaymentMethodId { get; set; }
        public string MethodName { get; set; } = string.Empty;
        public string MethodCode { get; set; } = string.Empty;
        public int PaymentCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPct { get; set; }

        public string TotalAmountText => $"PHP {TotalAmount:N2}";
        public string PctText => $"{AmountPct:N1}%";
    }

    public class SalesByPaymentMethodEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public SalesByPaymentMethodReportModel? Data { get; set; }
    }

    // ===== Order Status =====
    public class OrderStatusReportModel
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public int TotalOrders { get; set; }
        public List<OrderStatusRowModel> Rows { get; set; } = new();
    }

    public class OrderStatusRowModel
    {
        public int StatusId { get; set; }
        public string StatusCode { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal PercentOfTotal { get; set; }

        public string PctText => $"{PercentOfTotal:N1}%";
    }

    public class OrderStatusEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public OrderStatusReportModel? Data { get; set; }
    }

    // ===== Peak Hours =====
    public class PeakHoursReportModel
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public int TotalOrders { get; set; }
        public int BusiestHour { get; set; }
        public int BusiestHourCount { get; set; }
        public List<PeakHourRowModel> Rows { get; set; } = new();
    }

    public class PeakHourRowModel
    {
        public int Hour { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }

        public string HourText => $"{Hour:D2}:00";
        public string RevenueText => $"PHP {Revenue:N2}";
    }

    public class PeakHoursEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public PeakHoursReportModel? Data { get; set; }
    }
}
