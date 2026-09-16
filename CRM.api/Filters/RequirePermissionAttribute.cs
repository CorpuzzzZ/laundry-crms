using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using CRM.infrastructure.Services;

namespace CRM.api.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _permissions;
        private readonly bool _requireAll;

        public RequirePermissionAttribute(params string[] permissions)
        {
            _permissions = permissions;
            _requireAll = false;
        }

        public RequirePermissionAttribute(bool requireAll, params string[] permissions)
        {
            _permissions = permissions;
            _requireAll = requireAll;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;

            // 1. Must be authenticated
            if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    success = false,
                    message = "Authentication required."
                });
                return;
            }

            // 2. Get UserId from JWT claims (ASP.NET Identity uses GUID strings)
            var userId = httpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    success = false,
                    message = "Invalid user token (missing UserId claim)."
                });
                return;
            }

            // 3. Check permissions
            var permissionService = httpContext.RequestServices
                .GetRequiredService<IPermissionService>();

            bool hasAccess;
            if (_requireAll)
            {
                hasAccess = _permissions
                    .All(p => permissionService.HasPermissionAsync(userId, p).GetAwaiter().GetResult());
            }
            else
            {
                hasAccess = _permissions
                    .Any(p => permissionService.HasPermissionAsync(userId, p).GetAwaiter().GetResult());
            }

            if (!hasAccess)
            {
                context.Result = new ObjectResult(new
                {
                    success = false,
                    message = $"Access denied. Required permission(s): {string.Join(", ", _permissions)}",
                    code = "INSUFFICIENT_PERMISSIONS"
                })
                {
                    StatusCode = 403
                };
            }
        }
    }
}