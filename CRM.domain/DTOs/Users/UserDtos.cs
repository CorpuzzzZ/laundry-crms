using System;
using System.Collections.Generic;

namespace CRM.Domain.DTOs.Users
{
    public class UserDto
    {
        public string Id { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public int CompanyId { get; set; }
        public string Role { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public List<int> BranchIds { get; set; } = new();
        public List<string> BranchNames { get; set; } = new();

        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    public class CreateUserDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = "";
        public string Role { get; set; } = "Crew";  // Admin | Manager | Crew
        public List<int> BranchIds { get; set; } = new();
    }

    public class UpdateUserDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = "Crew";
        public bool IsActive { get; set; } = true;
    }

    public class ResetPasswordDto
    {
        public string NewPassword { get; set; } = "";
    }

    public class SetUserBranchesDto
    {
        public List<int> BranchIds { get; set; } = new();
        public int? PrimaryBranchId { get; set; }
    }

    public class UserFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public int? BranchId { get; set; }
        public bool IncludeArchived { get; set; } = false;
    }

    public class UserLimitInfoDto
    {
        public int CurrentUsers { get; set; }
        public int MaxUsers { get; set; }
        public bool CanAddMore { get; set; }
        public string? PlanName { get; set; }
    }

    public class AssignableRoleDto
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }
}