using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CRM.Domain.DTOs.SuperAdmin;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public class SuperAdminDashboardService : ISuperAdminDashboardService
    {
        private readonly MasterErpDbContext _db;

        public SuperAdminDashboardService(MasterErpDbContext db)
        {
            _db = db;
        }

        public async Task<SuperAdminDashboardDto> GetDashboardDataAsync()
        {
            var totalCompanies = await _db.Companies.CountAsync();
            var activeCompanies = await _db.Companies.CountAsync(c => c.IsActive);

            var activeSubs = await _db.TenantSubscriptions
                .AsNoTracking()
                .Include(ts => ts.Plan)
                .Where(ts => ts.IsActive)
                .ToListAsync();

            var activeSubCount = activeSubs.Count;
            var mrr = activeSubs.Sum(s => s.Plan?.PricePerMonth ?? 0m);

            var totalUsers = await _db.Users.CountAsync(u => u.IsActive);

            // Plan distribution
            var plans = await _db.SubscriptionPlans
                .AsNoTracking()
                .Include(p => p.TenantSubscriptions)
                .ToListAsync();

            var planDistribution = plans.Select(p =>
            {
                var companyCount = p.TenantSubscriptions.Count(s => s.IsActive);
                return new PlanDistributionItemDto
                {
                    PlanId = p.PlanId,
                    PlanName = p.PlanName,
                    CompanyCount = companyCount,
                    PricePerMonth = p.PricePerMonth,
                    TotalRevenue = companyCount * p.PricePerMonth
                };
            }).OrderByDescending(x => x.CompanyCount).ToList();

            // Recent Companies
            var recent = await _db.Companies
                .AsNoTracking()
                .Include(c => c.TenantSubscriptions)
                    .ThenInclude(ts => ts.Plan)
                .OrderByDescending(c => c.CreatedAt)
                .Take(8)
                .ToListAsync();

            var recentCompanies = recent.Select(c =>
            {
                var currentSub = c.TenantSubscriptions
                    .OrderByDescending(ts => ts.CreatedAt)
                    .FirstOrDefault(ts => ts.IsActive);

                return new RecentCompanyItemDto
                {
                    CompanyId = c.CompanyId,
                    CompanyCode = c.CompanyCode,
                    CompanyName = c.CompanyName,
                    PlanName = currentSub?.Plan?.PlanName ?? "Unassigned",
                    CreatedAt = c.CreatedAt,
                    IsActive = c.IsActive
                };
            }).ToList();

            return new SuperAdminDashboardDto
            {
                TotalCompanies = totalCompanies,
                ActiveCompanies = activeCompanies,
                ActiveSubscriptions = activeSubCount,
                MonthlyRecurringRevenue = mrr,
                TotalUsers = totalUsers,
                PlanDistribution = planDistribution,
                RecentCompanies = recentCompanies
            };
        }
    }
}
