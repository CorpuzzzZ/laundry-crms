using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Domain.DTOs.Terms;
using CRM.infrastructure.Services;

namespace CRM.api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/tenant/terms")]
    public class TenantTermsController : ControllerBase
    {
        private readonly ITermsService _termsService;

        public TenantTermsController(ITermsService termsService)
        {
            _termsService = termsService;
        }

        private int GetCallerCompanyId()
        {
            var claim = User.FindFirst("CompanyId")?.Value;
            if (int.TryParse(claim, out var cid) && cid > 0)
                return cid;
            throw new UnauthorizedAccessException("Valid CompanyId claim not found in user session.");
        }

        // ============================================================
        // GET: Accessible by Admin, Manager, Crew
        // ============================================================
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Crew,SuperAdmin")]
        public async Task<IActionResult> GetTerms()
        {
            try
            {
                int companyId = GetCallerCompanyId();
                var terms = await _termsService.GetCompanyTermsAsync(companyId);
                return Ok(new { success = true, data = terms });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Manager,Crew,SuperAdmin")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                int companyId = GetCallerCompanyId();
                var item = await _termsService.GetByIdAsync(id);
                if (item == null) 
                    return NotFound(new { success = false, message = $"Terms #{id} not found." });

                if (!User.IsInRole("SuperAdmin") && item.CompanyId.HasValue && item.CompanyId.Value != companyId)
                    return Forbid();

                return Ok(new { success = true, data = item });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // CREATE, UPDATE, DELETE: Accessible ONLY by Admin & SuperAdmin
        // (Crew is restricted from modifying)
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateTermsDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Version) || string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { success = false, message = "Version, Title, and Content are required." });

            try
            {
                int companyId = GetCallerCompanyId();
                dto.CompanyId = companyId; // Always assign to caller's company

                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Admin";
                var created = await _termsService.CreateAsync(dto, userEmail);
                return Ok(new { success = true, message = "Terms & Conditions document created successfully.", data = created });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTermsDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Version) || string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { success = false, message = "Version, Title, and Content are required." });

            try
            {
                int companyId = GetCallerCompanyId();
                var existing = await _termsService.GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { success = false, message = $"Terms #{id} not found." });

                // Non-superadmin can only update their own company's terms
                if (!User.IsInRole("SuperAdmin") && existing.CompanyId != companyId)
                    return Forbid();

                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Admin";
                var updated = await _termsService.UpdateAsync(id, dto, userEmail);
                return Ok(new { success = true, message = "Terms & Conditions updated successfully.", data = updated });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                int companyId = GetCallerCompanyId();
                var existing = await _termsService.GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { success = false, message = $"Terms #{id} not found." });

                // Non-superadmin can only delete their own company's terms
                if (!User.IsInRole("SuperAdmin") && existing.CompanyId != companyId)
                    return Forbid();

                var ok = await _termsService.DeleteAsync(id);
                return Ok(new { success = true, message = "Terms & Conditions deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
