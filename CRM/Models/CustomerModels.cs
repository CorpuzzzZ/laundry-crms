using System;

namespace CRM.WinForms.Models
{
    public class CustomerModel
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerType { get; set; } = "Individual";
        public string? CompanyName { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? Email { get; set; }
        public string PhonePrimary { get; set; } = string.Empty;
        public string? Street { get; set; }
        public string? Village { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Notes { get; set; }
        public int LoyaltyPoints { get; set; }
        public decimal LifetimeSpend { get; set; }
        public int TotalOrders { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? ArchivedAt { get; set; }
    }

    public class CreateCustomerRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhonePrimary { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string CustomerType { get; set; } = "Individual";
        public string? Street { get; set; }
        public string? Village { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateCustomerRequest : CreateCustomerRequest
    {
        public bool IsActive { get; set; } = true;
    }
}
