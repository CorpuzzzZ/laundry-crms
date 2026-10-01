using System;
using System.Collections.Generic;

namespace CRM.Domain.DTOs.SuperAdmin
{
    public class SuperAdminDashboardDto
    {
        public int TotalCompanies { get; set; }
        public int ActiveCompanies { get; set; }
        public int ActiveSubscriptions { get; set; }
        public decimal MonthlyRecurringRevenue { get; set; }
        public int TotalUsers { get; set; }
        public List<PlanDistributionItemDto> PlanDistribution { get; set; } = new();
        public List<RecentCompanyItemDto> RecentCompanies { get; set; } = new();
    }

    public class PlanDistributionItemDto
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public int CompanyCount { get; set; }
        public decimal PricePerMonth { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class RecentCompanyItemDto
    {
        public int CompanyId { get; set; }
        public string CompanyCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string PlanName { get; set; } = "None";
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string StatusText => IsActive ? "Active" : "Inactive";
    }

    public class SuperAdminSubscriptionReportDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalActiveSubscriptions { get; set; }
        public List<CompanySubscriptionReportRowDto> Subscriptions { get; set; } = new();
        public List<PlanSummaryReportRowDto> PlanSummaries { get; set; } = new();
    }

    public class CompanySubscriptionReportRowDto
    {
        public int CompanyId { get; set; }
        public string CompanyCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public decimal PricePerMonth { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal? AmountPaid { get; set; }
        public bool AutoRenew { get; set; }
        public bool IsActive { get; set; }
    }

    public class PlanSummaryReportRowDto
    {
        public string PlanName { get; set; } = string.Empty;
        public string PlanCode { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public int TenantCount { get; set; }
        public decimal EstimatedMonthlyRevenue { get; set; }
        public int MaxUsers { get; set; }
        public int MaxBranches { get; set; }
    }
}
