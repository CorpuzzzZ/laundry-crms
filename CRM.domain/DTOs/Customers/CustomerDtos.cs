using System;

namespace CRM.Domain.DTOs.Customers
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerType { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => (FirstName + " " + LastName).Trim();
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
        // Archive
        public bool IsArchived { get; set; }
        public DateTime? ArchivedAt { get; set; }
    }

    public class CreateCustomerDto
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

    public class UpdateCustomerDto
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
        public bool IsActive { get; set; } = true;
    }

    public class CustomerFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? CustomerType { get; set; }
        public bool? IsActive { get; set; }
        public bool? HasLoyaltyPoints { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "DESC";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool IncludeArchived { get; set; } = false;
    }
}
