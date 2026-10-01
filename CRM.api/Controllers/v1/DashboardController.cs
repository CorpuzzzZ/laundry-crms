using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Domain.Common;
using CRM.api.Filters;
using CRM.infrastructure.Services;

namespace CRM.api.Controllers.v1
{
    [Route("api/v1/dashboard")]
    [Authorize]
    public class DashboardController : TenantControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(
            ITenantDbContextFactory tenantFactory,
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor,
            IDashboardService dashboardService)
            : base(tenantFactory, permissionService, httpContextAccessor)
        {
            _dashboardService = dashboardService;
        }

        // ============================================================
        // GET: api/v1/dashboard/crew?from=...&to=...
        // ============================================================
        [HttpGet("crew")]
        [RequirePermission(Permissions.DashboardRead)]
        public async Task<IActionResult> GetCrewDashboard(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            // Default: last 24h if not provided
            var fromUtc = from ?? DateTime.UtcNow.Date;
            var toUtc = to ?? fromUtc.AddDays(1);

            if (toUtc <= fromUtc)
                return BadRequest(new { success = false, message = "'to' must be after 'from'." });

            await using var db = await GetTenantDbAsync();
            var dto = await _dashboardService.GetCrewDashboardAsync(db, fromUtc, toUtc);
            return Success(dto);
        }

        // ============================================================
        // GET: api/v1/dashboard/admin?from=...&to=...
        // ============================================================
        [HttpGet("admin")]
        [RequirePermission(Permissions.DashboardRead)]
        public async Task<IActionResult> GetAdminDashboard(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var fromUtc = from ?? DateTime.UtcNow.Date.AddDays(-30);
            var toUtc = to ?? DateTime.UtcNow.Date.AddDays(1);

            if (toUtc <= fromUtc)
                return BadRequest(new { success = false, message = "'to' must be after 'from'." });

            await using var db = await GetTenantDbAsync();
            var dto = await _dashboardService.GetAdminDashboardAsync(db, fromUtc, toUtc);
            return Success(dto);
        }
    }
}

