using System.Threading.Tasks;

namespace CRM.infrastructure.Services
{
    public interface ITenantDatabaseResolver
    {
        Task<TenantDatabaseInfo> GetDatabaseInfoAsync(int companyId);
    }
}