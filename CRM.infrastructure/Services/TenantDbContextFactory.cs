using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CRM.infrastructure.Data.Context;

namespace CRM.infrastructure.Services
{
    public class TenantDbContextFactory : ITenantDbContextFactory
    {
        private readonly ITenantDatabaseResolver _resolver;
        private readonly IConfiguration _configuration;

        public TenantDbContextFactory(
            ITenantDatabaseResolver resolver,
            IConfiguration configuration)
        {
            _resolver = resolver;
            _configuration = configuration;
        }

        public async Task<TenantErpDbContext> CreateAsync(int companyId)
        {
            var databaseInfo = await _resolver.GetDatabaseInfoAsync(companyId);

            // 1. Check if a direct connection string is provided under ConnectionStrings by DatabaseName or CredentialKey
            var directConnStr = _configuration.GetConnectionString(databaseInfo.DatabaseName)
                                ?? _configuration.GetConnectionString(databaseInfo.CredentialKey);

            string connectionString;
            if (!string.IsNullOrWhiteSpace(directConnStr))
            {
                connectionString = directConnStr;
            }
            else
            {
                // 2. Check for explicit SQL authentication credentials under TenantCredentials
                var userId = _configuration[$"TenantCredentials:{databaseInfo.CredentialKey}:UserId"];
                var password = _configuration[$"TenantCredentials:{databaseInfo.CredentialKey}:Password"];

                var isLocalServer = string.IsNullOrWhiteSpace(databaseInfo.ServerName) ||
                                    databaseInfo.ServerName.Contains("localdb", StringComparison.OrdinalIgnoreCase) ||
                                    databaseInfo.ServerName.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
                                    databaseInfo.ServerName.Equals(".", StringComparison.OrdinalIgnoreCase) ||
                                    databaseInfo.ServerName.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);

                var isTrustedKey = string.Equals(databaseInfo.CredentialKey, "TrustedConnection", StringComparison.OrdinalIgnoreCase) ||
                                   string.Equals(databaseInfo.CredentialKey, "Local", StringComparison.OrdinalIgnoreCase) ||
                                   string.Equals(databaseInfo.CredentialKey, "DefaultConnection", StringComparison.OrdinalIgnoreCase);

                if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(password))
                {
                    // Cloud / remote SQL Authentication (e.g. MonsterASP.NET)
                    connectionString =
                        $"Server={databaseInfo.ServerName};" +
                        $"Database={databaseInfo.DatabaseName};" +
                        $"User Id={userId};" +
                        $"Password={password};" +
                        $"Encrypt=True;" +
                        $"TrustServerCertificate=True;" +
                        $"MultipleActiveResultSets=True;";
                }
                else if (isLocalServer || isTrustedKey)
                {
                    // Local SQL Server / SSMS Windows Authentication
                    var server = string.IsNullOrWhiteSpace(databaseInfo.ServerName) ? "(localdb)\\MSSQLLocalDB" : databaseInfo.ServerName;
                    connectionString =
                        $"Server={server};" +
                        $"Database={databaseInfo.DatabaseName};" +
                        $"Trusted_Connection=True;" +
                        $"TrustServerCertificate=True;" +
                        $"MultipleActiveResultSets=True;";
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Credentials not found for key '{databaseInfo.CredentialKey}' and server '{databaseInfo.ServerName}' requires authentication. " +
                        $"Configure 'ConnectionStrings:{databaseInfo.DatabaseName}' or 'TenantCredentials:{databaseInfo.CredentialKey}' in appsettings.json.");
                }
            }

            var options = new DbContextOptionsBuilder<TenantErpDbContext>()
                .UseSqlServer(connectionString, sql =>
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null))
                .Options;

            return new TenantErpDbContext(options);
        }
    }
}