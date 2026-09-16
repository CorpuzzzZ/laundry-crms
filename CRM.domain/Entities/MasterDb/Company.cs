using System;
using System.Collections.Generic;

namespace CRM.Domain.Entities.MasterDb
{
    public class Company
    {
        public int CompanyId { get; set; }
        public string CompanyCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public string? TaxId { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Website { get; set; }
        public string? Industry { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation Properties
        public virtual ICollection<CompanyDatabase> CompanyDatabases { get; set; } = new List<CompanyDatabase>();
                public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public virtual ICollection<TenantSubscription> TenantSubscriptions { get; set; } = new List<TenantSubscription>();
        public virtual ICollection<TermsAndConditions> TermsAndConditions { get; set; } = new List<TermsAndConditions>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
    }
}