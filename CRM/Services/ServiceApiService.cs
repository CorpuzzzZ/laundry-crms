using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.WinForms.Config;
using CRM.WinForms.Models;

namespace CRM.WinForms.Services
{
    public class ServiceApiService
    {
        private readonly ApiClient _api;

        public ServiceApiService(ApiClient api)
        {
            _api = api;
        }

        // ══════════════════════════════════════════════════════
        // GET ALL
        // ══════════════════════════════════════════════════════
        public async Task<List<ServiceModel>> GetAllAsync(
            int? serviceType = null,
            int? categoryId = null,
            bool? isActive = null,
            bool? isExpress = null,
            string? search = null,
            bool includeArchived = false)
        {
            var url = AppConfig.ApiV1Url("services");
            var q = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) q.Add($"searchTerm={Uri.EscapeDataString(search)}");
            if (serviceType.HasValue) q.Add($"serviceType={serviceType.Value}");
            if (categoryId.HasValue) q.Add($"serviceCategoryId={categoryId.Value}");
            if (isActive.HasValue) q.Add($"isActive={isActive.Value.ToString().ToLower()}");
            if (isExpress.HasValue) q.Add($"isExpressService={isExpress.Value.ToString().ToLower()}");
            if (includeArchived) q.Add("includeArchived=true");
            if (q.Count > 0) url += "?" + string.Join("&", q);

            var response = await _api.GetDataAsync<ServiceListResponse>(url);
            return response?.Data ?? new List<ServiceModel>();
        }

        // ══════════════════════════════════════════════════════
        // GET BY ID
        // ══════════════════════════════════════════════════════
        public async Task<ServiceModel?> GetByIdAsync(int serviceId)
        {
            var response = await _api.GetDataAsync<ServiceResponse>(
                AppConfig.ApiV1Url($"services/{serviceId}"));
            return response?.Data;
        }

        // ══════════════════════════════════════════════════════
        // CREATE
        // ══════════════════════════════════════════════════════
        public async Task<ServiceModel?> CreateAsync(CreateServiceRequest request)
        {
            var response = await _api.PostDataAsync<ServiceResponse>(
                AppConfig.ApiV1Url("services"), request);
            return response?.Data;
        }

        // ══════════════════════════════════════════════════════
        // UPDATE
        // ══════════════════════════════════════════════════════
        public async Task<ServiceModel?> UpdateAsync(int serviceId, UpdateServiceRequest request)
        {
            var response = await _api.PutDataAsync<ServiceResponse>(
                AppConfig.ApiV1Url($"services/{serviceId}"), request);
            return response?.Data;
        }

        // ══════════════════════════════════════════════════════
        // ARCHIVE (DELETE)
        // ══════════════════════════════════════════════════════
        public async Task<bool> ArchiveAsync(int serviceId)
        {
            return await _api.DeleteDataAsync(
                AppConfig.ApiV1Url($"services/{serviceId}"));
        }

        // ══════════════════════════════════════════════════════
        // RESTORE
        // ══════════════════════════════════════════════════════
        public async Task<ServiceModel?> RestoreAsync(int serviceId)
        {
            var response = await _api.PostDataAsync<ServiceResponse>(
                AppConfig.ApiV1Url($"services/{serviceId}/restore"), new { });
            return response?.Data;
        }

        // ══════════════════════════════════════════════════════
        // ADD-ONS
        // ══════════════════════════════════════════════════════
        public async Task<ServiceAddOnModel?> AddAddOnAsync(int serviceId, CreateServiceAddOnRequest request)
        {
            var response = await _api.PostDataAsync<ServiceAddOnResponse>(
                AppConfig.ApiV1Url($"services/{serviceId}/addons"), request);
            return response?.Data;
        }

        public async Task<ServiceAddOnModel?> UpdateAddOnAsync(int serviceId, int addOnId, UpdateServiceAddOnRequest request)
        {
            var response = await _api.PutDataAsync<ServiceAddOnResponse>(
                AppConfig.ApiV1Url($"services/{serviceId}/addons/{addOnId}"), request);
            return response?.Data;
        }

        public async Task<bool> DeleteAddOnAsync(int serviceId, int addOnId)
        {
            return await _api.DeleteDataAsync(
                AppConfig.ApiV1Url($"services/{serviceId}/addons/{addOnId}"));
        }

        // ══════════════════════════════════════════════════════
        // CATEGORIES
        // ══════════════════════════════════════════════════════
        public async Task<List<ServiceCategoryModel>> GetCategoriesAsync()
        {
            var response = await _api.GetDataAsync<ServiceCategoryListResponse>(
                AppConfig.ApiV1Url("services/categories"));
            return response?.Data ?? new List<ServiceCategoryModel>();
        }
    }
}