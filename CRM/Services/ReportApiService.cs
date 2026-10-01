using System;
using System.Threading.Tasks;
using CRM.WinForms.Config;
using CRM.WinForms.Models;

namespace CRM.WinForms.Services
{
    public class ReportApiService
    {
        private readonly ApiClient _api;

        public ReportApiService(ApiClient api)
        {
            _api = api;
        }

        private static string Range(DateTime fromUtc, DateTime toUtc, int? branchId = null)
        {
            var f = Uri.EscapeDataString(fromUtc.ToString("o"));
            var t = Uri.EscapeDataString(toUtc.ToString("o"));
            var q = $"from={f}&to={t}";
            if (branchId.HasValue && branchId.Value > 0)
            {
                q += $"&branchId={branchId.Value}";
            }
            return q;
        }

        public async Task<DailySalesReportModel?> GetDailySalesAsync(DateTime from, DateTime to, int? branchId = null)
        {
            try
            {
                var url = AppConfig.ApiV1Url($"reports/daily-sales?{Range(from, to, branchId)}");
                var r = await _api.GetDataAsync<DailySalesEnvelope>(url);
                return r?.Data;
            }
            catch { return null; }
        }

        public async Task<SalesByServiceReportModel?> GetSalesByServiceAsync(DateTime from, DateTime to, int? branchId = null)
        {
            try
            {
                var url = AppConfig.ApiV1Url($"reports/sales-by-service?{Range(from, to, branchId)}");
                var r = await _api.GetDataAsync<SalesByServiceEnvelope>(url);
                return r?.Data;
            }
            catch { return null; }
        }

        public async Task<SalesByPaymentMethodReportModel?> GetSalesByPaymentMethodAsync(DateTime from, DateTime to, int? branchId = null)
        {
            try
            {
                var url = AppConfig.ApiV1Url($"reports/sales-by-payment-method?{Range(from, to, branchId)}");
                var r = await _api.GetDataAsync<SalesByPaymentMethodEnvelope>(url);
                return r?.Data;
            }
            catch { return null; }
        }

        public async Task<OrderStatusReportModel?> GetOrderStatusAsync(DateTime from, DateTime to, int? branchId = null)
        {
            try
            {
                var url = AppConfig.ApiV1Url($"reports/order-status?{Range(from, to, branchId)}");
                var r = await _api.GetDataAsync<OrderStatusEnvelope>(url);
                return r?.Data;
            }
            catch { return null; }
        }

        public async Task<PeakHoursReportModel?> GetPeakHoursAsync(DateTime from, DateTime to, int? branchId = null)
        {
            try
            {
                var url = AppConfig.ApiV1Url($"reports/peak-hours?{Range(from, to, branchId)}");
                var r = await _api.GetDataAsync<PeakHoursEnvelope>(url);
                return r?.Data;
            }
            catch { return null; }
        }
    }
}
