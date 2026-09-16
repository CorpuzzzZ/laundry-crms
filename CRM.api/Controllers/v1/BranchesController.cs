using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CRM.api.Filters;
using CRM.Domain.Common;
using CRM.Domain.DTOs.Branches;
using CRM.infrastructure.Services;

namespace CRM.api.Controllers.v1
{
    [Route("api/v1/branches")]
    public class BranchesController : TenantControllerBase
    {
        private readonly IBranchService _branchService;

        public BranchesController(
            ITenantDbContextFactory tenantFactory,
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor,
            IBranchService branchService)
            : base(tenantFactory, permissionService, httpContextAccessor)
        {
            _branchService = branchService;
        }

        // Note: branches live in MasterDb, so we use the MasterErpDbContext.
        // TenantControllerBase doesn't expose it directly, so we resolve via DI.

        [HttpGet]
        [RequirePermission(Permissions.BranchRead)]
        public async Task<IActionResult> GetAll([FromQuery] BranchFilterDto filter)
        {
            var db = HttpContext.RequestServices.GetRequiredService<CRM.infrastructure.Data.Context.MasterErpDbContext>();
            var list = await _branchService.GetAllAsync(db, CompanyId, filter);
            return Success(list);
        }

        [HttpGet("{id:int}")]
        [RequirePermission(Permissions.BranchRead)]
        public async Task<IActionResult> GetById(int id)
        {
            var db = HttpContext.RequestServices.GetRequiredService<CRM.infrastructure.Data.Context.MasterErpDbContext>();
            var item = await _branchService.GetByIdAsync(db, CompanyId, id);

            return item == null
                ? NotFound(new { success = false, message = $"Branch {id} not found." })
                : Success(item);
        }

        [HttpPost]
        [RequirePermission(Permissions.BranchCreate)]
        public async Task<IActionResult> Create([FromBody] CreateBranchDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                var db = HttpContext.RequestServices.GetRequiredService<CRM.infrastructure.Data.Context.MasterErpDbContext>();
                var created = await _branchService.CreateAsync(db, CompanyId, dto, UserId);
                return CreatedAtAction(nameof(GetById), new { id = created.BranchId }, new
                {
                    success = true,
                    message = "Branch created successfully.",
                    data = created
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [RequirePermission(Permissions.BranchUpdate)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBranchDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                var db = HttpContext.RequestServices.GetRequiredService<CRM.infrastructure.Data.Context.MasterErpDbContext>();
                var updated = await _branchService.UpdateAsync(db, CompanyId, id, dto, UserId);

                return updated == null
                    ? NotFound(new { success = false, message = $"Branch {id} not found." })
                    : Success(updated, "Branch updated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{id:int}/toggle-active")]
        [RequirePermission(Permissions.BranchUpdate)]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var db = HttpContext.RequestServices.GetRequiredService<CRM.infrastructure.Data.Context.MasterErpDbContext>();
            var result = await _branchService.ToggleActiveAsync(db, CompanyId, id);

            return result == null
                ? NotFound(new { success = false, message = $"Branch {id} not found." })
                : Success(result, "Branch status changed.");
        }

        [HttpDelete("{id:int}")]
        [RequirePermission(Permissions.BranchDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var db = HttpContext.RequestServices.GetRequiredService<CRM.infrastructure.Data.Context.MasterErpDbContext>();
            var archived = await _branchService.DeleteAsync(db, CompanyId, id, UserId);

            return archived
                ? Success<object?>(null, "Branch archived.")
                : NotFound(new { success = false, message = $"Branch {id} not found." });
        }

        [HttpPost("{id:int}/restore")]
        [RequirePermission(Permissions.BranchUpdate)]
        public async Task<IActionResult> Restore(int id)
        {
            var db = HttpContext.RequestServices.GetRequiredService<CRM.infrastructure.Data.Context.MasterErpDbContext>();
            var restored = await _branchService.RestoreAsync(db, CompanyId, id);

            return restored == null
                ? NotFound(new { success = false, message = $"Branch {id} not found." })
                : Success(restored, "Branch restored.");
        }

        [HttpGet("limit-info")]
        [RequirePermission(Permissions.BranchRead)]
        public async Task<IActionResult> GetLimitInfo()
        {
            var db = HttpContext.RequestServices.GetRequiredService<CRM.infrastructure.Data.Context.MasterErpDbContext>();
            var info = await _branchService.GetLimitInfoAsync(db, CompanyId);
            return Success(info);
        }
    }
}