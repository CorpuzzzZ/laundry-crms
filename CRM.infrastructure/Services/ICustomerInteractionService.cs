using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Domain.DTOs.Customers;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public interface ICustomerInteractionService
    {
        Task<List<CustomerInteractionDto>> GetByCustomerAsync(TenantErpDbContext db, CustomerInteractionFilterDto filter);
        Task<CustomerInteractionDto?> GetByIdAsync(TenantErpDbContext db, int interactionId);
        Task<CustomerInteractionDto> CreateAsync(TenantErpDbContext db, CreateCustomerInteractionDto dto, string userId);
        Task<CustomerInteractionDto?> UpdateAsync(TenantErpDbContext db, int interactionId, UpdateCustomerInteractionDto dto, string userId);
        Task<CustomerInteractionDto?> ChangeStatusAsync(TenantErpDbContext db, int interactionId, ChangeInteractionStatusDto dto, string userId);
        Task<List<CustomerInteractionStatusHistoryDto>> GetStatusHistoryAsync(TenantErpDbContext db, int interactionId);
        Task<bool> DeleteAsync(TenantErpDbContext db, int interactionId);
    }
}