using System;
using System.Threading.Tasks;
using CRM.Domain.DTOs.SuperAdmin;

namespace CRM.infrastructure.Services
{
    public interface ISuperAdminReportService
    {
        Task<SuperAdminSubscriptionReportDto> GetSubscriptionReportAsync(DateTime? from, DateTime? to);
    }
}
