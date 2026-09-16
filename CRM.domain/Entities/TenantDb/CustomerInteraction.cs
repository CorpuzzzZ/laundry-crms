using System;
using System.ComponentModel.DataAnnotations;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities.TenantDb
{
    public class CustomerInteraction
    {
        [Key]
        public int InteractionId { get; set; }
        public int CustomerId { get; set; }
        public CustomerInteractionType InteractionType { get; set; }

        public string Subject { get; set; } = "";
        public string Description { get; set; } = "";
        public CustomerInteractionStatus Status { get; set; } = CustomerInteractionStatus.Open;
        public CustomerInteractionPriority Priority { get; set; } = CustomerInteractionPriority.Normal;
        public int? Rating { get; set; }

        public string? AssignedToUserId { get; set; }
        public string CreatedByUserId { get; set; } = "";
        public string? UpdatedByUserId { get; set; }

        public string? ResolutionNotes { get; set; }
        public string? CustomerResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public virtual Customer? Customer { get; set; }
    }
}