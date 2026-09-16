using System;

namespace CRM.Domain.Entities.TenantDb
{
    /// <summary>
    /// Optional add-on attached to a base service.
    /// Example: "Extra Rinse" +₱20 attached to "Wash & Fold".
    /// </summary>
    public class ServiceAddOn
    {
        public int ServiceAddOnId { get; set; }
        public int ServiceId { get; set; }
        public string AddOnName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal AdditionalPrice { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Service? Service { get; set; }
    }
}