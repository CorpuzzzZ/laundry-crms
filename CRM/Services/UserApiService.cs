using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.WinForms.Config;
using CRM.WinForms.Models;

namespace CRM.WinForms.Services
{
    public class UserApiService
    {
        private readonly ApiClient _api;

        public UserApiService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<UserModel>> GetAllAsync(
            string? search = null, string? role = null, bool? isActive = null, bool includeArchived = false)
        {
            var url = AppConfig.ApiV1Url("users");
            var q = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) q.Add($"searchTerm={System.Uri.EscapeDataString(search)}");
            if (!string.IsNullOrWhiteSpace(role)) q.Add($"role={System.Uri.EscapeDataString(role)}");
            if (isActive.HasValue) q.Add($"isActive={isActive.Value.ToString().ToLower()}");
            if (includeArchived) q.Add("includeArchived=true");
            if (q.Count > 0) url += "?" + string.Join("&", q);

            var response = await _api.GetDataAsync<UserListResponse>(url);
            return response?.Data ?? new List<UserModel>();
        }

        public async Task<UserModel?> GetByIdAsync(string userId)
        {
            var response = await _api.GetDataAsync<UserResponse>(
                AppConfig.ApiV1Url($"users/{userId}"));
            return response?.Data;
        }

        public async Task<UserModel?> CreateAsync(CreateUserRequest request)
        {
            var response = await _api.PostDataAsync<UserResponse>(
                AppConfig.ApiV1Url("users"), request);
            return response?.Data;
        }

        public async Task<UserModel?> UpdateAsync(string userId, UpdateUserRequest request)
        {
            var response = await _api.PutDataAsync<UserResponse>(
                AppConfig.ApiV1Url($"users/{userId}"), request);
            return response?.Data;
        }

        public async Task<bool> ResetPasswordAsync(string userId, string newPassword)
        {
            return await _api.PostDataAsync<object>(
                AppConfig.ApiV1Url($"users/{userId}/reset-password"),
                new ResetPasswordRequest { NewPassword = newPassword }) != null;
        }

        public async Task<UserModel?> ToggleActiveAsync(string userId)
        {
            var response = await _api.PostDataAsync<UserResponse>(
                AppConfig.ApiV1Url($"users/{userId}/toggle-active"), new { });
            return response?.Data;
        }

        public async Task<bool> ArchiveAsync(string userId)
        {
            return await _api.DeleteDataAsync(
                AppConfig.ApiV1Url($"users/{userId}"));
        }

        public async Task<UserModel?> RestoreAsync(string userId)
        {
            var response = await _api.PostDataAsync<UserResponse>(
                AppConfig.ApiV1Url($"users/{userId}/restore"), new { });
            return response?.Data;
        }

        public async Task<UserModel?> SetBranchesAsync(string userId, SetUserBranchesRequest request)
        {
            var response = await _api.PostDataAsync<UserResponse>(
                AppConfig.ApiV1Url($"users/{userId}/branches"), request);
            return response?.Data;
        }

        public async Task<UserLimitInfoModel?> GetLimitInfoAsync()
        {
            var response = await _api.GetDataAsync<UserLimitInfoResponse>(
                AppConfig.ApiV1Url("users/limit-info"));
            return response?.Data;
        }

        public async Task<List<AssignableRoleModel>> GetAssignableRolesAsync()
        {
            var response = await _api.GetDataAsync<AssignableRolesResponse>(
                AppConfig.ApiV1Url("users/assignable-roles"));
            return response?.Data ?? new List<AssignableRoleModel>();
        }
    }
}