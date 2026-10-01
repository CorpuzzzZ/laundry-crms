using System;

namespace CRM.Domain.DTOs.Terms
{
    public class TermsDto
    {
        public int TermsId { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsMandatory { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateTermsDto
    {
        public string Version { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsMandatory { get; set; } = true;
        public int? CompanyId { get; set; }
    }

    public class UpdateTermsDto
    {
        public string Version { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsMandatory { get; set; }
    }
}
