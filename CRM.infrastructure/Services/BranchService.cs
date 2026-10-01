using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CRM.Domain.DTOs.Branches;
using CRM.Domain.Entities.MasterDb;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public class BranchService : IBranchService
    {
        public async Task<List<BranchDto>> GetAllAsync(MasterErpDbContext db, int companyId, BranchFilterDto filter)
        {
            var query = db.Branches
                .Where(b => b.CompanyId == companyId)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(b =>
                    b.BranchCode.Contains(term) ||
                    b.BranchName.Contains(term) ||
                    b.City.Contains(term) ||
                    b.Phone.Contains(term));
            }

            if (filter.IsActive.HasValue)
                query = query.Where(b => b.IsActive == filter.IsActive.Value);

            // Hide archived by default; show them only if IncludeArchived = true
            if (!filter.IncludeArchived)
                query = query.Where(b => !b.IsArchived);

            var rows = await query
                .OrderBy(b => b.BranchName)
                .ToListAsync();

            // Batch-resolve managers for all rows in one query
            var branchIds = rows.Select(b => b.BranchId).ToList();
            var managersByBranch = await ResolveManagersAsync(db, branchIds);

            var result = new List<BranchDto>();
            foreach (var b in rows)
            {
                var dto = Map(b);
                if (managersByBranch.TryGetValue(b.BranchId, out var mgrName))
                    dto.ManagerName = mgrName;
                result.Add(dto);
            }

            return result;
        }

        public async Task<BranchDto?> GetByIdAsync(MasterErpDbContext db, int companyId, int branchId)
        {
            var branch = await db.Branches
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BranchId == branchId && b.CompanyId == companyId);

            return branch == null ? null : await MapWithManagerAsync(db, branch);
        }

        // ============================================================
        // GENERATE BRANCH CODE  (BR-2026-0001, BR-2026-0002, ...)
        // ============================================================
        private static async Task<string> GenerateBranchCodeAsync(MasterErpDbContext db, int companyId)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"BR-{year}-";

            var lastCode = await db.Branches
                .Where(b => b.CompanyId == companyId && b.BranchCode.StartsWith(prefix))
                .OrderByDescending(b => b.BranchId)
                .Select(b => b.BranchCode)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCode))
            {
                var parts = lastCode.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var lastNumber))
                    nextNumber = lastNumber + 1;
            }

            return $"{prefix}{nextNumber:D4}";
        }

        public async Task<BranchDto> CreateAsync(
            MasterErpDbContext db, int companyId, CreateBranchDto dto, string currentUserId)
        {
            // ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ Subscription cap check ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬
            var limitInfo = await GetLimitInfoAsync(db, companyId);
            if (limitInfo.MaxBranches <= 0)
            {
                throw new InvalidOperationException(
                    $"Your current subscription plan ({limitInfo.PlanName ?? "current"}) does not include Branch Management.");
            }
            if (!limitInfo.CanAddMore)
            {
                throw new InvalidOperationException(
                    $"Branch limit reached. Your plan ({limitInfo.PlanName ?? "current"}) allows {limitInfo.MaxBranches} " +
                    $"branch(es) and you already have {limitInfo.CurrentBranches}. " +
                    $"Upgrade your subscription to add more.");
            }

            // ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ Generate unique branch code ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚ÂÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬
            var branchCode = await GenerateBranchCodeAsync(db, companyId);

            var branch = new Branch
            {
                CompanyId = companyId,
                BranchCode = branchCode,
                BranchName = dto.BranchName.Trim(),
                Street = dto.Street.Trim(),
                Village = dto.Village?.Trim(),
                City = dto.City.Trim(),
                Province = dto.Province?.Trim(),
                PostalCode = dto.PostalCode.Trim(),
                Country = string.IsNullOrWhiteSpace(dto.Country) ? "Philippines" : dto.Country.Trim(),
                Phone = dto.Phone.Trim(),
                Email = dto.Email?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Branches.Add(branch);
            await db.SaveChangesAsync();

            // Manager nav removed ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â no reload needed
            return await MapWithManagerAsync(db, branch);
        }

        public async Task<BranchDto?> UpdateAsync(
            MasterErpDbContext db, int companyId, int branchId, UpdateBranchDto dto, string currentUserId)
        {
            var branch = await db.Branches
                .FirstOrDefaultAsync(b => b.BranchId == branchId && b.CompanyId == companyId);

            if (branch == null) return null;

            branch.BranchName = dto.BranchName.Trim();
            branch.Street = dto.Street.Trim();
            branch.Village = dto.Village?.Trim();
            branch.City = dto.City.Trim();
            branch.Province = dto.Province?.Trim();
            branch.PostalCode = dto.PostalCode.Trim();
            branch.Country = string.IsNullOrWhiteSpace(dto.Country) ? "Philippines" : dto.Country.Trim();
            branch.Phone = dto.Phone.Trim();
            branch.Email = dto.Email?.Trim();
            branch.IsActive = dto.IsActive;
            branch.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return await MapWithManagerAsync(db, branch);
        }

        public async Task<bool> DeleteAsync(MasterErpDbContext db, int companyId, int branchId, string? currentUserId = null)
        {
            var branch = await db.Branches
                .FirstOrDefaultAsync(b => b.BranchId == branchId && b.CompanyId == companyId);

            if (branch == null) return false;

            // Soft delete — archive instead of hard delete
            branch.IsArchived = true;
            branch.ArchivedAt = DateTime.UtcNow;
            branch.ArchivedBy = currentUserId;
            branch.IsActive = false;
            branch.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // RESTORE — unarchive a branch
        // ============================================================
        public async Task<BranchDto?> RestoreAsync(MasterErpDbContext db, int companyId, int branchId)
        {
            var branch = await db.Branches
                .FirstOrDefaultAsync(b => b.BranchId == branchId && b.CompanyId == companyId);

            if (branch == null) return null;

            branch.IsArchived = false;
            branch.ArchivedAt = null;
            branch.ArchivedBy = null;
            branch.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return await MapWithManagerAsync(db, branch);
        }

        public async Task<BranchDto?> ToggleActiveAsync(MasterErpDbContext db, int companyId, int branchId)
        {
            var branch = await db.Branches
                .FirstOrDefaultAsync(b => b.BranchId == branchId && b.CompanyId == companyId);

            if (branch == null) return null;

            branch.IsActive = !branch.IsActive;
            branch.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return await MapWithManagerAsync(db, branch);
        }

        public async Task<BranchLimitInfoDto> GetLimitInfoAsync(MasterErpDbContext db, int companyId)
        {
            var currentCount = await db.Branches.CountAsync(b => b.CompanyId == companyId);

            // Find active subscription -> plan -> MaxBranches
            var subscription = await db.TenantSubscriptions
                .Include(ts => ts.Plan)
                .Where(ts => ts.CompanyId == companyId && ts.IsActive)
                .OrderByDescending(ts => ts.StartDate)
                .FirstOrDefaultAsync();

            int maxBranches = subscription?.Plan?.MaxBranches ?? 1;
            string? planName = subscription?.Plan?.PlanName;

            return new BranchLimitInfoDto
            {
                CurrentBranches = currentCount,
                MaxBranches = maxBranches,
                CanAddMore = currentCount < maxBranches,
                PlanName = planName
            };
        }

        // ============================================================
        // MAPPER
        // ============================================================
        // ============================================================
        // MANAGER RESOLUTION Ã¢â‚¬â€ batches all branches in one query
        // ============================================================
        private static async Task<Dictionary<int, string>> ResolveManagersAsync(
            MasterErpDbContext db, List<int> branchIds)
        {
            if (branchIds == null || branchIds.Count == 0)
                return new Dictionary<int, string>();

            // One query: users with role 'Manager' assigned to any of these branches
            var rows = await (
                from ub in db.UserBranches
                join u in db.Users on ub.UserId equals u.Id
                join ur in db.UserRoles on u.Id equals ur.UserId
                join r in db.Roles on ur.RoleId equals r.Id
                where branchIds.Contains(ub.BranchId) && r.Name == "Manager"
                orderby ub.IsPrimary descending, ub.AssignedAt
                select new { ub.BranchId, u.FirstName, u.LastName, u.Email }
            ).ToListAsync();

            var map = new Dictionary<int, string>();
            foreach (var row in rows)
            {
                if (map.ContainsKey(row.BranchId)) continue;
                var name = ($"{row.FirstName} {row.LastName}").Trim();
                if (string.IsNullOrWhiteSpace(name)) name = row.Email ?? "(unknown)";
                map[row.BranchId] = name;
            }
            return map;
        }

        // ============================================================
        // Single-branch wrapper
        // ============================================================
        private static async Task<BranchDto> MapWithManagerAsync(MasterErpDbContext db, Branch b)
        {
            var dto = Map(b);
            var lookup = await ResolveManagersAsync(db, new List<int> { b.BranchId });
            if (lookup.TryGetValue(b.BranchId, out var mgrName))
                dto.ManagerName = mgrName;
            return dto;
        }

        // ============================================================
        private static BranchDto Map(Branch b) => new()
        {
            BranchId = b.BranchId,
            CompanyId = b.CompanyId,
            BranchCode = b.BranchCode,
            BranchName = b.BranchName,
            Street = b.Street,
            Village = b.Village,
            City = b.City,
            Province = b.Province,
            PostalCode = b.PostalCode,
            Country = b.Country,
            Phone = b.Phone,
            Email = b.Email,
            ManagerName = null,   // populated by ResolveManagersAsync/MapWithManagerAsync
            IsActive = b.IsActive,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt,
            IsArchived = b.IsArchived,
            ArchivedAt = b.ArchivedAt
        };
    }
}
