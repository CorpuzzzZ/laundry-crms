using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM.api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok(new { message = "This is a public endpoint", timestamp = DateTime.UtcNow });
        }

        [Authorize]
        [HttpGet("secure")]
        public IActionResult Secure()
        {
            var userId = User.FindFirst("UserId")?.Value;
            var companyId = User.FindFirst("CompanyId")?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            return Ok(new
            {
                message = "This is a secure endpoint",
                userId = userId,
                companyId = companyId,
                roles = roles,
                timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok(new { message = "Only SuperAdmin can access this" });
        }
    }
}