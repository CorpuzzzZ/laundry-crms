using System;
using System.Collections.Generic;

namespace CRM.Domain.Entities.TenantDb
{
    public class Customer
    {
        // ============================================================
        // PRIMARY KEY
        // ============================================================
        public int CustomerId { get; set; }

        // ============================================================
        // CORE FIELDS
        // ============================================================
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerType { get; set; } = "Individual"; // Individual / Business
        public string? CompanyName { get; set; } // For business customers
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string PhonePrimary { get; set; } = string.Empty;
        public string? PreferredLanguage { get; set; } = "English";

        // ============================================================
        // ADDRESS (flat ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€šÃ‚Â but CustomerAddresses is the normalized version)
        // ============================================================
        public string? Street { get; set; }
        public string? Village { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }

        // ============================================================
        // CRM FIELDS
        // ============================================================
        public string? Notes { get; set; }
        public int LoyaltyPoints { get; set; } = 0;
        public decimal LifetimeSpend { get; set; } = 0;
        public int TotalOrders { get; set; } = 0;
        public DateTime? LastOrderDate { get; set; }

        // ============================================================
        // STATUS + AUDIT
        // ============================================================
        public bool IsActive { get; set; } = true;

        // ── Archive (soft-delete) ─────────────────────────
        public bool IsArchived { get; set; } = false;
        public DateTime? ArchivedAt { get; set; }
        public string? ArchivedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }

        // ============================================================
        // NAVIGATION PROPERTIES
        // ============================================================
        public virtual ICollection<CustomerAddress> CustomerAddresses { get; set; } = new List<CustomerAddress>();
        public virtual CustomerLoyalty? CustomerLoyalty { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();
    }
}
