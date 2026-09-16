using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CRM.Domain.DTOs.Customers;
using CRM.Domain.Entities.TenantDb;
using CRM.Domain.Enums;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public class CustomerInteractionService : ICustomerInteractionService
    {
        public async Task<List<CustomerInteractionDto>> GetByCustomerAsync(
            TenantErpDbContext db, CustomerInteractionFilterDto filter)
        {
            var query = db.CustomerInteractions
                .Include(i => i.Customer)
                .Where(i => i.CustomerId == filter.CustomerId)
                .AsNoTracking()
                .AsQueryable();

            if (filter.InteractionType.HasValue)
                query = query.Where(i => i.InteractionType == filter.InteractionType.Value);

            if (filter.Status.HasValue)
                query = query.Where(i => i.Status == filter.Status.Value);

            var rows = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
            return rows.Select(Map).ToList();
        }

        public async Task<CustomerInteractionDto?> GetByIdAsync(TenantErpDbContext db, int interactionId)
        {
            var row = await db.CustomerInteractions
                .Include(i => i.Customer)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.InteractionId == interactionId);

            return row == null ? null : Map(row);
        }

        public async Task<CustomerInteractionDto> CreateAsync(
            TenantErpDbContext db, CreateCustomerInteractionDto dto, string userId)
        {
            var customer = await db.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId)
                ?? throw new InvalidOperationException("Customer " + dto.CustomerId + " not found.");

            var entity = new CustomerInteraction
            {
                CustomerId = dto.CustomerId,
                InteractionType = dto.InteractionType,
                Subject = dto.Subject ?? "",
                Description = dto.Description ?? "",
                Status = CustomerInteractionStatus.Open,
                Priority = dto.Priority,
                Rating = dto.Rating,
                AssignedToUserId = dto.AssignedToUserId,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            db.CustomerInteractions.Add(entity);
            await db.SaveChangesAsync();

            db.CustomerInteractionStatusHistories.Add(new CustomerInteractionStatusHistory
            {
                InteractionId = entity.InteractionId,
                FromStatus = null,
                ToStatus = CustomerInteractionStatus.Open,
                ChangedByUserId = userId,
                ChangedAt = DateTime.UtcNow,
                Notes = "Interaction created"
            });
            await db.SaveChangesAsync();

            entity.Customer = customer;
            return Map(entity);
        }

        public async Task<CustomerInteractionDto?> UpdateAsync(
            TenantErpDbContext db, int interactionId, UpdateCustomerInteractionDto dto, string userId)
        {
            var entity = await db.CustomerInteractions
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.InteractionId == interactionId);

            if (entity == null) return null;

            if (dto.Subject != null) entity.Subject = dto.Subject;
            if (dto.Description != null) entity.Description = dto.Description;
            if (dto.Priority.HasValue) entity.Priority = dto.Priority.Value;
            if (dto.Rating.HasValue) entity.Rating = dto.Rating;
            if (dto.AssignedToUserId != null) entity.AssignedToUserId = dto.AssignedToUserId;
            if (dto.ResolutionNotes != null) entity.ResolutionNotes = dto.ResolutionNotes;
            if (dto.CustomerResponse != null) entity.CustomerResponse = dto.CustomerResponse;

            entity.UpdatedByUserId = userId;
            entity.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<CustomerInteractionDto?> ChangeStatusAsync(
            TenantErpDbContext db, int interactionId, ChangeInteractionStatusDto dto, string userId)
        {
            var entity = await db.CustomerInteractions
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.InteractionId == interactionId);

            if (entity == null) return null;

            var oldStatus = entity.Status;
            entity.Status = dto.NewStatus;
            entity.UpdatedByUserId = userId;
            entity.UpdatedAt = DateTime.UtcNow;

            if (dto.NewStatus == CustomerInteractionStatus.Resolved && entity.ResolvedAt == null)
                entity.ResolvedAt = DateTime.UtcNow;

            if (dto.NewStatus == CustomerInteractionStatus.Closed && entity.ClosedAt == null)
                entity.ClosedAt = DateTime.UtcNow;

            db.CustomerInteractionStatusHistories.Add(new CustomerInteractionStatusHistory
            {
                InteractionId = interactionId,
                FromStatus = oldStatus,
                ToStatus = dto.NewStatus,
                ChangedByUserId = userId,
                ChangedAt = DateTime.UtcNow,
                Notes = dto.Notes
            });

            await db.SaveChangesAsync();
            return Map(entity);
        }
        public async Task<List<CustomerInteractionStatusHistoryDto>> GetStatusHistoryAsync(
            TenantErpDbContext db, int interactionId)
        {
            var rows = await db.CustomerInteractionStatusHistories
                .Where(h => h.InteractionId == interactionId)
                .OrderBy(h => h.ChangedAt)
                .AsNoTracking()
                .ToListAsync();

            return rows.Select(h => new CustomerInteractionStatusHistoryDto
            {
                StatusHistoryId = h.StatusHistoryId,
                FromStatus = h.FromStatus,
                FromStatusName = h.FromStatus.HasValue ? h.FromStatus.Value.ToString() : "",
                ToStatus = h.ToStatus,
                ToStatusName = h.ToStatus.ToString(),
                ChangedByUserId = h.ChangedByUserId,
                ChangedAt = h.ChangedAt,
                Notes = h.Notes
            }).ToList();
        }

        public async Task<bool> DeleteAsync(TenantErpDbContext db, int interactionId)
        {
            var entity = await db.CustomerInteractions
                .FirstOrDefaultAsync(i => i.InteractionId == interactionId);

            if (entity == null) return false;

            db.CustomerInteractions.Remove(entity);
            await db.SaveChangesAsync();
            return true;
        }

        private static CustomerInteractionDto Map(CustomerInteraction i)
        {
            return new CustomerInteractionDto
            {
                InteractionId = i.InteractionId,
                CustomerId = i.CustomerId,
                CustomerName = i.Customer != null
                    ? (i.Customer.FirstName + " " + i.Customer.LastName).Trim()
                    : "",
                InteractionType = i.InteractionType,
                InteractionTypeName = i.InteractionType.ToString(),
                Subject = i.Subject,
                Description = i.Description,
                Status = i.Status,
                StatusName = i.Status.ToString(),
                Priority = i.Priority,
                PriorityName = i.Priority.ToString(),
                Rating = i.Rating,
                AssignedToUserId = i.AssignedToUserId,
                CreatedByUserId = i.CreatedByUserId,
                UpdatedByUserId = i.UpdatedByUserId,
                ResolutionNotes = i.ResolutionNotes,
                CustomerResponse = i.CustomerResponse,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                ResolvedAt = i.ResolvedAt,
                ClosedAt = i.ClosedAt
            };
        }
    }
}