using System;
using System.ComponentModel.DataAnnotations;

namespace CRM.Domain.Entities.TenantDb
{
    public class EmployeeWorkLog
    {
        [Key]
        public int WorkLogId { get; set; }
        public int UserId { get; set; }
        public int? OrderId { get; set; }
        public string WorkType { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Notes { get; set; }

        // Navigation Properties
        public virtual Order? Order { get; set; }
    }
}