using System;
using System.Collections.Generic;

namespace CRM.WinForms.Models
{
    public class UserModel
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
        public string BranchSummary => BranchNames.Count == 0
            ? "No branches assigned"
            : string.Join(", ", BranchNames);
    }

    public class CreateUserRequest
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = "";
        public string Role { get; set; } = "Crew";
        public List<int> BranchIds { get; set; } = new();
    }

    public class UpdateUserRequest
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = "Crew";
        public bool IsActive { get; set; } = true;
    }

    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; } = "";
    }

    public class SetUserBranchesRequest
    {
        public List<int> BranchIds { get; set; } = new();
        public int? PrimaryBranchId { get; set; }
    }

    public class UserLimitInfoModel
    {
        public int CurrentUsers { get; set; }
        public int MaxUsers { get; set; }
        public bool CanAddMore { get; set; }
        public string? PlanName { get; set; }
    }

    public class AssignableRoleModel
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class UserResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public UserModel? Data { get; set; }
    }

    public class UserListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public List<UserModel> Data { get; set; } = new();
    }

    public class AssignableRolesResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public List<AssignableRoleModel> Data { get; set; } = new();
    }

    public class UserLimitInfoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public UserLimitInfoModel? Data { get; set; }
    }
}