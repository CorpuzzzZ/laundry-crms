using System;
using System.Collections.Generic;

namespace CRM.Domain.Entities.TenantDb
{
    public class GarmentType
    {
        public int GarmentTypeId { get; set; }
        public string GarmentCode { get; set; } = string.Empty;
        public string GarmentName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? DefaultPrice { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<PriceMatrix> PriceMatrices { get; set; } = new List<PriceMatrix>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
