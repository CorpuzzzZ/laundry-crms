using System;
using System.Collections.Generic;

namespace CRM.Domain.Entities.TenantDb
{
    public class ServiceCategory
    {
        public int ServiceCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconPath { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
