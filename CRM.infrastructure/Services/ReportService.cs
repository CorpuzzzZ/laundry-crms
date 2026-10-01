using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CRM.Domain.DTOs.Reports;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public class ReportService : IReportService
    {
        // ============================================================
        // 1. DAILY SALES
        // ============================================================
        public async Task<DailySalesReportDto> GetDailySalesAsync(
            TenantErpDbContext db, DateTime fromUtc, DateTime toUtc, int? branchId = null)
        {
            var pQuery = db.OrderPayments
                .AsNoTracking()
                .Where(p => p.PaymentDate >= fromUtc && p.PaymentDate < toUtc);

            if (branchId.HasValue && branchId.Value > 0)
            {
                pQuery = pQuery.Where(p => p.Order != null && p.Order.BranchId == branchId.Value);
            }

            var payments = await pQuery
                .Select(p => new { p.PaymentDate, p.Amount, p.OrderId })
                .ToListAsync();

            var oQuery = db.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate >= fromUtc && o.OrderDate < toUtc);

            if (branchId.HasValue && branchId.Value > 0)
            {
                oQuery = oQuery.Where(o => o.BranchId == branchId.Value);
            }

            var orders = await oQuery
                .Select(o => o.OrderDate)
                .ToListAsync();

            var rows = new List<DailySalesRow>();
            var dayCount = (int)Math.Ceiling((toUtc - fromUtc).TotalDays);
            for (int d = 0; d < dayCount; d++)
            {
                var dayStart = fromUtc.Date.AddDays(d);
                var dayEnd = dayStart.AddDays(1);

                var dayPmt = payments
                    .Where(p => p.PaymentDate >= dayStart && p.PaymentDate < dayEnd)
                    .ToList();

                var dayOrders = orders.Count(o => o >= dayStart && o < dayEnd);
                var dayRevenue = dayPmt.Sum(p => p.Amount);

                rows.Add(new DailySalesRow
                {
                    Day = dayStart,
                    OrderCount = dayOrders,
                    Revenue = dayRevenue,
                    AverageOrderValue = dayOrders > 0 ? dayRevenue / dayOrders : 0m
                });
            }

            var totalRevenue = rows.Sum(r => r.Revenue);
            var totalOrders = rows.Sum(r => r.OrderCount);

            return new DailySalesReportDto
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AverageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0m,
                Rows = rows
            };
        }

        // ============================================================
        // 2. SALES BY SERVICE
        // ============================================================
        public async Task<SalesByServiceReportDto> GetSalesByServiceAsync(
            TenantErpDbContext db, DateTime fromUtc, DateTime toUtc, int? branchId = null)
        {
            var query = db.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Order != null
                          && oi.Order.OrderDate >= fromUtc
                          && oi.Order.OrderDate < toUtc
                          && oi.Service != null);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(oi => oi.Order!.BranchId == branchId.Value);
            }

            var items = await query
                .Select(oi => new
                {
                    oi.ServiceId,
                    ServiceName = oi.Service!.ServiceName,
                    ServiceCode = oi.Service.ServiceCode,
                    oi.UnitPrice,
                    oi.Quantity
                })
                .ToListAsync();

            var grouped = items
                .GroupBy(x => new { x.ServiceId, x.ServiceName, x.ServiceCode })
                .Select(g => new SalesByServiceRow
                {
                    ServiceId = g.Key.ServiceId,
                    ServiceName = g.Key.ServiceName,
                    ServiceCode = g.Key.ServiceCode,
                    ItemCount = g.Count(),
                    Revenue = g.Sum(x => x.UnitPrice * x.Quantity)
                })
                .OrderByDescending(r => r.Revenue)
                .ToList();

            var total = grouped.Sum(r => r.Revenue);
            foreach (var row in grouped)
                row.RevenuePct = total > 0 ? (row.Revenue / total) * 100m : 0m;

            return new SalesByServiceReportDto
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                TotalRevenue = total,
                TotalItems = grouped.Sum(r => r.ItemCount),
                Rows = grouped
            };
        }

        // ============================================================
        // 3. SALES BY PAYMENT METHOD
        // ============================================================
        public async Task<SalesByPaymentMethodReportDto> GetSalesByPaymentMethodAsync(
            TenantErpDbContext db, DateTime fromUtc, DateTime toUtc, int? branchId = null)
        {
            var query = db.OrderPayments
                .AsNoTracking()
                .Where(p => p.PaymentDate >= fromUtc && p.PaymentDate < toUtc && p.PaymentMethod != null);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(p => p.Order != null && p.Order.BranchId == branchId.Value);
            }

            var payments = await query
                .Select(p => new
                {
                    p.PaymentMethodId,
                    MethodName = p.PaymentMethod!.MethodName,
                    MethodCode = p.PaymentMethod.MethodCode,
                    p.Amount
                })
                .ToListAsync();

            var grouped = payments
                .GroupBy(x => new { x.PaymentMethodId, x.MethodName, x.MethodCode })
                .Select(g => new SalesByPaymentMethodRow
                {
                    PaymentMethodId = g.Key.PaymentMethodId,
                    MethodName = g.Key.MethodName,
                    MethodCode = g.Key.MethodCode,
                    PaymentCount = g.Count(),
                    TotalAmount = g.Sum(x => x.Amount)
                })
                .OrderByDescending(r => r.TotalAmount)
                .ToList();

            var total = grouped.Sum(r => r.TotalAmount);
            foreach (var row in grouped)
                row.AmountPct = total > 0 ? (row.TotalAmount / total) * 100m : 0m;

            return new SalesByPaymentMethodReportDto
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                TotalRevenue = total,
                TotalPayments = grouped.Sum(r => r.PaymentCount),
                Rows = grouped
            };
        }

        // ============================================================
        // 4. ORDER STATUS BREAKDOWN
        // ============================================================
        public async Task<OrderStatusReportDto> GetOrderStatusAsync(
            TenantErpDbContext db, DateTime fromUtc, DateTime toUtc, int? branchId = null)
        {
            var query = db.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate >= fromUtc && o.OrderDate < toUtc && o.Status != null);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(o => o.BranchId == branchId.Value);
            }

            var orders = await query
                .Select(o => new { o.Status!.StatusId, o.Status.StatusCode, o.Status.StatusName })
                .ToListAsync();

            var grouped = orders
                .GroupBy(x => new { x.StatusId, x.StatusCode, x.StatusName })
                .Select(g => new OrderStatusRow
                {
                    StatusId = g.Key.StatusId,
                    StatusCode = g.Key.StatusCode,
                    StatusName = g.Key.StatusName,
                    OrderCount = g.Count()
                })
                .OrderByDescending(r => r.OrderCount)
                .ToList();

            var total = grouped.Sum(r => r.OrderCount);
            foreach (var row in grouped)
                row.PercentOfTotal = total > 0 ? ((decimal)row.OrderCount / total) * 100m : 0m;

            return new OrderStatusReportDto
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                TotalOrders = total,
                Rows = grouped
            };
        }

        // ============================================================
        // 5. PEAK HOURS
        // ============================================================
        public async Task<PeakHoursReportDto> GetPeakHoursAsync(
            TenantErpDbContext db, DateTime fromUtc, DateTime toUtc, int? branchId = null)
        {
            var query = db.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate >= fromUtc && o.OrderDate < toUtc);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(o => o.BranchId == branchId.Value);
            }

            var orders = await query
                .Select(o => new { o.OrderDate, o.TotalAmount })
                .ToListAsync();

            var rows = new List<PeakHourRow>();
            for (int h = 0; h < 24; h++)
            {
                var atHour = orders.Where(o => o.OrderDate.Hour == h).ToList();
                rows.Add(new PeakHourRow
                {
                    Hour = h,
                    OrderCount = atHour.Count,
                    Revenue = atHour.Sum(o => o.TotalAmount)
                });
            }

            var busiest = rows.OrderByDescending(r => r.OrderCount).FirstOrDefault();

            return new PeakHoursReportDto
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                TotalOrders = orders.Count,
                BusiestHour = busiest?.Hour ?? 0,
                BusiestHourCount = busiest?.OrderCount ?? 0,
                Rows = rows
            };
        }
    }
}
