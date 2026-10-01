using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Domain.DTOs.Subscriptions;
using CRM.infrastructure.Services;

namespace CRM.api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/superadmin/subscriptions")]
    [Authorize(Roles = "SuperAdmin")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        // ============================================================
        // PLANS
        // ============================================================
        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans()
        {
            var plans = await _subscriptionService.GetPlansAsync();
            return Ok(new { success = true, data = plans });
        }

        [HttpGet("plans/{id:int}")]
        public async Task<IActionResult> GetPlanById(int id)
        {
            var plan = await _subscriptionService.GetPlanByIdAsync(id);
            if (plan == null) return NotFound(new { success = false, message = $"Plan {id} not found." });
            return Ok(new { success = true, data = plan });
        }

        [HttpPost("plans")]
        public async Task<IActionResult> CreatePlan([FromBody] CreateSubscriptionPlanDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PlanName) || string.IsNullOrWhiteSpace(dto.PlanCode))
                return BadRequest(new { success = false, message = "Plan Name and Plan Code are required." });

            var created = await _subscriptionService.CreatePlanAsync(dto);
            return Ok(new { success = true, message = "Subscription plan created successfully.", data = created });
        }

        [HttpPut("plans/{id:int}")]
        public async Task<IActionResult> UpdatePlan(int id, [FromBody] CreateSubscriptionPlanDto dto)
        {
            var updated = await _subscriptionService.UpdatePlanAsync(id, dto);
            if (updated == null) return NotFound(new { success = false, message = $"Plan {id} not found." });
            return Ok(new { success = true, message = "Subscription plan updated successfully.", data = updated });
        }

        [HttpDelete("plans/{id:int}")]
        public async Task<IActionResult> DeletePlan(int id)
        {
            var ok = await _subscriptionService.DeletePlanAsync(id);
            if (!ok) return NotFound(new { success = false, message = $"Plan {id} not found." });
            return Ok(new { success = true, message = "Plan deleted/deactivated successfully." });
        }

        // ============================================================
        // COMPANIES & SUBSCRIPTIONS
        // ============================================================
        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies()
        {
            var list = await _subscriptionService.GetCompaniesAsync();
            return Ok(new { success = true, data = list });
        }

        [HttpPost("companies")]
        public async Task<IActionResult> CreateCompanyWithPlan([FromBody] CreateCompanyWithPlanDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CompanyName) || string.IsNullOrWhiteSpace(dto.CompanyCode))
                return BadRequest(new { success = false, message = "Company Name and Company Code are required." });

            if (dto.PlanId <= 0)
                return BadRequest(new { success = false, message = "A valid Subscription Plan must be selected." });

            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "SuperAdmin";
            try
            {
                var company = await _subscriptionService.CreateCompanyWithPlanAsync(dto, userEmail);
                return Ok(new { success = true, message = "Company registered and subscription activated successfully.", data = company });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("companies/assign-plan")]
        public async Task<IActionResult> AssignPlan([FromBody] AssignCompanyPlanDto dto)
        {
            if (dto.CompanyId <= 0 || dto.PlanId <= 0)
                return BadRequest(new { success = false, message = "Company ID and Plan ID are required." });

            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "SuperAdmin";
            var ok = await _subscriptionService.AssignPlanAsync(dto, userEmail);
            if (!ok) return BadRequest(new { success = false, message = "Failed to assign subscription plan." });

            return Ok(new { success = true, message = "Subscription plan assigned successfully." });
        }
    }
}
