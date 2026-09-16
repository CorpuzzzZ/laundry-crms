using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Domain.DTOs.Branches;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public interface IBranchService
    {
        Task<List<BranchDto>> GetAllAsync(MasterErpDbContext db, int companyId, BranchFilterDto filter);
        Task<BranchDto?> GetByIdAsync(MasterErpDbContext db, int companyId, int branchId);
        Task<BranchDto> CreateAsync(MasterErpDbContext db, int companyId, CreateBranchDto dto, string currentUserId);
        Task<BranchDto?> UpdateAsync(MasterErpDbContext db, int companyId, int branchId, UpdateBranchDto dto, string currentUserId);
        Task<bool> DeleteAsync(MasterErpDbContext db, int companyId, int branchId, string? currentUserId);
        Task<BranchDto?> RestoreAsync(MasterErpDbContext db, int companyId, int branchId);
        Task<BranchDto?> ToggleActiveAsync(MasterErpDbContext db, int companyId, int branchId);
        Task<BranchLimitInfoDto> GetLimitInfoAsync(MasterErpDbContext db, int companyId);
    }
}