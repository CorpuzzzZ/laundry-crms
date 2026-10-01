using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using CRM.Domain.Entities.MasterDb;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<MasterErpDbContext>();

            // Create default roles
            string[] roleNames = { "SuperAdmin", "Admin", "Manager", "Crew" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Ensure 3 tenant companies exist
            var company1 = context.Companies.FirstOrDefault(c => c.CompanyCode == "CRM001");
            if (company1 == null)
            {
                company1 = new Company
                {
                    CompanyCode = "CRM001",
                    CompanyName = "CRM Solutions Inc.",
                    LegalName = "CRM Solutions Inc.",
                    TaxId = "123-456-789",
                    RegistrationNumber = "REG-2026-001",
                    Website = "https://crmsolutions.com",
                    Industry = "Laundry Services",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                context.Companies.Add(company1);
                await context.SaveChangesAsync();
            }

            var company2 = context.Companies.FirstOrDefault(c => c.CompanyCode == "CRM002");
            if (company2 == null)
            {
                company2 = new Company
                {
                    CompanyCode = "CRM002",
                    CompanyName = "Fresh & Clean Laundromat",
                    LegalName = "Fresh & Clean Laundromat Ltd.",
                    TaxId = "234-567-890",
                    RegistrationNumber = "REG-2026-002",
                    Website = "https://freshclean.com",
                    Industry = "Laundry Services",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                context.Companies.Add(company2);
                await context.SaveChangesAsync();
            }

            var company3 = context.Companies.FirstOrDefault(c => c.CompanyCode == "CRM003");
            if (company3 == null)
            {
                company3 = new Company
                {
                    CompanyCode = "CRM003",
                    CompanyName = "QuickWash Express",
                    LegalName = "QuickWash Express Co.",
                    TaxId = "345-678-901",
                    RegistrationNumber = "REG-2026-003",
                    Website = "https://quickwash.com",
                    Industry = "Laundry Services",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                context.Companies.Add(company3);
                await context.SaveChangesAsync();
            }

            // Ensure CompanyDatabases routing entries exist for TenantDB1, TenantDB2, TenantDB3
            var tenantMappings = new[]
            {
                new { Company = company1, Server = "(localdb)\\TenantServer1", DbName = "TenantDB1", CredKey = "TenantDB1" },
                new { Company = company2, Server = "(localdb)\\TenantServer2", DbName = "TenantDB2", CredKey = "TenantDB2" },
                new { Company = company3, Server = "(localdb)\\TenantServer3", DbName = "TenantDB3", CredKey = "TenantDB3" }
            };

            foreach (var mapping in tenantMappings)
            {
                var existingDb = context.CompanyDatabases.FirstOrDefault(cd => cd.CompanyId == mapping.Company.CompanyId);
                if (existingDb == null)
                {
                    context.CompanyDatabases.Add(new CompanyDatabase
                    {
                        CompanyId = mapping.Company.CompanyId,
                        ServerName = mapping.Server,
                        DatabaseName = mapping.DbName,
                        CredentialKey = mapping.CredKey,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    existingDb.ServerName = mapping.Server;
                    existingDb.DatabaseName = mapping.DbName;
                    existingDb.CredentialKey = mapping.CredKey;
                    existingDb.IsActive = true;
                }
            }
            await context.SaveChangesAsync();

            // Create SuperAdmin user
            var superAdminEmail = "superadmin@crm.com";
            var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
            if (superAdminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    FirstName = "Super",
                    LastName = "Admin",
                    CompanyId = company1.CompanyId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                var result = await userManager.CreateAsync(user, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "SuperAdmin");
                }
            }

            // Seed default Subscription Plans if none exist
            if (!context.SubscriptionPlans.Any())
            {
                var plan1 = new SubscriptionPlan
                {
                    PlanName = "Plan 1",
                    PlanCode = "PLAN1",
                    Description = "Admin can access Subscription View, Terms and Condition, Reports, Dashboard, Branch Management, Service Management, User Management, Customer Management, Loyalty Management, Order Management.",
                    PricePerMonth = 4999m,
                    PricePerYear = 49990m,
                    MaxUsers = 999,
                    MaxBranches = 999,
                    MaxOrdersPerMonth = 10000,
                    MaxStorageGB = 100,
                    IsActive = true,
                    SortOrder = 1,
                    CreatedAt = DateTime.UtcNow
                };

                var plan2 = new SubscriptionPlan
                {
                    PlanName = "Plan 2",
                    PlanCode = "PLAN2",
                    Description = "Admin can access Subscription View, Terms and Condition, Dashboard, Branch - only 1 branch, Service Management, User Management - only 1 manager and 1 crew, Customer Management, Loyalty Management, Order Management.",
                    PricePerMonth = 2499m,
                    PricePerYear = 24990m,
                    MaxUsers = 3,
                    MaxBranches = 1,
                    MaxOrdersPerMonth = 2000,
                    MaxStorageGB = 20,
                    IsActive = true,
                    SortOrder = 2,
                    CreatedAt = DateTime.UtcNow
                };

                var plan3 = new SubscriptionPlan
                {
                    PlanName = "Plan 3",
                    PlanCode = "PLAN3",
                    Description = "Admin can access Subscription View, Terms and Condition, Dashboard, Report, Service Management, User Management, Customer Management, Order Management.",
                    PricePerMonth = 1999m,
                    PricePerYear = 19990m,
                    MaxUsers = 10,
                    MaxBranches = 0,
                    MaxOrdersPerMonth = 5000,
                    MaxStorageGB = 50,
                    IsActive = true,
                    SortOrder = 3,
                    CreatedAt = DateTime.UtcNow
                };

                context.SubscriptionPlans.AddRange(plan1, plan2, plan3);
                await context.SaveChangesAsync();
            }

            // Ensure all companies have an active subscription
            var defaultPlan = context.SubscriptionPlans.OrderBy(p => p.SortOrder).FirstOrDefault();
            if (defaultPlan != null)
            {
                var allCompanies = context.Companies.ToList();
                foreach (var c in allCompanies)
                {
                    if (!context.TenantSubscriptions.Any(ts => ts.CompanyId == c.CompanyId))
                    {
                        context.TenantSubscriptions.Add(new TenantSubscription
                        {
                            CompanyId = c.CompanyId,
                            PlanId = defaultPlan.PlanId,
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddYears(1),
                            IsActive = true,
                            PaymentStatus = "Paid",
                            PaymentMethod = "System Seed",
                            AmountPaid = defaultPlan.PricePerYear ?? 9990m,
                            AutoRenew = true,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
                await context.SaveChangesAsync();
            }

            // Seed default Terms & Conditions if none exist
            if (!context.TermsAndConditions.Any())
            {
                context.TermsAndConditions.Add(new TermsAndConditions
                {
                    Version = "v1.0",
                    Title = "Standard Laundry CRM Platform Terms of Service",
                    Content = @"# Platform Terms of Service

## 1. Acceptance of Terms
By accessing or using the Laundry CRM system, all tenant organizations, administrators, and authorized users agree to comply with and be bound by these Terms of Service.

## 2. Permitted Use
The CRM platform is provided solely for lawful laundry business management, customer relationship handling, order tracking, and inventory operations.

## 3. Data Protection and Confidentiality
Tenant business data, customer personally identifiable information (PII), and order records are strictly protected. Each tenant database is isolated.

## 4. Subscription & Payment Terms
Subscription fees are billed in advance on a monthly or annual basis depending on the chosen tier. Services remain active subject to valid payment status.

## 5. Modifications
Platform administrators reserve the right to revise these Terms upon notifying users of version updates.",
                    EffectiveDate = DateTime.UtcNow,
                    IsActive = true,
                    IsMandatory = true,
                    CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }
        }
    }
}