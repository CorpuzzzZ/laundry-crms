using System;
using System.Collections.Generic;

namespace CRM.Domain.DTOs.Subscriptions
{
    public class SubscriptionPlanDto
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
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public int CompanyCount { get; set; }
    }

    public class CreateSubscriptionPlanDto
    {
        public string PlanName { get; set; } = string.Empty;
        public string PlanCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal PricePerMonth { get; set; }
        public decimal? PricePerYear { get; set; }
        public int MaxUsers { get; set; } = 10;
        public int MaxBranches { get; set; } = 1;
        public int? MaxOrdersPerMonth { get; set; }
        public int? MaxStorageGB { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CompanySubscriptionDto
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

    public class CreateCompanyWithPlanDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public string? Industry { get; set; }
        public string? TaxId { get; set; }
        public string? Website { get; set; }
        public int PlanId { get; set; }
        public string BillingCycle { get; set; } = "Monthly"; // "Monthly" or "Yearly"
        public decimal AmountPaid { get; set; }
        public string? PaymentMethod { get; set; } = "Manual";
    }

    public class AssignCompanyPlanDto
    {
        public int CompanyId { get; set; }
        public int PlanId { get; set; }
        public string BillingCycle { get; set; } = "Monthly";
        public decimal AmountPaid { get; set; }
        public string? PaymentMethod { get; set; } = "Manual";
    }

    public class AvailedSubscriptionDto
    {
        public int CompanyId { get; set; }
        public string CompanyCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public string? Industry { get; set; }
        public string? TaxId { get; set; }
        public string? Website { get; set; }

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

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public string PaymentStatus { get; set; } = "Paid";
        public string? PaymentMethod { get; set; }
        public decimal? AmountPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        public bool AutoRenew { get; set; }

        public int DaysRemaining => EndDate.HasValue 
            ? Math.Max(0, (int)(EndDate.Value.Date - DateTime.UtcNow.Date).TotalDays) 
            : 0;
        public bool IsExpired => EndDate.HasValue && EndDate.Value < DateTime.UtcNow;

        // Module access permissions configured by Plan:
        public bool CanAccessReports { get; set; } = true;
        public bool CanAccessBranches { get; set; } = true;
        public bool CanAccessLoyalty { get; set; } = true;
        public bool CanAccessServices { get; set; } = true;
        public bool CanAccessUsers { get; set; } = true;
        public bool CanAccessCustomers { get; set; } = true;
        public bool CanAccessOrders { get; set; } = true;
        public bool CanAccessSubscription { get; set; } = true;
        public bool CanAccessTerms { get; set; } = true;
        public bool CanAccessDashboard { get; set; } = true;

        // Role-specific constraints
        public int? MaxManagers { get; set; }
        public int? MaxCrew { get; set; }

        public static void ApplyPlanFeatures(AvailedSubscriptionDto dto)
        {
            var code = (dto.PlanCode ?? string.Empty).Trim().ToUpperInvariant();
            var name = (dto.PlanName ?? string.Empty).Trim().ToUpperInvariant();

            if (code == "PLAN1" || name.Contains("PLAN 1") || dto.PlanId == 1)
            {
                // Plan 1: Full Access (All modules, unlimited branches/users)
                dto.CanAccessReports = true;
                dto.CanAccessBranches = true;
                dto.CanAccessLoyalty = true;
                dto.CanAccessServices = true;
                dto.CanAccessUsers = true;
                dto.CanAccessCustomers = true;
                dto.CanAccessOrders = true;
                dto.CanAccessSubscription = true;
                dto.CanAccessTerms = true;
                dto.CanAccessDashboard = true;
                dto.MaxManagers = null;
                dto.MaxCrew = null;
            }
            else if (code == "PLAN2" || name.Contains("PLAN 2") || dto.PlanId == 2)
            {
                // Plan 2: No Reports. Branch: only 1 branch. Users: only 1 manager and 1 crew.
                dto.CanAccessReports = false;
                dto.CanAccessBranches = true;
                dto.CanAccessLoyalty = true;
                dto.CanAccessServices = true;
                dto.CanAccessUsers = true;
                dto.CanAccessCustomers = true;
                dto.CanAccessOrders = true;
                dto.CanAccessSubscription = true;
                dto.CanAccessTerms = true;
                dto.CanAccessDashboard = true;
                dto.MaxBranches = 1;
                dto.MaxManagers = 1;
                dto.MaxCrew = 1;
            }
            else if (code == "PLAN3" || name.Contains("PLAN 3") || dto.PlanId == 3)
            {
                // Plan 3: Reports, Service, User, Customer, Order. NO Branches, NO Loyalty.
                dto.CanAccessReports = true;
                dto.CanAccessBranches = false;
                dto.CanAccessLoyalty = false;
                dto.CanAccessServices = true;
                dto.CanAccessUsers = true;
                dto.CanAccessCustomers = true;
                dto.CanAccessOrders = true;
                dto.CanAccessSubscription = true;
                dto.CanAccessTerms = true;
                dto.CanAccessDashboard = true;
                dto.MaxBranches = 0;
                dto.MaxManagers = null;
                dto.MaxCrew = null;
            }
        }
    }
}
