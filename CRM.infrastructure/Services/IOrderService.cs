using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Domain.DTOs.Orders;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public interface IOrderService
    {
        Task<OrderDto?> GetOrderByIdAsync(TenantErpDbContext db, int orderId);
        Task<(List<OrderDto> Orders, int TotalCount)> GetOrdersAsync(TenantErpDbContext db, OrderFilterDto filter);
        Task<OrderDto> CreateOrderAsync(TenantErpDbContext db, CreateOrderDto dto, string userId);
        Task<OrderDto?> UpdateOrderAsync(TenantErpDbContext db, int orderId, UpdateOrderDto dto, string userId);
        Task<bool> DeleteOrderAsync(TenantErpDbContext db, int orderId);
        Task<OrderDto?> ChangeStatusAsync(TenantErpDbContext db, int orderId, OrderStatusChangeDto dto, string userId);
        Task<OrderPaymentDto> AddPaymentAsync(TenantErpDbContext db, int orderId, CreateOrderPaymentDto dto, string userId);
        Task<string> GenerateOrderNumberAsync(TenantErpDbContext db);
    }
}
