using System;

namespace CRM.Domain.Entities.MasterDb
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? RoleDescription { get; set; }
        public string PermissionJSON { get; set; } = string.Empty;
        public bool IsSystemRole { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}