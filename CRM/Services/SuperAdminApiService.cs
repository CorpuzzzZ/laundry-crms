using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.WinForms.Config;
using CRM.WinForms.Models;

namespace CRM.WinForms.Services
{
    public class SuperAdminApiService
    {
        private readonly ApiClient _api;

        public SuperAdminApiService(ApiClient api)
        {
            _api = api;
        }

        // ============================================================
        // DASHBOARD
        // ============================================================
        public async Task<SuperAdminDashboardModel?> GetDashboardAsync()
        {
            try
            {
                var resp = await _api.GetDataAsync<SuperAdminDashboardEnvelope>(
                    AppConfig.ApiV1Url("superadmin/dashboard"));
                return resp?.Data;
            }
            catch
            {
                return null;
            }
        }

        // ============================================================
        // SUBSCRIPTION PLANS
        // ============================================================
        public async Task<List<SubscriptionPlanModel>> GetPlansAsync()
        {
            try
            {
                var resp = await _api.GetDataAsync<SubscriptionPlanListEnvelope>(
                    AppConfig.ApiV1Url("superadmin/subscriptions/plans"));
                return resp?.Data ?? new List<SubscriptionPlanModel>();
            }
            catch
            {
                return new List<SubscriptionPlanModel>();
            }
        }

        public async Task<SubscriptionPlanModel?> CreatePlanAsync(CreateSubscriptionPlanRequest req)
        {
            try
            {
                var resp = await _api.PostDataAsync<SubscriptionPlanEnvelope>(
                    AppConfig.ApiV1Url("superadmin/subscriptions/plans"), req);
                return resp?.Data;
            }
            catch
            {
                return null;
            }
        }

        public async Task<SubscriptionPlanModel?> UpdatePlanAsync(int planId, CreateSubscriptionPlanRequest req)
        {
            try
            {
                var resp = await _api.PutDataAsync<SubscriptionPlanEnvelope>(
                    AppConfig.ApiV1Url($"superadmin/subscriptions/plans/{planId}"), req);
                return resp?.Data;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeletePlanAsync(int planId)
        {
            try
            {
                return await _api.DeleteDataAsync(
                    AppConfig.ApiV1Url($"superadmin/subscriptions/plans/{planId}"));
            }
            catch
            {
                return false;
            }
        }

        // ============================================================
        // COMPANIES & SUBSCRIPTIONS
        // ============================================================
        public async Task<List<CompanySubscriptionModel>> GetCompaniesAsync()
        {
            try
            {
                var resp = await _api.GetDataAsync<CompanySubscriptionListEnvelope>(
                    AppConfig.ApiV1Url("superadmin/subscriptions/companies"));
                return resp?.Data ?? new List<CompanySubscriptionModel>();
            }
            catch
            {
                return new List<CompanySubscriptionModel>();
            }
        }

        public async Task<CompanySubscriptionModel?> CreateCompanyWithPlanAsync(CreateCompanyWithPlanRequest req)
        {
            try
            {
                var resp = await _api.PostDataAsync<CompanySubscriptionEnvelope>(
                    AppConfig.ApiV1Url("superadmin/subscriptions/companies"), req);
                return resp?.Data;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> AssignPlanAsync(AssignCompanyPlanRequest req)
        {
            try
            {
                var resp = await _api.PostDataAsync<CompanySubscriptionEnvelope>(
                    AppConfig.ApiV1Url("superadmin/subscriptions/companies/assign-plan"), req);
                return resp?.Success == true;
            }
            catch
            {
                return false;
            }
        }

        // ============================================================
        // TERMS AND CONDITIONS
        // ============================================================
        public async Task<List<TermsModel>> GetTermsAsync()
        {
            try
            {
                var resp = await _api.GetDataAsync<TermsListEnvelope>(
                    AppConfig.ApiV1Url("superadmin/terms"));
                return resp?.Data ?? new List<TermsModel>();
            }
            catch
            {
                return new List<TermsModel>();
            }
        }

        public async Task<TermsModel?> GetTermsByIdAsync(int id)
        {
            try
            {
                var resp = await _api.GetDataAsync<TermsEnvelope>(
                    AppConfig.ApiV1Url($"superadmin/terms/{id}"));
                return resp?.Data;
            }
            catch
            {
                return null;
            }
        }

        public async Task<TermsModel?> CreateTermsAsync(CreateTermsRequest req)
        {
            try
            {
                var resp = await _api.PostDataAsync<TermsEnvelope>(
                    AppConfig.ApiV1Url("superadmin/terms"), req);
                return resp?.Data;
            }
            catch
            {
                return null;
            }
        }

        public async Task<TermsModel?> UpdateTermsAsync(int id, UpdateTermsRequest req)
        {
            try
            {
                var resp = await _api.PutDataAsync<TermsEnvelope>(
                    AppConfig.ApiV1Url($"superadmin/terms/{id}"), req);
                return resp?.Data;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteTermsAsync(int id)
        {
            try
            {
                return await _api.DeleteDataAsync(
                    AppConfig.ApiV1Url($"superadmin/terms/{id}"));
            }
            catch
            {
                return false;
            }
        }

        // ============================================================
        // REPORTS
        // ============================================================
        public async Task<SuperAdminReportModel?> GetSubscriptionReportAsync(DateTime? from, DateTime? to)
        {
            try
            {
                var query = new List<string>();
                if (from.HasValue) query.Add($"from={Uri.EscapeDataString(from.Value.ToString("o"))}");
                if (to.HasValue) query.Add($"to={Uri.EscapeDataString(to.Value.ToString("o"))}");
                var url = AppConfig.ApiV1Url("superadmin/reports/subscriptions");
                if (query.Count > 0) url += "?" + string.Join("&", query);

                var resp = await _api.GetDataAsync<SuperAdminReportEnvelope>(url);
                return resp?.Data;
            }
            catch
            {
                return null;
            }
        }
    }
}
