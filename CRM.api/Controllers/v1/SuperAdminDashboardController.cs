using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.infrastructure.Services;

namespace CRM.api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/superadmin/dashboard")]
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminDashboardController : ControllerBase
    {
        private readonly ISuperAdminDashboardService _dashboardService;

        public SuperAdminDashboardController(ISuperAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var data = await _dashboardService.GetDashboardDataAsync();
            return Ok(new
            {
                success = true,
                message = "Super Admin dashboard loaded successfully.",
                data
            });
        }
    }
}
