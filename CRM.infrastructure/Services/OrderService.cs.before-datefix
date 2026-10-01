using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CRM.Domain.DTOs.Orders;
using CRM.Domain.Entities.TenantDb;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public class OrderService : IOrderService
    {
        // ----------------------------------------------------------
        // GENERATE ORDER NUMBER
        // ----------------------------------------------------------
        public async Task<string> GenerateOrderNumberAsync(TenantErpDbContext db)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"ORD-{year}-";

            var lastOrder = await db.Orders
                .Where(o => o.OrderNumber.StartsWith(prefix))
                .OrderByDescending(o => o.OrderId)
                .Select(o => o.OrderNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastOrder))
            {
                var parts = lastOrder.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var lastNumber))
                    nextNumber = lastNumber + 1;
            }

            return $"{prefix}{nextNumber:D4}";
        }

        // ----------------------------------------------------------
        // GET ORDER BY ID
        // ----------------------------------------------------------
        public async Task<OrderDto?> GetOrderByIdAsync(TenantErpDbContext db, int orderId)
        {
            var order = await db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Status)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Service)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.GarmentType)
                .Include(o => o.Payments).ThenInclude(p => p.PaymentMethod)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            return order == null ? null : MapToDto(order);
        }

        // ----------------------------------------------------------
        // GET ORDERS (with filtering & pagination)
        // ----------------------------------------------------------
        public async Task<(List<OrderDto> Orders, int TotalCount)> GetOrdersAsync(
            TenantErpDbContext db, OrderFilterDto filter)
        {
            var query = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Status)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Service)
                .AsNoTracking()
                .AsQueryable();

            // Filters
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(o =>
                    o.OrderNumber.Contains(term) ||
                    o.Customer!.FirstName.Contains(term) ||
                    o.Customer.LastName.Contains(term) ||
                    o.Customer.PhonePrimary.Contains(term));
            }

            if (filter.CustomerId.HasValue)
                query = query.Where(o => o.CustomerId == filter.CustomerId.Value);

            if (!string.IsNullOrWhiteSpace(filter.StatusCode))
                query = query.Where(o => o.Status!.StatusCode == filter.StatusCode);

            if (!string.IsNullOrWhiteSpace(filter.Priority))
                query = query.Where(o => o.Priority == filter.Priority);

            if (filter.DateFrom.HasValue)
                query = query.Where(o => o.OrderDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(o => o.OrderDate <= filter.DateTo.Value);

            // Count before pagination
            var totalCount = await query.CountAsync();

            // Sorting
            query = filter.SortBy?.ToLower() switch
            {
                "ordernumber" => filter.SortDirection == "ASC"
                    ? query.OrderBy(o => o.OrderNumber)
                    : query.OrderByDescending(o => o.OrderNumber),
                "totalamount" => filter.SortDirection == "ASC"
                    ? query.OrderBy(o => o.TotalAmount)
                    : query.OrderByDescending(o => o.TotalAmount),
                "status" => filter.SortDirection == "ASC"
                    ? query.OrderBy(o => o.Status!.SortOrder)
                    : query.OrderByDescending(o => o.Status!.SortOrder),
                _ => filter.SortDirection == "ASC"
                    ? query.OrderBy(o => o.OrderDate)
                    : query.OrderByDescending(o => o.OrderDate)
            };

            // Pagination
            var orders = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (orders.Select(MapToDto).ToList(), totalCount);
        }

        // ----------------------------------------------------------
        // CREATE ORDER
        // ----------------------------------------------------------
        public async Task<OrderDto> CreateOrderAsync(
            TenantErpDbContext db, CreateOrderDto dto, string userId)
        {
            // Validate customer exists
            var customer = await db.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId)
                ?? throw new InvalidOperationException($"Customer {dto.CustomerId} not found.");

            // Get default "Pending" status
            var pendingStatus = await db.OrderStatuses
                .FirstOrDefaultAsync(s => s.StatusCode == "PE")
                ?? throw new InvalidOperationException("Default order status 'PE' (Pending) not found.");

            // Generate order number
            var orderNumber = await GenerateOrderNumberAsync(db);

            // Calculate totals
            decimal subtotal = 0;
            int totalItems = 0;

            var orderItems = new List<OrderItem>();
            foreach (var itemDto in dto.Items)
            {
                var lineTotal = (itemDto.Quantity * itemDto.UnitPrice) - itemDto.DiscountAmount;
                subtotal += lineTotal;
                totalItems += (int)itemDto.Quantity;

                orderItems.Add(new OrderItem
                {
                    ServiceId = itemDto.ServiceId,
                    GarmentTypeId = itemDto.GarmentTypeId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    DiscountAmount = itemDto.DiscountAmount,
                    SpecialInstructions = itemDto.SpecialInstructions,
                    IsCompleted = false
                });
            }

            var totalAmount = subtotal - dto.DiscountAmount + dto.TaxAmount;

            // Create order
            var order = new Order
            {
                OrderNumber = orderNumber,
                CustomerId = dto.CustomerId,
                BranchId = dto.BranchId,
                OrderType = dto.OrderType,
                OrderDate = DateTime.UtcNow,
                StatusId = pendingStatus.StatusId,
                Priority = dto.Priority,
                Subtotal = subtotal,
                DiscountAmount = dto.DiscountAmount,
                TaxAmount = dto.TaxAmount,
                TotalAmount = totalAmount,
                TotalWeight = dto.TotalWeight,
                TotalItems = totalItems,
                SpecialInstructions = dto.SpecialInstructions,
                PickupDate = dto.PickupDate,
                DeliveryDate = dto.DeliveryDate,
                Notes = dto.Notes,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // Add initial status history
            db.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.OrderId,
                StatusId = pendingStatus.StatusId,
                ChangedByUserId = userId,
                Notes = "Order created",
                ChangedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            // Return full DTO
            return (await GetOrderByIdAsync(db, order.OrderId))!;
        }

        // ----------------------------------------------------------
        // UPDATE ORDER
        // ----------------------------------------------------------
        public async Task<OrderDto?> UpdateOrderAsync(
            TenantErpDbContext db, int orderId, UpdateOrderDto dto, string userId)
        {
            var order = await db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return null;

            order.CustomerId = dto.CustomerId;
            order.Priority = dto.Priority;
            order.DiscountAmount = dto.DiscountAmount;
            order.TaxAmount = dto.TaxAmount;
            order.TotalWeight = dto.TotalWeight;
            order.SpecialInstructions = dto.SpecialInstructions;
            order.PickupDate = dto.PickupDate;
            order.DeliveryDate = dto.DeliveryDate;
            order.Notes = dto.Notes;
            order.UpdatedAt = DateTime.UtcNow;

            // Replace items if provided
            if (dto.Items != null && dto.Items.Count > 0)
            {
                db.OrderItems.RemoveRange(order.OrderItems);
                order.OrderItems.Clear();

                decimal subtotal = 0;
                int totalItems = 0;

                foreach (var itemDto in dto.Items)
                {
                    var lineTotal = (itemDto.Quantity * itemDto.UnitPrice) - itemDto.DiscountAmount;
                    subtotal += lineTotal;
                    totalItems += (int)itemDto.Quantity;

                    order.OrderItems.Add(new OrderItem
                    {
                        ServiceId = itemDto.ServiceId,
                        GarmentTypeId = itemDto.GarmentTypeId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        DiscountAmount = itemDto.DiscountAmount,
                        SpecialInstructions = itemDto.SpecialInstructions
                    });
                }

                order.Subtotal = subtotal;
                order.TotalItems = totalItems;
                order.TotalAmount = subtotal - dto.DiscountAmount + dto.TaxAmount;
            }

            await db.SaveChangesAsync();
            return await GetOrderByIdAsync(db, orderId);
        }

        // ----------------------------------------------------------
        // DELETE ORDER
        // ----------------------------------------------------------
        public async Task<bool> DeleteOrderAsync(TenantErpDbContext db, int orderId)
        {
            var order = await db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return false;

            db.OrderItems.RemoveRange(order.OrderItems);
            db.Orders.Remove(order);
            await db.SaveChangesAsync();
            return true;
        }

        // ----------------------------------------------------------
        // CHANGE STATUS
        // ----------------------------------------------------------
        public async Task<OrderDto?> ChangeStatusAsync(
            TenantErpDbContext db, int orderId, OrderStatusChangeDto dto, string userId)
        {
            var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return null;

            var newStatus = await db.OrderStatuses.FindAsync(dto.StatusId);
            if (newStatus == null)
                throw new InvalidOperationException($"Status {dto.StatusId} not found.");

            order.StatusId = dto.StatusId;
            order.UpdatedAt = DateTime.UtcNow;

            // Set actual pickup/delivery timestamps based on status
            if (newStatus.StatusCode == "PU" && !order.ActualPickupDate.HasValue)
                order.ActualPickupDate = DateTime.UtcNow;
            else if (newStatus.StatusCode == "DE" && !order.ActualDeliveryDate.HasValue)
                order.ActualDeliveryDate = DateTime.UtcNow;

            // Add status history
            db.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = orderId,
                StatusId = dto.StatusId,
                ChangedByUserId = userId,
                Notes = dto.Notes,
                ChangedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            return await GetOrderByIdAsync(db, orderId);
        }

        // ----------------------------------------------------------
        // ADD PAYMENT
        // ----------------------------------------------------------
        public async Task<OrderPaymentDto> AddPaymentAsync(
            TenantErpDbContext db, int orderId, CreateOrderPaymentDto dto)
        {
            var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId)
                ?? throw new InvalidOperationException($"Order {orderId} not found.");

            var paymentMethod = await db.PaymentMethods.FindAsync(dto.PaymentMethodId)
                ?? throw new InvalidOperationException($"Payment method {dto.PaymentMethodId} not found.");

            var payment = new OrderPayment
            {
                OrderId = orderId,
                PaymentMethodId = dto.PaymentMethodId,
                Amount = dto.Amount,
                ReferenceNumber = dto.ReferenceNumber,
                PaymentDate = DateTime.UtcNow,
                Status = "Completed",
                Notes = dto.Notes
            };

            db.OrderPayments.Add(payment);
            await db.SaveChangesAsync();

            return new OrderPaymentDto
            {
                OrderPaymentId = payment.OrderPaymentId,
                PaymentMethodId = payment.PaymentMethodId,
                PaymentMethodName = paymentMethod.MethodName,
                Amount = payment.Amount,
                ReferenceNumber = payment.ReferenceNumber,
                PaymentDate = payment.PaymentDate,
                Status = payment.Status,
                Notes = payment.Notes
            };
        }

        // ----------------------------------------------------------
        // MAPPER
        // ----------------------------------------------------------
        private static OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer != null
                    ? $"{order.Customer.FirstName} {order.Customer.LastName}".Trim()
                    : string.Empty,
                CustomerPhone = order.Customer?.PhonePrimary ?? string.Empty,
                OrderType = order.OrderType,
                OrderDate = order.OrderDate,
                StatusCode = order.Status?.StatusCode ?? string.Empty,
                StatusName = order.Status?.StatusName ?? string.Empty,
                Priority = order.Priority,
                Subtotal = order.Subtotal,
                DiscountAmount = order.DiscountAmount,
                TaxAmount = order.TaxAmount,
                TotalAmount = order.TotalAmount,
                TotalWeight = order.TotalWeight,
                TotalItems = order.TotalItems,
                SpecialInstructions = order.SpecialInstructions,
                PickupDate = order.PickupDate,
                DeliveryDate = order.DeliveryDate,
                ActualPickupDate = order.ActualPickupDate,
                ActualDeliveryDate = order.ActualDeliveryDate,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                Items = order.OrderItems?.Select(oi => new OrderItemDto
                {
                    OrderItemId = oi.OrderItemId,
                    ServiceId = oi.ServiceId,
                    ServiceName = oi.Service?.ServiceName ?? string.Empty,
                    GarmentTypeId = oi.GarmentTypeId,
                    GarmentTypeName = oi.GarmentType?.GarmentName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    DiscountAmount = oi.DiscountAmount,
                    LineTotal = (oi.Quantity * oi.UnitPrice) - oi.DiscountAmount,
                    SpecialInstructions = oi.SpecialInstructions,
                    IsCompleted = oi.IsCompleted,
                    CompletedAt = oi.CompletedAt
                }).ToList() ?? new List<OrderItemDto>(),
                Payments = order.Payments?.Select(p => new OrderPaymentDto
                {
                    OrderPaymentId = p.OrderPaymentId,
                    PaymentMethodId = p.PaymentMethodId,
                    PaymentMethodName = p.PaymentMethod?.MethodName ?? string.Empty,
                    Amount = p.Amount,
                    ReferenceNumber = p.ReferenceNumber,
                    PaymentDate = p.PaymentDate,
                    Status = p.Status,
                    Notes = p.Notes
                }).ToList() ?? new List<OrderPaymentDto>()
            };
        }

        // ============================================================
        // CREATE ORDER WITH RULES (backend-driven)
        // ============================================================
        public async Task<CRM.Domain.DTOs.Orders.OrderItemResponseV2> BuildItemWithRulesAsync(
            CRM.Domain.DTOs.Orders.CreateOrderItemRequestV2 item)
        {
            // 1. Detergent & Load from rules
            var (detergent, load) = CRM.Domain.Rules.DetergentRuleEngine.Calculate(item.Category, item.Weight);

            // 2. Service line total
            decimal serviceLine = CRM.Domain.Rules.ServicePricingRules.CalculateServiceLine(item.ServiceId, item.Weight);
            var serviceInfo = CRM.Domain.Rules.ServicePricingRules.GetService(item.ServiceId);

            // 3. Chemical add-ons
            var chemItems = item.ChemicalAddOns.Select(c => (c.AddOnId, c.Quantity));
            decimal chemTotal = CRM.Domain.Rules.AddOnRules.CalculateChemicalTotal(chemItems);

            // 4. Machine add-ons
            var machItems = item.MachineAddOns.Select(m => (m.AddOnId, m.Quantity));
            decimal machTotal = CRM.Domain.Rules.AddOnRules.CalculateMachineTotal(machItems);

            // 5. Total
            decimal lineTotal = serviceLine + chemTotal + machTotal;

            return await Task.FromResult(new CRM.Domain.DTOs.Orders.OrderItemResponseV2
            {
                Category = item.Category,
                ServiceName = serviceInfo?.ServiceName ?? "Unknown",
                Weight = item.Weight,
                DetergentGrams = detergent,
                LoadCount = load,
                ServiceLineTotal = serviceLine,
                ChemicalTotal = chemTotal,
                MachineTotal = machTotal,
                LineTotal = lineTotal
            });
        }
    }
}
