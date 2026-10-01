using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CRM.Domain.DTOs.SuperAdmin;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public class SuperAdminReportService : ISuperAdminReportService
    {
        private readonly MasterErpDbContext _db;

        public SuperAdminReportService(MasterErpDbContext db)
        {
            _db = db;
        }

        public async Task<SuperAdminSubscriptionReportDto> GetSubscriptionReportAsync(DateTime? from, DateTime? to)
        {
            var query = _db.TenantSubscriptions
                .AsNoTracking()
                .Include(ts => ts.Company)
                .Include(ts => ts.Plan)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(ts => ts.StartDate >= from.Value);

            if (to.HasValue)
                query = query.Where(ts => ts.StartDate <= to.Value);

            var list = await query
                .OrderByDescending(ts => ts.StartDate)
                .ToListAsync();

            var rows = list.Select(ts => new CompanySubscriptionReportRowDto
            {
                CompanyId = ts.CompanyId,
                CompanyCode = ts.Company?.CompanyCode ?? "",
                CompanyName = ts.Company?.CompanyName ?? "Unknown",
                PlanName = ts.Plan?.PlanName ?? "Unassigned",
                PricePerMonth = ts.Plan?.PricePerMonth ?? 0m,
                StartDate = ts.StartDate,
                EndDate = ts.EndDate,
                PaymentStatus = ts.PaymentStatus,
                AmountPaid = ts.AmountPaid,
                AutoRenew = ts.AutoRenew,
                IsActive = ts.IsActive
            }).ToList();

            var plans = await _db.SubscriptionPlans
                .AsNoTracking()
                .Include(p => p.TenantSubscriptions)
                .ToListAsync();

            var planSummaries = plans.Select(p => new PlanSummaryReportRowDto
            {
                PlanName = p.PlanName,
                PlanCode = p.PlanCode,
                MonthlyPrice = p.PricePerMonth,
                TenantCount = p.TenantSubscriptions.Count(ts => ts.IsActive),
                EstimatedMonthlyRevenue = p.TenantSubscriptions.Count(ts => ts.IsActive) * p.PricePerMonth,
                MaxUsers = p.MaxUsers,
                MaxBranches = p.MaxBranches
            }).ToList();

            var totalRev = rows.Sum(r => r.AmountPaid ?? r.PricePerMonth);

            return new SuperAdminSubscriptionReportDto
            {
                TotalRevenue = totalRev,
                TotalActiveSubscriptions = rows.Count(r => r.IsActive),
                Subscriptions = rows,
                PlanSummaries = planSummaries
            };
        }
    }
}
