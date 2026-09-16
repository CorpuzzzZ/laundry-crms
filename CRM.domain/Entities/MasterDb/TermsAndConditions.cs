using System;
using System.Collections.Generic;

namespace CRM.Domain.Entities.MasterDb
{
    public class TermsAndConditions
    {
        public int TermsId { get; set; }
        public int? CompanyId { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsMandatory { get; set; } = true;
        public string? AcceptedBy { get; set; }
        public DateTime? AcceptedDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }

        public virtual Company? Company { get; set; }
        public virtual ICollection<UserTermsAcceptance> UserTermsAcceptances { get; set; } = new List<UserTermsAcceptance>();
    }
}
