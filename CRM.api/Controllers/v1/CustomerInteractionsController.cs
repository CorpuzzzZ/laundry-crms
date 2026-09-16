using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CRM.api.Filters;
using CRM.Domain.Common;
using CRM.Domain.DTOs.Customers;
using CRM.infrastructure.Services;

namespace CRM.api.Controllers.v1
{
    [Route("api/v1/customers/{customerId:int}/interactions")]
    public class CustomerInteractionsController : TenantControllerBase
    {
        private readonly ICustomerInteractionService _service;

        public CustomerInteractionsController(
            ITenantDbContextFactory tenantFactory,
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor,
            ICustomerInteractionService service)
            : base(tenantFactory, permissionService, httpContextAccessor)
        {
            _service = service;
        }

        [HttpGet]
        [RequirePermission(Permissions.CustomerInteractionRead)]
        public async Task<IActionResult> GetByCustomer(
            int customerId,
            [FromQuery] string? type,
            [FromQuery] string? status)
        {
            await using var db = await GetTenantDbAsync();

            var filter = new CustomerInteractionFilterDto { CustomerId = customerId };

            if (!string.IsNullOrWhiteSpace(type) &&
                Enum.TryParse<CRM.Domain.Enums.CustomerInteractionType>(type, true, out var t))
                filter.InteractionType = t;

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<CRM.Domain.Enums.CustomerInteractionStatus>(status, true, out var s))
                filter.Status = s;

            var items = await _service.GetByCustomerAsync(db, filter);
            return Success(items);
        }

        [HttpGet("{interactionId:int}")]
        [RequirePermission(Permissions.CustomerInteractionRead)]
        public async Task<IActionResult> GetById(int customerId, int interactionId)
        {
            await using var db = await GetTenantDbAsync();
            var item = await _service.GetByIdAsync(db, interactionId);

            return item == null
                ? NotFound(new { success = false, message = "Interaction " + interactionId + " not found." })
                : Success(item);
        }

        [HttpPost]
        [RequirePermission(Permissions.CustomerInteractionCreate)]
        public async Task<IActionResult> Create(int customerId, [FromBody] CreateCustomerInteractionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            dto.CustomerId = customerId;

            try
            {
                await using var db = await GetTenantDbAsync();
                var item = await _service.CreateAsync(db, dto, UserId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { customerId, interactionId = item.InteractionId },
                    new
                    {
                        success = true,
                        message = "Interaction recorded successfully.",
                        data = item
                    });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{interactionId:int}")]
        [RequirePermission(Permissions.CustomerInteractionUpdate)]
        public async Task<IActionResult> Update(
            int customerId, int interactionId, [FromBody] UpdateCustomerInteractionDto dto)
        {
            await using var db = await GetTenantDbAsync();
            var item = await _service.UpdateAsync(db, interactionId, dto, UserId);

            return item == null
                ? NotFound(new { success = false, message = "Interaction " + interactionId + " not found." })
                : Success(item, "Interaction updated successfully.");
        }

        [HttpPost("{interactionId:int}/status")]
        [RequirePermission(Permissions.CustomerInteractionUpdate)]
        public async Task<IActionResult> ChangeStatus(
            int customerId, int interactionId, [FromBody] ChangeInteractionStatusDto dto)
        {
            await using var db = await GetTenantDbAsync();
            var item = await _service.ChangeStatusAsync(db, interactionId, dto, UserId);

            return item == null
                ? NotFound(new { success = false, message = "Interaction " + interactionId + " not found." })
                : Success(item, "Status changed successfully.");
        }

        [HttpGet("{interactionId:int}/history")]
        [RequirePermission(Permissions.CustomerInteractionRead)]
        public async Task<IActionResult> GetHistory(int customerId, int interactionId)
        {
            await using var db = await GetTenantDbAsync();
            var history = await _service.GetStatusHistoryAsync(db, interactionId);
            return Success(history);
        }

        [HttpDelete("{interactionId:int}")]
        [RequirePermission(Permissions.CustomerInteractionDelete)]
        public async Task<IActionResult> Delete(int customerId, int interactionId)
        {
            await using var db = await GetTenantDbAsync();
            var deleted = await _service.DeleteAsync(db, interactionId);

            return deleted
                ? Success<object?>(null, "Interaction deleted.")
                : NotFound(new { success = false, message = "Interaction " + interactionId + " not found." });
        }
    }
}