using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System;

namespace CRM.Domain.Entities.MasterDb
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public bool IsActive { get; set; } = true;

        // ── Archive (soft-delete) ─────────────────────────
        public bool IsArchived { get; set; } = false;
        public DateTime? ArchivedAt { get; set; }
        public string? ArchivedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual Company? Company { get; set; }
        public virtual ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
    }
}