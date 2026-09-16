using System;

namespace CRM.Domain.Entities.MasterDb
{
    public class CompanyDatabase
    {
        public int CompanyDatabaseId { get; set; }
        public int CompanyId { get; set; }
        public string ServerName { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? PasswordHash { get; set; }
        public string CredentialKey { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Company? Company { get; set; }
    }
}