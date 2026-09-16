namespace CRM.Domain.Enums
{
    public enum CustomerInteractionType
    {
        Inquiry = 1,
        Complaint = 2,
        Feedback = 3
    }

    public enum CustomerInteractionStatus
    {
        Open = 1,
        InProgress = 2,
        Resolved = 3,
        Closed = 4
    }

    public enum CustomerInteractionPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4
    }
}