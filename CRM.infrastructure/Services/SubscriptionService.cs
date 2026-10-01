using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CRM.Domain.DTOs.Subscriptions;
using CRM.Domain.Entities.MasterDb;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly MasterErpDbContext _db;

        public SubscriptionService(MasterErpDbContext db)
        {
            _db = db;
        }

        public async Task<List<SubscriptionPlanDto>> GetPlansAsync()
        {
            var plans = await _db.SubscriptionPlans
                .AsNoTracking()
                .Include(p => p.TenantSubscriptions)
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.PlanName)
                .ToListAsync();

            return plans.Select(p => new SubscriptionPlanDto
            {
                PlanId = p.PlanId,
                PlanName = p.PlanName,
                PlanCode = p.PlanCode,
                Description = p.Description,
                PricePerMonth = p.PricePerMonth,
                PricePerYear = p.PricePerYear,
                MaxUsers = p.MaxUsers,
                MaxBranches = p.MaxBranches,
                MaxOrdersPerMonth = p.MaxOrdersPerMonth,
                MaxStorageGB = p.MaxStorageGB,
                IsActive = p.IsActive,
                SortOrder = p.SortOrder,
                CompanyCount = p.TenantSubscriptions.Count(s => s.IsActive)
            }).ToList();
        }

        public async Task<SubscriptionPlanDto?> GetPlanByIdAsync(int planId)
        {
            var p = await _db.SubscriptionPlans
                .AsNoTracking()
                .Include(p => p.TenantSubscriptions)
                .FirstOrDefaultAsync(x => x.PlanId == planId);

            if (p == null) return null;

            return new SubscriptionPlanDto
            {
                PlanId = p.PlanId,
                PlanName = p.PlanName,
                PlanCode = p.PlanCode,
                Description = p.Description,
                PricePerMonth = p.PricePerMonth,
                PricePerYear = p.PricePerYear,
                MaxUsers = p.MaxUsers,
                MaxBranches = p.MaxBranches,
                MaxOrdersPerMonth = p.MaxOrdersPerMonth,
                MaxStorageGB = p.MaxStorageGB,
                IsActive = p.IsActive,
                SortOrder = p.SortOrder,
                CompanyCount = p.TenantSubscriptions.Count(s => s.IsActive)
            };
        }

        public async Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanDto dto)
        {
            var plan = new SubscriptionPlan
            {
                PlanName = dto.PlanName.Trim(),
                PlanCode = dto.PlanCode.Trim().ToUpperInvariant(),
                Description = dto.Description,
                PricePerMonth = dto.PricePerMonth,
                PricePerYear = dto.PricePerYear ?? (dto.PricePerMonth * 10), // 2 months free discount if null
                MaxUsers = dto.MaxUsers,
                MaxBranches = dto.MaxBranches,
                MaxOrdersPerMonth = dto.MaxOrdersPerMonth,
                MaxStorageGB = dto.MaxStorageGB,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _db.SubscriptionPlans.Add(plan);
            await _db.SaveChangesAsync();

            return new SubscriptionPlanDto
            {
                PlanId = plan.PlanId,
                PlanName = plan.PlanName,
                PlanCode = plan.PlanCode,
                Description = plan.Description,
                PricePerMonth = plan.PricePerMonth,
                PricePerYear = plan.PricePerYear,
                MaxUsers = plan.MaxUsers,
                MaxBranches = plan.MaxBranches,
                MaxOrdersPerMonth = plan.MaxOrdersPerMonth,
                MaxStorageGB = plan.MaxStorageGB,
                IsActive = plan.IsActive,
                SortOrder = plan.SortOrder,
                CompanyCount = 0
            };
        }

        public async Task<SubscriptionPlanDto?> UpdatePlanAsync(int planId, CreateSubscriptionPlanDto dto)
        {
            var plan = await _db.SubscriptionPlans.FindAsync(planId);
            if (plan == null) return null;

            plan.PlanName = dto.PlanName.Trim();
            plan.PlanCode = dto.PlanCode.Trim().ToUpperInvariant();
            plan.Description = dto.Description;
            plan.PricePerMonth = dto.PricePerMonth;
            plan.PricePerYear = dto.PricePerYear ?? (dto.PricePerMonth * 10);
            plan.MaxUsers = dto.MaxUsers;
            plan.MaxBranches = dto.MaxBranches;
            plan.MaxOrdersPerMonth = dto.MaxOrdersPerMonth;
            plan.MaxStorageGB = dto.MaxStorageGB;
            plan.IsActive = dto.IsActive;
            plan.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return await GetPlanByIdAsync(planId);
        }

        public async Task<bool> DeletePlanAsync(int planId)
        {
            var plan = await _db.SubscriptionPlans
                .Include(p => p.TenantSubscriptions)
                .FirstOrDefaultAsync(p => p.PlanId == planId);

            if (plan == null) return false;

            if (plan.TenantSubscriptions.Any())
            {
                // Soft delete by deactivating if active subscriptions exist
                plan.IsActive = false;
                plan.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.SubscriptionPlans.Remove(plan);
            }

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<CompanySubscriptionDto>> GetCompaniesAsync()
        {
            var companies = await _db.Companies
                .AsNoTracking()
                .Include(c => c.TenantSubscriptions)
                    .ThenInclude(ts => ts.Plan)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return companies.Select(c =>
            {
                var activeSub = c.TenantSubscriptions
                    .OrderByDescending(ts => ts.CreatedAt)
                    .FirstOrDefault(ts => ts.IsActive);

                return new CompanySubscriptionDto
                {
                    CompanyId = c.CompanyId,
                    CompanyCode = c.CompanyCode,
                    CompanyName = c.CompanyName,
                    LegalName = c.LegalName,
                    Industry = c.Industry,
                    TaxId = c.TaxId,
                    Website = c.Website,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    CurrentPlanId = activeSub?.PlanId,
                    CurrentPlanName = activeSub?.Plan?.PlanName ?? "No Plan",
                    PricePerMonth = activeSub?.Plan?.PricePerMonth,
                    StartDate = activeSub?.StartDate,
                    EndDate = activeSub?.EndDate,
                    PaymentStatus = activeSub?.PaymentStatus ?? "None"
                };
            }).ToList();
        }

        public async Task<CompanySubscriptionDto> CreateCompanyWithPlanAsync(CreateCompanyWithPlanDto dto, string createdBy)
        {
            var existing = await _db.Companies.AnyAsync(c => c.CompanyCode == dto.CompanyCode.Trim());
            if (existing)
                throw new InvalidOperationException($"Company code '{dto.CompanyCode}' is already in use.");

            var plan = await _db.SubscriptionPlans.FindAsync(dto.PlanId);
            if (plan == null)
                throw new InvalidOperationException($"Subscription plan with ID {dto.PlanId} does not exist.");

            var company = new Company
            {
                CompanyCode = dto.CompanyCode.Trim().ToUpperInvariant(),
                CompanyName = dto.CompanyName.Trim(),
                LegalName = dto.LegalName?.Trim() ?? dto.CompanyName.Trim(),
                Industry = dto.Industry?.Trim() ?? "Laundry Services",
                TaxId = dto.TaxId?.Trim(),
                Website = dto.Website?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            _db.Companies.Add(company);
            await _db.SaveChangesAsync();

            // Create default CompanyDatabase mapping to local tenant DB
            var dbMapping = new CompanyDatabase
            {
                CompanyId = company.CompanyId,
                ServerName = "(localdb)\\MSSQLLocalDB",
                DatabaseName = $"TenantDB{company.CompanyId}",
                CredentialKey = $"TenantDB{company.CompanyId}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.CompanyDatabases.Add(dbMapping);

            // Create subscription
            var isYearly = string.Equals(dto.BillingCycle, "Yearly", StringComparison.OrdinalIgnoreCase);
            var now = DateTime.UtcNow;
            var sub = new TenantSubscription
            {
                CompanyId = company.CompanyId,
                PlanId = plan.PlanId,
                StartDate = now,
                EndDate = isYearly ? now.AddYears(1) : now.AddMonths(1),
                IsActive = true,
                PaymentStatus = "Paid",
                PaymentMethod = dto.PaymentMethod ?? "Manual",
                AmountPaid = dto.AmountPaid > 0 ? dto.AmountPaid : (isYearly ? (plan.PricePerYear ?? (plan.PricePerMonth * 10)) : plan.PricePerMonth),
                PaymentDate = now,
                AutoRenew = true,
                CreatedAt = now
            };
            _db.TenantSubscriptions.Add(sub);
            await _db.SaveChangesAsync();

            return new CompanySubscriptionDto
            {
                CompanyId = company.CompanyId,
                CompanyCode = company.CompanyCode,
                CompanyName = company.CompanyName,
                LegalName = company.LegalName,
                Industry = company.Industry,
                TaxId = company.TaxId,
                Website = company.Website,
                IsActive = company.IsActive,
                CreatedAt = company.CreatedAt,
                CurrentPlanId = plan.PlanId,
                CurrentPlanName = plan.PlanName,
                PricePerMonth = plan.PricePerMonth,
                StartDate = sub.StartDate,
                EndDate = sub.EndDate,
                PaymentStatus = sub.PaymentStatus
            };
        }

        public async Task<bool> AssignPlanAsync(AssignCompanyPlanDto dto, string updatedBy)
        {
            var company = await _db.Companies.FindAsync(dto.CompanyId);
            if (company == null) return false;

            var plan = await _db.SubscriptionPlans.FindAsync(dto.PlanId);
            if (plan == null) return false;

            // Deactivate older active subscriptions
            var existingSubs = await _db.TenantSubscriptions
                .Where(ts => ts.CompanyId == dto.CompanyId && ts.IsActive)
                .ToListAsync();

            foreach (var s in existingSubs)
            {
                s.IsActive = false;
                s.UpdatedAt = DateTime.UtcNow;
            }

            var isYearly = string.Equals(dto.BillingCycle, "Yearly", StringComparison.OrdinalIgnoreCase);
            var now = DateTime.UtcNow;
            var newSub = new TenantSubscription
            {
                CompanyId = dto.CompanyId,
                PlanId = dto.PlanId,
                StartDate = now,
                EndDate = isYearly ? now.AddYears(1) : now.AddMonths(1),
                IsActive = true,
                PaymentStatus = "Paid",
                PaymentMethod = dto.PaymentMethod ?? "Manual",
                AmountPaid = dto.AmountPaid > 0 ? dto.AmountPaid : (isYearly ? (plan.PricePerYear ?? (plan.PricePerMonth * 10)) : plan.PricePerMonth),
                PaymentDate = now,
                AutoRenew = true,
                CreatedAt = now
            };

            _db.TenantSubscriptions.Add(newSub);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<AvailedSubscriptionDto?> GetAvailedSubscriptionAsync(int companyId)
        {
            var company = await _db.Companies
                .AsNoTracking()
                .Include(c => c.TenantSubscriptions)
                    .ThenInclude(ts => ts.Plan)
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            if (company == null) return null;

            var activeSub = company.TenantSubscriptions
                .OrderByDescending(ts => ts.CreatedAt)
                .FirstOrDefault(ts => ts.IsActive)
                ?? company.TenantSubscriptions.OrderByDescending(ts => ts.CreatedAt).FirstOrDefault();

            if (activeSub?.Plan == null) return null;

            var plan = activeSub.Plan;
            var dto = new AvailedSubscriptionDto
            {
                CompanyId = company.CompanyId,
                CompanyCode = company.CompanyCode,
                CompanyName = company.CompanyName,
                LegalName = company.LegalName,
                Industry = company.Industry,
                TaxId = company.TaxId,
                Website = company.Website,

                PlanId = plan.PlanId,
                PlanName = plan.PlanName,
                PlanCode = plan.PlanCode,
                Description = plan.Description,
                PricePerMonth = plan.PricePerMonth,
                PricePerYear = plan.PricePerYear,
                MaxUsers = plan.MaxUsers,
                MaxBranches = plan.MaxBranches,
                MaxOrdersPerMonth = plan.MaxOrdersPerMonth,
                MaxStorageGB = plan.MaxStorageGB,

                StartDate = activeSub.StartDate,
                EndDate = activeSub.EndDate,
                IsActive = activeSub.IsActive,
                PaymentStatus = activeSub.PaymentStatus,
                PaymentMethod = activeSub.PaymentMethod,
                AmountPaid = activeSub.AmountPaid,
                PaymentDate = activeSub.PaymentDate,
                AutoRenew = activeSub.AutoRenew
            };

            AvailedSubscriptionDto.ApplyPlanFeatures(dto);
            return dto;
        }
    }
}
