using System;
using System.ComponentModel.DataAnnotations;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities.TenantDb
{
    public class CustomerInteractionStatusHistory
    {
        [Key]
        public int StatusHistoryId { get; set; }
        public int InteractionId { get; set; }
        public CustomerInteractionStatus? FromStatus { get; set; }
        public CustomerInteractionStatus ToStatus { get; set; }
        public string ChangedByUserId { get; set; } = "";
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        public virtual CustomerInteraction? Interaction { get; set; }
    }
}