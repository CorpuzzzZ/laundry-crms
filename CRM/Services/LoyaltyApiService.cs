using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.WinForms.Config;
using CRM.WinForms.Models;

namespace CRM.WinForms.Services
{
    public class LoyaltyApiService
    {
        private readonly ApiClient _api;

        public LoyaltyApiService(ApiClient api)
        {
            _api = api;
        }

        // ----- SETTINGS -----
        public async Task<LoyaltySettingModel?> GetSettingsAsync()
        {
            var resp = await _api.GetDataAsync<LoyaltySettingEnvelope>(
                AppConfig.ApiV1Url("loyalty/settings"));
            return resp?.Data;
        }

        public async Task<LoyaltySettingModel?> UpdateSettingsAsync(UpdateLoyaltySettingRequest req)
        {
            var resp = await _api.PutDataAsync<LoyaltySettingEnvelope>(
                AppConfig.ApiV1Url("loyalty/settings"), req);
            return resp?.Data;
        }

        // ----- TIERS -----
        public async Task<List<LoyaltyTierModel>> GetTiersAsync()
        {
            try
            {
                var resp = await _api.GetDataAsync<LoyaltyTierListEnvelope>(
                    AppConfig.ApiV1Url("loyalty/tiers"));
                return resp?.Data ?? new List<LoyaltyTierModel>();
            }
            catch
            {
                return new List<LoyaltyTierModel>();
            }
        }

        public async Task<LoyaltyTierModel?> CreateTierAsync(CreateLoyaltyTierRequest req)
        {
            var resp = await _api.PostDataAsync<LoyaltyTierEnvelope>(
                AppConfig.ApiV1Url("loyalty/tiers"), req);
            return resp?.Data;
        }

        public async Task<LoyaltyTierModel?> UpdateTierAsync(int id, UpdateLoyaltyTierRequest req)
        {
            var resp = await _api.PutDataAsync<LoyaltyTierEnvelope>(
                AppConfig.ApiV1Url($"loyalty/tiers/{id}"), req);
            return resp?.Data;
        }

        public Task<bool> DeleteTierAsync(int id)
            => _api.DeleteDataAsync(AppConfig.ApiV1Url($"loyalty/tiers/{id}"));

        // ----- CUSTOMERS -----
        public async Task<System.Collections.Generic.List<LoyaltyCustomerModel>> GetCustomersAsync()
        {
            try
            {
                var resp = await _api.GetDataAsync<LoyaltyCustomerListEnvelope>(
                    AppConfig.ApiV1Url("loyalty/customers"));
                return resp?.Data ?? new System.Collections.Generic.List<LoyaltyCustomerModel>();
            }
            catch
            {
                return new System.Collections.Generic.List<LoyaltyCustomerModel>();
            }
        }

        public async Task<RedeemableInfoModel?> GetRedeemableInfoAsync(int customerId, decimal orderTotal)
        {
            try
            {
                var url = AppConfig.ApiV1Url(
                    $"loyalty/customers/{customerId}/redeemable?orderTotal={orderTotal}");
                var resp = await _api.GetDataAsync<RedeemableInfoEnvelope>(url);
                return resp?.Data;
            }
            catch
            {
                return null;
            }
        }

        public async Task<LoyaltyCustomerModel?> GetCustomerSummaryAsync(int customerId)
        {
            try
            {
                var resp = await _api.GetDataAsync<LoyaltyCustomerEnvelope>(
                    AppConfig.ApiV1Url($"loyalty/customers/{customerId}/summary"));
                return resp?.Data;
            }
            catch
            {
                return null;
            }
        }

        public async Task<System.Collections.Generic.List<LoyaltyTransactionModel>> GetCustomerTransactionsAsync(int customerId)
        {
            try
            {
                var resp = await _api.GetDataAsync<LoyaltyTransactionListEnvelope>(
                    AppConfig.ApiV1Url($"loyalty/customers/{customerId}/transactions"));
                return resp?.Data ?? new System.Collections.Generic.List<LoyaltyTransactionModel>();
            }
            catch
            {
                return new System.Collections.Generic.List<LoyaltyTransactionModel>();
            }
        }
    }
}



