using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CRM.Domain.Entities.MasterDb;
using CRM.infrastructure.Data;
using CRM.infrastructure.Data.Context;
using CRM.infrastructure.Services;

namespace CRM.infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ============================================================
            // DATABASES
            // ============================================================
            var masterConnectionString = configuration.GetConnectionString("MasterErp")
                ?? "Server=(localdb)\\MSSQLLocalDB;Database=MSME_MasterERP;Trusted_Connection=True;MultipleActiveResultSets=True;";

            services.AddDbContext<MasterErpDbContext>(options =>
                options.UseSqlServer(masterConnectionString));

            var tenantConnectionString = configuration.GetConnectionString("TenantErp")
                ?? "Server=(localdb)\\MSSQLLocalDB;Database=MSME_TenantERP;Trusted_Connection=True;MultipleActiveResultSets=True;";

            services.AddDbContext<TenantDbContext>(options =>
                options.UseSqlServer(tenantConnectionString));

            services.AddDbContext<TenantErpDbContext>(options =>
                options.UseSqlServer(tenantConnectionString));

            // ============================================================
            // IDENTITY
            // ============================================================
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<MasterErpDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.User.RequireUniqueEmail = true;
            });

            // ============================================================
            // SERVICES
            // ÃƒÂ¢Ã…Â¡Ã‚Â ÃƒÂ¯Ã‚Â¸Ã‚Â Use fully qualified names to avoid ambiguity between
            //    Data.Context.TenantDbContextFactory and
            //    Services.TenantDbContextFactory
            // ============================================================
            services.AddScoped<ITenantDatabaseResolver,
                CRM.infrastructure.Services.TenantDatabaseResolver>();
            services.AddScoped<ITenantDbContextFactory,
                CRM.infrastructure.Services.TenantDbContextFactory>();
            services.AddScoped<IPermissionService,
                CRM.infrastructure.Services.PermissionService>();
            services.AddScoped<IOrderService,
                CRM.infrastructure.Services.OrderService>();
            services.AddScoped<ICustomerService,
                CRM.infrastructure.Services.CustomerService>();
            services.AddScoped<ICustomerInteractionService,
                CRM.infrastructure.Services.CustomerInteractionService>();
            services.AddScoped<IBranchService,
                CRM.infrastructure.Services.BranchService>();
            services.AddScoped<IUserService,
                CRM.infrastructure.Services.UserService>();
            services.AddScoped<IServiceService,
                CRM.infrastructure.Services.ServiceService>();

            return services;
        }
    }
}
