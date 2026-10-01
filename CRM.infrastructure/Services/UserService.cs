using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CRM.Domain.DTOs.Users;
using CRM.Domain.Entities.MasterDb;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly MasterErpDbContext _db;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            MasterErpDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        // ============================================================
        // LIST
        // ============================================================
        public async Task<List<UserDto>> GetAllAsync(int companyId, UserFilterDto filter)
        {
            var query = _db.Users
                .Where(u => u.CompanyId == companyId)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(term) ||
                    u.LastName.ToLower().Contains(term) ||
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));
            }

            if (filter.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filter.IsActive.Value);

            // Hide archived by default; show only if IncludeArchived = true
            if (!filter.IncludeArchived)
                query = query.Where(u => !u.IsArchived);

            var users = await query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ToListAsync();

            // Filter by role if requested
            if (!string.IsNullOrWhiteSpace(filter.Role))
            {
                var roleUsers = await _userManager.GetUsersInRoleAsync(filter.Role);
                var roleIds = roleUsers.Select(u => u.Id).ToHashSet();
                users = users.Where(u => roleIds.Contains(u.Id)).ToList();
            }

            // Filter by branch if requested
            if (filter.BranchId.HasValue)
            {
                var branchUserIds = await _db.UserBranches
                    .Where(ub => ub.BranchId == filter.BranchId.Value)
                    .Select(ub => ub.UserId)
                    .ToListAsync();
                var branchSet = branchUserIds.ToHashSet();
                users = users.Where(u => branchSet.Contains(u.Id)).ToList();
            }

            // Load roles + branches for the result set
            var result = new List<UserDto>();
            foreach (var u in users)
                result.Add(await MapAsync(u));

            return result;
        }

        // ============================================================
        // GET BY ID
        // ============================================================
        public async Task<UserDto?> GetByIdAsync(int companyId, string userId)
        {
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == companyId);

            return user == null ? null : await MapAsync(user);
        }

        // ============================================================
        // CREATE
        // ============================================================
        public async Task<UserDto> CreateAsync(int companyId, CreateUserDto dto, string currentUserId)
        {
            // Ã¢â€â‚¬Ã¢â€â‚¬ Subscription cap check Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
            var limitInfo = await GetLimitInfoAsync(companyId);
            if (!limitInfo.CanAddMore)
            {
                throw new InvalidOperationException(
                    $"User limit reached. Your plan ({limitInfo.PlanName ?? "current"}) allows {limitInfo.MaxUsers} " +
                    $"user(s) and you already have {limitInfo.CurrentUsers}. " +
                    $"Upgrade your subscription to add more.");
            }

            // Ã¢â€â‚¬Ã¢â€â‚¬ Validate role Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
            var assignable = new[] { "Admin", "Manager", "Crew" };
            if (!assignable.Contains(dto.Role))
                throw new InvalidOperationException($"Role '{dto.Role}' is not assignable.");

            // Plan 2 enforcement: only 1 manager and 1 crew
            var activeSub = await _db.TenantSubscriptions
                .Include(ts => ts.Plan)
                .Where(ts => ts.CompanyId == companyId && ts.IsActive)
                .OrderByDescending(ts => ts.StartDate)
                .FirstOrDefaultAsync();

            var currentPlan = activeSub?.Plan;
            bool isPlan2 = currentPlan != null && (currentPlan.PlanCode == "PLAN2" || currentPlan.PlanId == 2 || (currentPlan.PlanName != null && currentPlan.PlanName.Contains("Plan 2", StringComparison.OrdinalIgnoreCase)));

            if (isPlan2)
            {
                if (dto.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                {
                    var managerRole = await _roleManager.FindByNameAsync("Manager");
                    if (managerRole != null)
                    {
                        var managerCount = await _db.UserRoles
                            .Where(ur => ur.RoleId == managerRole.Id)
                            .Join(_db.Users.Where(u => u.CompanyId == companyId && !u.IsArchived),
                                  ur => ur.UserId,
                                  u => u.Id,
                                  (ur, u) => u)
                            .CountAsync();
                        if (managerCount >= 1)
                        {
                            throw new InvalidOperationException("Plan 2 allows a maximum of 1 Manager account. You have already reached this limit.");
                        }
                    }
                }
                else if (dto.Role.Equals("Crew", StringComparison.OrdinalIgnoreCase))
                {
                    var crewRole = await _roleManager.FindByNameAsync("Crew");
                    if (crewRole != null)
                    {
                        var crewCount = await _db.UserRoles
                            .Where(ur => ur.RoleId == crewRole.Id)
                            .Join(_db.Users.Where(u => u.CompanyId == companyId && !u.IsArchived),
                                  ur => ur.UserId,
                                  u => u.Id,
                                  (ur, u) => u)
                            .CountAsync();
                        if (crewCount >= 1)
                        {
                            throw new InvalidOperationException("Plan 2 allows a maximum of 1 Crew account. You have already reached this limit.");
                        }
                    }
                }
            }

            // Ã¢â€â‚¬Ã¢â€â‚¬ Duplicate email check Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use.");

            // Ã¢â€â‚¬Ã¢â€â‚¬ Create Identity user Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                CompanyId = companyId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            // Ã¢â€â‚¬Ã¢â€â‚¬ Assign role Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
            await _userManager.AddToRoleAsync(user, dto.Role);

            // Ã¢â€â‚¬Ã¢â€â‚¬ Assign branches Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
            if (dto.BranchIds != null && dto.BranchIds.Count > 0)
                await AssignBranchesAsync(user.Id, dto.BranchIds, dto.BranchIds.First());

            return await MapAsync(user);
        }

        // ============================================================
        // UPDATE
        // ============================================================
        public async Task<UserDto?> UpdateAsync(int companyId, string userId, UpdateUserDto dto, string currentUserId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.CompanyId != companyId) return null;

            // Ã¢â€â‚¬Ã¢â€â‚¬ Validate role Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
            var assignable = new[] { "Admin", "Manager", "Crew" };
            if (!assignable.Contains(dto.Role))
                throw new InvalidOperationException($"Role '{dto.Role}' is not assignable.");

            // Ã¢â€â‚¬Ã¢â€â‚¬ Update profile Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.PhoneNumber = dto.PhoneNumber?.Trim();
            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = currentUserId;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to update user: {string.Join("; ", updateResult.Errors.Select(e => e.Description))}");

            // Ã¢â€â‚¬Ã¢â€â‚¬ Update role Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(dto.Role))
            {
                // Plan 2 enforcement: only 1 manager and 1 crew
                var activeSub = await _db.TenantSubscriptions
                    .Include(ts => ts.Plan)
                    .Where(ts => ts.CompanyId == companyId && ts.IsActive)
                    .OrderByDescending(ts => ts.StartDate)
                    .FirstOrDefaultAsync();

                var currentPlan = activeSub?.Plan;
                bool isPlan2 = currentPlan != null && (currentPlan.PlanCode == "PLAN2" || currentPlan.PlanId == 2 || (currentPlan.PlanName != null && currentPlan.PlanName.Contains("Plan 2", StringComparison.OrdinalIgnoreCase)));

                if (isPlan2)
                {
                    if (dto.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                    {
                        var managerRole = await _roleManager.FindByNameAsync("Manager");
                        if (managerRole != null)
                        {
                            var managerCount = await _db.UserRoles
                                .Where(ur => ur.RoleId == managerRole.Id)
                                .Join(_db.Users.Where(u => u.CompanyId == companyId && !u.IsArchived && u.Id != userId),
                                      ur => ur.UserId,
                                      u => u.Id,
                                      (ur, u) => u)
                                .CountAsync();
                            if (managerCount >= 1)
                            {
                                throw new InvalidOperationException("Plan 2 allows a maximum of 1 Manager account. You have already reached this limit.");
                            }
                        }
                    }
                    else if (dto.Role.Equals("Crew", StringComparison.OrdinalIgnoreCase))
                    {
                        var crewRole = await _roleManager.FindByNameAsync("Crew");
                        if (crewRole != null)
                        {
                            var crewCount = await _db.UserRoles
                                .Where(ur => ur.RoleId == crewRole.Id)
                                .Join(_db.Users.Where(u => u.CompanyId == companyId && !u.IsArchived && u.Id != userId),
                                      ur => ur.UserId,
                                      u => u.Id,
                                      (ur, u) => u)
                                .CountAsync();
                            if (crewCount >= 1)
                            {
                                throw new InvalidOperationException("Plan 2 allows a maximum of 1 Crew account. You have already reached this limit.");
                            }
                        }
                    }
                }

                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, dto.Role);
            }

            return await MapAsync(user);
        }

        // ============================================================
        // RESET PASSWORD
        // ============================================================
        public async Task<bool> ResetPasswordAsync(int companyId, string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.CompanyId != companyId) return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to reset password: {string.Join("; ", result.Errors.Select(e => e.Description))}");

            return true;
        }

        // ============================================================
        // TOGGLE ACTIVE
        // ============================================================
        public async Task<UserDto?> ToggleActiveAsync(int companyId, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.CompanyId != companyId) return null;

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return await MapAsync(user);
        }

        // ============================================================
        // SET BRANCHES
        // ============================================================
        public async Task<UserDto?> SetBranchesAsync(int companyId, string userId, SetUserBranchesDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.CompanyId != companyId) return null;

            await AssignBranchesAsync(userId, dto.BranchIds, dto.PrimaryBranchId ?? dto.BranchIds.FirstOrDefault());

            return await MapAsync(user);
        }

        private async Task AssignBranchesAsync(string userId, List<int> branchIds, int primaryBranchId)
        {
            // Remove existing assignments
            var existing = await _db.UserBranches.Where(ub => ub.UserId == userId).ToListAsync();
            _db.UserBranches.RemoveRange(existing);

            // Add new
            foreach (var branchId in branchIds.Distinct())
            {
                _db.UserBranches.Add(new UserBranch
                {
                    UserId = userId,
                    BranchId = branchId,
                    IsPrimary = branchId == primaryBranchId,
                    AssignedAt = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();
        }

        // ============================================================
        // ============================================================
        // ARCHIVE (soft-delete)
        // ============================================================
        public async Task<UserDto?> ArchiveAsync(int companyId, string userId, string currentUserId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.CompanyId != companyId) return null;

            user.IsArchived = true;
            user.ArchivedAt = DateTime.UtcNow;
            user.ArchivedBy = currentUserId;
            user.IsActive = false;      // also deactivate
            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
            return await MapAsync(user);
        }

        // ============================================================
        // RESTORE
        // ============================================================
        public async Task<UserDto?> RestoreAsync(int companyId, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.CompanyId != companyId) return null;

            user.IsArchived = false;
            user.ArchivedAt = null;
            user.ArchivedBy = null;
            user.IsActive = true;       // restore to active
            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
            return await MapAsync(user);
        }

        // ============================================================
        // LIMIT INFO
        // ============================================================
        public async Task<UserLimitInfoDto> GetLimitInfoAsync(int companyId)
        {
            var currentCount = await _db.Users.CountAsync(u => u.CompanyId == companyId);

            var subscription = await _db.TenantSubscriptions
                .Include(ts => ts.Plan)
                .Where(ts => ts.CompanyId == companyId && ts.IsActive)
                .OrderByDescending(ts => ts.StartDate)
                .FirstOrDefaultAsync();

            int maxUsers = subscription?.Plan?.MaxUsers ?? 5;
            string? planName = subscription?.Plan?.PlanName;

            return new UserLimitInfoDto
            {
                CurrentUsers = currentCount,
                MaxUsers = maxUsers,
                CanAddMore = currentCount < maxUsers,
                PlanName = planName
            };
        }

        // ASSIGNABLE ROLES
        public Task<List<AssignableRoleDto>> GetAssignableRolesAsync()
        {
            // Admin is provisioned only by SuperAdmin. Admins may only
            // assign Manager or Crew within their tenant.
            return Task.FromResult(new List<AssignableRoleDto>
            {
                new() { Name = "Manager", Description = "Operational access to orders, customers, and reports" },
                new() { Name = "Crew",    Description = "Day-to-day order entry and customer service" }
            });
        }

        // MAPPER
        private async Task<UserDto> MapAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "";

            var branchIds = await _db.UserBranches
                .Where(ub => ub.UserId == user.Id)
                .Select(ub => ub.BranchId)
                .ToListAsync();

            var branchNames = await _db.Branches
                .Where(b => branchIds.Contains(b.BranchId))
                .Select(b => b.BranchName)
                .ToListAsync();

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                CompanyId = user.CompanyId,
                Role = role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                IsArchived = user.IsArchived,
                ArchivedAt = user.ArchivedAt,
                BranchIds = branchIds,
                BranchNames = branchNames
            };
        }
    }
}
