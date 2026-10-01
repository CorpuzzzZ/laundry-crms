using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Domain.Common;
using CRM.api.Filters;
using CRM.infrastructure.Services;

namespace CRM.api.Controllers.v1
{
    [Route("api/v1/reports")]
    [Authorize]
    public class ReportsController : TenantControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(
            ITenantDbContextFactory tenantFactory,
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor,
            IReportService reportService)
            : base(tenantFactory, permissionService, httpContextAccessor)
        {
            _reportService = reportService;
        }

        private (DateTime fromUtc, DateTime toUtc, IActionResult? err) ParseRange(DateTime? from, DateTime? to)
        {
            var fromUtc = from ?? DateTime.UtcNow.Date.AddDays(-30);
            var toUtc = to ?? DateTime.UtcNow.Date.AddDays(1);
            if (toUtc <= fromUtc)
                return (fromUtc, toUtc, BadRequest(new { success = false, message = "'to' must be after 'from'." }));
            return (fromUtc, toUtc, null);
        }

        // ============================================================
        // GET: api/v1/reports/daily-sales?from=...&to=...
        // ============================================================
        [HttpGet("daily-sales")]
        [RequirePermission(Permissions.ReportsRead)]
        public async Task<IActionResult> GetDailySales([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? branchId)
        {
            var (fromUtc, toUtc, err) = ParseRange(from, to);
            if (err != null) return err;

            await using var db = await GetTenantDbAsync();
            return Success(await _reportService.GetDailySalesAsync(db, fromUtc, toUtc, branchId));
        }

        // ============================================================
        // GET: api/v1/reports/sales-by-service?from=...&to=...&branchId=...
        // ============================================================
        [HttpGet("sales-by-service")]
        [RequirePermission(Permissions.ReportsRead)]
        public async Task<IActionResult> GetSalesByService([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? branchId)
        {
            var (fromUtc, toUtc, err) = ParseRange(from, to);
            if (err != null) return err;

            await using var db = await GetTenantDbAsync();
            return Success(await _reportService.GetSalesByServiceAsync(db, fromUtc, toUtc, branchId));
        }

        // ============================================================
        // GET: api/v1/reports/sales-by-payment-method?from=...&to=...&branchId=...
        // ============================================================
        [HttpGet("sales-by-payment-method")]
        [RequirePermission(Permissions.ReportsRead)]
        public async Task<IActionResult> GetSalesByPaymentMethod([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? branchId)
        {
            var (fromUtc, toUtc, err) = ParseRange(from, to);
            if (err != null) return err;

            await using var db = await GetTenantDbAsync();
            return Success(await _reportService.GetSalesByPaymentMethodAsync(db, fromUtc, toUtc, branchId));
        }

        // ============================================================
        // GET: api/v1/reports/order-status?from=...&to=...&branchId=...
        // ============================================================
        [HttpGet("order-status")]
        [RequirePermission(Permissions.ReportsRead)]
        public async Task<IActionResult> GetOrderStatus([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? branchId)
        {
            var (fromUtc, toUtc, err) = ParseRange(from, to);
            if (err != null) return err;

            await using var db = await GetTenantDbAsync();
            return Success(await _reportService.GetOrderStatusAsync(db, fromUtc, toUtc, branchId));
        }

        // ============================================================
        // GET: api/v1/reports/peak-hours?from=...&to=...&branchId=...
        // ============================================================
        [HttpGet("peak-hours")]
        [RequirePermission(Permissions.ReportsRead)]
        public async Task<IActionResult> GetPeakHours([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? branchId)
        {
            var (fromUtc, toUtc, err) = ParseRange(from, to);
            if (err != null) return err;

            await using var db = await GetTenantDbAsync();
            return Success(await _reportService.GetPeakHoursAsync(db, fromUtc, toUtc, branchId));
        }
    }
}
