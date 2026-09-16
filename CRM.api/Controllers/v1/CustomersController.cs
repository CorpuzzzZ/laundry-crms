using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.api.Filters;
using CRM.Domain.Common;
using CRM.Domain.DTOs.Customers;
using CRM.infrastructure.Services;

namespace CRM.api.Controllers.v1
{
    [Route("api/v1/customers")]
    public class CustomersController : TenantControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(
            ITenantDbContextFactory tenantFactory,
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor,
            ICustomerService customerService)
            : base(tenantFactory, permissionService, httpContextAccessor)
        {
            _customerService = customerService;
        }

        // ============================================================
        // DEBUG: what does the API think my permissions are?
        // ============================================================
        [HttpGet("debug-permissions")]
        [AllowAnonymous]
        public async Task<IActionResult> DebugPermissions()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            var emailClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var roleClaims = User.FindAll(System.Security.Claims.ClaimTypes.Role)
                                 .Select(c => c.Value).ToArray();
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Ok(new
                {
                    authenticated = false,
                    note = "No UserId claim â€” not logged in, or JWT missing claims",
                    emailClaim,
                    roleClaims,
                    companyIdClaim
                });
            }

            var userId = userIdClaim; // GUID string from Identity

            var dbRole = await PermissionService.GetUserRoleAsync(userId);
            var perms = await PermissionService.GetUserPermissionsAsync(userId);

            var hasRead = await PermissionService.HasPermissionAsync(userId, Permissions.CustomerRead);
            var hasCreate = await PermissionService.HasPermissionAsync(userId, Permissions.CustomerCreate);
            var hasUpdate = await PermissionService.HasPermissionAsync(userId, Permissions.CustomerUpdate);
            var hasDelete = await PermissionService.HasPermissionAsync(userId, Permissions.CustomerDelete);

            return Ok(new
            {
                authenticated = true,
                userId,
                email = emailClaim,
                companyId = companyIdClaim,
                jwtRoleClaims = roleClaims,
                databaseRole = dbRole,
                permissionCount = perms.Length,
                allPermissions = perms,
                customerRead = hasRead,
                customerCreate = hasCreate,
                customerUpdate = hasUpdate,
                customerDelete = hasDelete
            });
        }

        // ============================================================
        // GET: api/v1/customers
        // ============================================================
        [HttpGet]
        [RequirePermission(Permissions.CustomerRead)]
        public async Task<IActionResult> GetCustomers([FromQuery] CustomerFilterDto filter)
        {
            await using var db = await GetTenantDbAsync();
            var (customers, totalCount) = await _customerService.GetCustomersAsync(db, filter);

            return Ok(new
            {
                success = true,
                data = customers,
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
        // GET: api/v1/customers/{id}
        // ============================================================
        [HttpGet("{id:int}")]
        [RequirePermission(Permissions.CustomerRead)]
        public async Task<IActionResult> GetCustomer(int id)
        {
            await using var db = await GetTenantDbAsync();
            var customer = await _customerService.GetCustomerByIdAsync(db, id);

            return customer == null
                ? NotFound(new { success = false, message = $"Customer {id} not found." })
                : Success(customer);
        }

        // ============================================================
        // POST: api/v1/customers
        // ============================================================
        [HttpPost]
        [RequirePermission(Permissions.CustomerCreate)]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                await using var db = await GetTenantDbAsync();
                var customer = await _customerService.CreateCustomerAsync(db, dto);
                return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, new
                {
                    success = true,
                    message = "Customer created successfully.",
                    data = customer
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // PUT: api/v1/customers/{id}
        // ============================================================
        [HttpPut("{id:int}")]
        [RequirePermission(Permissions.CustomerUpdate)]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                await using var db = await GetTenantDbAsync();
                var customer = await _customerService.UpdateCustomerAsync(db, id, dto);

                return customer == null
                    ? NotFound(new { success = false, message = $"Customer {id} not found." })
                    : Success(customer, "Customer updated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // DELETE: api/v1/customers/{id}
        // ============================================================
        [HttpDelete("{id:int}")]
        [RequirePermission(Permissions.CustomerDelete)]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            await using var db = await GetTenantDbAsync();
            var archived = await _customerService.DeleteCustomerAsync(db, id, UserId);

            return archived
                ? Success<object?>(null, "Customer archived.")
                : NotFound(new { success = false, message = $"Customer {id} not found." });
        }

        [HttpPost("{id:int}/restore")]
        [RequirePermission(Permissions.CustomerUpdate)]
        public async Task<IActionResult> RestoreCustomer(int id)
        {
            await using var db = await GetTenantDbAsync();
            var restored = await _customerService.RestoreAsync(db, id);

            return restored == null
                ? NotFound(new { success = false, message = $"Customer {id} not found." })
                : Success(restored, "Customer restored.");
        }

        // ============================================================
        // GET: api/v1/customers/lookup
        // ============================================================
        [HttpGet("lookup")]
        [RequirePermission(Permissions.CustomerRead)]
        public async Task<IActionResult> GetLookup()
        {
            await using var db = await GetTenantDbAsync();
            var customers = await _customerService.GetCustomerLookupAsync(db);
            return Success(customers);
        }
    }
}