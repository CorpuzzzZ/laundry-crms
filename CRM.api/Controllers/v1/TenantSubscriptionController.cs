using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.infrastructure.Services;

namespace CRM.api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/tenant/subscription")]
    [Authorize(Roles = "Admin,SuperAdmin,Manager,Crew")]
    public class TenantSubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public TenantSubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet("my-plan")]
        public async Task<IActionResult> GetMyAvailedPlan([FromQuery] int? companyId)
        {
            int targetCompanyId;
            if (User.IsInRole("SuperAdmin") && companyId.HasValue && companyId.Value > 0)
            {
                targetCompanyId = companyId.Value;
            }
            else
            {
                var claim = User.FindFirst("CompanyId")?.Value;
                if (!int.TryParse(claim, out targetCompanyId) || targetCompanyId <= 0)
                {
                    return Unauthorized(new { success = false, message = "Valid CompanyId claim not found in user session." });
                }
            }

            var plan = await _subscriptionService.GetAvailedSubscriptionAsync(targetCompanyId);
            if (plan == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"No active subscription plan was found for company #{targetCompanyId}."
                });
            }

            return Ok(new
            {
                success = true,
                data = plan
            });
        }
    }
}
