using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.WinForms.Config;
using CRM.WinForms.Models;

namespace CRM.WinForms.Services
{
    public class TenantApiService
    {
        private readonly ApiClient _api;

        public TenantApiService(ApiClient api)
        {
            _api = api;
        }

        // ============================================================
        // SUBSCRIPTION: READ-ONLY AVAILED PLAN
        // ============================================================
        public async Task<AvailedSubscriptionModel?> GetMyAvailedSubscriptionAsync()
        {
            try
            {
                var resp = await _api.GetDataAsync<AvailedSubscriptionEnvelope>(
                    AppConfig.ApiV1Url("tenant/subscription/my-plan"));
                if (resp?.Data != null)
                {
                    resp.Data.ApplyPlanFeatures();
                }
                return resp?.Data;
            }
            catch
            {
                return null;
            }
        }

        // ============================================================
        // TERMS & CONDITIONS: TENANT LEVEL
        // ============================================================
        public async Task<List<TermsModel>> GetTenantTermsAsync()
        {
            try
            {
                var resp = await _api.GetDataAsync<TermsListEnvelope>(
                    AppConfig.ApiV1Url("tenant/terms"));
                return resp?.Data ?? new List<TermsModel>();
            }
            catch
            {
                return new List<TermsModel>();
            }
        }

        public async Task<TermsModel?> GetTenantTermsByIdAsync(int id)
        {
            try
            {
                var resp = await _api.GetDataAsync<TermsEnvelope>(
                    AppConfig.ApiV1Url($"tenant/terms/{id}"));
                return resp?.Data;
            }
            catch
            {
                return null;
            }
        }

        public async Task<TermsModel?> CreateTenantTermsAsync(CreateTermsRequest req)
        {
            try
            {
                var resp = await _api.PostDataAsync<TermsEnvelope>(
                    AppConfig.ApiV1Url("tenant/terms"), req);
                return resp?.Data;
            }
            catch
            {
                return null;
            }
        }

        public async Task<TermsModel?> UpdateTenantTermsAsync(int id, UpdateTermsRequest req)
        {
            try
            {
                var resp = await _api.PutDataAsync<TermsEnvelope>(
                    AppConfig.ApiV1Url($"tenant/terms/{id}"), req);
                return resp?.Data;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteTenantTermsAsync(int id)
        {
            try
            {
                return await _api.DeleteDataAsync(
                    AppConfig.ApiV1Url($"tenant/terms/{id}"));
            }
            catch
            {
                return false;
            }
        }
    }
}
