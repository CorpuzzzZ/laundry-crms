using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Data.Context
{
    public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
    {
        public TenantDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
            
            // Use LocalDB connection string for design-time
            var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=MSME_TenantERP;Trusted_Connection=True;MultipleActiveResultSets=True;";
            
            optionsBuilder.UseSqlServer(connectionString);

            return new TenantDbContext(optionsBuilder.Options);
        }
    }
}