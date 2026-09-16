using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using CRM.Domain.Entities.MasterDb;
using CRM.Domain.Common;

namespace CRM.infrastructure.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<bool> HasPermissionAsync(string userId, string permission)
        {
            var role = await GetUserRoleAsync(userId);
            if (string.IsNullOrEmpty(role)) return false;

            var rolePermissions = GetPermissionsForRole(role);
            return rolePermissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<bool> HasAnyPermissionAsync(string userId, params string[] permissions)
        {
            var role = await GetUserRoleAsync(userId);
            if (string.IsNullOrEmpty(role)) return false;

            var rolePermissions = GetPermissionsForRole(role);
            return permissions.Any(p =>
                rolePermissions.Contains(p, StringComparer.OrdinalIgnoreCase));
        }

        public async Task<string[]> GetUserPermissionsAsync(string userId)
        {
            var role = await GetUserRoleAsync(userId);
            return string.IsNullOrEmpty(role)
                ? Array.Empty<string>()
                : GetPermissionsForRole(role);
        }

        public async Task<string?> GetUserRoleAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault();
        }

        private string[] GetPermissionsForRole(string roleName)
        {
            var rolePermissions = RolePermissions.GetDefaultPermissions();
            return rolePermissions.TryGetValue(roleName, out var permissions)
                ? permissions
                : Array.Empty<string>();
        }
    }
}