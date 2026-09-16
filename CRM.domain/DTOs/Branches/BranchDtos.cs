using System;

namespace CRM.Domain.DTOs.Branches
{
    public class BranchDto
    {
        public int BranchId { get; set; }
        public int CompanyId { get; set; }
        public string BranchCode { get; set; } = "";
        public string BranchName { get; set; } = "";
        public string Street { get; set; } = "";
        public string? Village { get; set; }
        public string City { get; set; } = "";
        public string? Province { get; set; }
        public string PostalCode { get; set; } = "";
        public string Country { get; set; } = "Philippines";
        public string Phone { get; set; } = "";
        public string? Email { get; set; }
        public string? ManagerName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? ArchivedAt { get; set; }
    }

    public class CreateBranchDto
    {
        // BranchCode is auto-generated on the server
        public string BranchName { get; set; } = "";
        public string Street { get; set; } = "";
        public string? Village { get; set; }
        public string City { get; set; } = "";
        public string? Province { get; set; }
        public string PostalCode { get; set; } = "";
        public string Country { get; set; } = "Philippines";
        public string Phone { get; set; } = "";
        public string? Email { get; set; }
    }

    public class UpdateBranchDto
    {
        public string BranchName { get; set; } = "";
        public string Street { get; set; } = "";
        public string? Village { get; set; }
        public string City { get; set; } = "";
        public string? Province { get; set; }
        public string PostalCode { get; set; } = "";
        public string Country { get; set; } = "Philippines";
        public string Phone { get; set; } = "";
        public string? Email { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class BranchFilterDto
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public bool IncludeArchived { get; set; } = false;
    }

    public class BranchLimitInfoDto
    {
        public int CurrentBranches { get; set; }
        public int MaxBranches { get; set; }
        public bool CanAddMore { get; set; }
        public string? PlanName { get; set; }
    }
}
