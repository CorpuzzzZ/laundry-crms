using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Domain.DTOs.Services;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public interface IServiceService
    {
        // ── SERVICE CRUD ──────────────────────────────────────
        Task<List<ServiceDto>> GetAllAsync(TenantErpDbContext db, ServiceFilterDto filter);
        Task<ServiceDto?> GetByIdAsync(TenantErpDbContext db, int serviceId);
        Task<ServiceDto> CreateAsync(TenantErpDbContext db, CreateServiceDto dto, string? currentUserId);
        Task<ServiceDto?> UpdateAsync(TenantErpDbContext db, int serviceId, UpdateServiceDto dto, string? currentUserId);
        Task<bool> DeleteAsync(TenantErpDbContext db, int serviceId, string? currentUserId);
        Task<ServiceDto?> RestoreAsync(TenantErpDbContext db, int serviceId);

        // ── ADD-ONS ───────────────────────────────────────────
        Task<ServiceAddOnDto?> AddAddOnAsync(TenantErpDbContext db, int serviceId, CreateServiceAddOnDto dto);
        Task<ServiceAddOnDto?> UpdateAddOnAsync(TenantErpDbContext db, int serviceId, int addOnId, UpdateServiceAddOnDto dto);
        Task<bool> DeleteAddOnAsync(TenantErpDbContext db, int serviceId, int addOnId);

        // ── CATEGORIES ────────────────────────────────────────
        Task<List<ServiceCategoryDto>> GetCategoriesAsync(TenantErpDbContext db);
    }
}