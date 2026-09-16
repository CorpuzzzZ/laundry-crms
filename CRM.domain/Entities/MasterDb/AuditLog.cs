using System;

namespace CRM.Domain.Entities.MasterDb
{
    public class AuditLog
    {
        public long AuditId { get; set; }
        public int CompanyId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? TableName { get; set; }
        public string? RecordId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public virtual Company? Company { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}