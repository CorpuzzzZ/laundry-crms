using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Domain.DTOs.Terms;
using CRM.infrastructure.Services;

namespace CRM.api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/superadmin/terms")]
    [Authorize(Roles = "SuperAdmin")]
    public class TermsController : ControllerBase
    {
        private readonly ITermsService _termsService;

        public TermsController(ITermsService termsService)
        {
            _termsService = termsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _termsService.GetAllAsync();
            return Ok(new { success = true, data = list });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _termsService.GetByIdAsync(id);
            if (item == null) return NotFound(new { success = false, message = $"Terms {id} not found." });
            return Ok(new { success = true, data = item });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTermsDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Version) || string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { success = false, message = "Version, Title, and Content are required." });

            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "SuperAdmin";
            var created = await _termsService.CreateAsync(dto, userEmail);
            return Ok(new { success = true, message = "Terms and Conditions created successfully.", data = created });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTermsDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Version) || string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { success = false, message = "Version, Title, and Content are required." });

            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "SuperAdmin";
            var updated = await _termsService.UpdateAsync(id, dto, userEmail);
            if (updated == null) return NotFound(new { success = false, message = $"Terms {id} not found." });

            return Ok(new { success = true, message = "Terms and Conditions updated successfully.", data = updated });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _termsService.DeleteAsync(id);
            if (!ok) return NotFound(new { success = false, message = $"Terms {id} not found." });
            return Ok(new { success = true, message = "Terms and Conditions deleted successfully." });
        }
    }
}
