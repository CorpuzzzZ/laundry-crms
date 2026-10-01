using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Domain.DTOs.Terms;

namespace CRM.infrastructure.Services
{
    public interface ITermsService
    {
        Task<List<TermsDto>> GetAllAsync();
        Task<List<TermsDto>> GetCompanyTermsAsync(int companyId);
        Task<TermsDto?> GetByIdAsync(int id);
        Task<TermsDto> CreateAsync(CreateTermsDto dto, string createdBy);
        Task<TermsDto?> UpdateAsync(int id, UpdateTermsDto dto, string updatedBy);
        Task<bool> DeleteAsync(int id);
    }
}
