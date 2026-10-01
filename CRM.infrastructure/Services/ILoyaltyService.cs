using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Domain.DTOs.Loyalty;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public interface ILoyaltyService
    {
        Task<LoyaltySettingDto?> GetSettingsAsync(TenantErpDbContext db);
        Task<LoyaltySettingDto?> UpdateSettingsAsync(
            TenantErpDbContext db, UpdateLoyaltySettingDto dto, string? userId);

        Task<List<LoyaltyTierDto>> GetTiersAsync(TenantErpDbContext db);
        Task<LoyaltyTierDto?> GetTierByIdAsync(TenantErpDbContext db, int tierId);
        Task<LoyaltyTierDto> CreateTierAsync(TenantErpDbContext db, CreateLoyaltyTierDto dto);
        Task<LoyaltyTierDto?> UpdateTierAsync(
            TenantErpDbContext db, int tierId, UpdateLoyaltyTierDto dto);
        Task<bool> DeleteTierAsync(TenantErpDbContext db, int tierId);

        // Customers with loyalty data
        Task<List<LoyaltyCustomerDto>> GetCustomersAsync(TenantErpDbContext db);
        Task<List<LoyaltyTransactionDto>> GetCustomerTransactionsAsync(
            TenantErpDbContext db, int customerId);
        Task<LoyaltyCustomerDto?> GetCustomerSummaryAsync(TenantErpDbContext db, int customerId);

        // Earn points when an order is completed (called on Picked Up)
        Task EvaluateEarningAsync(TenantErpDbContext db, int orderId, string? userId);

        // Redeem points at payment time
        Task<RedeemableInfoDto?> GetRedeemableInfoAsync(
            TenantErpDbContext db, int customerId, decimal orderTotal);

        Task<int> RedeemPointsAsync(
            TenantErpDbContext db, int customerId, int orderId,
            int pointsRequested, string? userId);
    }
}



