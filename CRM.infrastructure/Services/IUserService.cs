using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Domain.DTOs.Users;

namespace CRM.infrastructure.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllAsync(int companyId, UserFilterDto filter);
        Task<UserDto?> GetByIdAsync(int companyId, string userId);
        Task<UserDto> CreateAsync(int companyId, CreateUserDto dto, string currentUserId);
        Task<UserDto?> UpdateAsync(int companyId, string userId, UpdateUserDto dto, string currentUserId);
        Task<bool> ResetPasswordAsync(int companyId, string userId, string newPassword);
        Task<UserDto?> ToggleActiveAsync(int companyId, string userId);
        Task<UserDto?> SetBranchesAsync(int companyId, string userId, SetUserBranchesDto dto);
        Task<UserDto?> ArchiveAsync(int companyId, string userId, string currentUserId);
        Task<UserDto?> RestoreAsync(int companyId, string userId);
        Task<UserLimitInfoDto> GetLimitInfoAsync(int companyId);
        Task<List<AssignableRoleDto>> GetAssignableRolesAsync();
    }
}