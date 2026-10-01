using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.api.Filters;
using CRM.Domain.Common;
using CRM.Domain.DTOs.Loyalty;
using CRM.infrastructure.Services;

namespace CRM.api.Controllers.v1
{
    [Route("api/v1/loyalty")]
    [Authorize]
    public class LoyaltyController : TenantControllerBase
    {
        private readonly ILoyaltyService _loyaltyService;

        public LoyaltyController(
            ITenantDbContextFactory tenantFactory,
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor,
            ILoyaltyService loyaltyService)
            : base(tenantFactory, permissionService, httpContextAccessor)
        {
            _loyaltyService = loyaltyService;
        }

        // ============================================================
        // GET: api/v1/loyalty/settings
        // ============================================================
        [HttpGet("settings")]
        [RequirePermission(Permissions.LoyaltyRead)]
        public async Task<IActionResult> GetSettings()
        {
            await using var db = await GetTenantDbAsync();
            var settings = await _loyaltyService.GetSettingsAsync(db);
            return Success(settings);
        }

        // ============================================================
        // PUT: api/v1/loyalty/settings
        // ============================================================
        [HttpPut("settings")]
        [RequirePermission(Permissions.LoyaltyManage)]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateLoyaltySettingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            await using var db = await GetTenantDbAsync();
            var updated = await _loyaltyService.UpdateSettingsAsync(db, dto, UserId);
            return Success(updated, "Loyalty settings updated.");
        }

        // ============================================================
        // GET: api/v1/loyalty/tiers
        // ============================================================
        [HttpGet("tiers")]
        [RequirePermission(Permissions.LoyaltyRead)]
        public async Task<IActionResult> GetTiers()
        {
            await using var db = await GetTenantDbAsync();
            var tiers = await _loyaltyService.GetTiersAsync(db);
            return Success(tiers);
        }

        // ============================================================
        // GET: api/v1/loyalty/tiers/{id}
        // ============================================================
        [HttpGet("tiers/{id:int}")]
        [RequirePermission(Permissions.LoyaltyRead)]
        public async Task<IActionResult> GetTier(int id)
        {
            await using var db = await GetTenantDbAsync();
            var tier = await _loyaltyService.GetTierByIdAsync(db, id);
            return tier == null
                ? NotFound(new { success = false, message = $"Tier {id} not found." })
                : Success(tier);
        }

        // ============================================================
        // POST: api/v1/loyalty/tiers
        // ============================================================
        [HttpPost("tiers")]
        [RequirePermission(Permissions.LoyaltyManage)]
        public async Task<IActionResult> CreateTier([FromBody] CreateLoyaltyTierDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            await using var db = await GetTenantDbAsync();
            try
            {
                var created = await _loyaltyService.CreateTierAsync(db, dto);
                return Success(created, "Tier created.");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return Conflict(new { success = false, message =
                    "A tier with that name already exists." });
            }
        }

        // ============================================================
        // PUT: api/v1/loyalty/tiers/{id}
        // ============================================================
        [HttpPut("tiers/{id:int}")]
        [RequirePermission(Permissions.LoyaltyManage)]
        public async Task<IActionResult> UpdateTier(int id, [FromBody] UpdateLoyaltyTierDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            await using var db = await GetTenantDbAsync();
            try
            {
                var updated = await _loyaltyService.UpdateTierAsync(db, id, dto);
                return updated == null
                    ? NotFound(new { success = false, message = $"Tier {id} not found." })
                    : Success(updated, "Tier updated.");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return Conflict(new { success = false, message =
                    "A tier with that name already exists." });
            }
        }

        // ============================================================
        // DELETE: api/v1/loyalty/tiers/{id}
        // ============================================================
        [HttpDelete("tiers/{id:int}")]
        [RequirePermission(Permissions.LoyaltyManage)]
        public async Task<IActionResult> DeleteTier(int id)
        {
            await using var db = await GetTenantDbAsync();
            bool ok = await _loyaltyService.DeleteTierAsync(db, id);
            if (!ok)
                return Conflict(new { success = false, message =
                    "Cannot delete a tier that is assigned to customers." });
            return Success<object?>(null, "Tier deleted.");
        }

        // ============================================================
        // GET: api/v1/loyalty/customers
        // ============================================================
        [HttpGet("customers")]
        [RequirePermission(Permissions.LoyaltyRead)]
        public async Task<IActionResult> GetCustomers()
        {
            await using var db = await GetTenantDbAsync();
            var list = await _loyaltyService.GetCustomersAsync(db);
            return Success(list);
        }

        // ============================================================
        // GET: api/v1/loyalty/customers/{id}/transactions
        // ============================================================
        // ============================================================
        // GET: api/v1/loyalty/customers/{id}/summary
        // ============================================================
        [HttpGet("customers/{id:int}/summary")]
        [RequirePermission(Permissions.LoyaltyRead)]
        public async Task<IActionResult> GetCustomerSummary(int id)
        {
            await using var db = await GetTenantDbAsync();
            var summary = await _loyaltyService.GetCustomerSummaryAsync(db, id);
            return summary == null
                ? NotFound(new { success = false, message = $"Customer {id} not found." })
                : Success(summary);
        }

        [HttpGet("customers/{id:int}/redeemable")]
        [RequirePermission(Permissions.LoyaltyRead)]
        public async Task<IActionResult> GetRedeemable(
            int id, [FromQuery] decimal orderTotal)
        {
            await using var db = await GetTenantDbAsync();
            var info = await _loyaltyService.GetRedeemableInfoAsync(db, id, orderTotal);
            return Success(info);
        }

        [HttpGet("customers/{id:int}/transactions")]
        [RequirePermission(Permissions.LoyaltyRead)]
        public async Task<IActionResult> GetCustomerTransactions(int id)
        {
            await using var db = await GetTenantDbAsync();
            var list = await _loyaltyService.GetCustomerTransactionsAsync(db, id);
            return Success(list);
        }
    }
}



