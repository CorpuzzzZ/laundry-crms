using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.infrastructure.Services;

namespace CRM.api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/superadmin/reports")]
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminReportsController : ControllerBase
    {
        private readonly ISuperAdminReportService _reportService;

        public SuperAdminReportsController(ISuperAdminReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("subscriptions")]
        public async Task<IActionResult> GetSubscriptionReport([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var data = await _reportService.GetSubscriptionReportAsync(from, to);
            return Ok(new
            {
                success = true,
                message = "Super Admin subscription report generated successfully.",
                data
            });
        }
    }
}
