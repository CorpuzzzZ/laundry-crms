using System;

namespace CRM.Domain.Entities.MasterDb
{
    public class UserBranch
    {
        public int UserBranchId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public bool IsPrimary { get; set; } = false;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public virtual ApplicationUser? User { get; set; }
        public virtual Branch? Branch { get; set; }
    }
}