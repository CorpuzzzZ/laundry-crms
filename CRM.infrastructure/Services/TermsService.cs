using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CRM.Domain.DTOs.Terms;
using CRM.Domain.Entities.MasterDb;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public class TermsService : ITermsService
    {
        private readonly MasterErpDbContext _db;

        public TermsService(MasterErpDbContext db)
        {
            _db = db;
        }

        public async Task<List<TermsDto>> GetAllAsync()
        {
            var terms = await _db.TermsAndConditions
                .AsNoTracking()
                .Include(t => t.Company)
                .OrderByDescending(t => t.EffectiveDate)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();

            return terms.Select(t => new TermsDto
            {
                TermsId = t.TermsId,
                CompanyId = t.CompanyId,
                CompanyName = t.Company?.CompanyName,
                Version = t.Version,
                Title = t.Title,
                Content = t.Content,
                EffectiveDate = t.EffectiveDate,
                ExpiryDate = t.ExpiryDate,
                IsActive = t.IsActive,
                IsMandatory = t.IsMandatory,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            }).ToList();
        }

        public async Task<List<TermsDto>> GetCompanyTermsAsync(int companyId)
        {
            var terms = await _db.TermsAndConditions
                .AsNoTracking()
                .Include(t => t.Company)
                .Where(t => t.CompanyId == companyId || t.CompanyId == null)
                .OrderByDescending(t => t.EffectiveDate)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();

            return terms.Select(t => new TermsDto
            {
                TermsId = t.TermsId,
                CompanyId = t.CompanyId,
                CompanyName = t.Company?.CompanyName,
                Version = t.Version,
                Title = t.Title,
                Content = t.Content,
                EffectiveDate = t.EffectiveDate,
                ExpiryDate = t.ExpiryDate,
                IsActive = t.IsActive,
                IsMandatory = t.IsMandatory,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            }).ToList();
        }

        public async Task<TermsDto?> GetByIdAsync(int id)
        {
            var t = await _db.TermsAndConditions
                .AsNoTracking()
                .Include(x => x.Company)
                .FirstOrDefaultAsync(x => x.TermsId == id);

            if (t == null) return null;

            return new TermsDto
            {
                TermsId = t.TermsId,
                CompanyId = t.CompanyId,
                CompanyName = t.Company?.CompanyName,
                Version = t.Version,
                Title = t.Title,
                Content = t.Content,
                EffectiveDate = t.EffectiveDate,
                ExpiryDate = t.ExpiryDate,
                IsActive = t.IsActive,
                IsMandatory = t.IsMandatory,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            };
        }

        public async Task<TermsDto> CreateAsync(CreateTermsDto dto, string createdBy)
        {
            var t = new TermsAndConditions
            {
                Version = dto.Version.Trim(),
                Title = dto.Title.Trim(),
                Content = dto.Content.Trim(),
                EffectiveDate = dto.EffectiveDate,
                ExpiryDate = dto.ExpiryDate,
                IsActive = dto.IsActive,
                IsMandatory = dto.IsMandatory,
                CompanyId = dto.CompanyId,
                CreatedAt = DateTime.UtcNow
            };

            _db.TermsAndConditions.Add(t);
            await _db.SaveChangesAsync();

            return new TermsDto
            {
                TermsId = t.TermsId,
                CompanyId = t.CompanyId,
                Version = t.Version,
                Title = t.Title,
                Content = t.Content,
                EffectiveDate = t.EffectiveDate,
                ExpiryDate = t.ExpiryDate,
                IsActive = t.IsActive,
                IsMandatory = t.IsMandatory,
                CreatedAt = t.CreatedAt
            };
        }

        public async Task<TermsDto?> UpdateAsync(int id, UpdateTermsDto dto, string updatedBy)
        {
            var t = await _db.TermsAndConditions.FindAsync(id);
            if (t == null) return null;

            t.Version = dto.Version.Trim();
            t.Title = dto.Title.Trim();
            t.Content = dto.Content.Trim();
            t.EffectiveDate = dto.EffectiveDate;
            t.ExpiryDate = dto.ExpiryDate;
            t.IsActive = dto.IsActive;
            t.IsMandatory = dto.IsMandatory;
            t.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var t = await _db.TermsAndConditions.FindAsync(id);
            if (t == null) return false;

            _db.TermsAndConditions.Remove(t);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
