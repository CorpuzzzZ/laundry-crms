using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.api.Filters;
using CRM.Domain.Common;
using CRM.Domain.DTOs.Orders;
using CRM.infrastructure.Services;

namespace CRM.api.Controllers.v1
{
    [Route("api/v1/orders")]
    [Authorize]
    public class OrdersController : TenantControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILoyaltyService _loyaltyService;

        public OrdersController(
            ITenantDbContextFactory tenantFactory,
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor,
            IOrderService orderService,
            ILoyaltyService loyaltyService)
            : base(tenantFactory, permissionService, httpContextAccessor)
        {
            _orderService = orderService;
            _loyaltyService = loyaltyService;
        }

        // ============================================================
        // GET: api/v1/orders
        // ============================================================
        [HttpGet]
        [RequirePermission(Permissions.OrderRead)]
        public async Task<IActionResult> GetOrders([FromQuery] OrderFilterDto filter)
        {
            await using var db = await GetTenantDbAsync();
            var (orders, totalCount) = await _orderService.GetOrdersAsync(db, filter);

            return Ok(new
            {
                success = true,
                data = orders,
                pagination = new
                {
                    page = filter.Page,
                    pageSize = filter.PageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
                }
            });
        }

        // ============================================================
        // GET: api/v1/orders/{id}
        // ============================================================
        [HttpGet("{id:int}")]
        [RequirePermission(Permissions.OrderRead)]
        public async Task<IActionResult> GetOrder(int id)
        {
            await using var db = await GetTenantDbAsync();
            var order = await _orderService.GetOrderByIdAsync(db, id);

            return order == null
                ? NotFound(new { success = false, message = $"Order {id} not found." })
                : Success(order);
        }

        // ============================================================
        // POST: api/v1/orders
        // ============================================================
        [HttpPost]
        [RequirePermission(Permissions.OrderCreate)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                await using var db = await GetTenantDbAsync();
                var order = await _orderService.CreateOrderAsync(db, dto, UserId);
                return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, new
                {
                    success = true,
                    message = "Order created successfully.",
                    data = order
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // PUT: api/v1/orders/{id}
        // ============================================================
        [HttpPut("{id:int}")]
        [RequirePermission(Permissions.OrderUpdate)]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                await using var db = await GetTenantDbAsync();
                var order = await _orderService.UpdateOrderAsync(db, id, dto, UserId);

                return order == null
                    ? NotFound(new { success = false, message = $"Order {id} not found." })
                    : Success(order, "Order updated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // DELETE: api/v1/orders/{id}
        // ============================================================
        [HttpDelete("{id:int}")]
        [RequirePermission(Permissions.OrderDelete)]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            await using var db = await GetTenantDbAsync();
            var deleted = await _orderService.DeleteOrderAsync(db, id);

            return deleted
                ? Success<object?>(null, "Order deleted successfully.")
                : NotFound(new { success = false, message = $"Order {id} not found." });
        }

        // ============================================================
        // POST: api/v1/orders/{id}/status
        // ============================================================
        [HttpPost("{id:int}/status")]
        [RequirePermission(Permissions.OrderChangeStatus)]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] OrderStatusChangeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                await using var db = await GetTenantDbAsync();
                var order = await _orderService.ChangeStatusAsync(db, id, dto, UserId);



                if (order != null)
                {
                    try
                    {
                        await _loyaltyService.EvaluateEarningAsync(db, id, UserId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Loyalty] Earn evaluation failed for order {id}: {ex.Message}");
                    }
                }

                return order == null
                    ? NotFound(new { success = false, message = $"Order {id} not found." })
                    : Success(order, "Order status updated.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // POST: api/v1/orders/{id}/payments
        // ============================================================
        [HttpPost("{id:int}/payments")]
        [RequirePermission(Permissions.OrderProcessPayment)]
        public async Task<IActionResult> AddPayment(int id, [FromBody] CreateOrderPaymentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                await using var db = await GetTenantDbAsync();

                // Redeem loyalty points before recording the payment, if requested.
                if (dto.LoyaltyPointsToRedeem > 0)
                {
                    var order = await _orderService.GetOrderByIdAsync(db, id);
                    if (order == null)
                        return NotFound(new { success = false, message = $"Order {id} not found." });

                    try
                    {
                        await _loyaltyService.RedeemPointsAsync(
                            db, order.CustomerId, id, dto.LoyaltyPointsToRedeem, UserId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Loyalty] Redeem failed for order {id}: {ex.Message}");
                    }
                }

                var payment = await _orderService.AddPaymentAsync(db, id, dto, UserId);
                return Success(payment, "Payment recorded successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // GET: api/v1/orders/lookups/statuses
        // ============================================================
        [HttpGet("lookups/statuses")]
        [RequirePermission(Permissions.OrderRead)]
        public async Task<IActionResult> GetStatuses()
        {
            await using var db = await GetTenantDbAsync();
            var statuses = await db.OrderStatuses
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .Select(s => new { s.StatusId, s.StatusCode, s.StatusName, s.ColorCode })
                .ToListAsync();
            return Success(statuses);
        }

        // ============================================================
        // GET: api/v1/orders/lookups/payment-methods
        // ============================================================
        [HttpGet("lookups/payment-methods")]
        [RequirePermission(Permissions.OrderRead)]
        public async Task<IActionResult> GetPaymentMethods()
        {
            await using var db = await GetTenantDbAsync();
            var methods = await db.PaymentMethods
                .Where(m => m.IsActive)
                .OrderBy(m => m.SortOrder)
                .Select(m => new { m.PaymentMethodId, m.MethodCode, m.MethodName })
                .ToListAsync();
            return Success(methods);
        }

        // ============================================================
        // RULE-BASED CALCULATION ENDPOINTS
        // ============================================================

        /// <summary>
        /// Calculate detergent + load for a category and weight (preview for UI).
        /// </summary>
        [HttpGet("order-rules/calculate")]
        [AllowAnonymous]
        public IActionResult CalculateRules(
            [FromQuery] string category,
            [FromQuery] decimal weight)
        {
            try
            {
                var (detergent, load) = CRM.Domain.Rules.DetergentRuleEngine.Calculate(category, weight);
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        category,
                        weight,
                        detergentGrams = detergent,
                        loadCount = load
                    }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get the list of supported services with their pricing.
        /// </summary>
        [HttpGet("order-rules/services")]
        [AllowAnonymous]
        public IActionResult GetServices()
        {
            return Ok(new
            {
                success = true,
                data = CRM.Domain.Rules.ServicePricingRules.Services
            });
        }

        /// <summary>
        /// Get the list of chemical add-ons with prices.
        /// </summary>
        [HttpGet("order-rules/chemical-addons")]
        [AllowAnonymous]
        public IActionResult GetChemicalAddOns()
        {
            return Ok(new
            {
                success = true,
                data = CRM.Domain.Rules.AddOnRules.ChemicalAddOns
            });
        }

        /// <summary>
        /// Get the list of machine add-ons with prices.
        /// </summary>
        [HttpGet("order-rules/machine-addons")]
        [AllowAnonymous]
        public IActionResult GetMachineAddOns()
        {
            return Ok(new
            {
                success = true,
                data = CRM.Domain.Rules.AddOnRules.MachineAddOns
            });
        }
    }
}



