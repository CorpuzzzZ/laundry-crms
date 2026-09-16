using System.Threading.Tasks;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public interface ITenantDbContextFactory
    {
        Task<TenantErpDbContext> CreateAsync(int companyId);
    }
}