using System;

namespace CRM.Domain.Entities.MasterDb
{
    public class UserTermsAcceptance
    {
        public int UserTermsAcceptanceId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int TermsId { get; set; }
        public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual TermsAndConditions? Terms { get; set; }
    }
}