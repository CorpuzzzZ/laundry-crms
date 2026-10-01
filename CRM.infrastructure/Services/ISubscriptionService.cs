using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Domain.DTOs.Subscriptions;

namespace CRM.infrastructure.Services
{
    public interface ISubscriptionService
    {
        Task<List<SubscriptionPlanDto>> GetPlansAsync();
        Task<SubscriptionPlanDto?> GetPlanByIdAsync(int planId);
        Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanDto dto);
        Task<SubscriptionPlanDto?> UpdatePlanAsync(int planId, CreateSubscriptionPlanDto dto);
        Task<bool> DeletePlanAsync(int planId);
        Task<List<CompanySubscriptionDto>> GetCompaniesAsync();
        Task<CompanySubscriptionDto> CreateCompanyWithPlanAsync(CreateCompanyWithPlanDto dto, string createdBy);
        Task<bool> AssignPlanAsync(AssignCompanyPlanDto dto, string updatedBy);
        Task<AvailedSubscriptionDto?> GetAvailedSubscriptionAsync(int companyId);
    }
}
