using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CRM.WinForms.Config;
using CRM.WinForms.Models;

namespace CRM.WinForms.Services
{
    public class CustomerInteractionApiService
    {
        private readonly ApiClient _api;
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CustomerInteractionApiService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<CustomerInteractionModel>> GetForCustomerAsync(int customerId)
        {
            try
            {
                var response = await _api.GetDataAsync<CustomerInteractionListResponse>(
                    AppConfig.ApiV1Url($"customers/{customerId}/interactions"));

                return response?.Data ?? new List<CustomerInteractionModel>();
            }
            catch
            {
                return new List<CustomerInteractionModel>();
            }
        }

        public async Task<CustomerInteractionModel?> CreateAsync(CreateCustomerInteractionRequest req)
        {
            var response = await _api.PostDataAsync<CustomerInteractionModel>(
                AppConfig.ApiV1Url($"customers/{req.CustomerId}/interactions"), req);
            return response;
        }

        public async Task<CustomerInteractionModel?> ChangeStatusAsync(
            int customerId, int interactionId, ChangeInteractionStatusRequest req)
        {
            var response = await _api.PostDataAsync<CustomerInteractionModel>(
                AppConfig.ApiV1Url($"customers/{customerId}/interactions/{interactionId}/status"), req);
            return response;
        }

        public async Task<List<InteractionStatusHistoryModel>> GetHistoryAsync(
            int customerId, int interactionId)
        {
            try
            {
                var response = await _api.GetDataAsync<List<InteractionStatusHistoryModel>>(
                    AppConfig.ApiV1Url($"customers/{customerId}/interactions/{interactionId}/history"));
                return response ?? new List<InteractionStatusHistoryModel>();
            }
            catch
            {
                return new List<InteractionStatusHistoryModel>();
            }
        }

        public async Task<bool> DeleteAsync(int customerId, int interactionId)
        {
            try
            {
                await _api.DeleteDataAsync(                    AppConfig.ApiV1Url($"customers/{customerId}/interactions/{interactionId}"));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}