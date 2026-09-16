using System;

namespace CRM.Domain.Entities.TenantDb
{
    public class PriceMatrix
    {
        public int PriceMatrixId { get; set; }
        public int ServiceId { get; set; }
        public int? GarmentTypeId { get; set; }
        public string ServiceType { get; set; } = "Standard";
        public decimal UnitPrice { get; set; }
        public decimal? MinQuantity { get; set; } = 1;
        public decimal? MaxQuantity { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Service? Service { get; set; }
        public virtual GarmentType? GarmentType { get; set; }
    }
}
