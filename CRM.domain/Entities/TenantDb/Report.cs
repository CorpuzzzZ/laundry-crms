using System;

namespace CRM.Domain.Entities.TenantDb
{
    public class Report
    {
        public int ReportId { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public string ReportCategory { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string QueryDefinition { get; set; } = string.Empty;
        public string? ParametersJSON { get; set; }
        public bool IsSystem { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
