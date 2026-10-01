using System;
using System.Threading.Tasks;
using CRM.Domain.DTOs.Reports;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public interface IReportService
    {
        Task<DailySalesReportDto> GetDailySalesAsync(
            TenantErpDbContext db, DateTime fromUtc, DateTime toUtc, int? branchId = null);

        Task<SalesByServiceReportDto> GetSalesByServiceAsync(
            TenantErpDbContext db, DateTime fromUtc, DateTime toUtc, int? branchId = null);

        Task<SalesByPaymentMethodReportDto> GetSalesByPaymentMethodAsync(
            TenantErpDbContext db, DateTime fromUtc, DateTime toUtc, int? branchId = null);

        Task<OrderStatusReportDto> GetOrderStatusAsync(
            TenantErpDbContext db, DateTime fromUtc, DateTime toUtc, int? branchId = null);

        Task<PeakHoursReportDto> GetPeakHoursAsync(
            TenantErpDbContext db, DateTime fromUtc, DateTime toUtc, int? branchId = null);
    }
}
