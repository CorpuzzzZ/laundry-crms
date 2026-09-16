using System;
using CRM.Domain.Enums;

namespace CRM.Domain.DTOs.Customers
{
    public class CustomerInteractionDto
    {
        public int InteractionId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public CustomerInteractionType InteractionType { get; set; }
        public string InteractionTypeName { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Description { get; set; } = "";
        public CustomerInteractionStatus Status { get; set; }
        public string StatusName { get; set; } = "";
        public CustomerInteractionPriority Priority { get; set; }
        public string PriorityName { get; set; } = "";
        public int? Rating { get; set; }
        public string? AssignedToUserId { get; set; }
        public string CreatedByUserId { get; set; } = "";
        public string? UpdatedByUserId { get; set; }
        public string? ResolutionNotes { get; set; }
        public string? CustomerResponse { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }

    public class CreateCustomerInteractionDto
    {
        public int CustomerId { get; set; }
        public CustomerInteractionType InteractionType { get; set; }
        public string Subject { get; set; } = "";
        public string Description { get; set; } = "";
        public CustomerInteractionPriority Priority { get; set; } = CustomerInteractionPriority.Normal;
        public int? Rating { get; set; }
        public string? AssignedToUserId { get; set; }
    }

    public class UpdateCustomerInteractionDto
    {
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public CustomerInteractionPriority? Priority { get; set; }
        public int? Rating { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? ResolutionNotes { get; set; }
        public string? CustomerResponse { get; set; }
    }

    public class ChangeInteractionStatusDto
    {
        public CustomerInteractionStatus NewStatus { get; set; }
        public string? Notes { get; set; }
    }

    public class CustomerInteractionStatusHistoryDto
    {
        public int StatusHistoryId { get; set; }
        public CustomerInteractionStatus? FromStatus { get; set; }
        public string FromStatusName { get; set; } = "";
        public CustomerInteractionStatus ToStatus { get; set; }
        public string ToStatusName { get; set; } = "";
        public string ChangedByUserId { get; set; } = "";
        public DateTime ChangedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class CustomerInteractionFilterDto
    {
        public int CustomerId { get; set; }
        public CustomerInteractionType? InteractionType { get; set; }
        public CustomerInteractionStatus? Status { get; set; }
    }
}