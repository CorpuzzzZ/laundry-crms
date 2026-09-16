using System;
using System.Collections.Generic;

namespace CRM.WinForms.Models
{
    public enum InteractionType { Inquiry = 1, Complaint = 2, Feedback = 3 }
    public enum InteractionStatus { Open = 1, InProgress = 2, Resolved = 3, Closed = 4 }
    public enum InteractionPriority { Low = 1, Normal = 2, High = 3, Urgent = 4 }

    public class CustomerInteractionModel
    {
        public int InteractionId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public int InteractionType { get; set; }
        public string InteractionTypeName { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Description { get; set; } = "";
        public int Status { get; set; }
        public string StatusName { get; set; } = "";
        public int Priority { get; set; }
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

    public class CreateCustomerInteractionRequest
    {
        public int CustomerId { get; set; }
        public int InteractionType { get; set; }
        public string Subject { get; set; } = "";
        public string Description { get; set; } = "";
        public int Priority { get; set; } = 2;
        public int? Rating { get; set; }
    }

    public class ChangeInteractionStatusRequest
    {
        public int NewStatus { get; set; }
        public string? Notes { get; set; }
    }

    public class InteractionStatusHistoryModel
    {
        public int StatusHistoryId { get; set; }
        public int? FromStatus { get; set; }
        public string FromStatusName { get; set; } = "";
        public int ToStatus { get; set; }
        public string ToStatusName { get; set; } = "";
        public string ChangedByUserId { get; set; } = "";
        public DateTime ChangedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class CustomerInteractionListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public List<CustomerInteractionModel> Data { get; set; } = new();
    }
}