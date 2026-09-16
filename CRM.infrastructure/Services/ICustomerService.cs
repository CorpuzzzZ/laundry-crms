using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Domain.DTOs.Customers;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public interface ICustomerService
    {
        Task<(List<CustomerDto> Customers, int TotalCount)> GetCustomersAsync(
            TenantErpDbContext db, CustomerFilterDto filter);

        Task<CustomerDto?> GetCustomerByIdAsync(TenantErpDbContext db, int customerId);

        Task<CustomerDto> CreateCustomerAsync(
            TenantErpDbContext db, CreateCustomerDto dto);

        Task<CustomerDto?> UpdateCustomerAsync(
            TenantErpDbContext db, int customerId, UpdateCustomerDto dto);

        Task<bool> DeleteCustomerAsync(TenantErpDbContext db, int customerId, string? currentUserId);
        Task<CustomerDto?> RestoreAsync(TenantErpDbContext db, int customerId);

        Task<List<CustomerDto>> GetCustomerLookupAsync(TenantErpDbContext db);

        Task<string> GenerateCustomerCodeAsync(TenantErpDbContext db);
    }
}