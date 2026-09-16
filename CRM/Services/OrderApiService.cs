using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.WinForms.Config;
using CRM.WinForms.Models;

namespace CRM.WinForms.Services
{
    public class OrderApiService
    {
        public async Task<ApiListResponse<OrderModel>> GetOrdersAsync(
            int page = 1,
            int pageSize = 20,
            string? search = null,
            string? statusCode = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            var url = AppConfig.ApiV1Url($"orders?page={page}&pageSize={pageSize}");

            if (!string.IsNullOrWhiteSpace(search))
                url += $"&searchTerm={Uri.EscapeDataString(search)}";
            if (!string.IsNullOrWhiteSpace(statusCode))
                url += $"&statusCode={statusCode}";
            if (dateFrom.HasValue)
                url += $"&dateFrom={dateFrom.Value:yyyy-MM-dd}";
            if (dateTo.HasValue)
                url += $"&dateTo={dateTo.Value:yyyy-MM-dd}";

            var response = await ApiClient.Instance.GetAsync<ApiListResponse<OrderModel>>(url);
            return response.Data ?? new ApiListResponse<OrderModel>();
        }

        public async Task<OrderModel?> GetOrderByIdAsync(int orderId)
        {
            var url = AppConfig.ApiV1Url($"orders/{orderId}");
            var response = await ApiClient.Instance.GetAsync<ApiSingleResponse<OrderModel>>(url);
            return response.Success ? response.Data?.Data : null;
        }

        public async Task<(bool Success, OrderModel? Order, string? Error)> CreateOrderAsync(CreateOrderRequest request)
        {
            var url = AppConfig.ApiV1Url("orders");
            var response = await ApiClient.Instance.PostAsync<ApiSingleResponse<OrderModel>>(url, request);
            if (response.Success && response.Data != null && response.Data.Success)
                return (true, response.Data.Data, null);
            return (false, null, response.Data?.Message ?? response.ErrorMessage ?? "Failed to create order");
        }

        public async Task<(bool Success, OrderModel? Order, string? Error)> UpdateOrderAsync(int orderId, UpdateOrderRequest request)
        {
            var url = AppConfig.ApiV1Url($"orders/{orderId}");
            var response = await ApiClient.Instance.PutAsync<ApiSingleResponse<OrderModel>>(url, request);
            if (response.Success && response.Data != null && response.Data.Success)
                return (true, response.Data.Data, null);
            return (false, null, response.Data?.Message ?? response.ErrorMessage ?? "Failed to update order");
        }

        public async Task<(bool Success, string? Error)> DeleteOrderAsync(int orderId)
        {
            var url = AppConfig.ApiV1Url($"orders/{orderId}");
            var response = await ApiClient.Instance.DeleteAsync<object>(url);
            return (response.Success, response.Success ? null : response.ErrorMessage);
        }

        public async Task<(bool Success, OrderModel? Order, string? Error)> ChangeStatusAsync(int orderId, OrderStatusChangeRequest request)
        {
            var url = AppConfig.ApiV1Url($"orders/{orderId}/status");
            var response = await ApiClient.Instance.PostAsync<ApiSingleResponse<OrderModel>>(url, request);
            if (response.Success && response.Data != null && response.Data.Success)
                return (true, response.Data.Data, null);
            return (false, null, response.Data?.Message ?? response.ErrorMessage ?? "Failed to change status");
        }

        public async Task<(bool Success, OrderPaymentModel? Payment, string? Error)> AddPaymentAsync(int orderId, CreateOrderPaymentRequest request)
        {
            var url = AppConfig.ApiV1Url($"orders/{orderId}/payments");
            var response = await ApiClient.Instance.PostAsync<ApiSingleResponse<OrderPaymentModel>>(url, request);
            if (response.Success && response.Data != null && response.Data.Success)
                return (true, response.Data.Data, null);
            return (false, null, response.Data?.Message ?? response.ErrorMessage ?? "Failed to add payment");
        }

        // Lookups
        public async Task<List<OrderStatusLookup>> GetStatusesAsync()
        {
            var url = AppConfig.ApiV1Url("orders/lookups/statuses");
            var response = await ApiClient.Instance.GetAsync<ApiSingleResponse<List<OrderStatusLookup>>>(url);
            return response.Data?.Data ?? new List<OrderStatusLookup>();
        }

        public async Task<List<PaymentMethodLookup>> GetPaymentMethodsAsync()
        {
            var url = AppConfig.ApiV1Url("orders/lookups/payment-methods");
            var response = await ApiClient.Instance.GetAsync<ApiSingleResponse<List<PaymentMethodLookup>>>(url);
            return response.Data?.Data ?? new List<PaymentMethodLookup>();
        }
    }

    public class LookupApiService
    {
        // Placeholder — we'll add customer/service endpoints later
        public async Task<List<CustomerLookup>> GetCustomersAsync()
        {
            // TODO: implement after CustomerController is built
            return new List<CustomerLookup>();
        }

        public async Task<List<ServiceLookup>> GetServicesAsync()
        {
            // TODO: implement after ServiceController is built
            return new List<ServiceLookup>();
        }

        public async Task<List<GarmentTypeLookup>> GetGarmentTypesAsync()
        {
            // TODO: implement after ServiceController is built
            return new List<GarmentTypeLookup>();
        }
    }
}