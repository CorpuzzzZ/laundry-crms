using System;

namespace CRM.Domain.Entities.TenantDb
{
    public class ScheduledReport
    {
        public int ScheduledReportId { get; set; }
        public int ReportId { get; set; }
        public string Frequency { get; set; } = "Weekly";
        public DateTime? LastRunDate { get; set; }
        public DateTime NextRunDate { get; set; }
        public string RecipientsJSON { get; set; } = string.Empty;
        public string Format { get; set; } = "PDF";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
