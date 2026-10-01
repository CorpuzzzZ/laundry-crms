using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CRM.Domain.DTOs.Dashboard;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        public async Task<CrewDashboardDto> GetCrewDashboardAsync(
            TenantErpDbContext db, DateTime fromUtc, DateTime toUtc)
        {
            // Operational — orders created within range
            var ordersInRange = db.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate >= fromUtc && o.OrderDate < toUtc);

            int todayOrderCount = await ordersInRange.CountAsync();

            // Pending — total (not range-limited; still-open work matters)
            int pendingCount = await db.Orders
                .AsNoTracking()
                .Where(o => o.Status != null && o.Status.StatusCode == "PE")
                .CountAsync();

            // Ready for pickup — total
            int readyCount = await db.Orders
                .AsNoTracking()
                .Where(o => o.Status != null && o.Status.StatusCode == "RD")
                .CountAsync();

            // Picked up within range
            int pickedUpCount = await db.Orders
                .AsNoTracking()
                .Where(o => o.Status != null
                         && o.Status.StatusCode == "PU"
                         && o.ActualPickupDate.HasValue
                         && o.ActualPickupDate.Value >= fromUtc
                         && o.ActualPickupDate.Value < toUtc)
                .CountAsync();

            // Revenue — payments made within range
            var paymentsInRange = db.OrderPayments
                .AsNoTracking()
                .Where(p => p.PaymentDate >= fromUtc && p.PaymentDate < toUtc);

            int paymentCount = await paymentsInRange.CountAsync();
            decimal totalRevenue = paymentCount > 0
                ? await paymentsInRange.SumAsync(p => p.Amount)
                : 0m;
            decimal avgPayment = paymentCount > 0
                ? totalRevenue / paymentCount
                : 0m;

            // Loyalty — transactions within range
            var loyaltyInRange = db.LoyaltyTransactions
                .AsNoTracking()
                .Where(t => t.TransactionDate >= fromUtc && t.TransactionDate < toUtc);

            int pointsIssued = await loyaltyInRange
                .Where(t => t.TransactionType == "Earn")
                .SumAsync(t => (int?)t.PointsChange) ?? 0;

            int pointsRedeemedRaw = await loyaltyInRange
                .Where(t => t.TransactionType == "Redeem")
                .SumAsync(t => (int?)t.PointsChange) ?? 0;
            int pointsRedeemed = -pointsRedeemedRaw; // stored as negative

            // Loyalty discount = (points redeemed / RedeemPointsRequired) * RedeemDiscountAmount
            var settings = await db.LoyaltySettings
                .AsNoTracking()
                .FirstOrDefaultAsync();

            decimal loyaltyDiscount = 0m;
            if (settings != null && settings.RedeemPointsRequired > 0 && pointsRedeemed > 0)
            {
                int batches = (int)Math.Floor(pointsRedeemed / settings.RedeemPointsRequired);
                loyaltyDiscount = batches * settings.RedeemDiscountAmount;
            }

            // Recent orders — top 10 by OrderDate desc (all-time)
            var recentOrders = await db.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Status)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new RecentOrderDto
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.Customer != null
                        ? (o.Customer.FirstName + " " + o.Customer.LastName).Trim()
                        : string.Empty,
                    TotalAmount = o.TotalAmount,
                    StatusName = o.Status != null ? o.Status.StatusName : string.Empty,
                    StatusCode = o.Status != null ? o.Status.StatusCode : string.Empty,
                    OrderDate = o.OrderDate
                })
                .ToListAsync();

            // ============================================================
            // Chart data
            // ============================================================

            // Orders by hour (24 buckets, sparse)
            var hourlyRaw = await db.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate >= fromUtc && o.OrderDate < toUtc)
                .GroupBy(o => o.OrderDate.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .ToListAsync();

            var hourlyDict = hourlyRaw.ToDictionary(x => x.Hour, x => x.Count);
            var ordersByHour = new List<HourlyCountDto>();
            for (int h = 0; h < 24; h++)
            {
                ordersByHour.Add(new HourlyCountDto
                {
                    Hour = h,
                    Count = hourlyDict.ContainsKey(h) ? hourlyDict[h] : 0
                });
            }

            // Revenue for the last 7 days (including today)
            var sevenDaysAgo = fromUtc.AddDays(-6).Date;
            var revenueRaw = await db.OrderPayments
                .AsNoTracking()
                .Where(p => p.PaymentDate >= sevenDaysAgo && p.PaymentDate < toUtc)
                .Select(p => new { p.PaymentDate, p.Amount })
                .ToListAsync();

            var revenueByDay = new List<DailyRevenueDto>();
            for (int d = 0; d < 7; d++)
            {
                var dayStart = sevenDaysAgo.AddDays(d);
                var dayEnd = dayStart.AddDays(1);
                var dayPayments = revenueRaw
                    .Where(p => p.PaymentDate >= dayStart && p.PaymentDate < dayEnd)
                    .ToList();
                revenueByDay.Add(new DailyRevenueDto
                {
                    Day = dayStart,
                    Revenue = dayPayments.Sum(p => p.Amount),
                    OrderCount = dayPayments.Count
                });
            }

            // Orders by status (for the range)
            var statusRaw = await db.Orders
                .AsNoTracking()
                .Include(o => o.Status)
                .Where(o => o.OrderDate >= fromUtc && o.OrderDate < toUtc && o.Status != null)
                .Select(o => new { o.Status!.StatusCode, o.Status.StatusName })
                .ToListAsync();

            var ordersByStatus = statusRaw
                .GroupBy(x => new { x.StatusCode, x.StatusName })
                .Select(g => new StatusCountDto
                {
                    StatusCode = g.Key.StatusCode,
                    StatusName = g.Key.StatusName,
                    Count = g.Count()
                })
                .OrderBy(s => s.StatusCode)
                .ToList();

            return new CrewDashboardDto
            {
                TodayOrderCount = todayOrderCount,
                PendingOrderCount = pendingCount,
                ReadyOrderCount = readyCount,
                PickedUpCount = pickedUpCount,
                TotalRevenue = totalRevenue,
                PaymentCount = paymentCount,
                AveragePayment = avgPayment,
                PointsIssued = pointsIssued,
                PointsRedeemed = pointsRedeemed,
                LoyaltyDiscountGiven = loyaltyDiscount,
                RecentOrders = recentOrders,
                FromUtc = fromUtc,
                ToUtc = toUtc,
                OrdersByHour = ordersByHour,
                RevenueByDay = revenueByDay,
                OrdersByStatus = ordersByStatus
            };
        }

        // ============================================================
        // ADMIN DASHBOARD — 30-day analytics
        // ============================================================
        public async Task<AdminDashboardDto> GetAdminDashboardAsync(
            TenantErpDbContext db, DateTime fromUtc, DateTime toUtc)
        {
            var lastFromUtc = fromUtc.AddDays(-30);
            var lastToUtc = fromUtc;

            // ---- Revenue this month / last month ----
            var paymentsThis = await db.OrderPayments
                .AsNoTracking()
                .Where(p => p.PaymentDate >= fromUtc && p.PaymentDate < toUtc)
                .Select(p => p.Amount)
                .ToListAsync();
            decimal revenueThis = paymentsThis.Sum();

            var paymentsLast = await db.OrderPayments
                .AsNoTracking()
                .Where(p => p.PaymentDate >= lastFromUtc && p.PaymentDate < lastToUtc)
                .Select(p => p.Amount)
                .ToListAsync();
            decimal revenueLast = paymentsLast.Sum();

            // ---- Orders this month / last month ----
            int ordersThis = await db.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate >= fromUtc && o.OrderDate < toUtc)
                .CountAsync();

            int ordersLast = await db.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate >= lastFromUtc && o.OrderDate < lastToUtc)
                .CountAsync();

            // ---- New customers ----
            int newCustomersThis = await db.Customers
                .AsNoTracking()
                .Where(c => c.CreatedAt >= fromUtc && c.CreatedAt < toUtc)
                .CountAsync();

            int newCustomersLast = await db.Customers
                .AsNoTracking()
                .Where(c => c.CreatedAt >= lastFromUtc && c.CreatedAt < lastToUtc)
                .CountAsync();

            // ---- Average order value ----
            decimal aovThis = ordersThis > 0 ? revenueThis / ordersThis : 0m;
            decimal aovLast = ordersLast > 0 ? revenueLast / ordersLast : 0m;

            // ---- Percentage changes (guard against divide-by-zero) ----
            decimal RevenueChangePct = revenueLast > 0
                ? ((revenueThis - revenueLast) / revenueLast) * 100m
                : 0m;
            decimal OrdersChangePct = ordersLast > 0
                ? ((decimal)(ordersThis - ordersLast) / ordersLast) * 100m
                : 0m;
            decimal CustomersChangePct = newCustomersLast > 0
                ? ((decimal)(newCustomersThis - newCustomersLast) / newCustomersLast) * 100m
                : 0m;
            decimal AvgOrderChangePct = aovLast > 0
                ? ((aovThis - aovLast) / aovLast) * 100m
                : 0m;

            // ---- Revenue by day (last 30 days) ----
            var revenueRaw = await db.OrderPayments
                .AsNoTracking()
                .Where(p => p.PaymentDate >= fromUtc && p.PaymentDate < toUtc)
                .Select(p => new { p.PaymentDate, p.Amount })
                .ToListAsync();

            var revenueByDay = new List<DailyRevenueDto>();
            for (int d = 0; d < 30; d++)
            {
                var dayStart = fromUtc.Date.AddDays(d);
                var dayEnd = dayStart.AddDays(1);
                var dayPmt = revenueRaw
                    .Where(p => p.PaymentDate >= dayStart && p.PaymentDate < dayEnd)
                    .ToList();
                revenueByDay.Add(new DailyRevenueDto
                {
                    Day = dayStart,
                    Revenue = dayPmt.Sum(p => p.Amount),
                    OrderCount = dayPmt.Count
                });
            }

            // ---- Orders by day (last 30 days) ----
            var ordersRaw = await db.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate >= fromUtc && o.OrderDate < toUtc)
                .Select(o => o.OrderDate)
                .ToListAsync();

            var ordersByDay = new List<DailyOrderCountDto>();
            for (int d = 0; d < 30; d++)
            {
                var dayStart = fromUtc.Date.AddDays(d);
                var dayEnd = dayStart.AddDays(1);
                ordersByDay.Add(new DailyOrderCountDto
                {
                    Day = dayStart,
                    Count = ordersRaw.Count(x => x >= dayStart && x < dayEnd)
                });
            }

            // ---- Top 5 customers by revenue this month ----
            var topCustomersRaw = await db.OrderPayments
                .AsNoTracking()
                .Where(p => p.PaymentDate >= fromUtc && p.PaymentDate < toUtc && p.Order != null)
                .Select(p => new
                {
                    p.Order!.CustomerId,
                    FirstName = p.Order.Customer != null ? p.Order.Customer.FirstName : "",
                    LastName = p.Order.Customer != null ? p.Order.Customer.LastName : "",
                    p.Amount
                })
                .ToListAsync();

            var topCustomers = topCustomersRaw
                .GroupBy(x => new { x.CustomerId, x.FirstName, x.LastName })
                .Select(g => new TopCustomerDto
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerName = (g.Key.FirstName + " " + g.Key.LastName).Trim(),
                    OrderCount = g.Count(),
                    TotalRevenue = g.Sum(x => x.Amount)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(5)
                .ToList();

            // ---- Top 5 services by order-item count this month ----
            var topServicesRaw = await db.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Order != null
                          && oi.Order.OrderDate >= fromUtc
                          && oi.Order.OrderDate < toUtc
                          && oi.Service != null)
                .Select(oi => new
                {
                    oi.ServiceId,
                    ServiceName = oi.Service!.ServiceName,
                    Amount = oi.UnitPrice * oi.Quantity
                })
                .ToListAsync();

            var topServices = topServicesRaw
                .GroupBy(x => new { x.ServiceId, x.ServiceName })
                .Select(g => new TopServiceDto
                {
                    ServiceId = g.Key.ServiceId,
                    ServiceName = g.Key.ServiceName,
                    OrderItemCount = g.Count(),
                    Revenue = g.Sum(x => x.Amount)
                })
                .OrderByDescending(x => x.OrderItemCount)
                .Take(5)
                .ToList();

            // ---- Loyalty health ----
            var loyaltyThis = await db.LoyaltyTransactions
                .AsNoTracking()
                .Where(t => t.TransactionDate >= fromUtc && t.TransactionDate < toUtc)
                .Select(t => new { t.TransactionType, t.PointsChange })
                .ToListAsync();

            int pointsIssued = loyaltyThis
                .Where(t => t.TransactionType == "Earn")
                .Sum(t => t.PointsChange);
            int pointsRedeemedRaw = loyaltyThis
                .Where(t => t.TransactionType == "Redeem")
                .Sum(t => t.PointsChange);
            int pointsRedeemed = -pointsRedeemedRaw;

            decimal redemptionRate = pointsIssued > 0
                ? ((decimal)pointsRedeemed / pointsIssued) * 100m
                : 0m;

            // ---- Tier distribution ----
            var tierDistRaw = await db.CustomerLoyalties
                .AsNoTracking()
                .Include(c => c.LoyaltyTier)
                .Select(c => new { c.LoyaltyTierId, TierName = c.LoyaltyTier != null ? c.LoyaltyTier.TierName : null })
                .ToListAsync();

            var tierDistribution = tierDistRaw
                .GroupBy(x => new { x.LoyaltyTierId, x.TierName })
                .Select(g => new TierDistributionDto
                {
                    LoyaltyTierId = g.Key.LoyaltyTierId,
                    TierName = g.Key.TierName ?? "No Tier",
                    CustomerCount = g.Count()
                })
                .OrderByDescending(x => x.CustomerCount)
                .ToList();

            return new AdminDashboardDto
            {
                RevenueThisMonth = revenueThis,
                RevenueLastMonth = revenueLast,
                RevenueChangePct = RevenueChangePct,
                OrdersThisMonth = ordersThis,
                OrdersLastMonth = ordersLast,
                OrdersChangePct = OrdersChangePct,
                NewCustomersThisMonth = newCustomersThis,
                NewCustomersLastMonth = newCustomersLast,
                CustomersChangePct = CustomersChangePct,
                AverageOrderValue = aovThis,
                AverageOrderValueLastMonth = aovLast,
                AvgOrderChangePct = AvgOrderChangePct,
                RevenueByDay = revenueByDay,
                OrdersByDay = ordersByDay,
                TopCustomers = topCustomers,
                TopServices = topServices,
                PointsIssuedThisMonth = pointsIssued,
                PointsRedeemedThisMonth = pointsRedeemed,
                RedemptionRatePct = redemptionRate,
                TierDistribution = tierDistribution,
                FromUtc = fromUtc,
                ToUtc = toUtc,
                LastMonthFromUtc = lastFromUtc,
                LastMonthToUtc = lastToUtc
            };
        }
    }
}



