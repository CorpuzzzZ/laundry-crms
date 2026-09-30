using System;
using System.Collections.Generic;

namespace CRM.WinForms.Models
{
    // ============================================================
    // DASHBOARD
    // ============================================================
    public class SuperAdminDashboardEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public SuperAdminDashboardModel? Data { get; set; }
    }

    public class SuperAdminDashboardModel
    {
        public int TotalCompanies { get; set; }
        public int ActiveCompanies { get; set; }
        public int ActiveSubscriptions { get; set; }
        public decimal MonthlyRecurringRevenue { get; set; }
        public int TotalUsers { get; set; }
        public List<PlanDistributionModel> PlanDistribution { get; set; } = new();
        public List<RecentCompanyModel> RecentCompanies { get; set; } = new();
    }

    public class PlanDistributionModel
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public int CompanyCount { get; set; }
        public decimal PricePerMonth { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class RecentCompanyModel
    {
        public int CompanyId { get; set; }
        public string CompanyCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string StatusText => IsActive ? "Active" : "Inactive";
    }

    // ============================================================
    // SUBSCRIPTIONS & PLANS
    // ============================================================
    public class SubscriptionPlanEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public SubscriptionPlanModel? Data { get; set; }
    }

    public class SubscriptionPlanListEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<SubscriptionPlanModel> Data { get; set; } = new();
    }

    public class SubscriptionPlanModel
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string PlanCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal PricePerMonth { get; set; }
        public decimal? PricePerYear { get; set; }
        public int MaxUsers { get; set; }
        public int MaxBranches { get; set; }
        public int? MaxOrdersPerMonth { get; set; }
        public int? MaxStorageGB { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int CompanyCount { get; set; }
    }

    public class CreateSubscriptionPlanRequest
    {
        public string PlanName { get; set; } = string.Empty;
        public string PlanCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal PricePerMonth { get; set; }
        public decimal? PricePerYear { get; set; }
        public int MaxUsers { get; set; }
        public int MaxBranches { get; set; }
        public int? MaxOrdersPerMonth { get; set; }
        public int? MaxStorageGB { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CompanySubscriptionListEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<CompanySubscriptionModel> Data { get; set; } = new();
    }

    public class CompanySubscriptionEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public CompanySubscriptionModel? Data { get; set; }
    }

    public class CompanySubscriptionModel
    {
        public int CompanyId { get; set; }
        public string CompanyCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public string? Industry { get; set; }
        public string? TaxId { get; set; }
        public string? Website { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CurrentPlanId { get; set; }
        public string? CurrentPlanName { get; set; }
        public decimal? PricePerMonth { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? PaymentStatus { get; set; }
    }

    public class CreateCompanyWithPlanRequest
    {
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public string? Industry { get; set; }
        public string? TaxId { get; set; }
        public string? Website { get; set; }
        public int PlanId { get; set; }
        public string BillingCycle { get; set; } = "Monthly";
        public decimal AmountPaid { get; set; }
        public string? PaymentMethod { get; set; } = "Manual";
    }

    public class AssignCompanyPlanRequest
    {
        public int CompanyId { get; set; }
        public int PlanId { get; set; }
        public string BillingCycle { get; set; } = "Monthly";
        public decimal AmountPaid { get; set; }
        public string? PaymentMethod { get; set; } = "Manual";
    }

    // ============================================================
    // TERMS & CONDITIONS
    // ============================================================
    public class TermsListEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<TermsModel> Data { get; set; } = new();
    }

    public class TermsEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public TermsModel? Data { get; set; }
    }

    public class TermsModel
    {
        public int TermsId { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsMandatory { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateTermsRequest
    {
        public string Version { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsMandatory { get; set; } = true;
        public int? CompanyId { get; set; }
    }

    public class UpdateTermsRequest
    {
        public string Version { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsMandatory { get; set; }
    }

    // ============================================================
    // REPORTS
    // ============================================================
    public class SuperAdminReportEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public SuperAdminReportModel? Data { get; set; }
    }

    public class SuperAdminReportModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalActiveSubscriptions { get; set; }
        public List<CompanySubscriptionReportRowModel> Subscriptions { get; set; } = new();
        public List<PlanSummaryReportRowModel> PlanSummaries { get; set; } = new();
    }

    public class CompanySubscriptionReportRowModel
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

    public class PlanSummaryReportRowModel
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
