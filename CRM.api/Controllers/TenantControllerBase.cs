using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.infrastructure.Data.Context;
using CRM.infrastructure.Services;

namespace CRM.api.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class TenantControllerBase : ControllerBase
    {
        protected readonly ITenantDbContextFactory TenantFactory;
        protected readonly IPermissionService PermissionService;
        protected readonly IHttpContextAccessor HttpContextAccessor;

        protected TenantControllerBase(
            ITenantDbContextFactory tenantFactory,
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor)
        {
            TenantFactory = tenantFactory;
            PermissionService = permissionService;
            HttpContextAccessor = httpContextAccessor;
        }

        protected int CompanyId
        {
            get
            {
                var claim = HttpContextAccessor.HttpContext?.User.FindFirst("CompanyId")?.Value;
                return int.TryParse(claim, out var companyId)
                    ? companyId
                    : throw new UnauthorizedAccessException("CompanyId claim missing or invalid.");
            }
        }

        protected string UserId
        {
            get
            {
                var claim = HttpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(claim))
                    throw new UnauthorizedAccessException("UserId claim missing.");
                return claim;
            }
        }

        protected async Task<TenantErpDbContext> GetTenantDbAsync()
        {
            return await TenantFactory.CreateAsync(CompanyId);
        }

        protected IActionResult Success<T>(T data, string? message = null)
        {
            return Ok(new
            {
                success = true,
                message = message ?? "Operation completed successfully.",
                data
            });
        }

        protected IActionResult Error(string message, int statusCode = 400)
        {
            return StatusCode(statusCode, new { success = false, message });
        }
    }
}