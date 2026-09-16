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

            // Create default company if none exists
            if (!context.Companies.Any())
            {
                var company = new Company
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

                context.Companies.Add(company);
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
                        CompanyId = company.CompanyId,
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
            }
        }
    }
}