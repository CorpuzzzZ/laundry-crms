using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CRM.api.Filters;
using CRM.Domain.Common;
using CRM.Domain.DTOs.Services;
using CRM.infrastructure.Services;

namespace CRM.api.Controllers.v1
{
    [Route("api/v1/services")]
    public class ServicesController : TenantControllerBase
    {
        private readonly IServiceService _serviceService;

        public ServicesController(
            ITenantDbContextFactory tenantFactory,
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor,
            IServiceService serviceService)
            : base(tenantFactory, permissionService, httpContextAccessor)
        {
            _serviceService = serviceService;
        }

        [HttpGet]
        [RequirePermission(Permissions.ServiceRead)]
        public async Task<IActionResult> GetAll([FromQuery] ServiceFilterDto filter)
        {
            await using var db = await GetTenantDbAsync();
            var list = await _serviceService.GetAllAsync(db, filter);
            return Success(list);
        }

        [HttpGet("{id:int}")]
        [RequirePermission(Permissions.ServiceRead)]
        public async Task<IActionResult> GetById(int id)
        {
            await using var db = await GetTenantDbAsync();
            var item = await _serviceService.GetByIdAsync(db, id);
            return item == null
                ? NotFound(new { success = false, message = $"Service {id} not found." })
                : Success(item);
        }

        [HttpPost]
        [RequirePermission(Permissions.ServiceCreate)]
        public async Task<IActionResult> Create([FromBody] CreateServiceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                await using var db = await GetTenantDbAsync();
                var created = await _serviceService.CreateAsync(db, dto, UserId);
                return CreatedAtAction(nameof(GetById), new { id = created.ServiceId }, new
                {
                    success = true,
                    message = "Service created successfully.",
                    data = created
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [RequirePermission(Permissions.ServiceUpdate)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                await using var db = await GetTenantDbAsync();
                var updated = await _serviceService.UpdateAsync(db, id, dto, UserId);
                return updated == null
                    ? NotFound(new { success = false, message = $"Service {id} not found." })
                    : Success(updated, "Service updated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [RequirePermission(Permissions.ServiceDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            await using var db = await GetTenantDbAsync();
            var archived = await _serviceService.DeleteAsync(db, id, UserId);
            return archived
                ? Success<object?>(null, "Service archived.")
                : NotFound(new { success = false, message = $"Service {id} not found." });
        }

        [HttpPost("{id:int}/restore")]
        [RequirePermission(Permissions.ServiceUpdate)]
        public async Task<IActionResult> Restore(int id)
        {
            await using var db = await GetTenantDbAsync();
            var restored = await _serviceService.RestoreAsync(db, id);
            return restored == null
                ? NotFound(new { success = false, message = $"Service {id} not found." })
                : Success(restored, "Service restored.");
        }

        // ── ADD-ONS ─────────────────────────────────────────────
        [HttpPost("{id:int}/addons")]
        [RequirePermission(Permissions.ServiceUpdate)]
        public async Task<IActionResult> AddAddOn(int id, [FromBody] CreateServiceAddOnDto dto)
        {
            await using var db = await GetTenantDbAsync();
            var created = await _serviceService.AddAddOnAsync(db, id, dto);
            return created == null
                ? NotFound(new { success = false, message = $"Service {id} not found." })
                : Success(created, "Add-on created.");
        }

        [HttpPut("{id:int}/addons/{addOnId:int}")]
        [RequirePermission(Permissions.ServiceUpdate)]
        public async Task<IActionResult> UpdateAddOn(int id, int addOnId, [FromBody] UpdateServiceAddOnDto dto)
        {
            await using var db = await GetTenantDbAsync();
            var updated = await _serviceService.UpdateAddOnAsync(db, id, addOnId, dto);
            return updated == null
                ? NotFound(new { success = false, message = "Add-on not found." })
                : Success(updated, "Add-on updated.");
        }

        [HttpDelete("{id:int}/addons/{addOnId:int}")]
        [RequirePermission(Permissions.ServiceUpdate)]
        public async Task<IActionResult> DeleteAddOn(int id, int addOnId)
        {
            await using var db = await GetTenantDbAsync();
            var ok = await _serviceService.DeleteAddOnAsync(db, id, addOnId);
            return ok
                ? Success<object?>(null, "Add-on deleted.")
                : NotFound(new { success = false, message = "Add-on not found." });
        }

        // ── CATEGORIES ──────────────────────────────────────────
        [HttpGet("categories")]
        [RequirePermission(Permissions.ServiceRead)]
        public async Task<IActionResult> GetCategories()
        {
            await using var db = await GetTenantDbAsync();
            var list = await _serviceService.GetCategoriesAsync(db);
            return Success(list);
        }
    }
}