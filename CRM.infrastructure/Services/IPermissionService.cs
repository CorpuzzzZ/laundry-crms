using System.Threading.Tasks;

namespace CRM.infrastructure.Services
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(string userId, string permission);
        Task<bool> HasAnyPermissionAsync(string userId, params string[] permissions);
        Task<string[]> GetUserPermissionsAsync(string userId);
        Task<string?> GetUserRoleAsync(string userId);
    }
}