using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Data.Context
{
    public class MasterErpDbContextFactory : IDesignTimeDbContextFactory<MasterErpDbContext>
    {
        public MasterErpDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MasterErpDbContext>();
            
            var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=MSME_MasterERP;Trusted_Connection=True;MultipleActiveResultSets=True;";
            
            optionsBuilder.UseSqlServer(connectionString);

            return new MasterErpDbContext(optionsBuilder.Options);
        }
    }
}