using System;
using System.Collections.Generic;

namespace CRM.Domain.Entities.MasterDb
{
    public class Branch
    {
        public int BranchId { get; set; }
        public int CompanyId { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string? Village { get; set; }
        public string City { get; set; } = string.Empty;
        public string? Province { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "Philippines";
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsActive { get; set; } = true;
        public string? OperatingHoursJSON { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }

        // ── Archive (soft-delete) ─────────────────────────
        public bool IsArchived { get; set; } = false;
        public DateTime? ArchivedAt { get; set; }
        public string? ArchivedBy { get; set; }

        public virtual Company? Company { get; set; }
    }
}
