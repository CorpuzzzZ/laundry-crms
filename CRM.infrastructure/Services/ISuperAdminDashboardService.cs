using System.Threading.Tasks;
using CRM.Domain.DTOs.SuperAdmin;

namespace CRM.infrastructure.Services
{
    public interface ISuperAdminDashboardService
    {
        Task<SuperAdminDashboardDto> GetDashboardDataAsync();
    }
}
