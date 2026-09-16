using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CRM.Domain.DTOs.Customers;
using CRM.Domain.Entities.TenantDb;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        // ============================================================
        // GENERATE CUSTOMER CODE
        // ============================================================
        public async Task<string> GenerateCustomerCodeAsync(TenantErpDbContext db)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"CUST-{year}-";

            var lastCustomer = await db.Customers
                .Where(c => c.CustomerCode.StartsWith(prefix))
                .OrderByDescending(c => c.CustomerId)
                .Select(c => c.CustomerCode)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCustomer))
            {
                var parts = lastCustomer.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var lastNumber))
                    nextNumber = lastNumber + 1;
            }

            return $"{prefix}{nextNumber:D4}";
        }

        // ============================================================
        // GET CUSTOMERS (paged, filtered)
        // ============================================================
        public async Task<(List<CustomerDto> Customers, int TotalCount)> GetCustomersAsync(
            TenantErpDbContext db, CustomerFilterDto filter)
        {
            var query = db.Customers.AsNoTracking().AsQueryable();

            // Filters
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(c =>
                    c.CustomerCode.Contains(term) ||
                    c.FirstName.Contains(term) ||
                    c.LastName.Contains(term) ||
                    c.PhonePrimary.Contains(term) ||
                    (c.Email != null && c.Email.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(filter.CustomerType))
                query = query.Where(c => c.CustomerType == filter.CustomerType);

            if (filter.IsActive.HasValue)
                query = query.Where(c => c.IsActive == filter.IsActive.Value);

            // Hide archived by default
            if (!filter.IncludeArchived)
                query = query.Where(c => !c.IsArchived);

            if (filter.HasLoyaltyPoints == true)
                query = query.Where(c => c.LoyaltyPoints > 0);

            if (filter.DateFrom.HasValue)
                query = query.Where(c => c.CreatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(c => c.CreatedAt <= filter.DateTo.Value);

            var totalCount = await query.CountAsync();

            // Sorting
            query = filter.SortBy?.ToLower() switch
            {
                "firstname" => filter.SortDirection == "ASC"
                    ? query.OrderBy(c => c.FirstName)
                    : query.OrderByDescending(c => c.FirstName),
                "lastname" => filter.SortDirection == "ASC"
                    ? query.OrderBy(c => c.LastName)
                    : query.OrderByDescending(c => c.LastName),
                "loyaltypoints" => filter.SortDirection == "ASC"
                    ? query.OrderBy(c => c.LoyaltyPoints)
                    : query.OrderByDescending(c => c.LoyaltyPoints),
                "totalorders" => filter.SortDirection == "ASC"
                    ? query.OrderBy(c => c.TotalOrders)
                    : query.OrderByDescending(c => c.TotalOrders),
                _ => filter.SortDirection == "ASC"
                    ? query.OrderBy(c => c.CreatedAt)
                    : query.OrderByDescending(c => c.CreatedAt)
            };

            var customers = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (customers.Select(MapToDto).ToList(), totalCount);
        }

        // ============================================================
        // GET BY ID
        // ============================================================
        public async Task<CustomerDto?> GetCustomerByIdAsync(TenantErpDbContext db, int customerId)
        {
            var customer = await db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            return customer == null ? null : MapToDto(customer);
        }

        // ============================================================
        // CREATE
        // ============================================================
        public async Task<CustomerDto> CreateCustomerAsync(
            TenantErpDbContext db, CreateCustomerDto dto)
        {
            // Check for duplicate phone
            var existing = await db.Customers
                .FirstOrDefaultAsync(c => c.PhonePrimary == dto.PhonePrimary);

            if (existing != null)
                throw new InvalidOperationException(
                    $"A customer with phone '{dto.PhonePrimary}' already exists.");

            var customerCode = await GenerateCustomerCodeAsync(db);

            var customer = new Customer
            {
                CustomerCode = customerCode,
                CustomerType = dto.CustomerType,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = dto.Email?.Trim(),
                PhonePrimary = dto.PhonePrimary.Trim(),
                Street = dto.Street?.Trim(),
                Village = dto.Village?.Trim(),
                City = dto.City?.Trim(),
                State = dto.State?.Trim(),
                PostalCode = dto.PostalCode?.Trim(),
                Country = dto.Country?.Trim(),
                Notes = dto.Notes?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Customers.Add(customer);
            await db.SaveChangesAsync();

            return MapToDto(customer);
        }

        // ============================================================
        // UPDATE
        // ============================================================
        public async Task<CustomerDto?> UpdateCustomerAsync(
            TenantErpDbContext db, int customerId, UpdateCustomerDto dto)
        {
            var customer = await db.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null) return null;

            // Check duplicate phone (excluding self)
            var duplicate = await db.Customers
                .FirstOrDefaultAsync(c =>
                    c.PhonePrimary == dto.PhonePrimary &&
                    c.CustomerId != customerId);

            if (duplicate != null)
                throw new InvalidOperationException(
                    $"Another customer already has phone '{dto.PhonePrimary}'.");

            customer.FirstName = dto.FirstName.Trim();
            customer.LastName = dto.LastName.Trim();
            customer.Email = dto.Email?.Trim();
            customer.PhonePrimary = dto.PhonePrimary.Trim();
            customer.CustomerType = dto.CustomerType;
            customer.Street = dto.Street?.Trim();
            customer.Village = dto.Village?.Trim();
            customer.City = dto.City?.Trim();
            customer.State = dto.State?.Trim();
            customer.PostalCode = dto.PostalCode?.Trim();
            customer.Country = dto.Country?.Trim();
            customer.Notes = dto.Notes?.Trim();
            customer.IsActive = dto.IsActive;
            customer.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return MapToDto(customer);
        }

        // ============================================================
        // DELETE (soft)
        // ============================================================
        public async Task<bool> DeleteCustomerAsync(TenantErpDbContext db, int customerId, string? currentUserId = null)
        {
            var customer = await db.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null) return false;

            // Archive instead of hard delete
            customer.IsArchived = true;
            customer.ArchivedAt = DateTime.UtcNow;
            customer.ArchivedBy = currentUserId;
            customer.IsActive = false;
            customer.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return true;
        }

        // ============================================================
        // RESTORE — unarchive a customer
        // ============================================================
        public async Task<CustomerDto?> RestoreAsync(TenantErpDbContext db, int customerId)
        {
            var customer = await db.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null) return null;

            customer.IsArchived = false;
            customer.ArchivedAt = null;
            customer.ArchivedBy = null;
            customer.IsActive = true;
            customer.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return MapToDto(customer);
        }

        // ============================================================
        // LOOKUP (for order dropdown)
        // ============================================================
        public async Task<List<CustomerDto>> GetCustomerLookupAsync(TenantErpDbContext db)
        {
            var customers = await db.Customers
                .Where(c => c.IsActive && !c.IsArchived)
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .Take(500)
                .AsNoTracking()
                .ToListAsync();

            return customers.Select(MapToDto).ToList();
        }

        // ============================================================
        // MAPPER
        // ============================================================
        private static CustomerDto MapToDto(Customer c) => new()
        {
            CustomerId = c.CustomerId,
            CustomerCode = c.CustomerCode,
            CustomerType = c.CustomerType,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email,
            PhonePrimary = c.PhonePrimary,
            Street = c.Street,
            Village = c.Village,
            City = c.City,
            State = c.State,
            PostalCode = c.PostalCode,
            Country = c.Country,
            Notes = c.Notes,
            LoyaltyPoints = c.LoyaltyPoints,
            LifetimeSpend = c.LifetimeSpend,
            TotalOrders = c.TotalOrders,
            LastOrderDate = c.LastOrderDate,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            IsArchived = c.IsArchived,
            ArchivedAt = c.ArchivedAt
        };
    }
}